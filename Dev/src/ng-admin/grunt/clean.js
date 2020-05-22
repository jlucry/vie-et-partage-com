module.exports = function () {
  "use strict";

  return {
    html: '<%= config.html %>',
    css: '<%= config.destination.css %>',
    js: '<%= config.destination.js %>',
    jsnonmin: {
        options: {
            'force': true
        },
        src: ['<%= config.destination.js %>/**/*.js', '!<%= config.destination.js %>/**/*.min.js']
    },
    skins: '<%= config.destination.skins %>/**/*.css',
    examples: ['<%= config.destination.examples %>/css/**/*.css', '<%= config.destination.examples %>/js/**/*.js']
  };
};
