//(function () {
//    'use strict';

//    // Posts controllers.
//    angular
//        .module('app.posts')
//        .controller('PostSettingsController', PostSettingsController);

//    // Post settings controller.
//    PostSettingsController.$inject = ['$rootScope', '$scope', '$state', '$timeout', '$log', '$mdDialog', 'heads', 'navs', 'siteConf', 'posts'];

//    function PostSettingsController($rootScope, $scope, $state, $timeout, $log, $mdDialog, heads, navs, siteConf, posts) {
//        $log.debug("PostSettingsController...");

//        /*jshint validthis: true */
//        var sets = this;
//        sets.Navs = navs;
//        sets.Cats = siteConf.getClaimCategories();
//        sets.Tags = siteConf.getClaimTags();
//        sets.Groups = siteConf.getClaimGroups();
//        sets.Posts = posts;
//        sets.Mine = sets.Posts.getFilterMine();
//        sets.Block = false;
//        sets.SelectedCategory2 = sets.Posts.getFilterCategory();
//        sets.SelectedTag2 = sets.Posts.getFilterTag();
//        sets.SelectedGroup2 = sets.Posts.getFilterGroup();
//        //sets.SelectedCategory = siteConf.getClaimCategory(sets.SelectedCategory2);
//        //sets.SelectedTag = siteConf.getClaimTag(sets.SelectedTag2);
//        //sets.SelectedGroup = siteConf.getClaimGroup(sets.SelectedGroup2);
//        //sets.QuerySearchCategory = querySearchCategory;
//        //sets.QuerySearchTag = querySearchTag;
//        //sets.QuerySearchGroup = querySearchGroup;
//        sets.CatMarginLeft = catMarginLeft;
//        sets.Apply = apply;
//        sets.Reset = reset;
//        sets.Cancel = cancel;
//        sets.selectedPostState = sets.Posts.getFilterState();
//        sets.IsAdmin = siteConf.IsAdmin;
//        sets.IsPub = siteConf.IsPub;
//        sets.IsWriter = siteConf.IsWriter;

//        ///////////////////////

//        //function querySearchCategory(query) {
//        //    $log.debug('Search category ', query);
//        //    if (query == null) {
//        //        //$log.debug('Searched category=', siteConf.getClaimCategories());
//        //        return siteConf.getClaimCategories();
//        //    }
//        //    else {
//        //        var cats = [];
//        //        var lowercaseQuery = angular.lowercase(query);
//        //        angular.forEach(siteConf.getClaimCategories(), function (value) {
//        //            if (value != null && value.StringValue != null && angular.lowercase(value.StringValue).startsWith(lowercaseQuery) == true) {
//        //                //$log.debug(value);
//        //                this.push(value);
//        //            }
//        //        }, cats);
//        //        //$log.debug('Searched category=', cats);
//        //        return cats;
//        //    }
//        //}
//        //function querySearchTag(query) {
//        //    //$log.debug('Search tag ', query);
//        //    if (query == null) {
//        //        $log.debug('Searched tag=', siteConf.getClaimTags());
//        //        return siteConf.getClaimTags();
//        //    }
//        //    else {
//        //        var tags = [];
//        //        var lowercaseQuery = angular.lowercase(query);
//        //        angular.forEach(siteConf.getClaimTags(), function (value) {
//        //            if (value != null && value.StringValue != null && angular.lowercase(value.StringValue).startsWith(lowercaseQuery) == true) {
//        //                //$log.debug(value);
//        //                this.push(value);
//        //            }
//        //        }, tags);
//        //        //$log.debug('Searched tag=', tags);
//        //        return tags;
//        //    }
//        //}
//        //function querySearchGroup(query) {
//        //    $log.debug('Search group ', query);
//        //    if (query == null) {
//        //        return siteConf.getClaimGroups();
//        //    }
//        //    else {
//        //        var grps = [];
//        //        var lowercaseQuery = angular.lowercase(query);
//        //        angular.forEach(siteConf.getClaimGroups(), function (value) {
//        //            if (value != null && value.StringValue != null && angular.lowercase(value.StringValue).startsWith(lowercaseQuery) == true) {
//        //                //$log.debug(value);
//        //                this.push(value);
//        //            }
//        //        }, grps);
//        //        return grps;
//        //    }
//        //}

//        function catMarginLeft(cat) {
//            if (cat != null) {
//                return (cat.Deep * 20) + "px";
//            }
//            return "0px"
//        }

//        function apply() {
//            sets.Posts.setFilterMine(sets.Mine);
//            sets.Posts.setFilterState(sets.selectedPostState);
//            sets.Posts.setFilterCategory(sets.SelectedCategory2);
//            sets.Posts.setFilterTag(sets.SelectedTag2);
//            sets.Posts.setFilterGroup(sets.SelectedGroup2);
//            sets.Posts.setSkip(0);
//            $mdDialog.hide('');
//        }

//        function reset() {
//            sets.SelectedCategory2 = null;
//            sets.SelectedTag2 = null;
//            sets.SelectedGroup2 = null;
//            sets.selectedPostState = "1";
//            apply();
//        }

//        function cancel() {
//            $mdDialog.cancel();
//        }

//        ///////////////////////

//        //function _reload(reset) {
//        //    sets.Navs.LoadStart();
//        //    sets.Posts.gets(reset).then(function successCallback(response) {
//        //        $log.debug('Post reloaded (posts settings controller).');
//        //        sets.Navs.LoadEnd();
//        //    }, function errorCallback(response) {
//        //        $log.error('Failed to reload posts (posts settings controller).');
//        //        sets.Navs.LoadEnd();
//        //    });
//        //}
//    };

//})();