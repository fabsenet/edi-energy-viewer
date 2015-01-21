using System;
using JetBrains.Annotations;

namespace Fabsenet.EdiEnergy
{
    [UsedImplicitly]
    public class EdiDocument
    {
        public string Id { get; set; }
        public string DocumentName { get; set; }
        public string DocumentUri { get; set; }
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
        public bool IsLatestVersion { get; set; }
    }
}