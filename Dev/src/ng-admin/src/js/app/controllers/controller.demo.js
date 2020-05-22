(function () {
    'use strict';

    // Demos controller.
    angular
        .module('app')
        .controller('DocsCtrl', ['$scope', '$mdSidenav', '$timeout', '$mdDialog', '$location', '$rootScope', '$log', 'menu', 'navs', 'siteConf', 'siteConfData', 'heads', 'regions', function ($scope, /*COMPONENTS, BUILDCONFIG,*/ $mdSidenav, $timeout, $mdDialog, $location, $rootScope, $log, menu, navs, siteConf, siteConfData, heads, regions) {
            var self = this;

            ///////////////////////////////////////////////

            // Site domain...
            $scope.Domain = $location.host();

            // To manage the  progress bar...
            $scope.navs = navs;

            // To manage region change...
            $scope.regionName = null;
            regions.OnChange = function (region) {
                //$log.debug('DocsCtrl:OnChange: region=', region);
                $scope.regionName = (region == null || region.StringValue == null)
                    ? null 
                    : region.StringValue;
            }

            // Site configuration...
            $scope.Conf = null;
            siteConfData.then(function (conf) {
                $log.debug('DocsCtrl:conf=', conf);
                $scope.Conf = conf;
                $scope.Domain = conf.Name;
            }, function (reason) {
                $log.debug('DocsCtrl:reason=', reason);
            });

            ///////////////////////////////////////////////

            $scope.searchOpen = false;
            $scope.searchText = null;
            $scope.togleSearch = function () {
                $scope.searchOpen = ($scope.searchOpen == false) ? true : false;
            }
            $scope.executeSearch = function () {
                heads.search = $scope.searchText;
                $log.debug('Change search text to:', heads.search);
                // Reload the page...
                heads.Reload(2);
            }
            $scope.closeSearch = function () {
                $scope.searchOpen = false;
                $scope.searchText = null;
                $scope.executeSearch();
            }

            ///////////////////////////////////////////////

            $scope.menu = menu;

            $scope.path = path;
            $scope.goHome = goHome;
            $scope.openMenu = openMenu;
            $scope.closeMenu = closeMenu;
            $scope.isSectionSelected = isSectionSelected;

            // Grab the current year so we don't have to update the license every year
            $scope.thisYear = (new Date()).getFullYear();

            $rootScope.$on('$locationChangeSuccess', openPage);
            $scope.focusMainContent = focusMainContent;

            //-- Define a fake model for the related page selector
            Object.defineProperty($rootScope, "relatedPage", {
                get: function () { return null; },
                set: angular.noop,
                enumerable: true,
                configurable: true
            });

            $rootScope.redirectToUrl = function (url) {
                $location.path(url);
                $timeout(function () { $rootScope.relatedPage = null; }, 100);
            };

            // Methods used by menuLink and menuToggle directives
            this.isOpen = isOpen;
            this.isSelected = isSelected;
            this.toggleOpen = toggleOpen;
            this.autoFocusContent = false;

            var mainContentArea = document.querySelector("[role='main']");

            // *********************
            // Internal methods
            // *********************

            function closeMenu() {
                $timeout(function () { $mdSidenav('left').close(); });
            }

            function openMenu() {
                $timeout(function () { $mdSidenav('left').open(); });
            }

            function path() {
                return $location.path();
            }

            function goHome($event) {
                menu.selectPage(null, null);
                $location.path('/');
            }

            function openPage() {
                $scope.closeMenu();

                if (self.autoFocusContent) {
                    focusMainContent();
                    self.autoFocusContent = false;
                }
            }

            function focusMainContent($event) {
                // prevent skip link from redirecting
                if ($event) { $event.preventDefault(); }

                $timeout(function () {
                    mainContentArea.focus();
                }, 90);

            }

            function isSelected(page) {
                return menu.isPageSelected(page);
            }

            function isSectionSelected(section) {
                var selected = false;
                var openedSection = menu.openedSection;
                if (openedSection === section) {
                    selected = true;
                }
                else if (section.children) {
                    section.children.forEach(function (childSection) {
                        if (childSection === openedSection) {
                            selected = true;
                        }
                    });
                }
                return selected;
            }

            function isOpen(section) {
                return menu.isSectionSelected(section);
            }

            function toggleOpen(section) {
                menu.toggleSelectSection(section);
            }
        }])


        .controller('HomeCtrl', ['$scope', '$rootScope', function ($scope, $rootScope) {
            $rootScope.currentComponent = $rootScope.currentDoc = null;
        }])
        .controller('GuideCtrl', ['$rootScope', '$http', function ($rootScope, $http) {
            $rootScope.currentComponent = $rootScope.currentDoc = null;
            if (!$rootScope.contributors) {
                $http
                    .get('/lib/material-docs-contributors.json')
                    .then(function (response) {
                        $rootScope.github = response.data;
                    })
            }
        }])
        .controller('LayoutCtrl', ['$scope', '$attrs', '$location', '$rootScope', function ($scope, $attrs, $location, $rootScope) {
            $rootScope.currentComponent = $rootScope.currentDoc = null;

            $scope.exampleNotEditable = true;
            $scope.layoutDemo = {
                mainAxis: 'center',
                crossAxis: 'center',
                direction: 'row'
            };
            $scope.layoutAlign = function () {
                return $scope.layoutDemo.mainAxis +
                    ($scope.layoutDemo.crossAxis ? ' ' + $scope.layoutDemo.crossAxis : '')
            };
        }])
        .controller('LayoutTipsCtrl', [function () {
            var self = this;

            /*
                * Flex Sizing - Odd
                */
            self.toggleButtonText = "Hide";

            self.toggleContentSize = function () {
                var contentEl = angular.element(document.getElementById('toHide'));

                contentEl.toggleClass("ng-hide");

                self.toggleButtonText = contentEl.hasClass("ng-hide") ? "Show" : "Hide";
            };
        }])
        .controller('ComponentDocCtrl', ['$scope', 'doc', 'component', '$rootScope', function ($scope, doc, component, $rootScope) {
            $rootScope.currentComponent = component;
            $rootScope.currentDoc = doc;
        }])
        .controller('DemoCtrl', ['$rootScope', '$scope', 'component', 'demos', '$templateRequest', function ($rootScope, $scope, component, demos, $templateRequest) {
            $rootScope.currentComponent = component;
            $rootScope.currentDoc = null;

            $scope.demos = [];

            angular.forEach(demos, function (demo) {
                // Get displayed contents (un-minified)
                var files = [demo.index]
                    .concat(demo.js || [])
                    .concat(demo.css || [])
                    .concat(demo.html || []);
                files.forEach(function (file) {
                    file.httpPromise = $templateRequest(file.outputPath)
                        .then(function (response) {
                            file.contents = response
                            .replace('<head/>', '');
                            return file.contents;
                        });
                });
                demo.$files = files;
                $scope.demos.push(demo);
            });

            $scope.demos = $scope.demos.sort(function (a, b) {
                return a.name > b.name ? 1 : -1;
            });

        }]);
})();