using System;

// ReSharper disable once CheckNamespace
namespace Fabsenet.EdiEnergy
{
    public class EdiDocument
    {
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
        public object BdewProcess { get; set; }
        public string DocumentNameRaw { get; set; }
    }

}