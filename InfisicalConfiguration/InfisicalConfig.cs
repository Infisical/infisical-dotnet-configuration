namespace InfisicalConfiguration;

public class InfisicalConfig
{
  // Required properties
  public string Environment { get; }
  public string ProjectId { get; }
  public InfisicalAuth Auth { get; }

  // Optional properties with defaults
  public string SecretPath { get; }
  public string InfisicalUrl { get; }
  public string Prefix { get; }
  public bool ExpandSecretReferences { get; set; }

  internal InfisicalConfig(
      string environment,
      string projectId,
      InfisicalAuth auth,
      string secretPath,
      string infisicalUrl,
      string prefix,
      bool expandSecretReferences
    )
  {
    Environment = environment;
    ProjectId = projectId;
    Auth = auth;
    SecretPath = secretPath;
    InfisicalUrl = infisicalUrl;
    Prefix = prefix;
    ExpandSecretReferences = expandSecretReferences;
  }
}

public class InfisicalConfigBuilder
{
  private string? _environment;
  private string? _projectId;
  private string? _prefix;
  private InfisicalAuth? _auth;
  private string _secretPath = "/";
  private string _infisicalUrl = "https://app.infisical.com";
  private bool _expandSecretReferences = true;

  public InfisicalConfigBuilder SetAuth(InfisicalAuth auth)
  {
    _auth = auth;
    return this;
  }

  public InfisicalConfigBuilder SetPrefix(string prefix)
  {
    _prefix = prefix;
    return this;
  }

  public InfisicalConfigBuilder SetEnvironment(string environment)
  {
    _environment = environment;
    return this;
  }

  public InfisicalConfigBuilder SetProjectId(string projectId)
  {
    _projectId = projectId;
    return this;
  }

  public InfisicalConfigBuilder SetSecretPath(string secretPath)
  {
    _secretPath = secretPath;
    return this;
  }

  public InfisicalConfigBuilder ShouldExpandSecretReferences(bool expandSecretReferences)
  {
    _expandSecretReferences = expandSecretReferences;
    return this;
  }

  public InfisicalConfigBuilder SetInfisicalUrl(string infisicalUrl)
  {
    if (infisicalUrl.EndsWith("/api"))
    {
      infisicalUrl = infisicalUrl[..^4];
    }
    _infisicalUrl = infisicalUrl;
    return this;
  }

  public InfisicalConfig Build()
  {
    ValidateRequiredFields();

    return new InfisicalConfig(
        environment: _environment!,
        projectId: _projectId!,
        auth: _auth!,
        secretPath: _secretPath,
        infisicalUrl: _infisicalUrl,
        prefix: _prefix ?? "",
        expandSecretReferences: _expandSecretReferences
    );
  }

  private void ValidateRequiredFields()
  {
    if (string.IsNullOrEmpty(_environment) || string.IsNullOrEmpty(_projectId))
    {
      throw new InvalidOperationException("Environment and ProjectId must be set");
    }

    if (_auth is null)
    {
      throw new InvalidOperationException("Auth must be set");
    }

    if (string.IsNullOrEmpty(_infisicalUrl))
    {
      throw new InvalidOperationException("InfisicalUrl must be set");
    }

    if (string.IsNullOrEmpty(_secretPath))
    {
      throw new InvalidOperationException("SecretPath must be set");
    }
  }
}