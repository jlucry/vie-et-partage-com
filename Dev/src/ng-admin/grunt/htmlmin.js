module.exports = function () {
    "use strict";

    return {
        options: {
            'removeComments': true,
            'collapseWhitespace': true
        },
        html: {
            expand: true,
            cwd: '<%= config.html %>',
            src: ['**/*.html'],
            dest: '<%= config.html %>'
        }
    };
};
