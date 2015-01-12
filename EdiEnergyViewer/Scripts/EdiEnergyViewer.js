/// <reference path="underscore.js" />
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



var ediEnergyViewer = angular.module("ediEnergyViewer", ["ediDocumentsService"]);

var ediDocumentController = function ($scope, ediDocument) {

    var vm = {
        

    };

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


    vm.messageTypeFilter = "ALL";
    vm.validityFilter = "ALL";
    vm.documentTypeFilter = "ALL";
    var documentTypeAhb = "AHB";
    var documentTypeMig = "MIG";
    var documentTypes = [documentTypeAhb, documentTypeMig];

    var validInPast = "vergangen";
    var validNow = "aktuell";
    var validInFuture = "zukünftig";
    vm.validitys = [validInPast, validNow, validInFuture];

    var today = new Date();

    console.log("date!!", today);

    vm.messageDocumentFilter = function (document) {
        if (document.IsGeneralDocument === true) return false;
        if (vm.validityFilter === validNow) {
            var isValidNow = document.ValidFrom < today && (document.ValidTo == null || document.ValidTo > today);
            if (!isValidNow) return false;
        }
        if (vm.validityFilter === validInFuture) {
            var isValidInFuture = document.ValidFrom >= today;
            if (!isValidInFuture) return false;
        }
        if (vm.validityFilter === validInPast) {
            var isValidInPast = document.ValidTo < today;
            if (!isValidInPast) return false;
        }
        if ( vm.messageTypeFilter !== "ALL" && !_.contains(document.ContainedMessageTypes, vm.messageTypeFilter)) return false;

        return true;
    }

    vm.generalDocumentFilter = function (document) {
        if (document.IsGeneralDocument === false) return false;
        if (vm.messageTypeFilter !== "ALL") return false;

        return true;
    }

    $scope.vm = vm;
};


ediEnergyViewer.controller("ediDocumentController", ediDocumentController);