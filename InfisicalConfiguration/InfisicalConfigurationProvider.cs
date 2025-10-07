using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace InfisicalConfiguration;

public class InfisicalConfigurationProvider : ConfigurationProvider
{
  private readonly HttpClient _httpClient;
  private Dictionary<string, string> _secretsCache = new();

  private string _accessToken;

  private readonly InfisicalConfig _config;


  public InfisicalConfigurationProvider(InfisicalConfig config)
  {
    _config = config;
    _httpClient = new HttpClient();

    _accessToken = Authenticate();

    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    _httpClient.BaseAddress = new Uri(_config.InfisicalUrl);
  }

  private string Authenticate()
  {

    var auth = _config.Auth;

    if (auth is null)
    {
      throw new InvalidOperationException("Auth details not provided");
    }

    var authMethod = auth.GetAuthMethod();
    if (authMethod == InfisicalAuthType.Universal)
    {
      return UniversalAuthLogin();
    }

    if (authMethod == InfisicalAuthType.AzureCustomProvider)
    {
      return AzureAuthLogin();
    }

    throw new InvalidOperationException("AuthType must be set. Are you missing a call to SetUniversalAuth?");
  }

  private string UniversalAuthLogin()
  {

    var auth = _config.Auth;

    if (auth is null)
    {
      throw new InvalidOperationException("Auth details not provided");
    }
    var universalAuth = auth.GetUniversalAuth();

    var body = new
    {
      clientId = universalAuth.ClientId,
      clientSecret = universalAuth.ClientSecret
    };

    var bodyJson = JsonSerializer.Serialize(body);
    var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

    var infisicalUrl = _config.InfisicalUrl;

    var url = $"{infisicalUrl}/api/v1/auth/universal-auth/login";

    var response = new HttpClient().PostAsync(url, content).GetAwaiter().GetResult();

    response.EnsureSuccessStatusCode();

    var machineIdentityLogin = MachineIdentityLogin.Deserialize(
      response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    );

    return machineIdentityLogin.AccessToken;
  }

  private string AzureAuthLogin()
  {
    var auth = _config.Auth;

    if (auth is null)
    {
      throw new InvalidOperationException("Auth details not provided");
    }
    var azureAuth = auth.GetAzureCustomProviderAuth();

    var body = new
    {
      identityId = azureAuth.IdentityId,
      jwt = azureAuth.TokenProvider().GetAwaiter().GetResult()
    };

    var bodyJson = JsonSerializer.Serialize(body);
    var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

    var response = new HttpClient().PostAsync(
      $"{_config.InfisicalUrl}/api/v1/auth/azure-auth/login",
      content
    ).GetAwaiter().GetResult();

    response.EnsureSuccessStatusCode();

    var machineIdentityLogin = MachineIdentityLogin.Deserialize(
      response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    );

    return machineIdentityLogin.AccessToken;
  }

  public override void Load()
  {
    var task = LoadAsync();
    task.GetAwaiter().GetResult();
    if (task.Exception is not null)
    {
      if (task.Exception.InnerException is not null)
      {
        throw task.Exception.InnerException;
      }

      throw task.Exception;
    }
  }

  private async Task LoadAsync()
  {
    try
    {

      var prefix = _config.Prefix ?? "";

      var url = $"{_config.InfisicalUrl}/api/v3/secrets/raw/?environment={_config.Environment}&workspaceId={_config.ProjectId}&secretPath={_config.SecretPath}&include_imports=true&expandSecretReferences={_config.ExpandSecretReferences}";

      var response = await _httpClient.GetAsync(url);
      var content = await response.Content.ReadAsStringAsync();
      response.EnsureSuccessStatusCode();
      var secrets = SecretsList.Deserialize(content);


      var allSecrets = secrets.Secrets.Select(
          secret => new KeyValuePair<string, string>(
              secret.SecretKey,
              secret.SecretValue
          )).ToList();

      allSecrets.Reverse();
      _secretsCache.Clear();

      foreach (var secret in allSecrets)
      {
        _secretsCache[secret.Key] = secret.Value;
      }

      foreach (var secret in _secretsCache)
      {
        var key = prefix + secret.Key.Replace("__", ":");
        Data.Add(key, secret.Value);
      }
    }
    catch
    {
      foreach (var secret in _secretsCache)
      {
        Data.Add(secret.Key, secret.Value);
      }

      throw;
    }
  }

}