# C# script introduction - exercises

In all these exercises, you will use the C# scripting feature of Tabular Editor to solve various problems.

Remember, you can always **undo** the model metadata changes caused by a script, by moving the focus over to the TOM Explorer and hitting **Ctrl+Z** (**Edit > Undo script**).

<details>
<summary><b>Tabular Editor 2 instructions</b></summary>

To write and run C# scripts in **Tabular Editor 2**, the general steps are the following:

1) Launch Tabular Editor 2
2) Open a model (**File > Open > From File...** or **File > Open > From DB...**)
3) Go to the **C# Script** tab
4) Enter the script code and hit F5 to run the script
5) (Optional) Save your script to a file by clicking the **Save** icon just above the script editor

<img src="https://github.com/user-attachments/assets/f527fb90-036a-4dbb-b085-49e7ecac3004" width="50%">

</details>
<details>
<summary><b>Tabular Editor 3 instructions</b></summary>

To write and run C# scripts in **Tabular Editor 2**, the general steps are the following:

1) Launch Tabular Editor 3
2) (Optional) Open a model (**File > Open > Model from File...** or **File > Open > Model from DB...**)
3) Create a new C# script tab (**File > New > New C# script**
4) Enter the script code and hit F5 to run the script
5) (Optional) Save your script to a file by hitting **Ctrl+S** (**File > Save**)

<img src="https://github.com/user-attachments/assets/8c1d02d1-f541-494f-b853-d8951fb87d22" width="50%">

**Note:** In Tabular Editor 3, scripts can be executed even when no model is loaded. However, you will not be able to access the `Model` or the `Selected` top-level objects, since no model metadata is present.

</details>

The exercises below will introduce you to the world of C# scripting in Tabular Editor. The exercises will teach you basic C# syntax, while also introducing you to Tabular Editor-specific concepts and helper methods / extensions from the TOMWrapper API. Below is a summary of key concepts that you will need to use in the scripts:

- **Top-level objects**: The common "entry points" for scripts that need to interact with the currently loaded model
  - `Model`: The root of the Tabular Object Model hierarchy. Provides read/write access to all of the semantic model metadata.
  - `Selected`: An object holding information about the currently selected objects in Tabular Editor's TOM Explorer UI. Useful when creating generic scripts that need to interact with the users' current selection.
