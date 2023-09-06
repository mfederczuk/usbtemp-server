// SPDX-License-Identifier: CC0-1.0

import path from "path";
import type webpack from "webpack";

export default {
	entry: path.join(__dirname, "polling-page-script-src/main.ts"),
	output: {
		path: path.join(__dirname, "wwwroot/"),
		filename: "polling-page.js",
	},
	resolve: {
		extensions: [".ts"],
	},
	module: {
		rules: [
			{
				test: /\.ts$/,
				use: "ts-loader",
				exclude: /node_modules/,
			},
		],
	},
} as webpack.Configuration;
