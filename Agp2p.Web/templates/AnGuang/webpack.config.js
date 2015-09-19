var CommonsChunkPlugin = require("webpack/lib/optimize/CommonsChunkPlugin");
var path = require('path');
var webpack = require("webpack");

module.exports = {
    cache: true,
    entry: {
        index: "./js/index.js",
        login: "./js/login.js",
        invest_detail: "./js/invest_detail.js"
    },
    output: {
        path: path.join(__dirname, './build'),
        filename: "[name].bundle.js",
        publicPath: "/templates/AnGuang/build/"
    },
    plugins: [
        new CommonsChunkPlugin("commons.bundle.js"),
        new webpack.ProvidePlugin({
            $: "jquery",
            jQuery: "jquery",
            "window.jQuery": "jquery",
            "root.jQuery": "jquery"
        }),
    ],
    module: {
        loaders: [
            { test: /\.js$/, exclude: /node_modules/, loader: 'babel?cacheDirectory' },
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
    }
};