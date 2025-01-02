// ReSharper disable InconsistentNaming
interface IEdiDocument {
    Id: string;
    DocumentName: string;
    DocumentUri: string;
    MirrorUri: string;
    IsMig: boolean;
    IsAhb: boolean;
    ContainedMessageTypes: string[];
    IsGeneralDocument: boolean;
    MessageTypeVersion: string;
    BdewProcess: string;
    DocumentNameRaw: string;
    IsLatestVersion: boolean;
    CheckIdentifier: number[];
    ValidFrom: string;
    ValidTo: string;
    DocumentDate: string;
    Filename: string;
    ValidFromDate: Date;
    ValidToDate: Date;
    DocumentDateDate: Date;
}