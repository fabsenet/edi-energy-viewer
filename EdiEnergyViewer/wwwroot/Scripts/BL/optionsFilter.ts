/// <reference path="root.d.ts" />

module OptionsFilterModule {
    var optionsFilterModule = angular.module("optionsFilterModule", []);

    optionsFilterModule.directive("optionsFilter", () => {
        return {
            restrict: "E",
            templateUrl: appBaseUri + "/Scripts/BL/options-filter.html",
            scope: {
                filter: "="
            }
        }
    });
}

