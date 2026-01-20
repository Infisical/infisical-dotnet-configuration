using System.Text.Json;

namespace InfisicalConfiguration;

public class MachineIdentityLogin
{
    public string AccessToken { get; set; }

    public static MachineIdentityLogin Deserialize(string content)
    {
        var result = JsonSerializer.Deserialize<MachineIdentityLogin>(content, InfisicalJsonOptions.Value);

        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize MachineIdentityLogin");
        }

        return result;
    }
}

public class SecretsList
{
    public record Secret(string Key, string Value);

    public required List<Secret> Secrets { get; set; }

    public static SecretsList Deserialize(Stream content)
    {
        var result = JsonSerializer.Deserialize<SecretsList>(content, InfisicalJsonOptions.Value);
        return result!;
    }
}