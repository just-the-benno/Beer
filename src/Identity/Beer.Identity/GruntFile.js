
/// <binding BeforeBuild='copy' />
module.exports = function (grunt) {

    grunt.initConfig({
        clean:
        {
            css: ["wwwroot/css/lib/*"],
            js: ["wwwroot/js/lib/*"],
            profilePictures: ["wwwroot/img/pp/*"],
        },
        watch: {
        },
        copy: {
            pp:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'FrontendAssets/AvatarImagesSmall', src: ['**'], dest: 'wwwroot/img/pp/', filter: 'isFile' },
                ]
            },
            mdl:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/material-design-lite/', src: ['material.css', 'material.min.css.map', 'material.min.css'], dest: 'wwwroot/css/lib/mdl/', filter: 'isFile' },
                    { expand: true, flatten: false, cwd: 'node_modules/material-design-lite/', src: ['material.js', 'material.min.js.map', 'material.min.js'], dest: 'wwwroot/js/lib/mdl/', filter: 'isFile' },
                ]
            },
            md_icons:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/material-design-icons-iconfont/dist/', src: ['material-design-icons.css', 'material-design-icons.css.map', ], dest: 'wwwroot/css/lib/md-icons/', filter: 'isFile' },
                    { expand: true, flatten: false, cwd: 'node_modules/material-design-icons-iconfont/dist/fonts/', src: '**', dest: 'wwwroot/css/lib/md-icons/fonts/', filter: 'isFile' }
                ]
            },
            
        },
    });

    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks('grunt-contrib-watch');
    grunt.loadNpmTasks('grunt-contrib-copy');

    grunt.registerTask("watch-all", ['watch']);
    grunt.registerTask("build", [
        'clean',
        'copy']);
  
}; 