string dax = "";

// If only a single object is selected, just display that:
if (Selected.DirectCount == 1)
{
    if(Selected.FirstOrDefault() is Measure)
    {
        dax = Selected.Measure.DaxObjectFullName;
    }
    else if(Selected.FirstOrDefault() is Column)
    {
        dax = "TOPN(1000, DISTINCT(" + Selected.Column.DaxObjectFullName + "))";
    }
    else if(Selected.FirstOrDefault() is Table)
    {
        dax = "TOPN(1000, " + Selected.Table.DaxObjectName + ")";
    }
    else { Warning("Can't execute query for: " + Selected.Summary()); return; }
}
else if (Selected.Measures.Count > 0)
{
    // If multiple measures are selected, return the result of each of them in a single row:
    dax = "ROW(" + string.Join(",", Selected.Measures.Select(m => "\"" + m.Name + "\"," + m.DaxObjectName)) + ")";

}
else if (Selected.Columns.Count > 0)
{
    // If multiple columns are selected, return a table containing stats for each:
    dax = "SELECTCOLUMNS({" + string.Join(",", Selected.Columns.Select(c => "(\"" + c.Name + "\",DISTINCTCOUNT(" + c.DaxObjectFullName + "),MIN(" + c.DaxObjectFullName + "),MAX(" + c.DaxObjectFullName + "))")) + "}," +
        "\"Column\",[Value1]," + 
        "\"Distinct values count\",[Value2]," + 
        "\"Min value\",[Value3]," + 
        "\"Max value\",[Value4]" + 
        ")";    
}
else if (Selected.Tables.Count > 0)
{
    // If multiple tables are selected, return a table containing the row count of each:
    dax = "SELECTCOLUMNS({" + string.Join(",", Selected.Tables.Select(t => "(\"" + t.Name + "\",COUNTROWS(" + t.DaxObjectName + "))")) + "},\"Table\",[Value1],\"Row count\",[Value2])";
}
else { Warning("Can't execute query for: " + Selected.Summary()); return; }

// Uncomment line below to view the generated DAX:
// Output(dax);

EvaluateDax(dax).Output();