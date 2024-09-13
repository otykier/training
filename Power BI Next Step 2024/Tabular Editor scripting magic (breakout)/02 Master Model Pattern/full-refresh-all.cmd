@echo off
set TE_DatabaseName=AdventureWorks Internet Operation
"c:\Program Files (x86)\Tabular Editor\TabularEditor.exe" "localhost" "%TE_DatabaseName%" -S full-refresh.csx
set TE_DatabaseName=AdventureWorks Inventory
"c:\Program Files (x86)\Tabular Editor\TabularEditor.exe" "localhost" "%TE_DatabaseName%" -S full-refresh.csx
set TE_DatabaseName=AdventureWorks Reseller Operation
"c:\Program Files (x86)\Tabular Editor\TabularEditor.exe" "localhost" "%TE_DatabaseName%" -S full-refresh.csx
