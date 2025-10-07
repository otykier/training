// Find or set up the calculated parameter table and the column and measure within it:
var paramTableName = Selected.Table.Name + " Measure";

var paramTable = Model.Tables.FindByName(paramTableName) as CalculatedTable;
if (paramTable == null) paramTable = Model.AddCalculatedTable(paramTableName);

var paramTableColumn = paramTable.Columns.FirstOrDefault();
if (paramTableColumn == null) paramTableColumn = paramTable.AddCalculatedTableColumn("Measure", "Measure");

var paramMeasure = paramTable.Measures.FirstOrDefault();
if (paramMeasure == null) paramMeasure = paramTable.AddMeasure(paramTableName);

// Construct the DAX expressions of the parameter table and measure based on the current selection of measures:
var paramTableDax = "DATATABLE(\"Measure\", STRING, {";
var measureDax = "SWITCH(SELECTEDVALUE(" + paramTableColumn.DaxObjectFullName + "),";

var first = true;
foreach(var measure in Selected.Measures)
{
    if(!first) {
        paramTableDax += ",";
        measureDax += ",";
    }
    paramTableDax += "{\"" + measure.Name + "\"}";
    measureDax += "\"" + measure.Name + "\"," + measure.DaxObjectName;

    first = false;
}
paramTableDax += "})";
measureDax += ",\"Select measure\")";

paramTable.Expression = paramTableDax;
paramMeasure.Expression = measureDax;