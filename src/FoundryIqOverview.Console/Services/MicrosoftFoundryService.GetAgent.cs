using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;

namespace FoundryIqOverview.Console.Services;

public partial class MicrosoftFoundryService
{
    public async Task<AgentRecord> GetAgentAsync(string agentName)
    {
        var projectClient = CreateAiProjectClient();
        var agent = await projectClient.Agents.GetAgentAsync(agentName);
        return agent == null ? throw new Exception("Not possible to retrieve the agent") : agent.Value;
    }
};