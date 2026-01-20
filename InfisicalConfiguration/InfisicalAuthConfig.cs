namespace InfisicalConfiguration;

public class UniversalAuthCredentials(string clientId, string clientSecret)
{
  public string ClientId { get; set; } = clientId;
  public string ClientSecret { get; set; } = clientSecret;
}

public class AzureCustomProviderAuthCredentials
{
  public string IdentityId { get; private set; }
  public Func<Task<string>> TokenProvider { get; private set; }

  public AzureCustomProviderAuthCredentials(string identityId, Func<Task<string>> tokenProvider)
  {
    if (string.IsNullOrEmpty(identityId))
    {
      throw new ArgumentNullException(nameof(identityId));
    }

    IdentityId = identityId;
    TokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
  }
}

public enum InfisicalAuthType
{
  Universal,
  AzureCustomProvider,
}

public class InfisicalAuth
{
  private InfisicalAuthType AuthType { get; set; }
  private UniversalAuthCredentials? universalAuthCredentials;
  private AzureCustomProviderAuthCredentials? azureCustomProviderAuthCredentials;


  internal InfisicalAuth() { }

  public InfisicalAuthType GetAuthMethod()
  {
    return AuthType;
  }

  public UniversalAuthCredentials GetUniversalAuth()
  {
    if (universalAuthCredentials == null)
    {
      throw new InvalidOperationException("UniversalAuth must be set");
    }

    if (AuthType == InfisicalAuthType.Universal)
    {
      return universalAuthCredentials;
    }

    throw new InvalidOperationException("AuthType must be set. Are you missing a call to SetUniversalAuth?");
  }

  public AzureCustomProviderAuthCredentials GetAzureCustomProviderAuth()
  {
    if (azureCustomProviderAuthCredentials == null)
    {
      throw new InvalidOperationException("Azure auth must be set");
    }

    if (AuthType == InfisicalAuthType.AzureCustomProvider)
    {
      return azureCustomProviderAuthCredentials;
    }

    throw new InvalidOperationException("AuthType must be set. Are you missing a call to SetAzureAuth?");
  }

  internal void SetUniversalAuthCredentials(UniversalAuthCredentials credentials)
  {
    universalAuthCredentials = credentials;
    AuthType = InfisicalAuthType.Universal;
  }

  internal void SetAzureAuthCredentials(AzureCustomProviderAuthCredentials credentials)
  {
    azureCustomProviderAuthCredentials = credentials;
    AuthType = InfisicalAuthType.AzureCustomProvider;
  }
}

public class InfisicalAuthBuilder
{
  private readonly InfisicalAuth _auth;

  public InfisicalAuthBuilder()
  {
    _auth = new InfisicalAuth();
  }

  public InfisicalAuthBuilder SetUniversalAuth(string clientId, string clientSecret)
  {
    _auth.SetUniversalAuthCredentials(new UniversalAuthCredentials(clientId, clientSecret));
    return this;
  }

  public InfisicalAuthBuilder SetAzureAuth(string identityId, Func<Task<string>> tokenProvider)
  {
    if (string.IsNullOrEmpty(identityId))
    {
      throw new InvalidOperationException("IdentityId must be set");
    }

    if (tokenProvider == null)
    {
      throw new InvalidOperationException("TokenProvider must be set");
    }

    _auth.SetAzureAuthCredentials(new AzureCustomProviderAuthCredentials(identityId, tokenProvider));
    return this;
  }

  public InfisicalAuth Build()
  {
    switch (_auth.GetAuthMethod())
    {
      case InfisicalAuthType.Universal:
        var universalAuth = _auth.GetUniversalAuth();
        if (string.IsNullOrEmpty(universalAuth.ClientId) || string.IsNullOrEmpty(universalAuth.ClientSecret))
        {
          throw new InvalidOperationException("ClientId and ClientSecret must be set");
        }
        break;
      case InfisicalAuthType.AzureCustomProvider:
        var azureCustomProviderAuth = _auth.GetAzureCustomProviderAuth();
        if (string.IsNullOrEmpty(azureCustomProviderAuth.IdentityId))
        {
          throw new InvalidOperationException("IdentityId must be set");
        }
        if (azureCustomProviderAuth.TokenProvider == null)
        {
          throw new InvalidOperationException("TokenProvider must be set");
        }

        break;
      default:
        throw new InvalidOperationException("AuthType must be set. Are you missing a call to SetUniversalAuth or SetAzureAuth?");
    }

    return _auth;
  }
}