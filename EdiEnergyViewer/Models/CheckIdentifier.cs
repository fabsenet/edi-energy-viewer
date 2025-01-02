using System.Collections.Generic;
using JetBrains.Annotations;

namespace Fabsenet.EdiEnergyViewer.Models;

public class CheckIdentifier
{
    public int Identifier { get; set; }
    public List<string> EdiDocIds { get; set; }
}