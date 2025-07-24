// ADD TIME INTELLIGENCE
// =====================
// Select one or more measures, then run the script. The script
// will prompt you to select a "Date" column within your model.
// That column must reside on a table marked as a "Date Table".
// Next, you will be prompted to choose which time intelligence
// calculations to add to the model, and provide a name suffix.
//
// https://docs.tabulareditor.com/te2/Useful-script-snippets.html#generate-time-intelligence-measures
// Credit: Daniel Otykier

#r "System.Drawing"
using System.Windows.Forms;
using System.Drawing;

if(Selected.Measures.Count == 0) {
    Info("No measures selected");
    return;
}

// ===================== UI code below this line ===========================
Application.UseWaitCursor = false;

var aggForm = new Form
{
    Text = "Add Time Intelligence",
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

Action<string> AddLabel = s => {
    var label = new Label { Margin = new Padding(0,10,0,5), Text = s, AutoSize = true };
    flowPanel.Controls.Add(label);
};
var checks = new List<CheckBox>();
var suffixes = new List<TextBox>();
Action<string, string> AddCalc = (s, t) => {
    var panel = new Panel{ AutoSize = true };
    var checkBox = new CheckBox { Text = s, AutoSize = true, Location = new Point(0, 2), Checked = true };
    panel.Controls.Add(checkBox);
    checks.Add(checkBox);
    var textBox = new TextBox { Location = new Point(150,0), Width = 200, Text = t };
    suffixes.Add(textBox);
    panel.Controls.Add(textBox);
    flowPanel.Controls.Add(panel);
};

AddLabel("Choose Date column:");
var dateColumnSelector = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = true, Width = 300 };
var dateColumns = Model.AllColumns.Where(c => c.DataType == DataType.DateTime && c.IsKey).ToList();
if(dateColumns.Count == 0) {
    Info("Model contains no Date columns (i.e. a DateTime column on a table marked as a \"Date Table\"");
    return;
}
dateColumnSelector.Items.AddRange(dateColumns.Select(c => new ComboBoxItem(c)).ToArray());
dateColumnSelector.SelectedIndex = 0;
flowPanel.Controls.Add(dateColumnSelector);

AddLabel("Choose Time Intelligence calculations and suffixes:");
AddCalc("Agg. month-to-date", "MTD");
AddCalc("Agg. quarter-to-date", "QTD");
AddCalc("Agg. year-to-date", "YTD");
AddCalc("1 year prior", "1YP");
AddCalc("2 years prior", "2YP");
AddCalc("Comp. year-over-year", "YoY");
AddCalc("Comp. year-over-year %", "YoY %");

AddLabel("(Optional) Enter Display Folder:");
var displayFolderTextbox = new TextBox{ Width = 300 };
flowPanel.Controls.Add(displayFolderTextbox);

var buttonPanel = new Panel{Margin = new Padding(0,20,0,0), AutoSize = true};
var okButton = new Button { Text = "OK", AutoSize = true, DialogResult = DialogResult.OK, Location = new Point(195, 0) };
var cancelButton = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel, Location = new Point(280,0) };
buttonPanel.Controls.Add(okButton);
buttonPanel.Controls.Add(cancelButton);
flowPanel.Controls.Add(buttonPanel);
aggForm.Controls.Add(flowPanel);
aggForm.AcceptButton = okButton;
aggForm.CancelButton = cancelButton;

if(aggForm.ShowDialog() == DialogResult.Cancel) return;

var displayFolder = displayFolderTextbox.Text;
var dateColumn = (dateColumnSelector.SelectedItem as ComboBoxItem).Column.DaxObjectFullName;

// ==================================== Actual logic for adding measures below this line =================================

// Creates time intelligence measures for every selected measure:
foreach(var m in Selected.Measures) 
{

    // Month-to-date:
    if(checks[0].Checked)
        m.Table.AddMeasure(
            m.Name + " " + suffixes[0].Text,
            "TOTALMTD(" + m.DaxObjectName + ", " + dateColumn + ")",
            displayFolder
        );

    // Quarter-to-date:
    if(checks[1].Checked)
        m.Table.AddMeasure(
            m.Name + " " + suffixes[1].Text,
            "TOTALQTD(" + m.DaxObjectName + ", " + dateColumn + ")",
            displayFolder
        );

    // Year-to-date:
    if(checks[2].Checked)
        m.Table.AddMeasure(
            m.Name + " " + suffixes[2].Text,
            "TOTALYTD(" + m.DaxObjectName + ", " + dateColumn + ")",
            displayFolder
        );

    // 1 year prior:
    if(checks[3].Checked)
        m.Table.AddMeasure(
            m.Name + " " + suffixes[3].Text,
            string.Format("CALCULATE({0}, PARALLELPERIOD({1}, -1, YEAR))", m.DaxObjectName, dateColumn),
            displayFolder
        );

    // 2 years prior:
    if(checks[4].Checked)
        m.Table.AddMeasure(
            m.Name + " " + suffixes[4].Text,
            string.Format("CALCULATE({0}, PARALLELPERIOD({1}, -2, YEAR))", m.DaxObjectName, dateColumn),
            displayFolder
        );
        
    var pyMeasure = checks[3].Checked 
        ? "[" + m.Name + " " + suffixes[3].Text + "]"
        : string.Format("CALCULATE({0}, PARALLELPERIOD({1}, -1, YEAR))", m.DaxObjectName, dateColumn);

    // Year-over-year:
    if(checks[5].Checked)
        m.Table.AddMeasure(
            m.Name + " " + suffixes[5].Text,
            m.DaxObjectName + " - " + pyMeasure,
            displayFolder
        );

    // 2 years prior:
    if(checks[6].Checked)
        m.Table.AddMeasure(
            m.Name + " " + suffixes[6].Text,
            string.Format("DIVIDE({0} - {1}, {1})", m.DaxObjectName, pyMeasure),
            displayFolder
        ).FormatString = "0.0 %";
}

// ===================== UI code below this line ===========================

class ComboBoxItem
{
    public Column Column { get; private set; }
    public ComboBoxItem(Column column) { this.Column = column; }
    public override string ToString() { return Column.DaxObjectFullName; }
}