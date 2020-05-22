(function () {
    'use strict';

    // Demos filter.
    angular
        .module('app')
        .filter('nospace', function () {
            return function (value) {
                return (!value) ? '' : value.replace(/ /g, '');
            };
        })
        .filter('humanizeDoc', function () {
            return function (doc) {
                if (!doc) return;
                if (doc.type === 'directive') {
                    return doc.name.replace(/([A-Z])/g, function ($1) {
                        return '-' + $1.toLowerCase();
                    });
                }
                return doc.label || doc.name;
            };
        })
        .filter('directiveBrackets', function () {
            return function (str, restrict) {
                if (restrict) {
                    // If it is restricted to only attributes
                    if (!restrict.element && restrict.attribute) {
                        return '[' + str + ']';
                    }

                    // If it is restricted to elements and isn't a service
                    if (restrict.element && str.indexOf('-') > -1) {
                        return '<' + str + '>';
                    }

                    // TODO: Handle class/comment restrictions if we ever have any to document
                }

                // Just return the original string if we don't know what to do with it
                return str;
            };
        });
})();