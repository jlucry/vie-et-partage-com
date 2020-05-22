// Services definition.
(function () {
    'use strict';

    angular
        .module('app.services')
        .factory('regions', regions);

    regions.$inject = ['$rootScope', '$log', 'siteConf', 'siteConfData'];

    function regions($rootScope, $log, siteConf, siteConfData) {
        var current = getByIndex(0);
        var service = {
            gets: gets,
            getById: getById,
            getByIndex: getByIndex,
            getNameById: getNameById,
            getCurrent: getCurrent,
            getCurrentName: getCurrentName,
            setCurrent: setCurrent
        };
        $log.debug('regions: siteConf=', siteConf);
        $log.debug('regions: Current region=', service.getCurrent());

        // Waiting site configuration...
        if ($rootScope.uiRouter == false) {
            $log.debug('regions: Waiting site configuration...');
            siteConfData.then(function (conf) {
                //$log.debug('regions:conf=', conf);
                current = getByIndex(0);
                $log.debug('regions: Current region=', service.getCurrent());
            }, function (reason) { });
        }

        // Return the service...
        return service;

        ///////////////////////

        function gets() {
            if (siteConf == null || siteConf.data == null) {
                return null;
            }
            return siteConf.data.Regions;
        };

        // TODO: REVOIR LES FONCTIONS GET....

        function getById(id) {
            var reg = null;
            if (siteConf == null || siteConf.data == null || siteConf.data.Regions == null
                || id == null || id == '0') {
                return null;
            }
            $.each(siteConf.data.Regions, function (index, sub) {
                if (sub.Id == id) {
                    reg = sub;
                }
            });
            $log.debug('Region(' + id + ')=', reg);
            return reg;
        };

        function getByIndex(index) {
            if (siteConf == null || siteConf.data == null || siteConf.data.Regions == null
                || siteConf.data.Regions.length == 0 || index > (siteConf.data.Regions.length - 1)) {
                return null;
            }
            var reg = siteConf.data.Regions[index];
            $log.debug('Region[' + index + ']=', reg);
            return reg;
        };

        function getNameById(id) {
            var reg = getById(id);
            if (reg == null) {
                return null;
            }
            $log.debug('Region(' + id + ').StringValue=', reg.StringValue);
            return reg.StringValue;
        };

        function getCurrent() {
            return current;
        };

        function getCurrentName() {
            if (current == null || current.StringValue == null) {
                return "??";
            }
            return current.StringValue;
        };

        function setCurrent(region) {
            current = region;
        };
    };
})();