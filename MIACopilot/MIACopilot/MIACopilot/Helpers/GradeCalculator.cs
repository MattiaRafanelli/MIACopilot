using System;
using System.Collections.Generic;
using System.Linq;

namespace MIACopilot.Helpers;

public static class GradeCalculator
{
    public static double WeightedAverage(IEnumerable<(double grade, double weight)> values)
    {
        var list = values.Where(v => v.weight > 0).ToList();
        if (!list.Any()) return 0;

        var sum = list.Sum(v => v.weight);
        return list.Sum(v => v.grade * (v.weight / sum));
    }

    public static double RoundToHalf(double value)
        => Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2.0;
}