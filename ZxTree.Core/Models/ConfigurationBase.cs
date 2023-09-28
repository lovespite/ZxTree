using System.IO;

using System.Text.Json;
using System.Text.Json.Serialization;
using ZxTree.Core.Utils;

namespace ZxTree.Core.Models;


/// <summary>
/// Provides a delegate to encrypt/decrypt the configuration file
/// </summary>
/// <param name="raw"></param>
/// <param name="direction">1: Encrytion, 0: Decryption</param>
/// <returns></returns>
public delegate string SecretProvider(string raw, int direction);

public class ConfigurationBase
{

    [JsonIgnore]
    public string SrcFile { get; private set; } = string.Empty;

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    private SecretProvider? _secretProvider = null;

    private string AsSecret(string raw)
    {
        if (_secretProvider is null) return raw;

        return _secretProvider.Invoke(raw, 1); // 1: Encryption
    }

    public static ConfigurationBase Open(Type configType, string srcFile, SecretProvider? sp)
    {
        var json = File.ReadAllText(srcFile);

        json = sp?.Invoke(json, 0) ?? json; // 0: Decryption

        var config = (ConfigurationBase)(JsonSerializer.Deserialize(json, configType, _options) ?? throw new Exception("Failed to deserialize configuration file"));

        config.SrcFile = srcFile;
        config._secretProvider = sp;

        return config;
    }

    public void Save(string? file = null)
    {
        var json = AsSecret(JsonSerializer.Serialize(this, _options));

        var targetFile = file ?? SrcFile;

        if (string.IsNullOrEmpty(targetFile)) throw new Exception("Target file is not specified");

        File.WriteAllText(targetFile, json);
    }
}