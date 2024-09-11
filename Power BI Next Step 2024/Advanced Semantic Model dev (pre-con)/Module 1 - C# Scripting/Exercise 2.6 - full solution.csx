class Util 
{
    public static string Replace(string original, int startIndex, int length, string replacement)
    {
        if (startIndex < 0 || startIndex >= original.Length)
            throw new ArgumentOutOfRangeException("startIndex");
        
        if (length < 0 || startIndex + length > original.Length)
            throw new ArgumentOutOfRangeException("length");

        return original.Substring(0, startIndex) + replacement + original.Substring(startIndex + length);
    }
}

foreach(var measure in Model.AllMeasures)
{
    var dax = measure.Expression;
    var tokens = measure.Tokenize(includeHidden: false);
    
    // If the expression does not contain a DaxToken.SUM, continue to the next measure...
    if(!tokens.Any(t => t.Type == DaxToken.SUM)) continue;
    
    // Perform replacements in reverse order (starting from the 5th to last token only since tokens after
    // that cannot possibly be SUM, as the full sequence is 5 tokens long).
    for(int i = tokens.Count - 5; i >= 0; i--)
    {
        if(tokens[i].Type == DaxToken.SUM)
        {
            var replaceFrom = tokens[i].StartIndex;
            var replaceLength = tokens[i + 4].StopIndex - replaceFrom + 1;
            var tableName = tokens[i + 2].Type == DaxToken.TABLE ? "'" + tokens[i + 2].Text + "'" : tokens[i + 2].Text;
            var columnName = "[" + tokens[i + 3].Text + "]";
            var replacement = "SUMX(" + tableName + ", " + tableName + columnName + ")";
            
            dax = Util.Replace(dax, replaceFrom, replaceLength, replacement);
        }
    }
    
    // Uncomment the line below to view the original and modified DAX:
    // Output("Original:\r\n" + measure.Expression + "\r\n\r\nModififed:\r\n" + dax);
    
    measure.Expression = dax;
}