using Newtonsoft.Json.Linq;

public class JsonMatcher
{
    public static (bool, Dictionary<string, string>) ValidateMappingWithApiResponse(string mapping, string generatedJson, string apiJson)
    {
        var issues = new Dictionary<string, string>();
        var mappingObj = JObject.Parse(mapping)["props"]; // Extract properties directly
        var generatedArray = JArray.Parse(generatedJson);
        var apiArray = JArray.Parse(apiJson);

        bool isValid = true;

        for (int i = 0; i < generatedArray.Count; i++)
        {
            var genItem = (JObject)generatedArray[i];
            var apiItem = (JObject)apiArray[i];

            ValidateObject(mappingObj, genItem, apiItem, "", issues, ref isValid);
        }

        return (isValid, issues);
    }

    private static void ValidateObject(JToken mapping, JObject generated, JObject api, string parentKey, Dictionary<string, string> issues, ref bool isValid)
    {
        foreach (var property in mapping.Children<JProperty>()) // Use JObject properties correctly
        {
            var key = property.Name;
            var rule = property.Value["rule"]?.ToString() ?? "exact";
            var type = property.Value["type"]?.ToString();

            var fullKey = string.IsNullOrEmpty(parentKey) ? key : $"{parentKey}.{key}";

            if (rule == "ignore") continue;

            if (!api.TryGetValue(key, out JToken apiValue))
            {
                issues[fullKey] = "Missing in API response";
                isValid = false;
                continue;
            }

            if (!generated.TryGetValue(key, out JToken genValue))
            {
                issues[fullKey] = "Missing in generated JSON";
                isValid = false;
                continue;
            }

            if (type == "object")
            {
                ValidateObject(property.Value["props"], (JObject)genValue, (JObject)apiValue, fullKey, issues, ref isValid);
                continue;
            }

            if (rule == "exact" && !JToken.DeepEquals(genValue, apiValue))
            {
                issues[fullKey] = $"Expected exact match but got {apiValue}";
                isValid = false;
            }
            else if (rule == "contains" && !apiValue.ToString().Contains(genValue.ToString()))
            {
                issues[fullKey] = $"Expected API value to contain {genValue}, but got {apiValue}";
                isValid = false;
            }
        }
    }
}
