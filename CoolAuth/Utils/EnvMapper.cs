using System.Collections;
using System.Collections.Frozen;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualBasic;

namespace CoolAuth.Utils;

public partial class EnvMapper
{
    private readonly IConfiguration _config;
    private readonly ILogger<EnvMapper> _logger;
    private bool _includeSystemVariables = true;
    public FrozenDictionary<string, string> Variables { get; private set; } = new Dictionary<string,string>().ToFrozenDictionary();
    
    [GeneratedRegex(@"\$\{(\w+)\}")]
    private static partial Regex PlaceholderRegex();

    public EnvMapper(IConfiguration config, ILogger<EnvMapper> logger)
    {
        _config = config;
        _logger = logger;
        
        ChangeToken.OnChange(
            () => _config.GetReloadToken(),
            MapVariables
        );
    }
    
    public EnvMapper WithSystemVariables(bool includeSystemVariables)
    {
        _includeSystemVariables = includeSystemVariables;
        return this;
    }
    
    public void MapVariables()
    {
        #if DEBUG
        var path = GetEnvironmentPath(".env.development.local", ".env.local");
        #else
        var path = GetEnvironmentPath(".env.production.local", ".env.local");
        #endif
        
        path ??= GetEnvironmentPath(".env");
        if (path == null)
        {
            _logger.LogWarning("No environment file found. Skipping variable mapping");
            return;
        }

        var systemVars = _includeSystemVariables ? 
            Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString() ?? string.Empty,
                              kvp => kvp.Value?.ToString() ?? string.Empty) :
            null;
        
        var dict = new Dictionary<string, string>(
            systemVars ?? new Dictionary<string, string>(),
            StringComparer.OrdinalIgnoreCase
        );
        
        var text = File.ReadAllText(path);
        var lines = text
            .Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line) && 
                           !line.TrimStart().StartsWith('#')
            );
        
        foreach (var line in lines)
        {
            var keyValueIndex = line.IndexOf('=');
            if (keyValueIndex == -1)
                continue;
                
            var key = line[..keyValueIndex].Trim();
            var value = line[(keyValueIndex + 1)..].Trim();
                
            if (value.StartsWith('"') && value.EndsWith('"') && value.Length > 1)
            {
                value = value[1..^1];
            }
            else if (value.StartsWith('\'') && value.EndsWith('\'') && value.Length > 1)
            {
                value = value[1..^1];
            }
                
            dict[key] = value;
        }
        
        Variables = dict.ToFrozenDictionary();
        
        // Inject variables into the configuration
        foreach (var keyValue in _config.AsEnumerable())
        {
            if(keyValue.Value == null)
                continue;
            
            PlaceholderRegex().Replace(keyValue.Value, match =>
            {
                if (Variables.TryGetValue(match.Groups[1].Value, out var value))
                {
                    _config[keyValue.Key] = value;
                }
                    
                return value!;
            });
        }
        _logger.LogInformation("Environment variables mapped from {Path}", path);
    }
    
    private static string? GetEnvironmentPath(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                return path;
            }
#if DEBUG
            if (File.Exists(Path.Combine("../../../", path)))
            {
                return Path.Combine("../../../", path);
            }
#endif
        }

        return null;
    }
}