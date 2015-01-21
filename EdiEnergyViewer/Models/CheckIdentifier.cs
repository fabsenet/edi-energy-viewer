using JetBrains.Annotations;

namespace Fabsenet.EdiEnergy
{
    [UsedImplicitly]
    public class CheckIdentifier
    {
        public string Id { get; set; }
        public string MessageType { get; set; }
        public int Identifier { get; set; }
        public string BdewProcess { get; set; }
    }
}