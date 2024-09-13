@echo off
set TE_DevPerspective=$Internet Operation
"c:\Program Files (x86)\Tabular Editor\TabularEditor.exe" "TMDL Source" -S "master-model-pattern.csx" -D "localhost" "AdventureWorks Internet Operation" -O
set TE_DevPerspective=$Inventory
"c:\Program Files (x86)\Tabular Editor\TabularEditor.exe" "TMDL Source" -S "master-model-pattern.csx" -D "localhost" "AdventureWorks Inventory" -O
set TE_DevPerspective=$Reseller Operation
"c:\Program Files (x86)\Tabular Editor\TabularEditor.exe" "TMDL Source" -S "master-model-pattern.csx" -D "localhost" "AdventureWorks Reseller Operation" -O
