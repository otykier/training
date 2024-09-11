# Tabular Editor script API - exercises
(Click on the section headers to expand/collapse)

> [!Note]
> As you may have realized by now, most of these exercises can be solved in many different ways. The solutions provided are just one way - so don't dispair if the solution you came up with doesn't match the suggested solutions. As long as your solution works ☺

<details>
<summary><h2>Introduction</h2></summary>

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

</details>
<details>
<summary><h3>Exercise 2.1 - Apply default properties</h3></summary>

Write a script which will work on any number of selected measures. The script should:

- Set the `FormatString` property to "#.##0,00", if the measure does not already have a FormatString
- Set the `DisplayFolder` property to "Measures", if the measure does not already belong to a DisplayFolder
- Set the `Description` propery to "TODO: Provide a description for this measure", if the measure does not already have a description **and** the measure is visible

Try to avoid the temptation to use `foreach` loops, `for` loops, or `if` statements.

<details><summary>Click to view solution</summary>

```csharp
Selected.Measures.Where(m => string.IsNullOrEmpty(m.FormatString)).FormatString = "#.##0,00";
Selected.Measures.Where(m => string.IsNullOrEmpty(m.DisplayFolder)).DisplayFolder = "Measures";
Selected.Measures.Where(m => string.IsNullOrEmpty(m.Description) && m.IsVisible).Description = "TODO: Provide a description for this measure";
```

</details>
</details>
<details>
<summary><h3>Exercise 2.2 - Formatting DAX</h3></summary>

### 2.2a
Write a script which will format the DAX of every single measure in the model. Bonus points for keeping it to a single line of code.

<details><summary>Click to view solution</summary>

```csharp
Model.AllMeasures.FormatDax();
```

</details>

### 2.2b
Extend your solution to 2.2a so that it also includes the following object types:

- Calculated Columns
- Calculated Tables
- Calculation Groups
- Calculation Items

<details><summary>Click to view solution</summary>

```csharp
Model.AllMeasures.FormatDax();
Model.AllColumns.OfType<CalculatedColumn>().FormatDax();
Model.Tables.OfType<CalculatedTable>().FormatDax();
Model.CalculationGroups.FormatDax();
Model.AllCalculationItems.FormatDax();
```

</details>
  
</details>
<details>
<summary><h3>Exercise 2.3 - "Can I export this to Excel?"</h3></summary>

Sometimes, it's useful to ask business users to provide their own descriptions of the various visible objects in the model, since descriptions show up as tooltips in client tools (Excel, Power BI). And since business users love Excel so much, we may as well just give them the list of objects in a format they can work with in Excel.

### 2.3a
Write a script that will create a TSV file containing all visible model objects (tables, columns, hierarchies, and measures), including only their "Description" property.

<details><summary>Click to view solution</summary>

```csharp
var objects = new List<TabularNamedObject>();
objects.AddRange(Model.Tables.Where(t => t.IsVisible));
objects.AddRange(Model.AllColumns.Where(c => c.IsVisible));
objects.AddRange(Model.AllHierarchies.Where(h => h.IsVisible));
objects.AddRange(Model.AllMeasures.Where(m => m.IsVisible));

var tsvData = ExportProperties(objects, "Description");
tsvData.Output();
// SaveFile("c:\\temp\\model-objects.tsv", tsvData);
```

</details>

### 2.3b
Write a script that will read a TSV file containing object descriptions, similar to the one created in **2.3a**, and apply those to the model.

<details><summary>Click to view solution</summary>

```csharp
var tsvData = ReadFile("c:\\temp\\model-objects.tsv");
ImportProperties(tsvData);
```

</details>

**Bonus:** Add a Danish translation to the model (da-DK). Then, extend the script from **2.3a** above, so that the TSV file also includes the `Name` property, the `DisplayFolder` property, as well as `TranslatedNames[da-DK]`, `TranslatedDescriptions[da-DK]`, and `TranslatedDisplayFolders[da-DK]`, so the business users also have a way to provide danish translations for the objects.

