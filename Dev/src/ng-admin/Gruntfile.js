/// <binding />
module.exports = function(grunt) {
  'use strict';

  var path = require('path');

  require('load-grunt-config')(grunt, {
    // path to task.js files, defaults to grunt dir
    configPath: path.join(process.cwd(), 'grunt'),

    // auto grunt.initConfig
    init: true,

    // data passed into config.  Can use with <%= test %>
    data: {
      pkg: grunt.file.readJSON('package.json'),
      config: grunt.file.readJSON('config.json'),
      banner: '/*!\n' +
            ' * <%= pkg.name %> (<%= pkg.homepage %>)\n' +
            ' * Copyright <%= grunt.template.today("yyyy") %> <%= pkg.author.name %>\n' +
            ' * Licensed under <%= pkg.license %>\n' +
            ' */\n'
    },

    // can optionally pass options to load-grunt-tasks.
    // If you set to false, it will disable auto loading tasks.
    loadGruntTasks: {
      pattern: 'grunt-*',
      config: require('./package.json'),
      scope: ['devDependencies' ,'dependencies']
    }
  });

  // Html distribution task.
  grunt.registerTask('dist-html', [/*'clean:html',*/ 'hb', 'htmlmin', /*'prettify', 'notify:html'*/]);

  // JS distribution task.
  grunt.registerTask('dist-js', [/*'clean:js',*/ 'concat:js', 'uglify:js', 'clean:jsnonmin'/*, 'notify:js'*/]);

  // Full distribution task.
  grunt.registerTask('dist', ['dist-html', 'dist-js'/*, 'notify:all'*/]);

  // Default task.
  grunt.registerTask('default', ['dist']);
};
