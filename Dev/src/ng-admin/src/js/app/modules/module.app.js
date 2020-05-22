(function () {
    'use strict';

    angular.module('app', [
        'ngRoute', 'ngCookies', 'ngMessages', 'ngMaterial',
        'app.services',
        'app.posts',
        'app.posts.service',
        'app.users',
        'app.users.service',
        'summernote',
        'ngFileUpload',
        'ngImgCrop'
    ]);
    angular.module('app.services', []);

    angular.module('app.posts', [
        'ui.router',
        'app.services',
        'app.posts.service',
        'ngSanitize',
        'ngMaterial',
        'summernote',
        'ngFileUpload',
        'ngImgCrop'
    ]);
    angular.module('app.posts.service', []);

    angular.module('app.users', [
        'ui.router',
        'app.services',
        'app.users.service',
        'ngSanitize',
        'ngMaterial',
        'summernote',
        'ngFileUpload',
        'ngImgCrop'
    ]);
    angular.module('app.users.service', []);

})();