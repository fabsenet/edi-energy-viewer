/**
 * This file was auto-generated by openapi-typescript.
 * Do not make direct changes to the file.
 */

export interface paths {
    "/api/EdiDocuments": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["EdiDocumentSlim"][];
                        "application/json": components["schemas"]["EdiDocumentSlim"][];
                        "text/json": components["schemas"]["EdiDocumentSlim"][];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/EdiDocuments/ClearCache": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": string;
                        "application/json": string;
                        "text/json": string;
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/EdiDocuments/{id}/part/{checkIdentifier}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: string;
                    checkIdentifier: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/EdiDocuments/{id}/full": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/EdiDocuments/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["EdiDocument"];
                        "application/json": components["schemas"]["EdiDocument"];
                        "text/json": components["schemas"]["EdiDocument"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/FilterData": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["FilterData"];
                        "application/json": components["schemas"]["FilterData"];
                        "text/json": components["schemas"]["FilterData"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
}
export type webhooks = Record<string, never>;
export interface components {
    schemas: {
        CheckidentiferWithStats: {
            /** Format: int32 */
            checkIdentifier: number;
            /** Format: int32 */
            largestPageBlock: number;
        };
        EdiDocument: {
            id?: string;
            documentName?: string;
            documentUri?: string;
            mirrorUri?: string;
            /** Format: date-time */
            validFrom?: string;
            /** Format: date-time */
            validTo?: string | null;
            isMig?: boolean;
            isAhb?: boolean;
            isGas?: boolean;
            isStrom?: boolean;
            isStromUndOderGas?: boolean;
            containedMessageTypes?: string[];
            isGeneralDocument?: boolean;
            messageTypeVersion?: string;
            /** Format: date-time */
            documentDate?: string;
            bdewProcess?: string;
            documentNameRaw?: string;
            filename?: string;
            isLatestVersion?: boolean;
            checkIdentifier?: {
                [key: string]: number[];
            };
        };
        EdiDocumentSlim: {
            id: string;
            documentName?: string;
            documentNameRaw?: string;
            documentUri?: string;
            mirrorUri?: string;
            /** Format: date-time */
            validFrom: string;
            /** Format: date-time */
            validTo?: string | null;
            isMig?: boolean;
            isAhb?: boolean;
            containedMessageTypes?: string[];
            isGeneralDocument?: boolean;
            messageTypeVersion?: string;
            /** Format: date-time */
            documentDate?: string;
            bdewProcess?: string;
            isLatestVersion?: boolean;
            filename?: string;
            checkIdentifiersWithStats?: components["schemas"]["CheckidentiferWithStats"][] | null;
            isStrom?: boolean;
            isGas?: boolean;
            isStromUndOderGas?: boolean;
            isHot?: boolean;
        };
        FilterData: {
            availableMessageTypes: string[];
        };
    };
    responses: never;
    parameters: never;
    requestBodies: never;
    headers: never;
    pathItems: never;
}
export type $defs = Record<string, never>;
export type operations = Record<string, never>;
