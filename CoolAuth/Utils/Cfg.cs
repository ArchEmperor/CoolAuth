using System.Collections;
using Microsoft.VisualBasic;

namespace CoolAuth.Utils;

public static class Cfg
{
     private static string CheckEnvPaths(params string[] paths)
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
        return string.Empty;
    }


    public static void MapEnvToConfig(ConfigurationManager configuration)
    {
        var envDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        LoadEnvFromFile(envDict);
        
        LoadEnvFromEnvironment(envDict);
        
        ApplyEnvToConfiguration(configuration, envDict);
    }

    private static void LoadEnvFromFile(Dictionary<string, string> envDict)
    {
        var path = CheckEnvPaths(".env.development.local", ".env.local", ".env");
        
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        
        try
        {
            var text = File.ReadAllText(path);
            var lines = text
                .Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith('#'));
            
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
                
                envDict[key] = value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not read .env file at {path}: {ex.Message}");
        }
    }

    private static void LoadEnvFromEnvironment(Dictionary<string, string> envDict)
    {
        var environmentVariables = Environment.GetEnvironmentVariables();
        
        foreach (DictionaryEntry envVar in environmentVariables)
        {
            var key = envVar.Key?.ToString();
            var value = envVar.Value?.ToString();
            
            if (!string.IsNullOrEmpty(key) && value != null)
            {
                envDict[key] = value;
            }
        }
    }

    private static void ApplyEnvToConfiguration(ConfigurationManager configuration, Dictionary<string, string> envDict)
    {
        foreach (var keyValue in configuration.AsEnumerable())
        {
            if (keyValue.Value == null)
                continue;
            
            var newValue = Constants.ConfigEnvPlaceholderRegex().Replace(keyValue.Value, match =>
            {
                var placeholderKey = match.Groups[1].Value;
                
                if (envDict.TryGetValue(placeholderKey, out var envValue))
                {
                    return envValue;
                }
                
                return match.Value;
            });
            
            if (newValue != keyValue.Value)
            {
                configuration[keyValue.Key] = newValue;
            }
        }
    }
}