using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

public class JsonBuilder
{
    public static JToken GenerateDynamicJson(List<SqlRow> rows, string mapping)
    {
        if (rows == null || rows.Count == 0)
            throw new ArgumentException("Database result set cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(mapping))
            throw new ArgumentException("Mapping cannot be null or empty.");

        JToken jsonMapping;
        try
        {
            jsonMapping = JToken.Parse(mapping);
        }
        catch (JsonReaderException ex)
        {
            throw new ArgumentException("Invalid JSON mapping format.", ex);
        }

        if (jsonMapping["type"]?.ToString() != "object" || jsonMapping["props"] == null)
            throw new ArgumentException("Invalid mapping format. Expected top-level 'type' as 'object' and a 'props' section.");

        JToken result = rows.Count == 1
            ? (JToken)ProcessJsonObject(rows[0], jsonMapping["props"])
            : new JArray(rows.Select(row => ProcessJsonObject(row, jsonMapping["props"])));

        return result;
    }

    private static JObject ProcessJsonObject(SqlRow row, JToken mappingProps)
    {
        var jsonObject = new JObject();

        foreach (var prop in mappingProps.Children<JProperty>())
        {
            string key = prop.Name;
            JToken propConfig = prop.Value;

            string type = propConfig["type"]?.ToString();
            string src = propConfig["src"]?.ToString();

            if (type == "object" && propConfig["props"] != null)
            {
                jsonObject[key] = ProcessJsonObject(row, propConfig["props"]);
            }
            else if (src != null)
            {
                jsonObject[key] = ResolveValue(row, src, type);
            }
            else
            {
                throw new ArgumentException($"Invalid mapping for '{key}'. Missing 'src' for non-object type.");
            }
        }

        return jsonObject;
    }

    private static JToken ResolveValue(SqlRow row, string src, string type)
    {
        if (src.StartsWith("$d."))
        {
            string columnName = src.Substring(3); // Remove "$d."
            if (!row.ContainsKey(columnName))
                throw new KeyNotFoundException($"Column '{columnName}' not found in database row.");

            string rawValue = row[columnName];

            if (string.IsNullOrEmpty(rawValue))
                return JValue.CreateNull();

            return type switch
            {
                "number" => int.TryParse(rawValue, out int intVal) ? intVal : int.Parse(rawValue, CultureInfo.InvariantCulture),
                "boolean" => bool.TryParse(rawValue, out bool boolVal) ? boolVal : throw new FormatException($"Invalid boolean value in column '{columnName}'"),
                "string" => rawValue,
                _ => throw new NotSupportedException($"Unsupported type '{type}' in mapping.")
            };
        }

        return JToken.FromObject(src); // Use static value if it's not a database reference
    }
}
