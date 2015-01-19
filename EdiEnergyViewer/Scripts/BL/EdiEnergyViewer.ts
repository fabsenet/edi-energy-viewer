/// <reference path="../typings/angularjs/angular.d.ts" />
/// <reference path="../typings/angularjs/angular-resource.d.ts" />
/// <reference path="../typings/underscore/underscore.d.ts" />
/// <reference path="edidocument.ts" />

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

var ediEnergyViewer = angular.module("ediEnergyViewer", ["ediDocumentsService", 'ngStorage','optionsFilterModule']);

ediEnergyViewer.controller("ediDocumentController", function(ediDocument: ng.resource.IResourceClass<IEdiDocument>, $localStorage) {


    this.storage = $localStorage.$default({
        messageTypeFilter: "ALL",
        documentTypeFilter: "ALL",
        validityFilter: "ALL"
    });

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

    this.ediDocuments = ediDocument.query((ediDocuments: ng.resource.IResourceArray<IEdiDocument>) => {
        console.log("ediDocuments loaded", ediDocuments);

        //make date strings to actual dates
        ediDocuments.forEach(doc => {
            doc.ValidFromDate = new Date(doc.ValidFrom);
            doc.ValidToDate = new Date(doc.ValidTo);
            doc.DocumentDateDate = new Date(doc.DocumentDate);
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

        return true;
    };

    this.generalDocumentFilter = (document: IEdiDocument) => {
        if (!document.IsGeneralDocument) return false;

        if (this.messageTypeFilter.selected !== "ALL") return false;
        if (this.documentTypeFilter.selected !== "ALL") return false;
        if (this.documentVersionFilter.selected !== "ALL" && !document.IsLatestVersion) return false;

        return true;
    };
});
