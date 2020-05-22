// Services definition.
(function () {
    'use strict';

    angular
        .module('app.services')
        .factory('heads', heads);

    heads.$inject = ['$rootScope', '$mdMedia', '$log'];

    function heads($rootScope, $mdMedia, $log) {
        var viewLoadedCount = 0;
        var search = "";
        var fullScreen = $mdMedia('xs') || $mdMedia('sm');
        var service = {
            search: search,
            FullScreen: fullScreen,
            Reload: null
        };
        $log.debug('FullScreen=', fullScreen);
        return service;
    };
})();