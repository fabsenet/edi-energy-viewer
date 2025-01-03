using Fabsenet.EdiEnergyViewer.Models;
using Raven.Client.Documents.Indexes;

namespace Fabsenet.EdiEnergyViewer.Util;

public class EdiDocuments_CheckIdentifiers : AbstractIndexCreationTask<EdiDocument, CheckIdentifier>
{
    public EdiDocuments_CheckIdentifiers()
    {
        Map = ediDocs => from ediDoc in ediDocs
                         from checkId in ediDoc.CheckIdentifier
                         select new CheckIdentifier()
                         {
                             Identifier = checkId.Key,
                             EdiDocIds = new List<string>() { ediDoc.Id.Substring("EdiDocuments/".Length) }
                         };

        Reduce = results => from result in results
                            group result by result.Identifier into g
                            select new CheckIdentifier()
                            {
                                Identifier = g.Key,
                                EdiDocIds = g.Select(h => h.EdiDocIds).Aggregate((ci1, ci2) => ci1.Union(ci2).ToList())
                            };

        StoreAllFields(FieldStorage.Yes);
    }
}


public class EdiDocuments_ContainedMessageTypes : AbstractIndexCreationTask<EdiDocument, EdiDocuments_ContainedMessageTypes.Result>
{
    public record Result
    {
        public required string MessageType { get; init; }
    }

    public EdiDocuments_ContainedMessageTypes()
    {
        Map = ediDocs => from ediDoc in ediDocs
                         where ediDoc.ContainedMessageTypes != null
                         from messageType in ediDoc.ContainedMessageTypes
                         select new Result()
                         {
                             MessageType = messageType
                         };

        Reduce = results => from result in results
                            group result by result.MessageType into g
                            select new Result()
                            {
                                MessageType = g.Key
                            };
    }
}