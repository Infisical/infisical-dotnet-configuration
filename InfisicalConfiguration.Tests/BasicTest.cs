using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Azure.Identity;
using Azure.Core;

namespace InfisicalConfiguration.Tests;

[TestClass]
public class SystemTests
{

  public TestContext TestContext { get; set; }


  [TestMethod]
  public void ShouldGetSecretsFromTestAccount()
  {


    var configuration = new ConfigurationBuilder()
    .AddInfisical(
        new InfisicalConfigBuilder()
            .SetProjectId("<project-id>")
            .SetEnvironment("dev")
            .SetSecretPath("/")
            .SetInfisicalUrl("http://localhost:8080")
            .SetAuth(
                new InfisicalAuthBuilder()
                    .SetAzureAuth("<identity-id>", async () =>
                    {
                      // From Azure package: Used to get credentials from Azure CLI
                      var cliCredential = new VisualStudioCredential();

                      // Get JWT token from Azure CLI
                      var token = await cliCredential.GetTokenAsync(
                          new TokenRequestContext(
                              ["https://management.azure.com/.default"]
                          ),
                          CancellationToken.None
                      );

                      // JWT token can be used to authenticate with Infisical

                      Console.WriteLine("Token: " + token.Token);

                      return token.Token;
                    })
                    .Build()
            )
            .Build()
    )
    .Build();


    foreach (var kvp in configuration.AsEnumerable())
    {
      TestContext.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
    }
  }
}