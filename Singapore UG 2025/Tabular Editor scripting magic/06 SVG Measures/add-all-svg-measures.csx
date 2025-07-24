if(Selected.Tables.Count == 0) {
    Info("Select the table where the SVG measures will be created.");
    return;   
}
var _Actual = SelectMeasure(Model.AllMeasures, null,"Select the measure that you want to measure:");
if(_Actual == null) return;
var _Target = SelectMeasure(Model.AllMeasures, null,"Select the measure that you want to compare to\n(For conditional formatting):");
if(_Target == null) return;
var _GroupBy = SelectColumn(Model.AllColumns, null, "Select the column for which you will group the data in\nthe table or matrix visual:");
if(_GroupBy == null) return;
try {
    Model.SetAnnotation("_svg_actual", _Actual.DaxObjectName);
    Model.SetAnnotation("_svg_target", _Target.DaxObjectName);
    Model.SetAnnotation("_svg_column", _GroupBy.DaxObjectFullName);

    CustomAction(@"Add SVG measure\Bar Chart\Adjacent Bars With Variance");
    CustomAction(@"Add SVG measure\Bar Chart\Diverging");
    CustomAction(@"Add SVG measure\Bar Chart\Overlapping Bars With Variance");

    CustomAction(@"Add SVG measure\Bar Chart\Rounded Corners\Conditional Formatting");
    CustomAction(@"Add SVG measure\Bar Chart\Rounded Corners\Labelled");
    CustomAction(@"Add SVG measure\Bar Chart\Rounded Corners\Standard");

    CustomAction(@"Add SVG measure\Bar Chart\Rounded Tops\Conditional Formatting");
    CustomAction(@"Add SVG measure\Bar Chart\Rounded Tops\Labelled");
    CustomAction(@"Add SVG measure\Bar Chart\Rounded Tops\Standard");

    CustomAction(@"Add SVG measure\Bar Chart\Standard\Conditional Formatting");
    CustomAction(@"Add SVG measure\Bar Chart\Standard\Labelled");
    CustomAction(@"Add SVG measure\Bar Chart\Standard\Standard");

    CustomAction(@"Add SVG measure\Bullet Chart\Action Dots");
    CustomAction(@"Add SVG measure\Bullet Chart\Conditional Bar");
    CustomAction(@"Add SVG measure\Bullet Chart\Label And Qualitative Ranges");
    CustomAction(@"Add SVG measure\Bullet Chart\Label");
    CustomAction(@"Add SVG measure\Bullet Chart\Qualitative Ranges");

    CustomAction(@"Add SVG measure\Dumbbell Plot");
    CustomAction(@"Add SVG measure\Lollipop Chart\Conditional Formatting");
    CustomAction(@"Add SVG measure\Lollipop Chart\With Label");
    CustomAction(@"Add SVG measure\Waterfall Chart");
    Info("Finished adding 21 SVG measures");
}
finally {
    Model.RemoveAnnotation("_svg_actual");
    Model.RemoveAnnotation("_svg_target");
    Model.RemoveAnnotation("_svg_column");    
}