using System.Text;
using Spectre.Console;
using FoundryIqOverview.Console.Services;
using FoundryIqOverview.Console.Services.Models;
using Spectre.Console.Rendering;

#pragma warning disable OPENAI001

var agentName = "contoso-software-agent";
var projectUrl = "https://tallesdsv8772-resource.services.ai.azure.com/api/projects/tallesdsv8772";

AnsiConsole.Clear();

var prompt = """
             Explain (1) the Azure Key Vault authentication scenario called “Application-plus-user” (what it means, how the auth flow works, and what permissions the application identity and the user each need), and (2) Contoso’s SLA policy, including availability commitments, incident severity levels, and its operational details.
             """;

AnsiConsole.Write(
    new FigletText("Contoso Agent")
        .Centered()
        .Color(Color.CornflowerBlue));

AnsiConsole.MarkupLine(
    $"[yellow]Prompt:[/] [cyan]{prompt} [/]\n"); 

if (string.IsNullOrWhiteSpace(projectUrl))
{
    AnsiConsole.MarkupLine("[red]PROJECT_ENDPOINT environment variable not set.[/]");
    return;
}

var service = new MicrosoftFoundryService(projectUrl);
var agent = await service.GetAgentAsync(agentName);
var conversation = await service.CreateConversationAsync();

var buffer = new StringBuilder();

// Initial renderable (recommended by the official tutorial)
IRenderable BuildPanel(string content) =>
    new Panel(new Markup(content))
    {
        Header = new PanelHeader("[bold green]Agent Response[/]"),
        Border = BoxBorder.Rounded,
        Padding = new Padding(1, 1)
    };

await AnsiConsole.Live(BuildPanel("[grey]Waiting for agent response...[/]"))
    .AutoClear(true)
    .Overflow(VerticalOverflow.Visible)
    .StartAsync(async ctx =>
    {
        await service.RunAsync(
            agent.Name,
            agent.Versions.Latest.Version,
            conversation,
            prompt,
            async ev =>
            {
                switch (ev)
                {
                    case ResponseContentEvent update:
                        buffer.Append(Markup.Escape(update.Message));
                        ctx.UpdateTarget(BuildPanel(buffer.ToString()));
                        break;

                    case ResponseCompletedEvent:
                        buffer.Append("\n\n[bold green]✔ Response completed[/]");
                        ctx.UpdateTarget(BuildPanel(buffer.ToString()));
                        break;

                    case ResponseMetadataEvent usage:
                        buffer.Append($"""
                            
                            [grey]────────────────────────────[/]
                            [bold yellow]Usage[/]
                            • Input Tokens: [cyan]{usage.InputTokenCount}[/]
                            • Output Tokens: [cyan]{usage.OutputTokenCount}[/]
                            
                            """);

                        foreach (var tool in usage.Tools)
                        {
                            buffer.Append($"• Tool used: [magenta]{tool}[/]\n");
                        }

                        ctx.UpdateTarget(BuildPanel(buffer.ToString()));
                        break;
                }

                await Task.CompletedTask;
            });
    });
AnsiConsole.Write(BuildPanel(buffer.ToString()));
