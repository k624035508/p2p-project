var path = require('path');

module.exports = {
    cache: true,
    entry: {
        manager_messages: "./js/manager-messages.jsx",
        predict_table: "./predict/predict-table.jsx",
    },
    output: {
        path: path.join(__dirname, './jsbuild'),
        filename: "[name].bundle.js",
    },
    plugins: [
    ],
    module: {
        loaders: [
            { test: /\.jsx?$/, exclude: /node_modules|build/, loader: 'babel?cacheDirectory' },
            { test: /\.css$/, loader: "style!css!autoprefixer" },
            { test: /\.png$/, loader: "url?limit=100000" },
            { test: /\.jpg$/, loader: "file" },
            { test: /\.less$/, loader: "style!css!autoprefixer!less" },
        ]
    }
};