- **Helper methods**: Methods that only exist in Tabular Editor, which can be used to debug scripts, or display information to the user.
  - `void Info(string message)`: Used to display an informational message to the user.
  - `void Warning(string message)`: Used to display a warning message to the user.
  - `void Error(string message)`: Used to display an error message to the user.
  - For a full list of helper methods, see the [API documentation](https://docs.tabulareditor.com/api/TabularEditor.Shared.Scripting.ScriptHost.html#methods).
- **TOM extensions**: Methods and properties from the TOMWrapper API, which make certain semantic modeling related tasks easier.
  - `int Model.AllMeasures`: Returns an enumeration of all measures across all tables in the model (same as `Model.Tables.SelectMany(t => t.Measures)`)
  - `Measure Table.AddMeasure(string name, string expression)`: Adds a measure to the current table, with the given name and expression
  - `string Column.DaxObjectFullName`: A property that returns a valid DAX reference to the current column, i.e.: `'Internet Sales'[Line Amount]`

## Exercise 1.1 - "Hello world!"

Write a script which will do nothing except show a message to the user.

<details>
  <summary>Click to view solution</summary>

  ```csharp
  Info("Hello World!");
  ```
</details>

## Exercise 1.2 - Basic model stats

Write a script that will output the following information:

- The name of the model
- The number of tables in the model
- The number of measures in the model
- The number of _visible_ measures in the model
- The number of _visible_ measures in the model, that does not have any description specified

<details>
  <summary>Click to view solution</summary>

  ```csharp
var modelName = Model.Name;
var tables = Model.Tables.Count;
var measures = Model.AllMeasures.Count();
var visibleMeasures = Model.AllMeasures.Count(m => m.IsVisible);
var visibleMeasuresNoDesc = Model.AllMeasures.Count(m => m.IsVisible && string.IsNullOrEmpty(m.Description));

var text = @"Model name: {0}
Tables: {1}
Measures: {2}
Visible measures: {3}
Visible measures without descriptions: {4}";

string.Format(text, modelName, tables, measures, visibleMeasures, visibleMeasuresNoDesc).Output();
  ```
</details>

## Exercise 1.3 - Output selection details

Write a script that will investigate the current selection, with the following conditions:

- If nothing is selected, show "Nothing selected".
- If exactly one object is selected, show its name followed by the object type (in parentheses). Also, if the selected object is a measure, show its DAX expression.
- If multiple objects are selected, show the number of objects selected.

<details>
  <summary>Click to view solution</summary>

```csharp
if(Selected.DirectCount == 0)
{
    Info("Nothing selected");
}
else if(Selected.DirectCount == 1)
{
    var item = Selected.First();
    if(item is Measure)
    {
        var measure = item as Measure;
        Info(string.Format("{0} ({1})\r\n{2}", item.Name, item.ObjectType, measure.Expression));
    }
    else
    {
        Info(string.Format("{0} ({1})", item.Name, item.ObjectType));
    }
}
else
{
    Info(string.Format("Selected {0} items", Selected.DirectCount));
}
```

</details>

## Exercise 1.4 - Create system measure

Write a script that will add a measure to the currently selected table. The measure should have the name "System Info", and contain the following DAX:

```dax
VAR crlf = UNICHAR(13) & UNICHAR(10)
RETURN
    "Time is: " & NOW() & crlf &
    "Logged in user: " & USERPRINCIPALNAME() & crlf &
    "Current culture: " & USERCULTURE()
```

If no table is currently selected, add the measure to the first table of the model. If no tables exist in the model, show an error message.

<details>
  <summary>Click to view solution</summary>

```csharp
var dax = @"VAR crlf = UNICHAR(13) & UNICHAR(10)
RETURN
    ""Time is: "" & NOW() & crlf &
    ""Logged in user: "" & USERPRINCIPALNAME() & crlf &
    ""Current culture: "" & USERCULTURE()";

if(Selected.Tables.Count() == 1)
    Selected.Table.AddMeasure("System Info", dax);
else if(Model.Tables.Count > 0)
    Model.Tables[0].AddMeasure("System Info", dax);
else
    Error("Cannot add measure when no tables exist in the model");
```

</details>

**Question:** What happens if you run the script multiple times?

**Bonus:** Add an annotation to the measure to indicate it was generated by a script. Extend the script so it automatically deletes an existing measure with that annotation, before adding the measure.

<details>
  <summary>Click to view solution</summary>

```csharp
const string AUTOGEN = "AUTOGEN";
const string SYSTEM_INFO = "SYSTEM_INFO";

var dax = @"VAR crlf = UNICHAR(13) & UNICHAR(10)
RETURN
    ""Time is: "" & NOW() & crlf &
    ""Logged in user: "" & USERPRINCIPALNAME() & crlf &
    ""Current culture: "" & USERCULTURE()";

var existingMeasures = Model.AllMeasures.Where(m => m.GetAnnotation(AUTOGEN) == SYSTEM_INFO).ToList();
foreach(var m in existingMeasures) m.Delete();

if(Selected.Tables.Count() == 1)
    Selected.Table.AddMeasure("System Info", dax).SetAnnotation(AUTOGEN, SYSTEM_INFO);
else if(Model.Tables.Count > 0)
    Model.Tables[0].AddMeasure("System Info", dax).SetAnnotation(AUTOGEN, SYSTEM_INFO);
else
    Error("Cannot add measure when no tables exist in the model");
```

</details>

## Exercise 1.5 - Create SUM measures from columns

A common use case for C# scripts, and a best practice when creating semantic models, is to hide numeric, aggregatable columns (aka. _implicit_ measures) and create an actual, _explicit_ measure that performs the aggregation (i.e. `SUM`) of the column.

Write a script that will create one measure for each currently selected column. The created measure should just return the `SUM` of that column. Add a suitable description to the measure, set the format string to `0.00` and hide the column.

**Hint:** Objects that can be referenced in DAX expressions (tables, columns, measures) have a `DaxObjectFullName` property, which returns a string that contains a valid DAX reference to the current object.

<details>
  <summary>Click to view solution</summary>

This is the first example on the [Useful script snippets](https://docs.tabulareditor.com/te2/Useful-script-snippets.html#create-measures-from-columns) article.

</details>

**Question:** What happens if you run this script on a column with a non-numeric data type (i.e. a `String` column)?

**Bonus:** Add a condition to your script, so it only creates explicit measures for columns that have the `DataType.Int64`, `DataType.Decimal` or `DataType.Double` data types.

<details>
  <summary>Click to view solution</summary>

```csharp
// Creates a SUM measure for every currently selected (numeric) column and hide the column.
foreach(var c in Selected.Columns)
{
    if(c.DataType != DataType.Int64 && c.DataType != DataType.Decimal && c.DataType != DataType.Double) continue;
    
    var newMeasure = c.Table.AddMeasure(
        "Sum of " + c.Name,                    // Name
        "SUM(" + c.DaxObjectFullName + ")",    // DAX expression
        c.DisplayFolder                        // Display Folder
    );
    
    // Set the format string on the new measure:
    newMeasure.FormatString = "0.00";

    // Provide some documentation:
    newMeasure.Description = "This measure is the sum of column " + c.DaxObjectFullName;

    // Hide the base column:
    c.IsHidden = true;
}
```

</details>
