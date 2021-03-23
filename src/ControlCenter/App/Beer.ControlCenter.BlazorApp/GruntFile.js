module.exports = function (grunt) {

    grunt.initConfig({
        clean:
        {
            css: ["wwwroot/css/lib/*"]
        },
        watch: {
        },
        copy: {
            font_awesome:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/@fortawesome/fontawesome-free/css', src: ['all.css', 'all.min.css'], dest: 'wwwroot/css/lib/fontawesome/', filter: 'isFile' },
                    { expand: true, flatten: false, cwd: 'node_modules/@fortawesome/fontawesome-free/webfonts', src: ['**'], dest: 'wwwroot/css/lib/webfonts/', filter: 'isFile' },
                ]
            }
        },
    });

    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks('grunt-contrib-copy');

    grunt.registerTask("watch-all", ['watch']);
    grunt.registerTask("build", [
        'clean',
        'copy']);

}; 