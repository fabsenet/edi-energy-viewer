using Raven.Client.Documents.Indexes;
using System.Collections.Generic;
using System.Linq;

namespace Fabsenet.EdiEnergy.Util
{
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
}