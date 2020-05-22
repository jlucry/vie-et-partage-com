(function () {
    'use strict';

    // Users controllers.
    angular
        .module('app.users')
        .controller('UsersController', UsersController);

    // User list controllers.
    UsersController.$inject = ['$rootScope', '$scope', '$state', '$timeout', '$log', '$mdDialog', '$mdSidenav', 'heads', 'navs', 'siteConf', 'siteConfData', 'users', 'user'];

    function UsersController($rootScope, $scope, $state, $timeout, $log, $mdDialog, $mdSidenav, heads, navs, siteConf, siteConfData, users, user) {
        $log.debug("UsersController...");

        /*jshint validthis: true */
        var vm = this;
        var allowReload = false;
        var showFilter = true;
        vm.Heads = heads;
        vm.Navs = navs;
        vm.Users = users;
        vm.User = user;
        vm.SideUserOpen = sideUserOpen;
        vm.SideUserClose = sideUserClose;
        vm.DialogSettingsOpen = dialogSettingsOpen;
        vm.OnPagePrev = onPagePrev;
        vm.OnPageChange = onPageChange;
        vm.OnPageNext = onPageNext;
        vm.IsAdmin = false;
        vm.SideMaxWidth = (vm.Heads.FullScreen == true) ? "100%" : "90%";
        vm.ShowFilter = showFilter;
        vm.OnGroupClick = onGroupClick;
        vm.SideUserOnCloseAction = 0;
        
        // Material case...
        if ($rootScope.uiRouter == false) {
            vm.CurrentPage = 0;
            vm.Groups = null;
            vm.SelectedGroupId = 0;
            vm.OnGroupChange = onGroupChange;

            // Waiting site configuration...
            $log.debug('UsersController: Waiting site configuration...');
            siteConfData.then(function (conf) {
                vm.Groups = siteConf.getClaimGroups();
                vm.IsAdmin = siteConf.IsAdmin;
                $log.debug('IsAdmin=', vm.IsAdmin);

                // Loading users...
                vm.Users.onlyValidedUser = false;
                vm.Users.gets(3).then(function (conf) {
                    var selectedGroup = vm.Users.getFilterGroup();
                    $log.debug('selectedGroup=', selectedGroup);
                    vm.SelectedGroupId = (selectedGroup == null) ? 0 : selectedGroup;
                    $log.debug('group=', vm.SelectedGroupIdSelectedMine);
                }, function (reason) { });

            }, function (reason) { });
        }

        // Set the reload function...
        heads.Reload = _reload;

        ///////////////////////

        function sideUserOpen(userId) {
            $mdSidenav(sId)
                .toggle()
                .then(function () {
                    $log.debug("toggle " + userId + " is done");
                    if (sId == 'UserEditSide') {
                        vm.User.get(userId);
                    }
                });
        }

        function sideUserClose(sId) {
            if (sId == 'UserEditSide') {
            }
            $mdSidenav(sId).close()
              .then(function () {
                  if (vm.SideUserOnCloseAction == 1) { }
                  else if (vm.SideUserOnCloseAction == 2) { }
                  else if (vm.SideUserOnCloseAction == 3) { onGroupChange(); }
                  vm.SideUserOnCloseAction = 0;
              });
        }

        function dialogSettingsOpen(ev) {
            sideUserOpen('UserSettingsSide', null);
        }

        function onPagePrev() {
            $log.debug('Load previous users page...');
            vm.Users.setSkip(vm.Users.getSkip() - 1);
            _reload(0);
        }

        function onPageChange(skip) {
            $log.debug('Load users page ', skip);
            vm.Users.setSkip(skip);
            _reload(0);
        }

        function onPageNext() {
            $log.debug('Load next users page...');
            vm.Users.setSkip(vm.Users.getSkip() + 1);
            _reload(0);
        }

        function onGroupClick(id) {
            if (id == vm.SelectedGroupId) {
                vm.SelectedGroupId = 0;
            }
            else {
                vm.SelectedGroupId = id;
            }
            vm.SidePostOnCloseAction = 3;
            sidePostClose('UserSettingsSide');
        }

        function onGroupChange() {
            var filter = vm.Users.getFilterGroup();
            if (filter == null) filter = 0;
            if (filter != vm.SelectedGroupId) {
                $log.debug('Group changed to', vm.SelectedGroupId);
                vm.Users.setFilterGroup((vm.SelectedGroupId == 0)
                    ? null : vm.SelectedGroupId);
                _reload(1);
            }
        }

        ///////////////////////

        function _reload(reset) {
            vm.Navs.LoadStart();
            vm.Users.gets(reset).then(function successCallback(response) {
                $log.debug('User reloaded (users controller).');
                if ($rootScope.uiRouter == false) {
                    var selectedGroup = vm.Users.getFilterGroup();
                    $log.debug('_reload: group=', selectedGroup);

                    vm.CurrentPage = vm.Users.currentPage();
                    vm.SelectedGroupId = (selectedGroup == null) ? 0 : selectedGroup;
                    $log.debug('_reload: group=', vm.SelectedGroupId);
                }
                vm.Navs.LoadEnd();
            }, function errorCallback(response) {
                $log.error('Failed to reload users (users controller).');
                vm.Navs.LoadEnd();
            });
        }

        ///////////////////////

        $rootScope.$on('$viewContentLoading',
            function (event, viewConfig) {
                // Access to all the view config properties.
                // and one special property 'targetView'
                // viewConfig.targetView 
                $log.debug('Users view loading:', event, viewConfig);
            });

        $scope.$on('$viewContentLoaded',
            function (event) {
                $log.debug('Users view loaded:', event);
                vm.Navs.LoadEnd();
            });
    };

})();