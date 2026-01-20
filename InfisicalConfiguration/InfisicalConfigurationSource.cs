using Microsoft.Extensions.Configuration;

namespace InfisicalConfiguration;

public class InfisicalConfigurationSource(InfisicalConfig config) : IConfigurationSource
{
  public IConfigurationProvider Build(IConfigurationBuilder builder)
  {
    return new InfisicalConfigurationProvider(config);
  }
}