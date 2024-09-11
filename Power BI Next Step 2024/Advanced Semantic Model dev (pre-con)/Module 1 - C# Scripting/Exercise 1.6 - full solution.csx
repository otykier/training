#r "System.Drawing"
using System.Windows.Forms;
using System.Drawing;

if(Selected.Columns.Count == 0) {
    Info("No columns selected");
    return;
}

// --------------------------------- UI code begin ---------------------------------
Application.UseWaitCursor = false;

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

var flowPanel = new FlowLayoutPanel
{
    Padding = new Padding(10),
    FlowDirection = FlowDirection.TopDown,
    WrapContents = false,
    AutoSize = true,
    AutoSizeMode = AutoSizeMode.GrowAndShrink
};

var label = new Label { Text = "Select aggregation method", AutoSize = true };
var comboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = true, };
comboBox.Items.AddRange(new object[] { "Sum", "Min", "Max", "Average", "Count", "DistinctCount" } );
comboBox.SelectedIndex = 0;

var button = new Button { Text = "OK", AutoSize = true, DialogResult = DialogResult.OK };

flowPanel.Controls.AddRange(new Control[] { label, comboBox, button });

aggForm.Controls.Add(flowPanel);
aggForm.AcceptButton = button;

if(aggForm.ShowDialog() == DialogResult.Cancel) return;
var aggFunction = comboBox.SelectedItem.ToString(); // SUM, MIN, MAX, AVERAGE, COUNT or DISTINCTCOUNT

Application.UseWaitCursor = true;
// --------------------------------- UI code end ---------------------------------

// Add a measure with the specified aggregation for every selected numeric column:
foreach(var c in Selected.Columns)
{
    if(c.DataType != DataType.Int64 && c.DataType != DataType.Decimal && c.DataType != DataType.Double) continue;
    
    var newMeasure = c.Table.AddMeasure(
        aggFunction + " of " + c.Name,                    // Name
        aggFunction.ToUpper() + "(" + c.DaxObjectFullName + ")",    // DAX expression
        c.DisplayFolder                        // Display Folder
    );
    
    // Set the format string on the new measure:
    newMeasure.FormatString = "0.00";

    // Provide some documentation:
    newMeasure.Description = "This measure is the " + aggFunction + " of column " + c.DaxObjectFullName;

    // Hide the base column:
    c.IsHidden = true;
}