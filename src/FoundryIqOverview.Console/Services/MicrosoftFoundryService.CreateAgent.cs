using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;

namespace FoundryIqOverview.Console.Services;

public partial class MicrosoftFoundryService
{
    public async Task<AgentRecord> CreateAgentAsync(string agentName, string instructions, string model)
    {
        var projectClient = CreateAiProjectClient();

        AgentDefinition agentDefinition = new PromptAgentDefinition(model)
        {
            Instructions = instructions,
        };

        await projectClient.Agents.CreateAgentVersionAsync(
            agentName,
            options: new AgentVersionCreationOptions(agentDefinition));

        return await GetAgentAsync(agentName);
    }
}