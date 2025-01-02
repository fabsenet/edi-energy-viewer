using System.Collections.Generic;
using JetBrains.Annotations;

namespace Fabsenet.EdiEnergy
{
    public class CheckIdentifier
    {
        public int Identifier { get; set; }
        public List<string> EdiDocIds { get; set; }
    }
}