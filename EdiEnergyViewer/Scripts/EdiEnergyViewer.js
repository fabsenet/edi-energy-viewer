/// <reference path="_references.js" />
"use strict";

var ediDocumentsService = angular.module('ediDocumentsService', ['ngResource']);

ediDocumentsService.factory('ediDocument', ['$resource',
  function ($resource) {
      return $resource('api/ediDocuments/:documentId', {}, {
          query: {
              method: 'GET',
              params: { documentId: '' },
              isArray: true,
          }
      });
  }]);



var ediEnergyViewer = angular.module("ediEnergyViewer", ["ediDocumentsService", 'ngStorage']);

var ediDocumentController = function ($scope, ediDocument, $localStorage) {

    var vm = {
        storage: $localStorage.$default({
            messageTypeFilter: "ALL",
            documentTypeFilter: "ALL",
            validityFilter: "ALL"
        }),
        documentTypeAhb: "AHB",
        documentTypeMig: "MIG"
    };

    vm.documentTypes = [vm.documentTypeMig, vm.documentTypeAhb];

    vm.ediDocuments = ediDocument.query(function (ediDocuments) {
        console.log("ediDocuments loaded", ediDocuments);

        ediDocuments.forEach(function(doc) {
            doc.ValidFrom = new Date(doc.ValidFrom);
            doc.ValidTo = new Date(doc.ValidTo);
            doc.DocumentDate = new Date(doc.DocumentDate);
        });

        //produce a list of the message types
        var types = _.map(ediDocuments, function (doc) { return doc.ContainedMessageTypes });
        types = _.flatten(types);
        types = _.without(types, null);
        types = _.uniq(types);
        types = _.sortBy(types);
        vm.messageTypes = types;
        console.log("Unique edi message types:",vm.messageTypes);
    });




    var validInPast = "vergangen";
    var validNow = "aktuell";
    var validInFuture = "zukünftig";
    vm.validitys = [validInPast, validNow, validInFuture];

    var today = new Date();

    vm.messageDocumentFilter = function (document) {
        if (document.IsGeneralDocument === true) return false;
        if (vm.storage.validityFilter === validNow) {
            var isValidNow = document.ValidFrom < today && (document.ValidTo == null || document.ValidTo > today);
            if (!isValidNow) return false;
        }
        if (vm.storage.validityFilter === validInFuture) {
            var isValidInFuture = document.ValidFrom >= today;
            if (!isValidInFuture) return false;
        }
        if (vm.storage.validityFilter === validInPast) {
            var isValidInPast = document.ValidTo < today;
            if (!isValidInPast) return false;
        }

        if (vm.storage.documentTypeFilter === vm.documentTypeAhb && !document.IsAhb || vm.storage.documentTypeFilter === vm.documentTypeMig && !document.IsMig) return false;

        if (vm.storage.messageTypeFilter !== "ALL" && !_.contains(document.ContainedMessageTypes, vm.storage.messageTypeFilter)) return false;

        return true;
    }

    vm.generalDocumentFilter = function (document) {
        if (document.IsGeneralDocument === false) return false;
        if (vm.storage.messageTypeFilter !== "ALL") return false;
        if (vm.storage.documentTypeFilter !== "ALL") return false;

        return true;
    }

    $scope.vm = vm;
};


ediEnergyViewer.controller("ediDocumentController", ediDocumentController);