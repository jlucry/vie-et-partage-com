(function () {
    'use strict';

    // Menus controller.
    angular
        .module('app')
        .controller('RegionsController', RegionsController);

    RegionsController.$inject = ['$rootScope', '$scope', '$state', '$location', '$log', 'heads', 'regions', 'siteConfData'];

    function RegionsController($rootScope, $scope, $state, $location, $log, heads, regions, siteConfData) {
        /*jshint validthis: true */
        var vm = this;
        vm.Regions = regions.gets();
        vm.Current = regions.getCurrent();
        vm.ChangeRegion = changeRegion;
        vm.OnChange = null;
        $log.debug('RegionsController: Regions=', vm.Regions, ', Current=', vm.Current);

        ///////////////////////

        // Waiting site configuration...
        if ($rootScope.uiRouter == false) {
            $log.debug('RegionsController: Waiting site configuration...');
            siteConfData.then(function (conf) {
                vm.Regions = regions.gets();
                vm.Current = regions.getCurrent();
                if (regions.OnChange != null) {
                    regions.OnChange(vm.Current);
                }
                $log.debug('RegionsController:(Wait) Regions=', vm.Regions, ', Current=', vm.Current);
            }, function (reason) { });
        }

        ///////////////////////

        function changeRegion(region) {
            regions.setCurrent(region);
            vm.Current = regions.getCurrent();
            if (regions.OnChange != null) {
                regions.OnChange(vm.Current);
            }
            $log.debug('Change region to:', vm.Current);
            // Reload the page...
            heads.Reload(2);
        }

    };
})();