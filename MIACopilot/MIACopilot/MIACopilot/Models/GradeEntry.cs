namespace MIACopilot.Models;

public class GradeEntry
{
    public int Id { get; set; }
    public int ApprenticeId { get; set; }

    public string Subject { get; set; } = string.Empty;
    public double Grade { get; set; }          // 1.0 – 6.0
    public double WeightPercent { get; set; }  // 0 – 100

    public int Semester { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
}