// Updates the name of every selected object so "CamelCase" becomes "Camel Case"
foreach(var obj in Selected)
{
    if (obj is Column && obj.Name.EndsWith("Key")) continue; // Ignore columns with the "Key" suffix
    obj.Name = obj.Name.SplitCamelCase().Replace("  ", " ").Replace("  ", " ").Trim();
}