/// <reference path="_references.js" />
"use strict";

var ediDocumentsService = angular.module('ediDocumentsService', ['ngResource']);

ediDocumentsService.factory('ediDocument', ['$resource',
  function ($resource) {
      return $resource('api/ediDocuments/:documentId', {}, {
          query: {
              method: 'GET',
              params: { documentId: '' },
              isArray: true
          }
      });
  }]);



var ediEnergyViewer = angular.module("ediEnergyViewer", ["ediDocumentsService", 'ngStorage']);



ediEnergyViewer.controller("ediDocumentController", function (ediDocument, $localStorage) {

    var that = this;

    this.documentTypeAhb = "AHB";
    this.documentTypeMig = "MIG";

    this.storage = $localStorage.$default({
        messageTypeFilter: "ALL",
        documentTypeFilter: "ALL",
        validityFilter: "ALL"
    });

    this.documentTypes = [this.documentTypeMig, this.documentTypeAhb];

    this.ediDocuments = ediDocument.query(function (ediDocuments) {
        console.log("ediDocuments loaded", ediDocuments);

        //make date strings to actual dates
        ediDocuments.forEach(function (doc) {
            doc.ValidFrom = new Date(doc.ValidFrom);
            doc.ValidTo = new Date(doc.ValidTo);
            doc.DocumentDate = new Date(doc.DocumentDate);
        });

        //produce a unique list of the known message types
        var types = _.map(ediDocuments, function (doc) { return doc.ContainedMessageTypes });
        types = _.flatten(types);
        types = _.without(types, null);
        types = _.uniq(types);
        types = _.sortBy(types);
        that.messageTypes = types;

        console.log("Unique edi message types:", that.messageTypes);
    });




    var validInPast = "vergangen";
    var validNow = "aktuell";
    var validInFuture = "zukünftig";
    this.validitys = [validInPast, validNow, validInFuture];

    var today = new Date();


    this.messageDocumentFilter = function(document) {
        if (document.IsGeneralDocument === true) return false;
        if (that.storage.validityFilter === validNow) {
            var isValidNow = document.ValidFrom < today && (document.ValidTo == null || document.ValidTo > today);
            if (!isValidNow) return false;
        }
        if (that.storage.validityFilter === validInFuture) {
            var isValidInFuture = document.ValidFrom >= today;
            if (!isValidInFuture) return false;
        }
        if (that.storage.validityFilter === validInPast) {
            var isValidInPast = document.ValidTo < today;
            if (!isValidInPast) return false;
        }

        if (that.storage.documentTypeFilter === that.documentTypeAhb && !document.IsAhb || that.storage.documentTypeFilter === that.documentTypeMig && !document.IsMig) return false;

        if (that.storage.messageTypeFilter !== "ALL" && !_.contains(document.ContainedMessageTypes, that.storage.messageTypeFilter)) return false;

        return true;
    };

    this.generalDocumentFilter = function(document) {
        if (document.IsGeneralDocument === false) return false;
        if (that.storage.messageTypeFilter !== "ALL") return false;
        if (that.storage.documentTypeFilter !== "ALL") return false;

        return true;
    };
});