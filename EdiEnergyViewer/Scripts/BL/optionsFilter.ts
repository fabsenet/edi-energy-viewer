/// <reference path="../typings/angularjs/angular.d.ts" />

module OptionsFilterModule {
    var optionsFilterModule = angular.module("optionsFilterModule", []);

    optionsFilterModule.directive("optionsFilter", () => {
        return {
            restrict: "E",
            templateUrl: "Scripts/BL/options-filter.html",
            scope: {
                filter: "="
            }
        }
    });
}

