using Fabsenet.EdiEnergyViewer.Util;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

namespace Fabsenet.EdiEnergyViewer.Controllers;


[Route("api/[controller]")]
[ApiController]
public class FilterDataController(IDocumentStore store)
{
    [HttpGet]
    public async Task<FilterData> GetFilterData()
    {
        using var session = store.OpenAsyncSession();

        var availableMessageTypes = await session.Query<EdiDocuments_ContainedMessageTypes.Result, EdiDocuments_ContainedMessageTypes>()
            .OrderBy(mt => mt.MessageType)
            .Select(mt => mt.MessageType)
            .ToListAsync();

        if (availableMessageTypes is []) availableMessageTypes = ["KEINE DOKUMENTE VORHANDEN"];

        return new FilterData
        {
            AvailableMessageTypes = availableMessageTypes
        };
    }
}

public record FilterData
{
    public required List<string> AvailableMessageTypes { get; init; }
}