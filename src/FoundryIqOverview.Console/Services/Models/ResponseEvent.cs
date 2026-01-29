namespace FoundryIqOverview.Console.Services.Models;

public abstract record ResponseEvent;

public record ResponseContentEvent(string Id, string Message) : ResponseEvent;


public record ResponseCompletedEvent() : ResponseEvent;

public record ResponseNonMappedEvent(string EventType) : ResponseEvent;

public record ResponseMetadataEvent(
    int InputTokenCount,
    int OutputTokenCount,
    IEnumerable<string> Tools) : ResponseEvent;