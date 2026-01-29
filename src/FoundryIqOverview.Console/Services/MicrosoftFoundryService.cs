using Azure.AI.Projects;
using Azure.Identity;

namespace FoundryIqOverview.Console.Services;

public partial class MicrosoftFoundryService(string projectEndpoint)
{
    private AIProjectClient CreateAiProjectClient()
    {
        return new(new Uri(projectEndpoint), new AzureCliCredential());
    }
}