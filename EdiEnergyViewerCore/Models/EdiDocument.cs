using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Fabsenet.EdiEnergy
{
    public class EdiDocument
    {
        public string Id { get; set; }
        public string DocumentName { get; set; }
        public string DocumentUri { get; set; }
        public string MirrorUri { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsMig { get; set; }
        public bool IsAhb { get; set; }
        public string[] ContainedMessageTypes { get; set; }
        public bool IsGeneralDocument { get; set; }
        public string MessageTypeVersion { get; set; }
        public DateTime DocumentDate { get; set; }
        public string BdewProcess { get; set; }
        public string DocumentNameRaw { get; set; }
        public string Filename { get; set; }
        public bool IsLatestVersion { get; set; }

        public Dictionary<int, List<int>> CheckIdentifier { get; set; } 
    }

    public class EdiDocumentSlim
    {
        public string Id { get; set; }
        public string DocumentName { get; set; }
        public string DocumentUri { get; set; }
        public string MirrorUri { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsMig { get; set; }
        public bool IsAhb { get; set; }
        public string[] ContainedMessageTypes { get; set; }
        public bool IsGeneralDocument { get; set; }
        public string MessageTypeVersion { get; set; }
        public DateTime DocumentDate { get; set; }
        public string BdewProcess { get; set; }
        public bool IsLatestVersion { get; set; }
        public string Filename { get; set; }


        public List<int> CheckIdentifier { get; set; } 
    }
}