<details><summary>Click to view solution</summary>

```csharp
var objects = new List<TabularNamedObject>();
objects.AddRange(Model.Tables.Where(t => t.IsVisible));
objects.AddRange(Model.AllColumns.Where(c => c.IsVisible));
objects.AddRange(Model.AllHierarchies.Where(h => h.IsVisible));
objects.AddRange(Model.AllMeasures.Where(m => m.IsVisible));

var tsvData = ExportProperties(objects, "Name,Description,DisplayFolder,TranslatedNames[da-DK],TranslatedDescriptions[da-DK],TranslatedDisplayFolders[da-DK]");
tsvData.Output();
// SaveFile("c:\\temp\\model-objects.tsv", tsvData);
```

</details>

</details>

</details>
<details>
<summary><h3>Exercise 2.4 - Parameter table</h3></summary>

A [parameter table](https://www.daxpatterns.com/parameter-table/) is a table that does not have any relationships to other tables in the model. Typically, the table only contains a single column. Any selection/filter made on the table, is observed in suitable DAX expressions within measures.

In this exercise, we'll write a script that dynamically creates a calculated table and a `SWITCH` measure, based on a selection of measures in the TOM Explorer. The idea is to have a single measure, which can display the result of any one of the selected measures, when the user applies a filter on the calculated table (which serves as our parameter table).

For example, if the user selects the following measures in the TOM Explorer and runs the script:

- [Sales Amount]
- [Cost Amount]
- [Margin Amount]
- [Margin Pct]

The script should generate a calculated table named 'Measure Selection', with the following DAX expression:

```dax
{
    NAMEOF([Sales Amount]),
    NAMEOF([Cost Amount]),
    NAMEOF([Margin Amount]),
    NAMEOF([Margin Pct])
}
```

It's recommended to use the [`NAMEOF`](https://dax.guide/nameof) function instead of hard-coding the names of the measures as strings. This way, if you ever rename one of the measures, the DAX inside the calculated table will be correctly updated.

The script should also generate a measure, on the same table as the original 4 measures were selected, with the name `Dynamic Measure`, and the following DAX expression:

```dax
SWITCH(
    SELECTEDVALUE('Measure Selection'[Value]),
    NAMEOF([Sales Amount]), [Sales Amount],
    NAMEOF([Cost Amount]), [Cost Amount],
    NAMEOF([Margin Amount]), [Margin Amount],
    NAMEOF([Margin Pct]), [Margin Pct],
    "Please make a selection on the 'Measure Selection' table"
)
```

<details><summary>Click to view solution</summary>

```csharp
if(Selected.Measures.Count == 0)
{
    Info("No measures selected!");
    return;
}

var table = Selected.Measures.First().Table;

var calcTableDax = @"{{
{0}
}}";
var switchMeasureDax = @"SWITCH(
    SELECTEDVALUE('Measure Selection'[Value]),
{0},
    ""Please make a selection on the 'Measure Selection' table""
)";

var calcTableInner = string.Join(",\r\n", Selected.Measures.Select(m => "    NAMEOF(" + m.DaxObjectName + ")"));
var switchMeasureInner = string.Join(",\r\n", Selected.Measures.Select(m => "    NAMEOF(" + m.DaxObjectName + "), " + m.DaxObjectName));

Model.AddCalculatedTable("Measure Selection", string.Format(calcTableDax, calcTableInner));
table.AddMeasure("Dynamic Measure", string.Format(switchMeasureDax, switchMeasureInner));
```

</details>

**Bonus:** Instead of putting the [Dynamic Measure] on the same table as the selected measures, let's ask the user nicely which table they want the [Dynamic Measure] to be created in. Modify the script so the users are prompted to select a table. If the user cancels the prompt, nothing should happen.

<details><summary>Click to view solution</summary>

```csharp
if(Selected.Measures.Count == 0)
{
    Info("No measures selected!");
    return;
}

var table = SelectTable(preselect: Selected.Measures.First().Table, label: "Which table should the Dynamic Measure be added to?");
if(table == null) return;

var calcTableDax = @"{{
{0}
}}";
var switchMeasureDax = @"SWITCH(
    SELECTEDVALUE('Measure Selection'[Value]),
{0},
    ""Please make a selection on the 'Measure Selection' table""
)";

var calcTableInner = string.Join(",\r\n", Selected.Measures.Select(m => "    NAMEOF(" + m.DaxObjectName + ")"));
var switchMeasureInner = string.Join(",\r\n", Selected.Measures.Select(m => "    NAMEOF(" + m.DaxObjectName + "), " + m.DaxObjectName));

Model.AddCalculatedTable("Measure Selection", string.Format(calcTableDax, calcTableInner));
table.AddMeasure("Dynamic Measure", string.Format(switchMeasureDax, switchMeasureInner));
```

</details>

After running the script, confirm everything works by refreshing the model (use Power BI Desktop or SSMS), and create a Matrix (in Power BI Desktop) or Pivot Table (in Excel), which slices [Dynamic Measure] by the [Value] column of the 'Measure Selection' table. You should see something like the following (depending on which measures you included in your selection when the script was executed):

<img src="https://github.com/user-attachments/assets/78a02fd6-94e6-4a1a-a9d1-0a2469c4e30f" width="50%">

</details>
<details>
<summary><h3>Exercise 2.5 - Data-driven metadata generation</h3></summary>

Sometimes it is useful to have measures which apply filters in DAX, corresponding to members of a dimension table. For example, in the [SpaceParts](https://github.com/otykier/training/tree/main/Sample%20models) sample models, we have a table called `'Invoice Document Type'` containing one row for each type of invoice in the model. Currently, there are 5 different types.

Let's write a script which automatically generates one measure for each Invoice Document Type. 

The measure name should be `xxx Invoice Value` where `xxx` is the value from the [Text] column of the 'Invoice Document Type' table.

The measure expression should look like:

```dax
CALCULATE(
    [Total Net Invoice Value],
    'Invoice Document Type'[Code] = "yyy"
)
```

where `yyy` is the value from the [Code] column of the 'Invoice Document Type' table.

<details><summary>Click to view solution</summary>

**Note:** This script assumes the model contains a table named 'Invoice Document Type' which has a [Code] column and a [Text] column. You'll get a runtime error otherwise, since the DAX query that we execute to read the values from the table will not work.

We provide two solutions here, to illustrate the difference between the `ExecuteReader` (which returns an [IDataReader](https://learn.microsoft.com/en-us/dotnet/api/system.data.idatareader?view=net-8.0)) and the `EvaluateDax` (which returns a [DataTable](https://learn.microsoft.com/en-us/dotnet/api/system.data.datatable?view=net-8.0) in this case) approaches.

**Using ExecuteReader:**
```csharp
var name = "{0} Invoice Value";
var dax = @"CALCULATE(
    [Total Net Invoice Value],
    'Invoice Document Type'[Code] = ""{0}""
)";

var reader = ExecuteReader("EVALUATE SELECTCOLUMNS('Invoice Document Type', [Code], [Text])");

while(reader.Read())
{
    var code = reader.GetString(0);
    var text = reader.GetString(1);
 
    Model.Tables["Invoice Document Type"].AddMeasure(
        string.Format(name, text),
        string.Format(dax, code)
    );
}
```

**Using EvaluateDax:**
```csharp
using System.Data;

var name = "{0} Invoice Value";
var dax = @"CALCULATE(
    [Total Net Invoice Value],
    'Invoice Document Type'[Code] = ""{0}""
)";

var data = EvaluateDax("SELECTCOLUMNS('Invoice Document Type', [Code], [Text])") as DataTable;
// Uncomment the line below if you want to see the data returned from the query above:
// data.Output();

foreach(DataRow row in data.Rows)
{
    var code = row[0].ToString();
    var text = row[1].ToString();
 
    Model.Tables["Invoice Document Type"].AddMeasure(
        string.Format(name, text),
        string.Format(dax, code)
    );
}
```

</details>
</details>
<details>
<summary><h3>Exercise 2.6 - (Advanced) The Tokenizer</h3></summary>

Let's explore the [`Tokenize()`](https://docs.tabulareditor.com/api/TabularEditor.TOMWrapper.Utils.DaxDependencyHelper.html#TabularEditor_TOMWrapper_Utils_DaxDependencyHelper_Tokenize_TabularEditor_TOMWrapper_IDaxDependantObject_) method.

For manipulating DAX expressions, it is often easier to work with a list of tokens, rather than a string of characters. For example, if we wanted to detect all occurrences of `/` (the [division operator](https://dax.guide/op/division/)) in our DAX expressions, we could do a simple token search like so:

```csharp
var usesDivision = Model.AllMeasures.Where(m => m.Tokenize().Any(t => t.Type == DaxToken.DIV)).ToList();
// Output the list of measures that use division:
usesDivision.Output();
```

Without tokenization, this would be much harder to do, because we cannot easily distinguish between the character '/' being used as a division operator, or simply being part of an object name, a string or a comment. RegEx ain't got nothing on Tokenization.

View the [full list of DaxTokens available in the TE2 tokenizer](https://github.com/TabularEditor/TabularEditor/blob/master/TOMWrapper/Utils/DaxToken.Generated.cs).

Now for the exercise. Some guy with an italian accent, has told you that `SUM('Table'[Column])` is just syntax sugar for `SUMX('Table', 'Table'[Column])`. Health experts say that sugar is bad for you, so naturally you want to change all occurrences of `SUM` in your measures, to the equivalent `SUMX`.

**Write a script which will use the tokenizer to detect all occurrences of `DaxToken.SUM` in the DAX expressions on your measures.**

**Hint:** If we ignore whitespace and comments, and assume that the column reference is always qualified with the table name, the token sequence would always look something like this:

1. `DaxToken.SUM`
2. `DaxToken.OPEN_PARENS`
3. `DaxToken.TABLE` (when the table name is enclosed in single quotes `'`) or `DaxToken.TABLE_OR_VARIABLE` (when the table name is unquoted)
4. `DaxToken.COLUMN_OR_MEASURE`
5. `DaxToken.CLOSE_PARENS`

**Hint:** Use the `Output()` method to view the list of tokens produced by the `Tokenize()` method.

```csharp
Selected.Measure.Tokenize(includeHidden: false).Output();
```

<img src="https://github.com/user-attachments/assets/5de466a1-4b84-4478-8ff2-bebc42e339df" width="75%">

**Hint:** .NET does not have a built-in method for replacing a section of a string with another string, based on character positions. However, we can easily create our own utility method to do this. While we're at it, let's also add a similar method which takes a DaxToken as input, rather than the character index and length:

```csharp
class Util {
    public static string Replace(string original, int startIndex, int length, string replacement)
    {
        if (startIndex < 0 || startIndex >= original.Length)
            throw new ArgumentOutOfRangeException("startIndex");
        
        if (length < 0 || startIndex + length > original.Length)
            throw new ArgumentOutOfRangeException("length");

        return original.Substring(0, startIndex) + replacement + original.Substring(startIndex + length);
    }
    
    public static string Relace(string original, DaxToken token, string replacement)
    {
        return Replace(original, token.StartIndex, token.StopIndex - token.StartIndex + 1, replacement);
    }
}

string original = "Hello, World!";
string result = Util.Replace(original, 7, 5, "Universe");

// Outputs "Hello, Universe!";
Info(result);
```

**Hint:** Since a single DAX expression can contain multiple occurrences of `SUM`, it is important that we replace all of them. However, once we perform a replace, the DaxToken character indexes no longer match the actual positions in the DAX expression, since the replace operation may have shifted characters to the right of the insertion. So we'll either need to run the tokenizer again, or, better yet, perform the replace in reverse order, starting with the **last** occurrence of `SUM` within the string.

<details><summary>Click to view solution</summary>

Full solution can be found here:

</details>

</details>
