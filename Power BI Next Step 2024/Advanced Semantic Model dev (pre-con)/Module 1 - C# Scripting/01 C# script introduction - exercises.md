# C# script introduction - exercises

In all these exercises, you will use the C# scripting feature of Tabular Editor to solve a problem.

<details>
<summary><b>Tabular Editor 2 instructions</b></summary>

To write and run C# scripts in **Tabular Editor 2**, the general steps are the following:

1) Launch Tabular Editor 2
2) Open a model (**File > Open > From File...** or **File > Open > From DB...**)
3) Go to the **C# Script** tab
4) Enter the script code and hit F5 to run the script

<img src="https://github.com/user-attachments/assets/f527fb90-036a-4dbb-b085-49e7ecac3004" width="50%">

</details>
<details>
<summary><b>Tabular Editor 3 instructions</b></summary>

To write and run C# scripts in **Tabular Editor 2**, the general steps are the following:

1) Launch Tabular Editor 3
2) (Optional) Open a model (**File > Open > Model from File...** or **File > Open > Model from DB...**)
3) Create a new C# script tab (**File > New > New C# script**
4) Enter the script code and hit F5 to run the script

<img src="https://github.com/user-attachments/assets/8c1d02d1-f541-494f-b853-d8951fb87d22" width="50%">

**Note:** In Tabular Editor 3, scripts can be executed even when no model is loaded. However, you will not be able to access the `Model` or the `Selected` top-level objects, since no model metadata is present.

</details>

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
