using System.Text.Json;
using System.Text.Json.Serialization;
using MIACopilot.Models;

namespace MIACopilot.Services;

/// <summary>
/// Tolerant DateTime converter: strips any duplicate time-suffix before parsing.
/// Handles malformed strings like "2024-08-01T00:00:00T00:00:00".
/// </summary>
internal sealed class RobustDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString() ?? "";
        // Remove any second T…-suffix (e.g. "…T00:00:00T00:00:00" → "…T00:00:00")
        var tIdx = raw.IndexOf('T');
        if (tIdx >= 0)
        {
            var second = raw.IndexOf('T', tIdx + 1);
            if (second >= 0) raw = raw[..second];
        }
        return DateTime.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
}

/// <summary>
/// Handles loading and saving all application data to and from JSON files.
/// Acts as a simple persistence layer.
/// </summary>
public class DataService
{
    // Base folder where all JSON data files are stored
    private readonly string _dataFolder = "data";

    // JSON options (pretty-printed, with a tolerant DateTime converter)
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters    = { new RobustDateTimeConverter() }
    };

    // File paths for each entity type
    private string ApprenticesFile => Path.Combine(_dataFolder, "apprentices.json");
    private string CompaniesFile   => Path.Combine(_dataFolder, "companies.json");
    private string TrainersFile    => Path.Combine(_dataFolder, "trainers.json");

    /// <summary>
    /// Ensures the data directory exists on application startup.
    /// </summary>
    public DataService()
    {
        Directory.CreateDirectory(_dataFolder);
    }

    /// <summary>
    /// Loads all apprentices (including their work journals) from JSON.
    /// Returns an empty list if the file does not exist.
    /// </summary>
    public List<Apprentice> LoadApprentices()
    {
        if (!File.Exists(ApprenticesFile))
            return new List<Apprentice>();

        var json = File.ReadAllText(ApprenticesFile);
        return JsonSerializer.Deserialize<List<Apprentice>>(json, _options) ?? new();
    }

    /// <summary>
    /// Saves all apprentices (including journals) to JSON.
    /// </summary>
    public void SaveApprentices(List<Apprentice> apprentices)
    {
        var json = JsonSerializer.Serialize(apprentices, _options);
        File.WriteAllText(ApprenticesFile, json);
    }

    /// <summary>
    /// Loads all companies from JSON.
    /// Returns an empty list if the file does not exist.
    /// </summary>
    public List<Company> LoadCompanies()
    {
        if (!File.Exists(CompaniesFile))
            return new List<Company>();

        var json = File.ReadAllText(CompaniesFile);
        return JsonSerializer.Deserialize<List<Company>>(json, _options) ?? new();
    }

    /// <summary>
    /// Saves all companies to JSON.
    /// </summary>
    public void SaveCompanies(List<Company> companies)
    {
        var json = JsonSerializer.Serialize(companies, _options);
        File.WriteAllText(CompaniesFile, json);
    }

    /// <summary>
    /// Loads all vocational trainers from JSON.
    /// Returns an empty list if the file does not exist.
    /// </summary>
    public List<VocationalTrainer> LoadTrainers()
    {
        if (!File.Exists(TrainersFile))
            return new List<VocationalTrainer>();

        var json = File.ReadAllText(TrainersFile);
        return JsonSerializer.Deserialize<List<VocationalTrainer>>(json, _options) ?? new();
    }

    /// <summary>
    /// Saves all vocational trainers to JSON.
    /// </summary>
    public void SaveTrainers(List<VocationalTrainer> trainers)
    {
        var json = JsonSerializer.Serialize(trainers, _options);
        File.WriteAllText(TrainersFile, json);
    }
}