var type = "full";
var database = Environment.GetEnvironmentVariable("TE_DatabaseName");
var tmsl = "{ \"refresh\": { \"type\": \"%type%\", \"objects\": [ { \"database\": \"%db%\" } ] } }"
    .Replace("%type%", type)
    .Replace("%db%", database);
Info("Performing full refresh: " + database);
ExecuteCommand(tmsl);
Info("Refresh complete!");