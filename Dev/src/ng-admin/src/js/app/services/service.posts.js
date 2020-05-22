// Services definition.
(function () {
    'use strict';

    angular
        .module('app.posts.service')
        .factory('posts', posts);

    posts.$inject = ['$rootScope', '$http', '$log', 'heads', 'regions', 'siteConf'];

    // A RESTful factory for retrieving posts
    function posts($rootScope, $http, $log, heads, regions, siteConf) {
        var data = null;
        var onlyValidedPost = true;
        var settings = null;
        var pagination = [];
        var lastPage = -1;
        var service = {
            settings: settings,
            data: data,

            onlyValidedPost: onlyValidedPost,

            gets: gets,

            getSkip: getSkip,
            setSkip: setSkip,
            getTake: getTake,
            setTake: setTake,
            getFilterState: getFilterState,
            setFilterState: setFilterState,
            getFilterCategory: getFilterCategory,
            setFilterCategory: setFilterCategory,
            getFilterTag: getFilterTag,
            setFilterTag: setFilterTag,
            getFilterGroup: getFilterGroup,
            setFilterGroup: setFilterGroup,
            getFilterMine: getFilterMine,
            setFilterMine: setFilterMine,

            pagination: pagination,
            currentPage: currentPage,
            lastPage: lastPage,
            isFirstPage: isFirstPage,
            isCurrentPage: isCurrentPage,
            isLastPage: isLastPage,
        };
        return service;

        ///////////////////////

        function gets(reset) {
            service.data = null;
            // Reset settings...
            if (service.settings != null && reset > 0) {
                if (reset >= 1) {
                    $log.debug('Reset post list level 1...');
                    service.settings.Skip = 0;
                    service.settings.Count = 0;
                    service.pagination = [];
                }
                if (reset >= 2) {
                    $log.debug('Reset post list level 2...');
                    //setFilterState("-1"); -- Don't change the current state event if we change the region!!!
                }
                if (reset >= 3) {
                    $log.debug('Reset post list level 3...');
                    setFilterState("-1");
                    setFilterCategory("");
                    setFilterTag("");
                    setFilterGroup("");
                }
            }
            // Add the search filter...
            setFilter("title", heads.search);

            // Apply settings...
            if (service.onlyValidedPost == true) {
                setFilterState("1");
            }

            // Apply default filters...
            var DefaultFilters = siteConf.getMenuFilter();
            $log.debug('@@@@@@@@@@@@@@DefaultFilters=', DefaultFilters);
            if (DefaultFilters != null) {
                if (service.settings == null)
                    service.settings = {};
                service.settings.DefaultFilters = DefaultFilters;
            }
            else if (DefaultFilters == null && service.settings != null) {
                service.settings.DefaultFilters = null;
            }

            // Querying...
            $log.debug('Getting posts...');
            var path = '/api/post/list?region=' + regions.getCurrentName();
            $log.debug('Getting posts with path=', path, ', filters=', service.settings);

            return $http.post(path, service.settings)
                .then(getComplete)
                .catch(getFailed);

            function getComplete(response) {
                $log.debug('Post got.');
                service.data = response.data.Posts;
                service.settings = response.data.Settings;
                $log.debug('data=', service.data, 'settings=', service.settings);
                // Set pagination...
                if (service.settings.Skip == 0
                    && service.settings.Take != 0
                    && service.settings.Count != 0) {
                    $log.debug('Setting list pagination...');
                    service.pagination = [];
                    var floatPage = service.settings.Count / service.settings.Take;
                    var intPage = parseInt(floatPage, 10);
                    if (intPage != NaN) {
                        var nbPage = intPage;
                        if ((floatPage - nbPage) != 0) { nbPage += 1; }
                        service.lastPage = nbPage - 1;
                        for (var pDx = 0; pDx <= service.lastPage; pDx += 1) {
                            service.pagination.push(pDx);
                            //angular.merge(service.pagination, pDx);
                        }
                        $log.debug(nbPage + ' pages of posts added (' + floatPage + ',' + intPage + ').');
                    }
                    $log.debug('pagination=', service.pagination);
                }
                // Test...
                //$log.debug('posts::getComplete: ===========');
                //debugFilters();
                //$log.debug('posts::getComplete: ===========');
                //getFilter();
                //getFilter(null);
                //getFilter('null');
                //getFilter('State');
                //$log.debug('posts::getComplete: ===========');
                //setFilter("kk1", "vv1");
                //debugFilters();
                //$log.debug('posts::getComplete: ===========');
                //setFilter("kk2", "vv2");
                //debugFilters();
                //$log.debug('posts::getComplete: ===========');
                //setFilter("kk1", "vv1bis");
                //debugFilters();
                return service/*.data*/;
            }

            function getFailed(error) {
                $log.error('Faile to get posts: ' + error.data);
            }
        }
        
        function setSkip(value) {
            if (value != null && value >= 0 && value <= service.lastPage) {
                service.settings.Skip = value;
                $log.debug('Skip set to ' + service.settings.Skip);
            }
        }
        function getSkip() {
            return service.settings.Skip;
        }

        function setTake(value) {
            service.settings.Take = value;
            $log.debug('Take set to ' + service.settings.Take);
        }
        function getTake(value) {
            return service.settings.Take;
        }

        function getFilterState() {
            return getFilter("state");
        }
        function setFilterState(value) {
            setFilter("state", value);
        }

        function getFilterCategory() {
            return getFilter("categorie");
        }
        function setFilterCategory(value) {
            setFilter("categorie", value);
        }
        
        function getFilterTag() {
            return getFilter("tag");
        }
        function setFilterTag(value) {
            setFilter("tag", value);
        }
        
        function getFilterGroup() {
            return getFilter("group");
        }
        function setFilterGroup(value) {
            setFilter("group", value);
        }
        
        function getFilterMine() {
            var val = getFilter("mine");
            return (val == null || val != "true") ? false : true;
        }
        function setFilterMine(value) {

            setFilter("mine", (value == true) ? "true" : "false");
        }

        function currentPage() {
            return (service.settings != null)
                ? service.settings.Skip
                : 0;
        }

        function isFirstPage() {
            if (service.settings != null
                && service.settings.Skip == 0) {
                return true;
            }
            return false;
        }

        function isCurrentPage(value) {
            if (value != null
                && service.settings != null
                && service.settings.Skip == value) {
                return true;
            }
            else if (service.pagination == null 
                || service.pagination.length == 0) {
                return true;
            }
            return false;
        }

        function isLastPage() {
            if (service.settings != null
                && service.settings.Skip == service.lastPage) {
                return true;
            }
            else if (service.pagination == null
                || service.pagination.length == 0) {
                return true;
            }
            return false;
        }

        ///////////////////////

        function debugFilters() {
            angular.forEach(service.settings.Filters, function (value, key) {
                $log.debug('Filter(' + key + ')=' + value);
            });
        }
        
        function getFilter(key) {
            var outValue = null;
            if (key != null) {
                if (service.settings != null && service.settings.Filters != null) {
                    outValue = service.settings.Filters[key];
                }
                if (outValue != null) {
                    $log.debug('Filter(' + key + ')=' + outValue);
                }
                else {
                    $log.debug('Filter(' + key + ') not found!');
                }
            }
            return outValue;
        }

        function setFilter(key, value) {
            if (key != null) {
                if (service.settings != null && service.settings.Filters != null) {
                    service.settings.Filters[key] = value;
                    $log.debug('Filter(' + key + ') set to ' + value);
                }
                else {
                    $log.debug('Filter(' + key + ') not set!');
                }
            }
        }
    };

    //$.each(service.settings.Filters, function (index, sub) {
    //    $log.debug('posts::debugFilters($.each): ' + index + '=' + sub);
    //});
    //$.each(service.settings.Filters, function (index, sub) {
    //    if (index == key) {
    //        outValue = sub;
    //    }
    //});
    //angular.forEach(service.settings.Filters, function (value, key) {
    //    if (key == inKey) {
    //        outValue = value;
    //    }
    //});
    //var set = false;
    //if (set == false) {
    //    angular.merge(service.settings.Filters, angular.fromJson('{"' + key + '":"' + value + '"}'));
    //    //service.settings.Filters.push(key + ': ' + value);
    //    $log.debug('posts::setFilters: ' + key + ' added.');
    //}
})();

/*angular.module('uiRouterSample.utils.service', [])
.factory('utils', function () {
  return {
    // Util for finding an object by its 'id' property among an array
    findById: function findById(a, id) {
      for (var i = 0; i < a.length; i++) {
        if (a[i].id == id) return a[i];
      }
      return null;
    }
  };
});*/