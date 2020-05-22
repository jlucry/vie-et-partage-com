// Services definition.
(function () {
    'use strict';

    angular
        .module('app.services')
        .factory('siteConf', siteConf);

    siteConf.$inject = ['$location', '$http', '$log'];

    function siteConf($location, $http, $log) {
        var data = null;
        var dataPromise = null;
        var lastLocation = null;
        var lastDefaultFilters = null;
        var lastFilteredCategories = null;
        var service = {
            data: data,
            dataPromise: dataPromise,
            get: get,
            getClaimCategories: getClaimCategories,
            getClaimCategory: getClaimCategory,
            getClaimTags: getClaimTags,
            getClaimTag: getClaimTag,
            getClaimGroups: getClaimGroups,
            getClaimGroup: getClaimGroup,
            getMenuFilter: getMenuFilter,
            getDefaultTopCategory: getDefaultTopCategory,
            //getDefaultShowChildsCategoryPosts: getDefaultShowChildsCategoryPosts,
            haveRole: haveRole,
            IsAdmin: false,
            IsPub: false,
            IsWriter: false,
        };
        return service;
        
        ///////////////////////

        function get() {
            if (service.data != null) {
                $log.debug('Site configuration already loaded.');
                return service.data;
            }
            service.IsAdmin = false;
            service.IsPub = false;
            service.IsWriter = false;
            $log.debug('Getting site configuration...');
            dataPromise = $http.get('/api/site/configuration?region=default')
                .then(getComplete)
                .catch(getFailed);
            return dataPromise;

            function getComplete(response) {
                service.data = response.data;
                $log.debug('Site configuration got:', service.data);
                if (service.haveRole("Administrator") == true) {
                    service.IsAdmin = true;
                }
                else if (service.haveRole("Publicator") == true) {
                    service.IsPub = true;
                }
                else if (service.haveRole("Contributor") == true) {
                    service.IsWriter = true;
                }
                //// Test on roles...
                //service.IsAdmin = false;
                //service.IsPub = false;
                //service.IsWriter = true;

                //test();
                return service.data;
            }

            function getFailed(error) {
                $log.error('Failed to get site configuration:', error.data);
            }
        }

        function getClaimCategories() {
            var topCategory = getDefaultTopCategory();
            if (service.data == null) {
                return null;
            }
            else if (topCategory == undefined) {
                //$log.debug('!!!!!!! cat: ', service.data.Categories);
                return service.data.Categories;
            }
            else if (lastFilteredCategories != null) {
                return lastFilteredCategories;
            }
            else {
                var levelToPush = -1;
                lastFilteredCategories = [];
                for (var i = 0; i < service.data.Categories.length; i++) {
                    if (levelToPush != -1 && service.data.Categories[i].Deep >= levelToPush) {
                        //$log.debug('!!!!!!! push: ', service.data.Categories[i]);
                        lastFilteredCategories.push(service.data.Categories[i]);
                    }
                    else if (levelToPush != -1 && service.data.Categories[i].Deep < levelToPush) {
                        break;
                    }
                    if (('' + service.data.Categories[i].Id) == topCategory) {
                        //lastFilteredCategories.push(service.data.Categories[i]);
                        levelToPush = service.data.Categories[i].Deep + 1;
                        //$log.debug('!!!!!!! levelToPush: ', levelToPush);
                    }
                }
                //$log.debug('!!!!!!! lastFilteredCategories: ', lastFilteredCategories);
                return lastFilteredCategories;
            }
        }
        function getClaimCategory(idStr) {
            if (idStr != null && service.data != null && service.data.Categories != null) {
                var id = parseInt(idStr, 10);
                if (id != NaN) {
                    for (var i = 0; i < service.data.Categories.length; i++) {
                        if (service.data.Categories[i].Id == id) {
                            $log.debug('Category claim of id ' + idStr + ' = ', service.data.Categories[i]);
                            return service.data.Categories[i];
                        }
                    }
                }
            }
            $log.debug('Category claim of id ' + idStr + ' not found!');
            return null;
        }

        function getClaimTags() {
            if (service.data == null) {
                return null;
            }
            //$log.debug('!!!!!!! tag: ', service.data.Tags);
            return service.data.Tags;
        }
        function getClaimTag(idStr) {
            if (idStr != null && service.data != null && service.data.Tags != null) {
                var id = parseInt(idStr, 10);
                if (id != NaN) {
                    for (var i = 0; i < service.data.Tags.length; i++) {
                        if (service.data.Tags[i].Id == id) {
                            $log.debug('Tags claim of id ' + idStr + ' = ', service.data.Tags[i]);
                            return service.data.Tags[i];
                        }
                    }
                }
            }
            $log.debug('Tags claim of id ' + idStr + ' not found!');
            return null;
        }

        function getClaimGroups() {
            if (service.data == null) {
                return null;
            }
            return service.data.Groups;
        }
        function getClaimGroup(idStr) {
            if (idStr != null && service.data != null && service.data.Groups != null) {
                var id = parseInt(idStr, 10);
                if (id != NaN) {
                    for (var i = 0; i < service.data.Groups.length; i++) {
                        if (service.data.Groups[i].Id == id) {
                            $log.debug('Groups claim of id ' + idStr + ' = ', service.data.Groups[i]);
                            return service.data.Groups[i];
                        }
                    }
                }
            }
            $log.debug('Groups claim of id ' + idStr + ' not found!');
            return null;
        }

        function getMenuFilter() {
            //$log.debug('@@@@@@@@@@@@@@getMenuFilter:$location', $location);
            var locationUrl = "#" + $location.url();
            if (lastLocation == locationUrl) {
                return lastDefaultFilters;
            }
            else {
                lastLocation = locationUrl;
                if (service.data != null && service.data.Menus != null) {
                    for (var i = 0; i < service.data.Menus.length; i++) {
                        //$log.debug('@@@@@@@@@@@@@@locationUrl=', locationUrl, " vs menu=", service.data.Menus[i].UrlMat);
                        if (locationUrl == service.data.Menus[i].UrlMat) {
                            lastDefaultFilters = service.data.Menus[i].DefaultFilters;
                            lastFilteredCategories = null;
                            return lastDefaultFilters;
                        }
                    };
                }
                lastDefaultFilters = null;
                lastFilteredCategories = null;
                return lastDefaultFilters;
            }
        }

        function getDefaultTopCategory() {
            var filter = getMenuFilter();
            if (filter != null) {
                return filter["TopCategorie"];
            }
            return undefined;
        }

        //function getDefaultShowChildsCategoryPosts() {
        //    var filter = getMenuFilter();
        //    if (filter != null) {
        //        if (filter["ShowChildsCategoriesPosts"] == "true")
        //            return true;
        //    }
        //    return false;
        //}

        function haveRole(role) {
            if (role != null && service.data != null && service.data.UserRoles != null) {
                for (var i = 0; i < service.data.UserRoles.length; i++) {
                    if (service.data.UserRoles[i] == role) {
                        return true;
                    }
                }
            }
            return false;
        }

        ///////////////////////

        function test() {
            getClaimCategory();
            getClaimCategory(null);
            getClaimCategory('4563');
            getClaimCategory('string');
            getClaimCategory('5');
            getClaimCategory('10');

            getClaimTag();
            getClaimTag(null);
            getClaimTag('4563');
            getClaimTag('string');
            getClaimTag('7');
            getClaimTag('18');

            getClaimGroup();
            getClaimGroup(null);
            getClaimGroup('4563');
            getClaimGroup('string');
            getClaimGroup('1');
            getClaimGroup('2');
        }
    };

    angular
        .module('app.services')
        .factory('siteConfData', siteConfData);

    siteConfData.$inject = ['$log', 'siteConf'];

    function siteConfData($log, siteConf) {
        $log.debug('Return siteConf promise...');
        return siteConf.get();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // TEST PURPOSE
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // AngularJS Corner - Using promises and $q to handle asynchronous calls:
    //      http://chariotsolutions.com/blog/post/angularjs-corner-using-promises-q-handle-asynchronous-calls/
    // Using Promises in AngularJS Views:
    //      http://markdalgleish.com/2013/06/using-promises-in-angularjs-views/
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    angular
        .module('app.services')
        .factory('greeting', greeting);

    greeting.$inject = ['$q', '$timeout', '$log'];
    
    function greeting($q, $timeout, $log) {
        return {
            get: get
        };

        function get() {
            $log.debug('Getting greeting...');
            var deferred = $q.defer();
            $timeout(function () {
                $log.debug('Got greeting.');
                deferred.resolve('Hello !');
            }, 5000);
            return deferred.promise;
        }
    };
})();