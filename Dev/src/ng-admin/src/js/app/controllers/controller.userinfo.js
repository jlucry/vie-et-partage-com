(function () {
    'use strict';

    // Menus controller.
    angular
        .module('app')
        .controller('UserInfoController', UserInfoController);

    UserInfoController.$inject = ['$rootScope', '$scope', '$state', '$location', '$log', '$window', 'siteConf', 'siteConfData'];

    function UserInfoController($rootScope, $scope, $state, $location, $log, $window, siteConf, siteConfData) {
        /*jshint validthis: true */
        var vm = this;
        vm.UserName = '';
        vm.UserGroup = '';
        vm.UserImg = '';
        vm.Logout = logout;
        vm.Manage = manage;
        $log.debug('UserInfoController: siteConf=', siteConf);

        ///////////////////////

        _Init();
        // Waiting site configuration...
        if ($rootScope.uiRouter == false) {
            $log.debug('UserInfoController: Waiting site configuration...');
            siteConfData.then(function (conf) {
                $log.debug('UserInfoController:conf=', conf);
                _Init();
            }, function (reason) { });
        }

        ///////////////////////

        function logout() {
            $window.location.href = '/account/AdminLogOff';
        }

        function manage() {
            $window.location.href = '/Manage/Index';
        }

        ///////////////////////

        function _Init() {
            if (siteConf.data != null) {
                vm.UserImg = siteConf.data.UserImg;
                vm.UserName = siteConf.data.UserName;
                if (siteConf.data.UserRoles != null) {
                    for (var i = 0; i < siteConf.data.UserRoles.length; i++) {
                        if (siteConf.data.UserRoles[i] != null) {
                            vm.UserGroup += (siteConf.data.UserRoles[i] + " ");
                        }
                    }
                }
            }
        }
    };
})();