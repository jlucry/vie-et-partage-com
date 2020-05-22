module.exports = function () {
  "use strict";

  return {
    options: {
      config: '<%= config.source.less %>/.csscomb.json'
    },
    css: {
      expand: true,
      cwd: '<%= config.destination.css %>',
      src: ['**/*.css', '!**/*.min.css'],
      dest: '<%= config.destination.css %>/'
    },
    skins: {
      expand: true,
      cwd: '<%= config.destination.skins %>',
      src: ['*.css', '!*.min.css'],
      dest: '<%= config.destination.skins %>'
    },
    examples: {
      expand: true,
      cwd: '<%= config.destination.examples %>/css',
      src: ['**/*.css', '!**/*.min.css'],
      dest: '<%= config.destination.examples %>/css'
    },
    apps: {
        expand: true,
        cwd: '<%= config.destination.css %>/apps',
        src: ['**/*.css', '!**/*.min.css'],
        dest: '<%= config.destination.css %>/apps'
    },
    fonts: {
        expand: true,
        cwd: '<%= config.destination.fonts %>',
        src: ['*/*.css', '!*/*.min.css'],
        dest: '<%= config.destination.fonts %>'
    }
  };
};
