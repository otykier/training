# Tabular Editor script API - exercises

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

The exercises below will build upon what you learned in the previous exercises, this time with an emphasis on using the various methods available in the [Tabular Editor script API](https://docs.tabulareditor.com/api/TabularEditor.Shared.Scripting.ScriptHost.html#methods).

- **`Selected`**: We used this object in the previous exercises as well, but now we'll take things one step further
  - Singular properties (when exactly one of that type of object is selected)
  - Plural properties (when zero or more of that type of object is selected)
- **Collection extensions**: Performing common tasks on collections, i.e.
  - Setting all properties at once
  - `Rename(string pattern, string replacement, [bool regex], [bool includeNameTranslations])`: For batch renaming
  - `ReplaceExpression(string pattern, string replcaement, [bool regex])`: For string replacing in `Expression` properties
  - `ShowInPerspective(string perspectiveName)` / `ShowInAllPerspectives()` and corresponding `Hide` methods
  - `Delete()`: Deleting multiple objects at once
- **File IO**:
  - `string ReadFile(string path)`: Read the contents of a file as text
  - `void SaveFile(string path, string content)`: Save a string to a text file (overwriting the file if it already exists)
- **Serializing / deserializing object properties to TSV**:
  - `string ExportProperties(IEnumerable<ITabularNamedObject> objects, string properties)`: Serializing the specified properties of the specified objects into TSV format (tab-separated values, readable in Excel)
  - `void ImportProperties(string tsvData)`: Deserializing properties from the TSV format into existing objects in the model
- **Querying Analysis Services**: Methods that send commands and queries to the connected AS instance, allowing you to work with actual _data_ rather than just _metadata_:
  - `EvaluateDax(string dax)`: Returns a .NET [DataTable](https://learn.microsoft.com/en-us/dotnet/api/system.data.datatable?view=net-8.0) or a primitive object, depending on whether `dax` is a table or scalar expression
  - `ExecuteDax(string query)`: Executes any valid DAX, MDX, or DMV query, and returns the result as a .NET [DataSet](https://learn.microsoft.com/en-us/dotnet/api/system.data.dataset?view=net-8.0) (a DAX query can have multiple `EVALUATE` statements, which is why this method returns a `DataSet` instead of a `DataTable`).
  - `ExecuteReader(string query)`: Same as above, except that it returns an [IDataReader](https://learn.microsoft.com/en-us/dotnet/api/system.data.idatareader?view=net-8.0), so you can manually iterate through the output of the query
  - `ExecuteCommand(string tmslOrXmla, bool isXmla = false)`: Allow you to execute a raw  [TMSL](https://learn.microsoft.com/en-us/analysis-services/tmsl/tabular-model-scripting-language-tmsl-reference?view=asallproducts-allversions) or [XMLA](https://docs.tabulareditor.com/te2/Useful-script-snippets.html#clearing-the-analysis-services-engine-cache) statement or command. This is commonly used to perform [refresh operations](https://docs.tabulareditor.com/te2/Useful-script-snippets.html#querying-analysis-services). **Note:** If you execute a command that modifies TOM objects in the currently connected model, you will have to reload the model in Tabular Editor, to view these changes.
- **Outputting and debugging**:
  - `void Output(this object value)`: Can output almost any type of value, collection of values, TOM object, or DataTable. Also available as an extension method so you can use the syntax: `value.Output();`. Don't be afraid to use this in a loop, as the output dialog has a checkbox that allows you to skip additional outputs.
  - `Info(string message)`, `Warning(string message)`, `Error(string message)`: You've seen these in the previous exercises
  - `throw new Exception(string message)`: Show an error message and also halts script execution (unless catched by a `try { ... } catch { ... }` block)
- **Prompting the user**:
  - `SelectTable(this IEnumerable<Table> tables = null, Table preselect = null, string label = null)`: Show a dailog that asks the user to select a table from the provided list of tables (or all tables in the model, if no list is provided)
  - `SelectColumn(...)`, `SelectMeasure(...)`: Same as above but for columns and measures respectively.
  - `SelectObject<T>(...)`: Same as above for any `T: TabularNamedObject`.
- **Formatting DAX**:
  - `FormatDax(this IDaxDependantObject obj)`: Extension method that flags an object for formatting after script execution (TE2 uses www.daxformatter.com, TE3 uses built-in formatter by default)
  - `ConvertDax(string dax, bool useSemicolons)`: Convert DAX expressions that use US locale to non-US, or vice versa
- **Tokenizing DAX**:
  - `Tokenize(this IDaxDepedantObject obj, DAXProperty property = DAXProperty.Expression, bool includeHidden = true)`: Tokenizes the specified DAX expression on the object. Includes hidden tokens (comments, whitespace) by default.

## Exercise 2.1 - Apply default properties

Write a script which will work on any number of selected measures. The script should:

- Set the `FormatString` property to "#.##0,00", if the measure does not already have a FormatString
- Set the `DisplayFolder` property to "Measures", if the measure does not already belong to a DisplayFolder
- Set the `Description` propery to "TODO: Provide a description for this measure", if the measure does not already have a description **and** the measure is visible

Try to avoid the temptation to use `foreach` loops, `for` loops, or `if` statements.

<details><summary>Click to view solution</summary>

```csharp
Selected.Measures.Where(m => !string.IsNullOrEmpty(m.FormatString)).FormatString = "#.##0,00";
Selected.Measures.Where(m => !string.IsNullOrEmpty(m.DisplayFolder)).DisplayFolder = "Measures";
Selected.Measures.Where(m => !string.IsNullOrEmpty(m.Description) && m.IsVisible).Description = "TODO: Provide a description for this measure";
```

</details>
