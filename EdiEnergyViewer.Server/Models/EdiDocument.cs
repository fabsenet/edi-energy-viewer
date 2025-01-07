namespace Fabsenet.EdiEnergyViewer.Models;

public record EdiDocument
{
    public string Id { get; set; }
    public string DocumentName { get; set; }
    public string DocumentUri { get; set; }
    public string MirrorUri { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsMig { get; set; }
    public bool IsAhb { get; set; }
    public bool IsGas { get; set; }
    public bool IsStrom { get; set; }
    public bool IsStromUndOderGas { get; set; }
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

public record EdiDocumentSlim
{
    public required string Id { get; set; }
    public string DocumentName { get; set; }
    public string DocumentNameRaw { get; set; }
    public string DocumentUri { get; set; }
    public string MirrorUri { get; set; }
    public required DateTime ValidFrom { get; set; }
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


    public List<CheckidentiferWithStats>? CheckIdentifiersWithStats { get; set; }
    public bool IsStrom { get; internal set; }
    public bool IsGas { get; internal set; }
    public bool IsStromUndOderGas { get; set; }
    public bool IsHot { get; internal set; }
}

public record CheckidentiferWithStats
{
    public required int CheckIdentifier { get; init; }
    public required int LargestPageBlock { get; init; }
}