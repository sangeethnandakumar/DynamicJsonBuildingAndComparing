public class SqlRow
{
    private readonly Dictionary<string, string> _columns = new();

    public string this[string columnName]
    {
        get
        {
            if (!_columns.ContainsKey(columnName))
                throw new KeyNotFoundException($"Column '{columnName}' not found.");
            return _columns[columnName];
        }
        set => _columns[columnName] = value;
    }

    public bool ContainsKey(string columnName) => _columns.ContainsKey(columnName);

    public void Add(string columnName, string value) => _columns[columnName] = value;
}
