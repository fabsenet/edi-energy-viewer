/// <reference path="IEdiDocument.ts" />
/// <reference path="ICheckIdentifier.ts" />
/// <reference path="root.d.ts" />

"use strict";
var angular: any;
var ng: any;
var _: any;

var ediDocumentsService = angular.module('ediDocumentsService', ['ngResource']);

ediDocumentsService.factory('ediDocument', ['$resource',
    $resource => $resource(appBaseUri +'/api/ediDocuments/:documentId', {}, {
      query: {
          method: 'GET',
          params: { documentId: '' },
          isArray: true
      }
  })]);
ediDocumentsService.factory('checkIdentifier', ['$resource',
    $resource => $resource(appBaseUri +'/api/checkIdentifier/:documentId', {}, {
      query: {
          method: 'GET',
          params: { documentId: '' },
          isArray: true
      }
  })]);

var ediEnergyViewer = angular.module("ediEnergyViewer", ["ediDocumentsService", 'ngStorage','optionsFilterModule']);

ediEnergyViewer.controller("ediDocumentController", ["checkIdentifier", "ediDocument", "$localStorage", function (checkIdentifier: any, ediDocument: any, $localStorage) {

    this.hasLoaded = false;
    var pendingLoadAction = 2; //Actions: ediDocuments,checkIdentifier
    var loadActionDone = (loadedAsset) => {
        console.log("loaded: "+loadedAsset);
        pendingLoadAction--;
        if (!pendingLoadAction) {
            console.log("finished loading app and ressources.");
            this.hasLoaded = true;
        }
    }
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

    this.checkIdentifiers = checkIdentifier.query((checkIds) => {
        console.log(checkIds.length + " checkIdentifiers loaded");
        loadActionDone("checkIdentifiers");
    });

    this.ediDocuments = ediDocument.query((ediDocuments: IEdiDocument[]) => {
        console.log(ediDocuments.length + " ediDocuments loaded");

        function nullOrDate(stringVal:string):Date {
            if (stringVal === null) return null;
            return new Date(stringVal);
        }

        //make date strings to actual dates
        ediDocuments.forEach(doc => {
            doc.ValidFromDate = nullOrDate(doc.ValidFrom);
            let newValidDate = nullOrDate(doc.ValidTo);
            if (newValidDate) {
                newValidDate.setHours(23);
                newValidDate.setMinutes(59);
                newValidDate.setSeconds(59);
            }
            doc.ValidToDate = newValidDate;
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
        loadActionDone("ediDocuments");
    });

    var validInPast = "vergangen";
    var validNow = "aktuell";
    var validInFuture = "zukünftig";
    this.validityFilter = {
        selected: "aktuell",
        options: [validInPast, validNow, validInFuture]
    };

    this.documentVersionFilter = { selected: "Letzte Fassung", options: ["Letzte Fassung"] };

    var today = new Date();
    var checkIdentifierIntroductionDate = new Date(2014, 10, 1);

    this.messageDocumentFilter = (ediDoc:IEdiDocument) => {
        if (ediDoc.IsGeneralDocument) return false;
        if (this.validityFilter.selected === validNow) {
            var isValidNow = ediDoc.ValidFromDate < today && (ediDoc.ValidTo == null || ediDoc.ValidToDate > today);
            if (!isValidNow) return false;
        }
        if (this.validityFilter.selected === validInFuture) {
            var isValidInFuture = ediDoc.ValidFromDate >= today || ediDoc.ValidToDate == null;
            if (!isValidInFuture) return false;
        }
        if (this.validityFilter.selected === validInPast) {
            var isValidInPast = ediDoc.ValidToDate !== null && ediDoc.ValidToDate < today;
            if (!isValidInPast) return false;
        }

        if (this.documentTypeFilter.selected === documentTypeAhb && !ediDoc.IsAhb || this.documentTypeFilter.selected === documentTypeMig && !ediDoc.IsMig) return false;

        if (this.messageTypeFilter.selected !== "ALL" && !_.contains(ediDoc.ContainedMessageTypes, this.messageTypeFilter.selected)) return false;

        if (this.documentVersionFilter.selected !== "ALL" && !ediDoc.IsLatestVersion) return false;

        if (this.checkIdentifierFilter !== null) {
            //check identifer where introduced on 01.10.2014 so earlier validtos cannot contain check identifiers!
            if (ediDoc.ValidToDate !== null && ediDoc.ValidToDate < checkIdentifierIntroductionDate) return false;

            //the checkidentifier is a number!
            if (isNaN(this.checkIdentifierFilter) || this.checkIdentifierFilter<=0) return false;

            //treat written value as value*
            var idRangeStart = this.checkIdentifierFilter;
            var idRangeEnd = this.checkIdentifierFilter + 1;
            while (idRangeStart < 10000) {
                idRangeStart *= 10;
                idRangeEnd *= 10;
            }

            var documentContainsCheckIdentifierInRange = _.any(ediDoc.CheckIdentifier, checkId => checkId>=idRangeStart && checkId<idRangeEnd);
            if (!documentContainsCheckIdentifierInRange) return false;
        }

        return true;
    };

    this.generalDocumentFilter = (ediDoc: IEdiDocument) => {
        if (!ediDoc.IsGeneralDocument) return false;

        if (this.messageTypeFilter.selected !== "ALL") return false;
        if (this.documentTypeFilter.selected !== "ALL") return false;
        if (this.documentVersionFilter.selected !== "ALL" && !ediDoc.IsLatestVersion) return false;
        if (this.checkIdentifierFilter !== null) return false;

        return true;
    };

    this.getFullMirrorUri = (ediDocument: IEdiDocument, checkId?: number) => {
        if (checkId) {
            return appBaseUri + "/api/" + ediDocument.Id + "/part/" + checkId;
        } else {
            return appBaseUri + "/api/" + ediDocument.Id + "/full";
        }
    }
}]);
