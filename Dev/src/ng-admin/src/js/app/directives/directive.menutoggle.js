//// Services definition.
//(function () {
//    'use strict';

//    angular
//        .module('app')
//        .directive('menuToggle', menuToggle);

//    menuToggle.$inject = ['$timeout', '$mdUtil'];
    
//    function menuToggle($timeout, $mdUtil) {
//        return {
//            scope: {
//                section: '='
//            },
//            //templateUrl: '/lib/material-docs-menu-toggle.tmpl.html',
//            template: "<md-button class=\"md-button-toggle\" \
//                      ng-click=\"toggle()\" \
//                      aria-controls=\"docs-menu-{{section.name | nospace}}\" \
//                      aria-expanded=\"{{isOpen()}}\"> \
//                      <div flex layout=\"row\"> \
//                        <md-icon md-font-set=\"material-icons\" ng-if=\"section.icon != null\">{{section.icon}}</md-icon> \
//                        {{section.name}} \
//                        <span flex></span> \
//                        <span aria-hidden=\"true\" class=\"md-toggle-icon\" \
//                        ng-class=\"{'toggled' : isOpen()}\"> \
//                          <md-icon class=\"material-icons\">keyboard_arrow_down</md-icon> \
//                        </span> \
//                      </div> \
//                      <span class=\"md-visually-hidden\"> \
//                        Toggle {{isOpen()? 'expanded' : 'collapsed'}} \
//                      </span> \
//                    </md-button> \
//                    <ul id=\"docs-menu-{{section.name | nospace}}\" class=\"menu-toggle-list\"> \
//                      <li ng-repeat=\"page in section.pages\"> \
//                        <menu-link section=\"page\"></menu-link> \
//                      </li> \
//                    </ul>",
//            link: function ($scope, $element) {
//                var controller = $element.parent().controller();

//                $scope.isOpen = function () {
//                    return controller.isOpen($scope.section);
//                };
//                $scope.toggle = function () {
//                    controller.toggleOpen($scope.section);
//                };

//                $mdUtil.nextTick(function () {
//                    $scope.$watch(
//                      function () {
//                          return controller.isOpen($scope.section);
//                      },
//                      function (open) {
//                          var $ul = $element.find('ul');

//                          var targetHeight = open ? getTargetHeight() : 0;
//                          $timeout(function () {
//                              $ul.css({ height: targetHeight + 'px' });
//                          }, 0, false);

//                          function getTargetHeight() {
//                              var targetHeight;
//                              $ul.addClass('no-transition');
//                              $ul.css('height', '');
//                              targetHeight = $ul.prop('clientHeight');
//                              $ul.css('height', 0);
//                              $ul.removeClass('no-transition');
//                              return targetHeight;
//                          }
//                      }
//                    );
//                });

//                var parentNode = $element[0].parentNode.parentNode.parentNode;
//                if (parentNode.classList.contains('parent-list-item')) {
//                    var heading = parentNode.querySelector('h2');
//                    $element[0].firstChild.setAttribute('aria-describedby', heading.id);
//                }
//            }
//        };
//    };
//})();