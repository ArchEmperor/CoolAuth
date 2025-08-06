using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CoolAuth;

public static partial class Constants
{
    [GeneratedRegex(@"\$\{(\w+)\}")]
    public static partial Regex ConfigEnvPlaceholderRegex();

    public static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}