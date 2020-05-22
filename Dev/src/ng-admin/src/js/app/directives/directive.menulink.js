// Services definition.
(function () {
    'use strict';

    angular
        .module('app')
        .directive('menuLink', menuLink);

    menuLink.$inject = [];
    
    function menuLink() {
        return {
            scope: {
                section: '='
            },
            //templateUrl: '/lib/material-docs-menu-link.tmpl.html',
            template: '<md-button \
                    ng-class=\"{\'active\' : isSelected()}\" \
                    ng-href=\"{{section.url}}\" \
                    ng-click=\"focusSection()\"> \
                        <md-icon md-font-set=\"material-icons\" ng-if=\"section.icon != null\" style=\"margin-top: -5px;\">{{section.icon}}</md-icon> \
                        {{section | humanizeDoc}} \
                        <!--<span class=\"_md-visually-hidden\" ng-if=\"isSelected()\">current page</span>--> \
                </md-button>',
            link: function ($scope, $element) {
                var controller = $element.parent().controller();
                $scope.isSelected = function () {
                    return controller.isSelected($scope.section);
                };
                $scope.focusSection = function () {
                    // set flag to be used later when
                    // $locationChangeSuccess calls openPage()
                    controller.autoFocusContent = true;
                };
            }
        };
    };
})();