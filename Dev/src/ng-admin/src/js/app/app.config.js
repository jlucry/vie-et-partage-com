(function () {
    'use strict';

    angular
        .module('app')
        .config(moduleConfig);

    moduleConfig.$inject = ['$compileProvider', '$routeProvider', '$locationProvider', '$mdThemingProvider', '$mdIconProvider', '$logProvider', '$qProvider'];

    function moduleConfig($compileProvider, $routeProvider, $locationProvider, $mdThemingProvider, $mdIconProvider, $logProvider, $qProvider) {
        // Running an AngularJS App in Production: https://docs.angularjs.org/guide/production
        $compileProvider.debugInfoEnabled(false);
        // Disable debug message: https://docs.angularjs.org/api/ng/provider/$logProvider
        $logProvider.debugEnabled(false);

        // https://docs.angularjs.org/guide/migration#commit-aa077e8
        // If you actually want to have no hash- prefix, then you can restore the previous behavior by adding a configuration block to you application:
        $locationProvider.hashPrefix('');

        // https://github.com/angular-ui/ui-router/issues/2889
        $qProvider.errorOnUnhandledRejections(false);

        // Use the HTML5 History API...
        //$locationProvider.html5Mode(true);
    
        // Define routes...
        $routeProvider
            .when('/', {
                templateUrl: '/admin/posts',
                controller: 'PostsController',
                controllerAs: 'vm',
                //resolve: { siteConfData: 'siteConfData' },
                //resolve: {
                //    siteConfData: ['siteConf', function (siteConf) {
                //        return siteConf.get();
                //    }]
                //}
                resolve: {
                    /*siteConfData: 'siteConfData',*/
                    /*posts: ['siteConfData', 'posts',
                      function (siteConfData, posts) {
                          posts.onlyValidedPost = false;
                          return posts.gets(3);
                      }],*/
                }
            })
            .when('/posts', {
                templateUrl: '/admin/posts',
                controller: 'PostsController',
                controllerAs: 'vm'
            })
            .when('/calendar', {
                templateUrl: '/admin/calendar',
                resolve: { siteConfData: 'siteConfData' }
            })
            .when('/pages', {
                templateUrl: '/admin/pages',
                resolve: { siteConfData: 'siteConfData' }
            })
            .when('/users', {
                templateUrl: '/admin/users',
                controller: 'UsersController',
                controllerAs: 'vm'
            })
            .when('/:tmpl', {
                templateUrl: function (params) {
                    return params.tmpl;
                },
                resolve: { siteConfData: 'siteConfData' },
            });

        // Configuring the default material theme: https://material.angularjs.org/latest/Theming/03_configuring_a_theme
        $mdThemingProvider.definePalette('docs-blue', $mdThemingProvider.extendPalette('blue', {
            '50': '#DCEFFF',
            '100': '#AAD1F9',
            '200': '#7BB8F5',
            '300': '#4C9EF1',
            '400': '#1C85ED',
            '500': '#106CC8',
            '600': '#0159A2',
            '700': '#025EE9',
            '800': '#014AB6',
            '900': '#013583',
            'contrastDefaultColor': 'light',
            'contrastDarkColors': '50 100 200 A100',
            'contrastStrongLightColors': '300 400 A200 A400'
        }));
        $mdThemingProvider.definePalette('docs-red', $mdThemingProvider.extendPalette('red', {
            'A100': '#DE3641'
        }));
        $mdThemingProvider.theme('docs-dark', 'default')
            .primaryPalette('yellow')
            .dark();
        $mdThemingProvider.theme('default')
            .primaryPalette('docs-blue')
            .accentPalette('docs-red');
        // Enable browser color
        $mdThemingProvider.enableBrowserColor({
            theme: 'default', // Default is 'default'
            palette: 'primary', // Default is 'primary', any basic material palette and extended palettes are available
            hue: '800' // Default is '800'
        });

        $mdIconProvider.icon('md-toggle-arrow', 'img/icons/toggle-arrow.svg', 48);

        $routeProvider.otherwise('/');
    };

})();