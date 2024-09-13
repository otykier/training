// MASTER MODEL PATTERN
// ====================
// Select a "dev perspective" (i.e. perspective whose name starts with "$"), then
// removes all objects from the model, which are not included in that perspective.
//
// https://docs.tabulareditor.com/te2/Master-model-pattern.html
// Credit: Daniel Otykier

var version = Environment.GetEnvironmentVariable("TE_DevPerspective");
if(string.IsNullOrEmpty(version)) {
    var devPerspectives = Model.Perspectives.Where(p => p.Name.StartsWith("$"));
    var perspective = SelectObject(devPerspectives);
    if(perspective == null) return;
    version = perspective.Name;
}

// Remove tables, measures, columns and hierarchies that are not part of the perspective:
foreach(var t in Model.Tables.ToList()) {
    if(!t.InPerspective[version]) 
    {
        foreach(var dep in t.ReferencedBy.Deep().OfType<TabularNamedObject>().ToList()) dep.Delete();
        t.Delete();
    }
    else {
        foreach(var m in t.Measures.ToList()) if(!m.InPerspective[version])
        {
            foreach(var dep in t.ReferencedBy.Deep().OfType<TabularNamedObject>().ToList()) dep.Delete();
            m.Delete();
        }
        foreach(var c in t.Columns.ToList()) if(!c.InPerspective[version]) {
            foreach(var dep in c.ReferencedBy.Deep().OfType<TabularNamedObject>().ToList()) dep.Delete();
            c.Delete();
        }
        foreach(var h in t.Hierarchies.ToList()) if(!h.InPerspective[version]) h.Delete();
    }
}

// Remove user perspectives based on annotations and all developer perspectives:
foreach(var p in Model.Perspectives.ToList()) {
    if(p.Name.StartsWith("`$`")) p.Delete();

    // Keep all other perspectives that do not have the "DevPerspectives" annotation, while removing
    // those that have the annotation, if <version> is not specified in the annotation:
    if(p.GetAnnotation("DevPerspectives") != null && !p.GetAnnotation("DevPerspectives").Contains(version)) 
        p.Delete();
}

// Remove data sources based on annotations:
foreach(var ds in Model.DataSources.ToList()) {
    if(ds.GetAnnotation("DevPerspectives") == null) continue;
    if(!ds.GetAnnotation("DevPerspectives").Contains(version)) ds.Delete();
}

// Remove roles based on annotations:
foreach(var r in Model.Roles.ToList()) {
    if(r.GetAnnotation("DevPerspectives") == null) continue;
    if(!r.GetAnnotation("DevPerspectives").Contains(version)) r.Delete();
}

// Modify measures based on annotations:
foreach(Measure m in Model.AllMeasures) {
    var expr = m.GetAnnotation(version + "_Expression"); if(expr == null) continue;
    m.Expression = expr;
    m.FormatString = m.GetAnnotation(version + "_FormatString");
    m.Description = m.GetAnnotation(version + "_Description");    
}

// Set partition queries according to annotations:
foreach(Table t in Model.Tables) {
    var queryWithPlaceholders = t.GetAnnotation(version + "_PartitionQuery"); if(queryWithPlaceholders == null) continue;
    
    // Loop through all partitions in this table:
    foreach(Partition p in t.Partitions) {
        
        var finalQuery = queryWithPlaceholders;

        // Replace all placeholder values:
        foreach(var placeholder in p.Annotations.Keys) {
            finalQuery = finalQuery.Replace("%" + placeholder + "%", p.GetAnnotation(placeholder));
        }

        p.Query = finalQuery;
    }
}

// TODO: Modify other objects based on annotations, if applicable...
