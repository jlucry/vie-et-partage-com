// Services definition.
(function () {
    'use strict';

    angular
        .module('app.users.service')
        .factory('users', users);

    users.$inject = ['$rootScope', '$http', '$log', 'heads', 'regions'];

    // A RESTful factory for retrieving users
    function users($rootScope, $http, $log, heads, regions) {
        var data = null;
        var settings = null;
        var pagination = [];
        var lastPage = -1;
        var service = {
            settings: settings,
            data: data,

            gets: gets,

            getSkip: getSkip,
            setSkip: setSkip,
            getTake: getTake,
            setTake: setTake,
            getFilterGroup: getFilterGroup,
            setFilterGroup: setFilterGroup,

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
                    $log.debug('Reset user list level 1...');
                    service.settings.Skip = 0;
                    service.settings.Count = 0;
                    service.pagination = [];
                }
                if (reset >= 2) {
                    $log.debug('Reset user list level 2...');
                    //setFilterState("-1"); -- Don't change the current state event if we change the region!!!
                }
                if (reset >= 3) {
                    $log.debug('Reset user list level 3...');
                    setFilterGroup("");
                }
            }
            // Add the search filter...
            setFilter("title", heads.search);

            // Querying...
            $log.debug('Getting users...');
            var path = '/api/user/list?region=' + regions.getCurrentName();
            $log.debug('Getting users with path=', path, ' and filters=', service.settings);

            return $http.post(path, service.settings)
                .then(getComplete)
                .catch(getFailed);

            function getComplete(response) {
                $log.debug('users got.');
                service.data = response.data.Users;
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
                        $log.debug(nbPage + ' pages of users added (' + floatPage + ',' + intPage + ').');
                    }
                    $log.debug('pagination=', service.pagination);
                }
                return service/*.data*/;
            }

            function getFailed(error) {
                $log.error('Faile to get users: ' + error.data);
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
        
        function getFilterGroup() {
            return getFilter("group");
        }
        function setFilterGroup(value) {
            setFilter("group", value);
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

})();