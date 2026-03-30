using System;
using System.Collections.Generic;
using System.Linq;

namespace MIACopilot.Helpers;

/// <summary>
/// Helper class for grade-related calculations.
/// </summary>
public static class GradeCalculator
{
    /// <summary>
    /// Calculates a weighted average from a list of (grade, weight) tuples.
    /// Grades with a weight <= 0 are ignored.
    /// Returns 0 if no valid values are provided.
    /// </summary>
    public static double WeightedAverage(IEnumerable<(double grade, double weight)> values)
    {
        // Filter out entries with non-positive weight
        var list = values.Where(v => v.weight > 0).ToList();

        // No valid values → average is 0
        if (!list.Any()) return 0;

        // Sum of all weights
        var sum = list.Sum(v => v.weight);

        // Weighted average: Σ(grade * (weight / totalWeight))
        return list.Sum(v => v.grade * (v.weight / sum));
    }

    /// <summary>
    /// Rounds a value to the nearest 0.5 step
    /// (e.g. 4.24 → 4.0, 4.25 → 4.5, 4.75 → 5.0).
    /// Uses AwayFromZero to avoid banker’s rounding.
    /// </summary>
    public static double RoundToHalf(double value)
        => Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2.0;
}