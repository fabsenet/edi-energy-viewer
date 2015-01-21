/// <reference path="../typings/angularjs/angular.d.ts" />
/// <reference path="../typings/angularjs/angular-resource.d.ts" />
/// <reference path="../typings/underscore/underscore.d.ts" />
/// <reference path="IEdiDocument.ts" />
/// <reference path="ICheckIdentifier.ts" />

"use strict";

var ediDocumentsService = angular.module('ediDocumentsService', ['ngResource']);

ediDocumentsService.factory('ediDocument', ['$resource',
  $resource => $resource('/api/ediDocuments/:documentId', {}, {
      query: {
          method: 'GET',
          params: { documentId: '' },
          isArray: true
      }
  })]);
ediDocumentsService.factory('checkIdentifier', ['$resource',
    $resource => $resource('/api/checkIdentifier/:documentId', {}, {
      query: {
          method: 'GET',
          params: { documentId: '' },
          isArray: true
      }
  })]);

var ediEnergyViewer = angular.module("ediEnergyViewer", ["ediDocumentsService", 'ngStorage','optionsFilterModule']);

ediEnergyViewer.controller("ediDocumentController", function (checkIdentifier: ng.resource.IResourceClass<ICheckIdentifier>  ,ediDocument: ng.resource.IResourceClass<IEdiDocument>, $localStorage) {


    //this.storage = $localStorage.$default({
    //    messageTypeFilter: "ALL",
    //    documentTypeFilter: "ALL",
    //    validityFilter: "ALL"
    //});

    this.messageTypeFilter = {
        selected: "ALL",
        options: []
    };

    var documentTypeAhb = "AHB";
    var documentTypeMig = "MIG";
    this.documentTypeFilter = {
        selected: "ALL",
        options: [documentTypeAhb, documentTypeMig]
    };

    //number or null
    this.checkIdentifierFilter = null;

    this.checkIdentifiers = checkIdentifier.query();
    
    this.ediDocuments = ediDocument.query((ediDocuments: ng.resource.IResourceArray<IEdiDocument>) => {
        console.log("ediDocuments loaded", ediDocuments);

        function nullOrDate(stringVal:string):Date {
            if (stringVal === null) return null;
            return new Date(stringVal);
        }

        //make date strings to actual dates
        ediDocuments.forEach(doc => {
            doc.ValidFromDate = nullOrDate(doc.ValidFrom);
            doc.ValidToDate = nullOrDate(doc.ValidTo);
            doc.DocumentDateDate = nullOrDate(doc.DocumentDate);
        });

        //produce a unique list of the known message types
        var types = _.map(ediDocuments, (doc: any) => doc.ContainedMessageTypes);
        types = _.flatten(types);
        types = _.without(types, null);
        types = _.uniq(types);
        types = _.sortBy(types);
        this.messageTypes = types;
        this.messageTypeFilter.options = types;
        console.log("Unique edi message types:", this.messageTypes);
    });

    var validInPast = "vergangen";
    var validNow = "aktuell";
    var validInFuture = "zukünftig";
    this.validityFilter = {
        selected: "ALL",
        options: [validInPast, validNow, validInFuture]
    };

    this.documentVersionFilter = { selected: "ALL", options: ["Letzte Fassung"] };

    var today = new Date();
    var checkIdentifierIntroductionDate = new Date(2014, 10, 1);

    this.messageDocumentFilter = (document:IEdiDocument) => {
        if (document.IsGeneralDocument) return false;
        if (this.validityFilter.selected === validNow) {
            var isValidNow = document.ValidFromDate < today && (document.ValidTo == null || document.ValidToDate > today);
            if (!isValidNow) return false;
        }
        if (this.validityFilter.selected === validInFuture) {
            var isValidInFuture = document.ValidFromDate >= today;
            if (!isValidInFuture) return false;
        }
        if (this.validityFilter.selected === validInPast) {
            var isValidInPast = document.ValidToDate < today;
            if (!isValidInPast) return false;
        }

        if (this.documentTypeFilter.selected === documentTypeAhb && !document.IsAhb || this.documentTypeFilter.selected === documentTypeMig && !document.IsMig) return false;

        if (this.messageTypeFilter.selected !== "ALL" && !_.contains(document.ContainedMessageTypes, this.messageTypeFilter.selected)) return false;

        if (this.documentVersionFilter.selected !== "ALL" && !document.IsLatestVersion) return false;

        if (this.checkIdentifierFilter !== null) {
            //check identifer where introduced on 01.10.2014 so earlier validtos cannot contain check identifiers!
            if (document.ValidToDate !== null && document.ValidToDate < checkIdentifierIntroductionDate) return false;

            //the checkidentifier is a number!
            if (isNaN(this.checkIdentifierFilter) || this.checkIdentifierFilter<=0) return false;

            //treat written value als value*
            var idRangeStart = this.checkIdentifierFilter;
            var idRangeEnd = this.checkIdentifierFilter + 1;
            while (idRangeStart < 10000) {
                idRangeStart *= 10;
                idRangeEnd *= 10;
            }

            var possibleCheckIdentifier = _.filter(this.checkIdentifiers, (id: ICheckIdentifier) => id.Identifier >= idRangeStart && id.Identifier < idRangeEnd);
            //the current document must match the messagetype and the AHB
            if (!_.any(possibleCheckIdentifier,
                    (id: ICheckIdentifier) => (document.BdewProcess === id.BdewProcess || document.IsAhb && document.BdewProcess === null)
                    && _.contains(document.ContainedMessageTypes, id.MessageType)
                )
            ) return false;

            //return false;
        }

        return true;
    };

    this.generalDocumentFilter = (document: IEdiDocument) => {
        if (!document.IsGeneralDocument) return false;

        if (this.messageTypeFilter.selected !== "ALL") return false;
        if (this.documentTypeFilter.selected !== "ALL") return false;
        if (this.documentVersionFilter.selected !== "ALL" && !document.IsLatestVersion) return false;
        if (this.checkIdentifierFilter !== null) return false;

        return true;
    };
});
