const path = require("path");

module.exports = (_env, argv) => {
    const isProd = argv.mode === "production";

    return {
        context: __dirname,
        mode: isProd ? "production" : "development",
        entry: "./typescript/interop.ts",
        output: {
            path: path.resolve(__dirname, "../DirectiveAthenaWeb.Client/wwwroot/js"),
            filename: "interop.bundle.min.js"
        },
        resolve: {
            extensions: [".ts", ".js"]
        },
        module: {
            rules: [
                {
                    test: /\.ts$/,
                    use: "ts-loader",
                    exclude: /node_modules/
                }
            ]
        },
        devtool: isProd ? false : "source-map",
        target: "web",
        optimization: {
            minimize: isProd
        }
    };
};
