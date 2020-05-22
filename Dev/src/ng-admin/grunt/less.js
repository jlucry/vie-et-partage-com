module.exports = function () {
  "use strict";

  return {
    options: {
      strictMath: false,
      paths: [
        '<%= config.source.less %>',
        '<%= config.global.less %>',
        '<%= config.bootstrap.less %>',
        '<%= config.bootstrap.mixins %>'
      ]
    },
    compileSite: {
      options: {
        strictMath: true
      },
      src: '<%= config.source.less %>/site.less',
      dest: '<%= config.destination.css %>/site.css'
    },
    skins: {
      options: {
        strictMath: true,
        paths: [
          '<%= config.source.skins %>/less',
          '<%= config.global.skins %>',
          '<%= config.source.less %>',
          '<%= config.global.less %>',
          '<%= config.bootstrap.less %>',
          '<%= config.bootstrap.mixins %>'
        ]
      },
      expand: true,
      cwd: '<%= config.source.skins %>',
      src: ['*.less'],
      dest: '<%= config.destination.skins %>',
      ext: '.css',
      extDot: 'last'
    },
    examples: {
      expand: true,
      cwd: '<%= config.source.examples %>/less',
      src: ['**/*.less'],
      dest: '<%= config.destination.examples %>/css',
      ext: '.css',
      extDot: 'last'
    },
    apps: {
        expand: true,
        cwd: '<%= config.source.less %>/apps',
        src: ['**/*.less'],
        dest: '<%= config.destination.css %>/apps',
        ext: '.css',
        extDot: 'last'
    },
    fonts: {
        expand: true,
        cwd: '<%= config.source.fonts %>',
        src: ['*/*.less', '!*/_*.less'],
        dest: '<%= config.destination.fonts %>',
        ext: '.css',
        extDot: 'last'
    },
  };
};
