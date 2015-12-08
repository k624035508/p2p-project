var path = require('path');
var webpack = require("webpack");
var CommonsChunkPlugin = require("webpack/lib/optimize/CommonsChunkPlugin");

module.exports = {
    entry: {
        index: "./js/index.js",
        projects: "./js/projects.js",
        project: "./js/project.js",
        login: "./js/login.js",
        register: "./js/register.js",
        usercenter: "./js/usercenter.js",
        recharge: "./js/recharge.js",
        withdraw: "./js/withdraw.js",
        mytrade: "./js/mytrade.js",
        myinvest: "./js/myinvest.js",
        myreceiveplan: "./js/myreceiveplan.js",
        settings: "./js/settings.js",
        mynews: "./js/mynews.js",
        newsdetail: "./js/newsdetail.js",
        mylottery: "./js/mylottery.js",
        safe: "./js/safe.js",
        mycard: "./js/mycard.js",
        aboutus: "./js/aboutus.js",
        citySelector: "./js/citySelector.js",
    },
    output: {
        path: path.join(__dirname, './build'),
        filename: "[name].bundle.js",
        publicPath: "/templates/weChat/build/"
    },
    module: {
        loaders: [
            { test: /\.jsx?$/, exclude: /node_modules|build/, loader: 'babel?cacheDirectory&presets[]=es2015' },
            { test: /\.css$/, loader: "style!css!autoprefixer" },
            { test: /\.png$/, loader: "url?limit=100000" },
            { test: /\.jpg$/, loader: "file" },
            { test: /\.less$/, loader: "style!css!autoprefixer!less" },

            // **IMPORTANT** This is needed so that each bootstrap js file required by
            // bootstrap-webpack has access to the jQuery object
            //{ test: /bootstrap\/js\//, loader: 'imports?jQuery=jquery' },

            // Needed for the css-loader when [bootstrap-webpack](https://github.com/bline/bootstrap-webpack)
            // loads bootstrap's css.
            { test: /\.woff(\?v=\d+\.\d+\.\d+)?$/,   loader: "url?limit=10000&mimetype=application/font-woff" },
            { test: /\.woff2(\?v=\d+\.\d+\.\d+)?$/,   loader: "url?limit=10000&mimetype=application/font-woff2" },
            { test: /\.ttf(\?v=\d+\.\d+\.\d+)?$/,    loader: "url?limit=10000&mimetype=application/octet-stream" },
            { test: /\.eot(\?v=\d+\.\d+\.\d+)?$/,    loader: "file" },
            { test: /\.svg(\?v=\d+\.\d+\.\d+)?$/,    loader: "url?limit=10000&mimetype=image/svg+xml" }
        ]
    },
    plugins: [
        new CommonsChunkPlugin("commons.bundle.js", ["index", "login", "register", "project", "projects", "usercenter", "recharge",
             "withdraw", "mytrade", "myinvest", "myreceiveplan", "settings", "mynews", "newsdetail", "mylottery", "safe", "mycard", "aboutus", "citySelector"]),
        new webpack.ProvidePlugin({
            $: "jquery",
            jQuery: "jquery",
            "window.jQuery": "jquery",
            "root.jQuery": "jquery"
        })
    ]
};