using Raven.Client.Documents.Transformers;
using System.Collections.Generic;
using System.Linq;

namespace Fabsenet.EdiEnergy.Util
{
    public class EdiDocumentsSlimTransformer : AbstractTransformerCreationTask<EdiDocument>
    {
        public EdiDocumentsSlimTransformer()
        {
            TransformResults = ediDocs => from ediDoc in ediDocs
                select new EdiDocumentSlim()
                {
                    BdewProcess = ediDoc.BdewProcess,
                    CheckIdentifier = ediDoc.CheckIdentifier.Select(kvp => kvp.Key).OrderBy(id => id).ToList(),
                    ContainedMessageTypes = ediDoc.ContainedMessageTypes,
                    DocumentDate = ediDoc.DocumentDate,
                    DocumentName = ediDoc.DocumentName,
                    DocumentUri = ediDoc.DocumentUri,
                    MirrorUri = ediDoc.MirrorUri,
                    Id = ediDoc.Id,
                    IsAhb = ediDoc.IsAhb,
                    IsGeneralDocument = ediDoc.IsGeneralDocument,
                    IsLatestVersion = ediDoc.IsLatestVersion,
                    IsMig = ediDoc.IsMig,
                    MessageTypeVersion = ediDoc.MessageTypeVersion,
                    ValidFrom = ediDoc.ValidFrom,
                    ValidTo = ediDoc.ValidTo,
                };
        }
    }
}