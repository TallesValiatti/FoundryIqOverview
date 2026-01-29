using Azure.AI.Projects.OpenAI;

namespace FoundryIqOverview.Console.Services;

public partial class MicrosoftFoundryService
{
    public async Task<ProjectConversation> CreateConversationAsync()
    {
        var projectClient = CreateAiProjectClient();
        
        ProjectConversation conversation = await projectClient.OpenAI.Conversations.CreateProjectConversationAsync();
        return conversation;
    }
};