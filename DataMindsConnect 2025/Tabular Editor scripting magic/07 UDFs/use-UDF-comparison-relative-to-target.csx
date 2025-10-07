using System.Text.RegularExpressions;

foreach (var measure in Model.AllMeasures.Where(m => m.Name.Contains("vs.") && m.Name.Contains("(%)")))
{
    var daxCode = measure.Expression;

    // Extract the first two bracketed references [Measure]
    var matches = Regex.Matches(daxCode, @"\[[^\]]+\]")
                       .Cast<Match>()
                       .Select(m => m.Value)
                       .ToList();

    // Replace measure expression with UDF call only if exactly two references are found
    if (matches.Count == 2)
    {
        var (target, actual) = (matches[0], matches[1]);

        measure.Expression = $"Comparison.RelativeToTarget({actual}, {target})";
        measure.FormatDax();
    }
}
CallDaxFormatter();