(function () {
    'use strict';

    // Posts controllers.
    angular
        .module('app.posts')
        .controller('PostsController', PostsController);

    // Post list controllers.
    PostsController.$inject = ['$rootScope', '$scope', '$route', '$state', '$timeout', '$log', '$mdDialog', '$mdSidenav', '$mdBottomSheet', '$mdToast', /*'$mdMedia',*/ 'heads', 'navs', 'siteConf', 'siteConfData', 'posts', 'post'];

    function PostsController($rootScope, $scope, $route, $state, $timeout, $log, $mdDialog, $mdSidenav, $mdBottomSheet, $mdToast, /*$mdMedia,*/ heads, navs, siteConf, siteConfData, posts, post) {
        $log.debug("PostsController...");

        /*jshint validthis: true */
        var vm = this;
        var allowReload = false;
        var showFilter = true;
        var sidePostOnCloseAction = 0;
        //var filterToShow = 0;
        vm.Heads = heads;
        vm.Navs = navs;
        vm.Posts = posts;
        vm.Post = post;
        //vm.ClassTitle = classTitle;
        //vm.ClassPagePrev = classPagePrev;
        //vm.ClassPage = classPage;
        //vm.ClassPageNext = classPageNext;
        vm.StyleTitle = styleTitle;
        //vm.TabSelectedIndex = _stateToTab();
        vm.SidePostOpen = sidePostOpen;
        vm.SidePostClose = sidePostClose;
        vm.DialogSettingsOpen = dialogSettingsOpen;
        //vm.OnTabSelect = onTabSelect;
        //vm.OnTabDeSelect = onTabDeSelect;
        vm.OnPagePrev = onPagePrev;
        vm.OnPageChange = onPageChange;
        vm.OnPageNext = onPageNext;
        vm.IsAdmin = false/*siteConf.IsAdmin*/;
        vm.IsPub = false/*siteConf.IsPub*/;
        vm.IsWriter = false/*siteConf.IsWriter*/;
        vm.IsContributor = false/*siteConf.IsAdmin || siteConf.IsPub || siteConf.IsWriter*/;
        vm.SideMaxWidth = (vm.Heads.FullScreen == true) ? "100%" : "90%";
        vm.ShowFilter = showFilter;
        //vm.FilterToShow = filterToShow;
        vm.CatMarginLeft = catMarginLeft;
        vm.OnStateClick = onStateClick;
        vm.OnCategoryClick = onCategoryClick;
        vm.OnTagClick = onTagClick;
        vm.OnGroupClick = onGroupClick;
        vm.OnMineClick = onMineClick;
        vm.SidePostOnCloseAction = sidePostOnCloseAction;
        vm.Type = 1;

        /*test*//*
        vm.hidden = false;
        vm.isOpen = false;
        vm.hover = false;
        // On opening, add a delayed property which shows tooltips after the speed dial has opened
        // so that they have the proper position; if closing, immediately hide the tooltips
        $scope.$watch('vm.isOpen', function(isOpen) {
            if (isOpen) {
                $timeout(function() {
                    vm.tooltipVisible = isOpen;
                }, 600);
            } else {
                vm.tooltipVisible = isOpen;
            }
        });

        vm.items = [
          { name: "Twitter", icon: "img/icons/twitter.svg", direction: "bottom" },
          { name: "Facebook", icon: "img/icons/facebook.svg", direction: "top" },
          { name: "Google Hangout", icon: "img/icons/hangout.svg", direction: "bottom" }
        ];

        vm.openDialog = function ($event, item) {
            // Show the dialog
        }
        *//*test ent*/

        //$log.debug("$route.current.params=", $route.current.params);
        //$log.debug("$route.current.params.type=", $route.current.params.type);
        if ($route.current.params.type == "calendar") {
            vm.Type = 2;
        }

        // Material case...
        if ($rootScope.uiRouter == false) {
            vm.CurrentPage = 0;
            //vm.OnCurrentPageChange = onCurrentPageChange;

            vm.Cats = null;
            vm.Tags = null;
            vm.Groups = null;
            vm.SelectedCategoryId = 0;
            vm.SelectedTagId = 0;
            vm.SelectedGroupId = 0;
            vm.SelectedState = 1;
            vm.SelectedMine = 0;
            vm.OnCategoryChange = onCategoryChange;
            vm.OnTagChange = onTagChange;
            vm.OnGroupChange = onGroupChange;
            vm.OnStateChange = onStateChange;
            vm.OnMineChange = onMineChange;

            // Waiting site configuration...
            $log.debug('PostsController: Waiting site configuration...');
            siteConfData.then(function (conf) {
                vm.Cats = siteConf.getClaimCategories();
                vm.Tags = siteConf.getClaimTags();
                vm.Groups = siteConf.getClaimGroups();
                vm.IsAdmin = siteConf.IsAdmin;
                vm.IsPub = siteConf.IsPub;
                vm.IsWriter = siteConf.IsWriter;
                vm.IsContributor = siteConf.IsAdmin || siteConf.IsPub || siteConf.IsWriter;
                $log.debug('IsAdmin, IsPub, IsWriter, IsContributor=', vm.IsAdmin, vm.IsPub, vm.IsWriter, vm.IsContributor);

                // Loading posts...
                vm.Posts.onlyValidedPost = false;
                vm.Posts.gets(3).then(function (conf) {
                    var selectedCategory = vm.Posts.getFilterCategory();
                    var selectedTag = vm.Posts.getFilterTag();
                    var selectedGroup = vm.Posts.getFilterGroup();
                    var selectedState = vm.Posts.getFilterState();
                    var selectedMine = vm.Posts.getFilterMine();
                    $log.debug('selectedCategory, selectedTag, selectedGroup=', selectedCategory, selectedTag, selectedGroup);
                    vm.SelectedCategoryId = (selectedCategory == null) ? 0 : selectedCategory;
                    vm.SelectedTagId = (selectedTag == null) ? 0 : selectedTag;
                    vm.SelectedGroupId = (selectedGroup == null) ? 0 : selectedGroup;
                    vm.SelectedState = (selectedState == null) ? 1 : selectedState;
                    vm.SelectedMine = (selectedMine == null) ? 0 : selectedMine;
                    $log.debug('cat, tag, group, state, mine=', vm.SelectedCategoryId, vm.SelectedTagId, vm.SelectedGroupId, vm.SelectedState, vm.SelectedMine);
                }, function (reason) { });

            }, function (reason) { });
        }

        // Set the reload function...
        heads.Reload = _reload;

        ///////////////////////

        //function classTitle(postElmt) {
        //    if (postElmt != null && postElmt.State == 0) {
        //        return "label label-lg-off label-round-off label-primary";
        //    }
        //    else if (postElmt != null && postElmt.State == 2) {
        //        return "label label-lg-off label-round-off label-danger";
        //    }
        //    return "";
        //}

        //function classPagePrev(value) {
        //    if (vm.Posts != null && vm.Posts.isFirstPage() == true) {
        //        return "disabled";
        //    }
        //    return "";
        //}

        //function classPage(page) {
        //    if (vm.Posts != null && vm.Posts.isCurrentPage(page) == true) {
        //        return "active";
        //    }
        //    return "";
        //}

        //function classPageNext(value) {
        //    if (vm.Posts != null && vm.Posts.isLastPage() == true) {
        //        return "disabled";
        //    }
        //    return "";
        //}

        function styleTitle(postElmt) {
            if (postElmt != null && postElmt.State == 0) {
                return "color:blue;";
            }
            else if (postElmt != null && postElmt.State == 2) {
                return "color:red;";
            }
            return "";
        }

        function sidePostOpen(sId, postId) {
            //$log.debug("editPost " + postId/*, $mdSidenav*/);
            //$log.debug("isOpen(PostEditSide)=" + $mdSidenav('PostEditSide').isOpen());
            //$log.debug("isLockedOpen(PostEditSide)=" + $mdSidenav('PostEditSide').isLockedOpen());
            //PostHideActions($rootScope.uiRouter);
            $mdSidenav(sId)
                .toggle()
                .then(function () {
                    $log.debug("toggle " + postId + " is done");
                    if (sId == 'PostEditSide') {
                        vm.Post.get(postId);
                    }
                });
        }

        function sidePostClose(sId) {
            if (sId == 'PostEditSide') {
            }
            $mdSidenav(sId).close()
              .then(function () {
                    //$log.debug("close RIGHT is done");
                    //$("#btnPostsAdd").show();
                  //$("#btnPostsSettings").show();

                  if (vm.SidePostOnCloseAction == 1) { onCategoryChange(); }
                  else if (vm.SidePostOnCloseAction == 2) { onTagChange(); }
                  else if (vm.SidePostOnCloseAction == 3) { onGroupChange(); }
                  else if (vm.SidePostOnCloseAction == 4) { onStateChange(); }
                  else if (vm.SidePostOnCloseAction == 5) { onMineChange(); }
                  vm.SidePostOnCloseAction = 0;
              });
        }

        function dialogSettingsOpen(ev) {
            sidePostOpen('PostSettingsSide', null);
            /*$scope.alert = '';
            $mdBottomSheet.show({
                template: '<md-bottom-sheet class="md-grid bottomSheetdemoBasicUsage" layout="column">\
                <div layout="row" layout-align="center center" ng-cloak>\
                    <h4>Sélectionner le filtre à appliquer:</h4>\
                </div>\
                <div ng-cloak>\
                <md-list flex layout="row" layout-align="center center">\
                  <md-list-item ng-if="vm.Cats != null">\
                    <div>\
                      <md-button class="md-grid-item-content" ng-click="vm.listItemClick(0)">\
                        <md-icon md-font-set="material-icons">turned_in</md-icon>\
                        <div class="md-grid-text">Cat&#233;gories</div>\
                      </md-button>\
                    </div>\
                  </md-list-item>\
                  <md-list-item ng-if-off="vm.Tags != null">\
                    <div>\
                      <md-button class="md-grid-item-content" ng-click="vm.listItemClick(1)">\
                        <md-icon md-font-set="material-icons">label</md-icon>\
                        <div class="md-grid-text">Tags</div>\
                      </md-button>\
                    </div>\
                  </md-list-item>\
                  <md-list-item ng-if-off="vm.Groups != null">\
                    <div>\
                      <md-button class="md-grid-item-content" ng-click="vm.listItemClick(2)">\
                        <md-icon md-font-set="material-icons">group</md-icon>\
                        <div class="md-grid-text">Groupes</div>\
                      </md-button>\
                    </div>\
                  </md-list-item>\
                  <md-list-item ng-if="vm.IsAdmin == true || vm.IsPub == true">\
                    <div>\
                      <md-button class="md-grid-item-content" ng-click="vm.listItemClick(3)">\
                        <md-icon md-font-set="material-icons">visibility</md-icon>\
                        <div class="md-grid-text">Publication</div>\
                      </md-button>\
                    </div>\
                  </md-list-item>\
                </md-list>\
                </div>\
                </md-bottom-sheet>',
                controller: 'GridBottomSheetCtrl',
                controllerAs: 'vm',
                clickOutsideToClose: true,
                parent: angular.element(document.body),//angular.element(document.getElementById('elmBody'))
            }).then(function(clickedItem) {
                vm.FilterToShow = clickedItem;
                sidePostOpen('PostSettingsSide', null);
                $mdToast.show(
                    $mdToast.simple()
                        .textContent(clickedItem + ' clicked!')
                        .position('top right')
                        .hideDelay(1500)
                );
            });*/
            //if (1 == 1) {
            //    $mdDialog.show({
            //        contentElement: '#myDialog',
            //        parent: angular.element(document.body),
            //        targetEvent: ev,
            //        clickOutsideToClose: true
            //    });
            //}
            //else if (1 == 1) {
            //    if (vm.ShowFilter == true) vm.ShowFilter = false;
            //    else vm.ShowFilter = true;
            //    if (vm.ShowFilter == true) {
            //        // TODO: Scrool to top
            //    }
            //}
            //else {
            //    PostHideActions($rootScope.uiRouter);
            //    //$log.debug('PostsController::showSettingsDialog: ev=', ev);
            //    $mdDialog.show({
            //        controller: 'PostSettingsController',
            //        controllerAs: 'ctrl',
            //        templateUrl: '/admin/posts.settings',
            //        //parent: angular.element(document.body),
            //        //parent: angular.element("#contentDiv"),
            //        //openFrom: '#btnPostsSettings',
            //        targetEvent: ev,
            //        clickOutsideToClose: false,
            //        fullscreen: vm.Heads.FullScreen
            //    })
            //    .then(function (answer) {
            //        $log.debug('Got dialog applied.');
            //        //vm.Posts.setFilterCategory(answer[0]);
            //        //vm.Posts.setFilterTag(answer[1]);
            //        //vm.Posts.setFilterGroup(answer[2]);
            //        //vm.Posts.setSkip(0);
            //        _reload(1);
            //        PostShowActions($rootScope.uiRouter);
            //    }, function () {
            //        $log.debug('Got dialog cancelled.');
            //        PostShowActions($rootScope.uiRouter);
            //    });
            //    //$scope.$watch(function () {
            //    //    return $mdMedia('xs') || $mdMedia('sm');
            //    //}, function (wantsFullScreen) {
            //    //    //vm.customFullscreen = (wantsFullScreen === true);
            //    //});
            //}
        };
        //function onTabSelect(postState) {
        //    // Reload the posts...
        //    if (allowReload == true) {
        //        $log.debug('Posts reload alowed.');
        //        allowReload = false;
        //        //_tabToState(vm.TabSelectedIndex);
        //        vm.Posts.setSkip(0);
        //        _reload(1);
        //    }
        //    else {
        //        $log.debug('Posts reload not alowed.');
        //    }
        //}
        //function onTabDeSelect(postState) {
        //    allowReload = true;
        //}

        function onPagePrev() {
            $log.debug('Load previous posts page...');
            vm.Posts.setSkip(vm.Posts.getSkip() - 1);
            _reload(0);
        }

        function onPageChange(skip) {
            $log.debug('Load posts page ', skip);
            vm.Posts.setSkip(skip);
            _reload(0);
        }

        function onPageNext() {
            $log.debug('Load next posts page...');
            vm.Posts.setSkip(vm.Posts.getSkip() + 1);
            _reload(0);
        }

        //function onCurrentPageChange() {
        //    $log.debug('Load posts page ', vm.CurrentPage);
        //    vm.Posts.setSkip(vm.CurrentPage);
        //    _reload(0);
        //}

        function onStateClick() {
            vm.SidePostOnCloseAction = 4;
            sidePostClose('PostSettingsSide');
        }

        function onCategoryClick(id) {
            if (id == vm.SelectedCategoryId) {
                vm.SelectedCategoryId = 0;
            }
            else {
                vm.SelectedCategoryId = id;
            }
            vm.SidePostOnCloseAction = 1;
            sidePostClose('PostSettingsSide');
        }

        function onTagClick(id) {
            if (id == vm.SelectedTagId) {
                vm.SelectedTagId = 0;
            }
            else {
                vm.SelectedTagId = id;
            }
            vm.SidePostOnCloseAction = 2;
            sidePostClose('PostSettingsSide');
        }

        function onGroupClick(id) {
            if (id == vm.SelectedGroupId) {
                vm.SelectedGroupId = 0;
            }
            else {
                vm.SelectedGroupId = id;
            }
            vm.SidePostOnCloseAction = 3;
            sidePostClose('PostSettingsSide');
        }

        function onMineClick(id) {
            vm.SidePostOnCloseAction = 5;
            sidePostClose('PostSettingsSide');
        }

        function onCategoryChange() {
            var filter = vm.Posts.getFilterCategory();
            if (filter == null) filter = 0;
            if (filter != vm.SelectedCategoryId) {
                $log.debug('Cat changed to', vm.SelectedCategoryId);
                vm.Posts.setFilterCategory((vm.SelectedCategoryId == 0)
                    ? null : vm.SelectedCategoryId);
                _reload(1);
            }
        }

        function onTagChange() {
            var filter = vm.Posts.getFilterTag();
            if (filter == null) filter = 0;
            if (filter != vm.SelectedTagId) {
                $log.debug('Tag changed to', vm.SelectedTagId);
                vm.Posts.setFilterTag((vm.SelectedTagId == 0)
                    ? null : vm.SelectedTagId);
                _reload(1);
            }
        }

        function onGroupChange() {
            var filter = vm.Posts.getFilterGroup();
            if (filter == null) filter = 0;
            if (filter != vm.SelectedGroupId) {
                $log.debug('Group changed to', vm.SelectedGroupId);
                vm.Posts.setFilterGroup((vm.SelectedGroupId == 0)
                    ? null : vm.SelectedGroupId);
                _reload(1);
            }
        }

        function onStateChange() {
            if (vm.Posts.getFilterState() != vm.SelectedState) {
                $log.debug('State changed to', vm.SelectedState);
                vm.Posts.setFilterState(vm.SelectedState);
                _reload(1);
            }
        }

        function onMineChange() {
            if (vm.Posts.getFilterMine() != vm.SelectedMine) {
                $log.debug('Mine changed to', vm.SelectedMine);
                vm.Posts.setFilterMine(vm.SelectedMine);
                _reload(1);
            }
        }

        ///////////////////////

        function catMarginLeft(cat) {
            if (cat != null) {
                return (cat.Deep * 20) + "px";
            }
            return "0px"
        }

        ///////////////////////

        //function _tabToState(tabIndex) {
        //    var state = "-1";
        //    if (tabIndex == 1) {
        //        state = "1";
        //    }
        //    else if (tabIndex == 2) {
        //        state = "0";
        //    }
        //    else if (tabIndex == 3) {
        //        state = "2";
        //    }
        //    $log.debug('_tabToState:(' + tabIndex + ')=' + state);
        //    vm.Posts.setFilterState(state);
        //}

        //function _stateToTab() {
        //    var tabIndex = 1;
        //    // Get state...
        //    var state = null;
        //    if (vm.Posts != null) {
        //        state = vm.Posts.getFilterState();
        //    }
        //    if (state == null) {
        //        state = "-1";
        //    }
        //    // State => tab index...
        //    if (state == "0") {
        //        tabIndex = 2;
        //    }
        //    else if (state == "1") {
        //        tabIndex = 1;
        //    }
        //    else if (state == "2") {
        //        tabIndex = 3;
        //    }
        //    $log.debug('_stateToTab(' + state + ')=' + tabIndex);
        //    return tabIndex;
        //}

        function _reload(reset) {
            vm.Navs.LoadStart();
            vm.Posts.gets(reset).then(function successCallback(response) {
                $log.debug('Post reloaded (posts controller).');
                if ($rootScope.uiRouter == false) {
                    var selectedCategory = vm.Posts.getFilterCategory();
                    var selectedTag = vm.Posts.getFilterTag();
                    var selectedGroup = vm.Posts.getFilterGroup();
                    $log.debug('_reload: cat, tag, group=', selectedCategory, selectedTag, selectedGroup);

                    vm.CurrentPage = vm.Posts.currentPage();
                    vm.SelectedCategoryId = (selectedCategory == null) ? 0 : selectedCategory;
                    vm.SelectedTagId = (selectedTag == null) ? 0 : selectedTag;
                    vm.SelectedGroupId = (selectedGroup == null) ? 0 : selectedGroup;
                    $log.debug('_reload: cat, tag, group=', vm.SelectedCategoryId, vm.SelectedTagId, vm.SelectedGroupId);
                }
                vm.Navs.LoadEnd();
            }, function errorCallback(response) {
                $log.error('Failed to reload posts (posts controller).');
                vm.Navs.LoadEnd();
            });
        }

        ///////////////////////

        //$rootScope.$on('$stateChangeStart',
        //    function (event, toState, toParams, fromState, fromParams) {
        //        $log.debug('Reset posts filters on state changed to', toState, "from", fromState);
        //        if (toState.name == "home.index"
        //            || toState.name == "home.post")
        //        {
        //            if (toState.name != fromState.name)
        //            {
        //                $log.debug('Reset posts filters.');
        //            }
        //        }
        //    });

        $rootScope.$on('$viewContentLoading',
            function (event, viewConfig) {
                // Access to all the view config properties.
                // and one special property 'targetView'
                // viewConfig.targetView 
                $log.debug('Posts view loading:', event, viewConfig);
            });

        $scope.$on('$viewContentLoaded',
            function (event) {
                $log.debug('Posts view loaded:', event);
                vm.Navs.LoadEnd();
            });
    };

    ///////////////////////
    /*angular
        .module('app.posts')
        .controller('GridBottomSheetCtrl', GridBottomSheetCtrl);
    GridBottomSheetCtrl.$inject = ['$rootScope', '$scope', '$mdBottomSheet', '$log', 'siteConf', 'siteConfData'];
    function GridBottomSheetCtrl($rootScope, $scope, $mdBottomSheet, $log, siteConf, siteConfData) {
        //$log.debug("GridBottomSheetCtrl...");*/

        /*jshint validthis: true */
        /*var vm = this;
        vm.listItemClick = function($index) {
            $mdBottomSheet.hide($index);
        };
        vm.Cats = null;
        vm.Tags = null;
        vm.Groups = null;
        vm.IsAdmin = false;
        vm.IsPub = false;
        vm.IsWriter = false;
        vm.IsContributor = false;
        // Waiting site configuration...
        //$log.debug('GridBottomSheetCtrl: Waiting site configuration...');
        siteConfData.then(function (conf) {
            vm.Cats = siteConf.getClaimCategories();
            vm.Tags = siteConf.getClaimTags();
            vm.Groups = siteConf.getClaimGroups();
            vm.IsAdmin = siteConf.IsAdmin;
            vm.IsPub = siteConf.IsPub;
            vm.IsWriter = siteConf.IsWriter;
            vm.IsContributor = siteConf.IsAdmin || siteConf.IsPub || siteConf.IsWriter;
            //$log.debug('GridBottomSheetCtrl: IsAdmin, IsPub, IsWriter, IsContributor=', vm.IsAdmin, vm.IsPub, vm.IsWriter, vm.IsContributor);
        }, function (reason) { });
    };*/

    ///////////////////////

    //function PostHideActions(uiRouter) {
    //    if (uiRouter == false) { return; }
    //    $("#btnPostsAdd").hide();
    //    $("#btnPostsSettings").hide();
    //}

    //function PostShowActions(uiRouter) {
    //    if (uiRouter == false) { return; }
    //    $("#btnPostsAdd").show();
    //    $("#btnPostsSettings").show();
    //}

})();