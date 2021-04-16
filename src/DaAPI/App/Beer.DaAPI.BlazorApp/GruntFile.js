const sass = require('node-sass');

module.exports = function (grunt) {

    grunt.initConfig({
        clean:
        {
            css: ["wwwroot/css/lib/*"],
            fonts: ["wwwroot/fonts"],
            wwwroot: ["wwwroot/css/*", "wwwroot/fonts/*"],
            temp: ["FrontendAssets/temp/*"]
        },
        watch: {
            scss: {
                files: 'FrontendAssets/scss/**/*.scss',
                tasks: ["clean:css", "sass", 'cssmin', "copy:css"]
            },
        },
        sass: {
            options: {
                implementation: sass,
                sourceMap: true
            },
            all: {
                files: {
                    'FrontendAssets/temp/css/app.css': 'FrontendAssets/scss/app.scss'
                }
            }
        },
        cssmin: {
            all: {
                files: [{
                    expand: true,
                    cwd: 'FrontendAssets/temp/css/',
                    src: ['*.css', '!*.min.css'],
                    dest: 'FrontendAssets/temp/css/',
                    ext: '.min.css'
                }]
            }
        },
        copy: {
            font_awesome:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/@fortawesome/fontawesome-free/css', src: ['all.css', 'all.min.css'], dest: 'wwwroot/css/lib/fontawesome/', filter: 'isFile' },
                    { expand: true, flatten: false, cwd: 'node_modules/@fortawesome/fontawesome-free/webfonts', src: ['**'], dest: 'wwwroot/css/lib/webfonts/', filter: 'isFile' },
                ]
            },
            css: {
                files: [
                    { expand: true, flatten: true, src: ['FrontendAssets/temp/css/**'], dest: 'wwwroot/css/', filter: 'isFile' }
                ]
            },
            fonts: {
                files: [
                    { expand: true, flatten: true, src: ['FrontendAssets/fonts/**'], dest: 'wwwroot/fonts/', filter: 'isFile' }
                ]
            },
        },
    });

    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks('grunt-contrib-copy');
    grunt.loadNpmTasks('grunt-sass');
    grunt.loadNpmTasks('grunt-contrib-cssmin')
    grunt.loadNpmTasks('grunt-contrib-watch');

    grunt.registerTask("watch-all", ['watch']);
    grunt.registerTask("build", [
        'clean',
        'sass', 'cssmin',
        'copy'
    ]);

}; 