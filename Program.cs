using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

List<SqlRow> dbResults = new List<SqlRow>
{
    new SqlRow { ["Id"] = "1", ["FName"] = "Sam", ["LName"] = "Altman", ["Age"] = "12", ["IsAvailable"] = "true" },
    new SqlRow { ["Id"] = "2", ["FName"] = "Elon", ["LName"] = "Musk", ["Age"] = "13", ["IsAvailable"] = "false" }
};

string jsonMapping = @"
{
    ""type"": ""object"",
    ""props"": {
        ""Id"": { ""type"": ""number"", ""src"": ""$d.Id"", ""rule"": ""exact"" },
        ""Age"": { ""type"": ""number"", ""src"": ""$d.Age"", ""rule"": ""exact"" },
        ""IsAvailable"": { ""type"": ""boolean"", ""src"": ""$d.IsAvailable"", ""rule"": ""exact"" },
        ""FullName"": {
            ""type"": ""object"",
            ""props"": {
                ""FName"": { ""type"": ""string"", ""src"": ""$d.FName"", ""rule"": ""contains"" },
                ""LName"": { ""type"": ""string"", ""src"": ""$d.LName"", ""rule"": ""ignore"" }
            }
        }
    }
}";


string resultJson = JsonBuilder.GenerateDynamicJson(dbResults, jsonMapping).ToString(Formatting.Indented);

string apiResponseJson = @"[
  {
    ""Id"": 1,
    ""Age"": 12,
    ""IsAvailable"": true,
    ""FullName"": {
      ""FName"": ""Sam"",
      ""LName"": ""Altman""
    }
  },
  {
    ""Id"": 3,
    ""Age"": 13,
    ""IsAvailable"": false,
    ""FullName"": {
      ""FName"": ""Elon"",
      ""LName"": ""Musk""
    }
  }
]";


Console.WriteLine(resultJson);

var (isValid, issues) = JsonMatcher.ValidateMappingWithApiResponse(jsonMapping, resultJson.ToString(), apiResponseJson);


Console.WriteLine();

Console.WriteLine($"Validation Result: {(isValid ? "Passed" : "Failed")}");
foreach (var issue in issues)
{
    Console.WriteLine($"{issue.Key}: {issue.Value}");
}