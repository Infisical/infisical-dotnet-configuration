using System.Text.Json;

namespace InfisicalConfiguration;

internal static class InfisicalJsonOptions
{
    public static readonly JsonSerializerOptions Value = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}