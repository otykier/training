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

## Exercise 1.6 - (Advanced) Custom UI

Scripts can use [WinForms](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/?view=netdesktop-8.0) to create customized UI, allowing for more user-friendly interactions for the person running the script.

In this exercise, we will extend the **Create SUM measures from column** script from exercise 1.5 above, to allow the user to select which type of aggregation they want the measures to perform. That is, instead of the script being hardcoded to use the `SUM` DAX function, we will allow the user to select between any of the following DAX aggregation functions (which all take a column reference as the first and only parameter):

- [`SUM`](https://dax.guide/sum)
- [`MIN`](https://dax.guide/min)
- [`MAX`](https://dax.guide/max)
- [`AVERAGE`](https://dax.guide/average)
- [`COUNT`](https://dax.guide/count)
- [`DISTINCTCOUNT`](https://dax.guide/distinctcount)

For this exercise, we'll need to instantiate a [Form](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.form?view=windowsdesktop-8.0) and populate it with the following controls:

- A [Label](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.label?view=windowsdesktop-8.0) which will show instructions to the user
- A [ComboBox](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.combobox?view=windowsdesktop-8.0) from which the user can choose which type of aggregation they want
- A [Button](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.button?view=windowsdesktop-8.0) which the user can press to confirm the selection

All these things are known as **Controls** in the world of WinForms application development. They live in the `System.Windows.Forms` namespace, so we'll need to import that by adding a suitable `using` clause at the top of the script. We'll also need the `System.Drawing` namespace, but since this originates from a .NET assembly that is not normally referenced in C# scripts, we will have to manually add the assembly reference. To that end, we can use the `#r` directive at the very top of the script. Lastly, we'll toggle off `Application.UseWaitCursor`, since Tabular Editor scripts normally show a wait cursor while scripts are executing. However, since we're going to display UI elements that the user can interact with using the mouse, we don't want to show a wait cursor.

The first few lines of the script should look like this:

```csharp
#r "System.Drawing"
using System.Windows.Forms;
using System.Drawing;

Application.UseWaitCursor = false;
```

Next, we want to start creating the various UI controls. The first thing we'll need, is a the `Form`. There are many properties we can set on this class, but we'll just apply the basics to obtain a rectangular, non-sizable form, with the title "Select aggregation", no icon and no maximize / minimize buttons. We'll also use `AutoSize = true` and `AutoSizeMode = AutoSizeMode.GrowAndShrink`, so the Form will automatically resize itself to accomodate the controls we put in.

> [!TIP]
> If you have access to ChatGPT, Claude, or a similar LLM, you can often learn a lot by prompting it to help you create the C# WinForms UI controls you need. For example: "Could you create a C# script using WinForms, which creates a single from with a label, a dropdown combobox and an "OK" button which will close the form when pressed?"

```csharp
var aggForm = new Form
{ 
    Text = "Select aggregation",
    MaximizeBox = false, 
    MinimizeBox = false, 
    ShowIcon = false,
    AutoSize = true,
    AutoSizeMode = AutoSizeMode.GrowAndShrink,
    FormBorderStyle = FormBorderStyle.FixedDialog,
    StartPosition = FormStartPosition.CenterParent
};
```

To actually display the form, we use `aggForm.ShowDialog();`. This method shows the form as a [modal dialog](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.form.showdialog?view=windowsdesktop-8.0#system-windows-forms-form-showdialog), blocking further script execution until the form is closed by the user. The method returns information about why the form was closed (i.e. if the user clicked an Accept or Cancel button, or if they hit the Close button in the top-right corner).

Before we show the form, let's add the other controls as well. While we could perform the positioning of controls manually by specifying their `Size` and `Location`, we prefer to use a [FlowLayoutPanel](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.flowlayoutpanel?view=netframework-4.8) since this takes care of positioning the child controls. This panel also needs to be marked with `AutoSize = true` and `AutoSizeMode = AutoSizeMode.GrowAndShrink`, for the same reason as we applied these settings to the Form.

```csharp
var flowPanel = new FlowLayoutPanel
{
    Padding = new Padding(10),
    FlowDirection = FlowDirection.TopDown,
    WrapContents = false,
    AutoSize = true,
    AutoSizeMode = AutoSizeMode.GrowAndShrink
};
```

Then, we create the three controls (label, combobox, and button) and add them to the flowPanel:

```csharp
var label = new Label { Text = "Select aggregation method", AutoSize = true };
var comboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = true, };
comboBox.Items.AddRange(new object[] { "SUM", "MIN", "MAX", "AVERAGE", "COUNT", "DISTINCTCOUNT" } );
comboBox.SelectedIndex = 0;
var button = new Button { Text = "OK", AutoSize = true, DialogResult = DialogResult.OK };

flowPanel.Controls.AddRange(new Control[] { label, comboBox, button });
```

Lastly, we add the flowPanel to the form, and we also assign out button as the [AcceptButton](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.form.acceptbutton?view=netframework-4.8) of the form, which is good practice.

```csharp
aggForm.Controls.Add(flowPanel);
aggForm.AcceptButton = button;
```

Finally, we're ready to show the dialog and behold our masterpiece. We use a conditional because we want to terminate our script if the user hits the close button, rather than our "OK" button. If the user didn't cancel, we assign the DAX aggregation function chosen to the `aggFunction` variable (which we'll use later when creating the measures):

```csharp
if(aggForm.ShowDialog() == DialogResult.Cancel) return;
var aggFunction = comboBox.SelectedItem.ToString(); // SUM, MIN, MAX, AVERAGE, COUNT or DISTINCTCOUNT
```

![image](https://github.com/user-attachments/assets/637975ce-a7cc-4f73-944c-fbfa495afccf)

I leave it up to you, to add the script from exercise 1.5, which will actually add the measures to the model. Don't forget to update the part of the code that assigns the DAX expression of the measure, so it uses `aggFunction` rather than the hard-coded `SUM`.

<details>
  <summary>Click to view solution</summary>

The full script solution can be found here: [Exercise 1.6 - full solution.csx](Exercise%201.6%20-%20full%20solution.csx)

</details>

**Bonus:** Looking for even more challenges? If time permits, here are some suggestions for more functionality you can build into the script:

- Add a [TextBox](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.textbox?view=windowsdesktop-8.0) to `aggForm`, where users can provide a custom format string to apply, instead of hard-coding `"0.00"` in the script.
- Add another TextBox, where users can specify a `DisplayFolder` for the measure.
- Add a third TextBox, where users can provide a custom prefix to the measure name, rather than using the hard-coded `"Sum of " + ...`,
- Use the `SelectTable()` helper method, to let users choose which table the measures should be created in, rather than creating the measures on the same table as the selected columns.
