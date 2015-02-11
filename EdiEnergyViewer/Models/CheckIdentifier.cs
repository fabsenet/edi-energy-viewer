using System.Collections.Generic;
using JetBrains.Annotations;

namespace Fabsenet.EdiEnergy
{
    [UsedImplicitly]
    public class CheckIdentifier
    {
        public int Identifier { get; set; }
        public List<string> EdiDocIds { get; set; }
    }
}