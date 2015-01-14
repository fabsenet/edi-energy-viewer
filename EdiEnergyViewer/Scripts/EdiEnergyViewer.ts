/// <reference path="typings/angularjs/angular.d.ts" />
/// <reference path="typings/angularjs/angular-resource.d.ts" />
/// <reference path="typings/underscore/underscore.d.ts" />

var ediDocumentsService = angular.module('ediDocumentsService', ['ngResource']);

ediDocumentsService.factory('ediDocument', ['$resource',
  $resource => $resource('api/ediDocuments/:documentId', {}, {
      query: {
          method: 'GET',
          params: { documentId: '' },
          isArray: true
      }
  })]);

var ediEnergyViewer = angular.module("ediEnergyViewer", ["ediDocumentsService", 'ngStorage']);

ediEnergyViewer.controller("ediDocumentController", function (ediDocument:ng.resource.IResourceClass<IEdiDocument>, $localStorage) {
    this.documentTypeAhb = "AHB";
    this.documentTypeMig = "MIG";

    this.storage = $localStorage.$default({
        messageTypeFilter: "ALL",
        documentTypeFilter: "ALL",
        validityFilter: "ALL"
    });

    this.documentTypes = [this.documentTypeMig, this.documentTypeAhb];

    this.ediDocuments = ediDocument.query(ediDocuments => {
        console.log("ediDocuments loaded", ediDocuments);

        //make date strings to actual dates
        ediDocuments.forEach(doc => {
            doc.ValidFrom = new Date(doc.ValidFrom);
            doc.ValidTo = new Date(doc.ValidTo);
            doc.DocumentDate = new Date(doc.DocumentDate);
        });

        //produce a unique list of the known message types
        var types = _.map(ediDocuments, (doc:any) => doc.ContainedMessageTypes);
        types = _.flatten(types);
        types = _.without(types, null);
        types = _.uniq(types);
        types = _.sortBy(types);
        this.messageTypes = types;

        console.log("Unique edi message types:", this.messageTypes);
    });




    var validInPast = "vergangen";
    var validNow = "aktuell";
    var validInFuture = "zukünftig";
    this.validitys = [validInPast, validNow, validInFuture];

    var today = new Date();


    this.messageDocumentFilter = document => {
        if (document.IsGeneralDocument === true) return false;
        if (this.storage.validityFilter === validNow) {
            var isValidNow = document.ValidFrom < today && (document.ValidTo == null || document.ValidTo > today);
            if (!isValidNow) return false;
        }
        if (this.storage.validityFilter === validInFuture) {
            var isValidInFuture = document.ValidFrom >= today;
            if (!isValidInFuture) return false;
        }
        if (this.storage.validityFilter === validInPast) {
            var isValidInPast = document.ValidTo < today;
            if (!isValidInPast) return false;
        }

        if (this.storage.documentTypeFilter === this.documentTypeAhb && !document.IsAhb || this.storage.documentTypeFilter === this.documentTypeMig && !document.IsMig) return false;

        if (this.storage.messageTypeFilter !== "ALL" && !_.contains(document.ContainedMessageTypes, this.storage.messageTypeFilter)) return false;

        return true;
    };

    this.generalDocumentFilter = document => {
        if (document.IsGeneralDocument === false) return false;
        if (this.storage.messageTypeFilter !== "ALL") return false;
        if (this.storage.documentTypeFilter !== "ALL") return false;

        return true;
    };
});