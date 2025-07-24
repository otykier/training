var keySuffix = "Key";

// Loop through all currently selected tables (assumed to be fact tables):
foreach(var fact in Selected.Tables)
{
    // Loop through all SK columns on the current table:
    foreach(var factColumn in fact.Columns.Where(c => c.Name.EndsWith(keySuffix)))
    {
        // Find the dimension table corresponding to the current SK column:
        var dim = Model.Tables.FirstOrDefault(t => factColumn.Name.EndsWith(t.Name.Replace(" ", "") + keySuffix));
        if(dim != null)
        {
            // Find the key column on the dimension table:
            var dimColumn = dim.Columns.FirstOrDefault(c => factColumn.Name.EndsWith(c.Name));
            if(dimColumn != null)
            {
                // Check whether a relationship already exists between the two columns:
                if(!Model.Relationships.Any(r => r.FromColumn == factColumn && r.ToColumn == dimColumn))
                {
                    // If relationships already exists between the two tables, new relationships will be created as inactive:
                    var makeInactive = Model.Relationships.Any(r => r.FromTable == fact && r.ToTable == dim);

                    // Add the new relationship:
                    var rel = Model.AddRelationship();
                    rel.FromColumn = factColumn;
                    rel.ToColumn = dimColumn;
                    factColumn.IsHidden = true;
                    if(makeInactive) rel.IsActive = false;
                }
            }
        }
    }
}
