// ReSharper disable InconsistentNaming
interface IEdiDocument {
    Id: string;
    DocumentName: string;
    DocumentUri: string;
    IsMig: boolean;
    IsAhb: boolean;
    ContainedMessageTypes: string[];
    IsGeneralDocument: boolean;
    MessageTypeVersion: string;
    BdewProcess: string;
    DocumentNameRaw: string;
    IsLatestVersion: boolean;

    ValidFrom: string;
    ValidTo: string;
    DocumentDate: string;

    ValidFromDate: Date;
    ValidToDate: Date;
    DocumentDateDate: Date;
}