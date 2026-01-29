using FoundryIqOverview.Console.Services;
using FoundryIqOverview.Console.Services.Models;

#pragma warning disable OPENAI001

var agentName = "contoso-software-agent";
var projectUrl = Environment.GetEnvironmentVariable("PROJECT_ENDPOINT");
var input = """
            Explain 
            (1) the Azure Key Vault authentication scenario called “Application-plus-user”. 
            (2) Contoso’s SLA policy, including availability commitments, incident severity levels, and its operational details.
            """;
    
var service = new MicrosoftFoundryService(projectUrl!);

var agent = await service.GetAgentAsync(agentName);
var conversation = await service.CreateConversationAsync();

await service.RunAsync(
    agent.Name, 
    agent.Versions.Latest.Version,
    conversation,
    input,
    OnUpdate);
return;

Task OnUpdate(ResponseEvent arg)
{
    switch (arg)
    {
        case ResponseContentEvent update: 
            Console.Write(update.Message);
            break;
        case ResponseCompletedEvent completed:
            Console.WriteLine("\n*** Response completed ***");
            break;
        case ResponseMetadataEvent usage:
            Console.WriteLine("\n*** Usage: Input Tokens: {0}, Output Tokens: {1} ***", usage.InputTokenCount, usage.OutputTokenCount);
            
            foreach(var tool in usage.Tools)
            {
                Console.WriteLine("*** Used tool: {0} ***", tool);
            }
            
            break;
        case ResponseNonMappedEvent nonMapped:
            // Console.WriteLine("*** Non-mapped event received {0}***", nonMapped.EventType);
            break;
    }
    
    return Task.CompletedTask;
}