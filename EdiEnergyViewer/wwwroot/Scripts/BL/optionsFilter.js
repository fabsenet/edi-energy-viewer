/// <reference path="root.d.ts" />
var OptionsFilterModule;
(function (OptionsFilterModule) {
    var optionsFilterModule = angular.module("optionsFilterModule", []);
    optionsFilterModule.directive("optionsFilter", function () {
        return {
            restrict: "E",
            templateUrl: appBaseUri + "/Scripts/BL/options-filter.html",
            scope: {
                filter: "="
            }
        };
    });
})(OptionsFilterModule || (OptionsFilterModule = {}));
//# sourceMappingURL=optionsFilter.js.map