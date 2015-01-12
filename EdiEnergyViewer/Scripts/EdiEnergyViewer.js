/// <reference path="underscore.js" />
"strict";


var ediDocumentsService = angular.module('ediDocumentsService', ['ngResource']);

ediDocumentsService.factory('ediDocument', ['$resource',
  function ($resource) {
      return $resource('api/ediDocuments/:documentId', {}, {
          query: { method: 'GET', params: { documentId: '' }, isArray: true }
      });
  }]);



var ediEnergyViewer = angular.module("ediEnergyViewer", ["ediDocumentsService"]);

var ediDocumentController = function ($scope, ediDocument) {

    var vm = {};

    vm.ediDocuments = ediDocument.query(function (ediDocuments) {
        console.log("ediDocuments loaded", ediDocuments);

        //produce a list of the message types
        var types = _.map(ediDocuments, function (doc) { return doc.ContainedMessageTypes });
        types = _.flatten(types);
        types = _.without(types, null);
        types = _.uniq(types);
        types = _.sortBy(types);
        vm.messageTypes = types;
        console.log(vm.messageTypes);
    });


    vm.messageTypeFilter = "ALL";


    vm.messageDocumentFilter = function (document) {
        if (document.IsGeneralDocument === true) return false;

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