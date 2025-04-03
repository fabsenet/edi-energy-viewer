namespace Fabsenet.EdiEnergyViewer.Models;

public class ExportRunStatistics
{
    public const string DefaultId = "ExportRunStatistics/1";

    public required string Id { get; set; }
    public DateTime RunFinishedUtc { get; set; }
}
