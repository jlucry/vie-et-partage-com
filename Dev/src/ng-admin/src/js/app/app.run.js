/*!
 * Wcm2015U1Rc ()
 * Copyright 2016 DFIDE
 * Licensed under the 
 */
(function () {
    'use strict';

    angular
        .module('app')
        .run(moduleRun);

    moduleRun.$inject = ['$rootScope', /*'$state', '$stateParams',*/ '$log', 'heads', 'navs'];

    function moduleRun($rootScope, /*$state, $stateParams,*/ $log, heads, navs) {
        $log.debug('App runs...');
        $rootScope.uiRouter = false;

        //TODO: Remove $rootScope dependencies.
        // Got from https://github.com/angular-ui/ui-router/blob/master/sample/app/app.js
        // It's very handy to add references to $state and $stateParams to the $rootScope
        // so that you can access them from any scope within your applications.For example,
        // <li ng-class="{ active: $state.includes('contacts.list') }"> will set the <li>
        // to active whenever 'contacts.list' or one of its decendents is active.
        //$rootScope.$state = $state;
        //$rootScope.$stateParams = $stateParams;

        $rootScope.$on('$routeChangeStart',
            function (event, next, current) {
                $log.debug('$routeChangeStart catched in app run.'/*, event, next, current*/);
                // Show loading notification...
                navs.LoadStart();
                // Reset the reload function...
                heads.Reload = null;
            });
    }

})();