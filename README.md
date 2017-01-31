### PostgreSQL Exporter

A simple library for exporting data that is compatible with PostgreSQL's `\COPY` command.

Here's a quick example:

```cs
using (var connection = new SqlConnection(ConnectionString))
{
    connection.Open();
    var command = connection.CreateCommand();
    command.CommandText = "SELECT * FROM [Cornholios]";
    using (var reader = command.ExecuteReader())
    {
        var exporter = new Exporter { ProgressInterval = 100000 };
        exporter.Progress += (sender, e) =>
        {
            Console.WriteLine($"Wrote {e.Count} records.");
        };
        exporter.Export(reader, Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Desktop\\Cornholios.tsv"));
    }
}
```
