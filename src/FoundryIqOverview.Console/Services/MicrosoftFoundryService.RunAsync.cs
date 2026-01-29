using System.Diagnostics.CodeAnalysis;
using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using FoundryIqOverview.Console.Services.Models;
using OpenAI.Responses;

namespace FoundryIqOverview.Console.Services;

public partial class MicrosoftFoundryService
{
    [Experimental("OPENAI001")]
    public async Task RunAsync(
        string agentName, 
        string agentVersion, 
        string conversationId,
        string input,
        Func<ResponseEvent, Task> onUpdate)
    {
        var projectClient = CreateAiProjectClient();
        
        AgentReference agentReference = new(name: agentName, version: agentVersion);
        
        ProjectResponsesClient responseClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(
            agentReference,
            defaultConversationId: conversationId);
        
        var updates = responseClient.CreateResponseStreamingAsync(input);
        
        var responseId = string.Empty;
        await foreach (var update in updates)
        {
            switch (update)
            {
                
                case StreamingResponseOutputTextDeltaUpdate deltaUpdate: 
                    await onUpdate(new ResponseContentEvent(deltaUpdate.ItemId, deltaUpdate.Delta));
                    break;
                case StreamingResponseCompletedUpdate completedUpdate:
                    responseId = completedUpdate.Response.Id;
                    await onUpdate(new ResponseCompletedEvent());
                    break;
                default:
                    await onUpdate(new ResponseNonMappedEvent(update.GetType().ToString()));
                    break;
            }
        }

        var result = await responseClient.GetResponseAsync(
            new GetResponseOptions(responseId));

        await onUpdate(new ResponseMetadataEvent(
            result.Value.Usage.InputTokenCount,
            result.Value.Usage.OutputTokenCount,
            result.Value.Tools.Select(x => x.GetType().ToString())));
    }
}

