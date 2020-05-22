// Services definition.
(function () {
    'use strict';

    angular
        .module('app.services')
        .factory('navs', navs);

    navs.$inject = ['$timeout', '$log'];

    function navs($timeout, $log) {
        var service = {
            ShowProgress: true,
            LoadStart: loadStart,
            LoadEnd: loadEnd,
        };
        return service;

        ///////////////////////

        function loadStart() {
            $log.debug('[NAVS]loadStart...');
            service.ShowProgress = true;
        };

        function loadEnd() {
            $log.debug('[NAVS]loadEnd...');
            //service.ShowProgress = false;
            $timeout(function () {
                service.ShowProgress = false;
                $log.debug('[NAVS]loadEnd+++++++++++++++++++');
            }, 100);
        };
    };
})();