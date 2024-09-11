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
