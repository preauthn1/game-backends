const WebpackObfuscator = require('webpack-obfuscator');

const path = require('path');
const host = '0.0.0.0';
const port = '9527';
const target = process.env.VUE_APP_API;
console.log(`${process.env.NODE_ENV}`);
console.log(target);
console.log(`${host}${port}`);

module.exports = {
	//publicPath: '/Gitlab_DDYNH/backend.portal/dist/', //本地測試build要指定相對路徑

	productionSourceMap: false,
	chainWebpack: (config) => {
		config.resolve.alias.set('icon', path.resolve('./node_modules/@mdi/font/css')).set('font', path.resolve('./node_modules/material-icons/iconfont'));
	},

	configureWebpack: (config) => {
		const isProduction = process.env.NODE_ENV === 'production';
		const isLocal = process.env.VUE_APP_HOST == 'local';

		console.log('---- configureWebpack ----');
		console.log('---- configureWebpack ----');
		console.log(process.env.VUE_APP_HOST);

		const obfuscatorOptions = {
			//rotateUnicodeArray: true,
			disableConsoleOutput: true,
			//transformObjectKeys: true,
			//stringArrayEncoding: ['base64'],

			//stringArray: true,
			//stringArrayThreshold: 0.5,
			compact: true,
			mangle: true,

			//勾選上面幾個 chunk-vendor.js 載入很慢 (2.6MB > 5.5MB) 不曉得是不是可以取捨?

			//   compact: true,
			//   controlFlowFlattening: true,
			//   controlFlowFlatteningThreshold: 0.75,
			//   deadCodeInjection: true,
			//   deadCodeInjectionThreshold: 0.4,
			//   debugProtection: true,
			//   identifierNamesGenerator: 'hexadecimal',
			//   log: false,
			//   renameGlobals: false,
			//   selfDefending: true,
			//   stringArray: true,
			//	 stringArrayThreshold: 0.75,
			//	 unicodeEscapeSequence: false
		};

		//本地local不要使用
		if (!isLocal) {
			config.plugins.push(new WebpackObfuscator(obfuscatorOptions, ['cn-lang.js', 'en-lang.js', 'tw-lang.js']));
		}
	},

	devServer: {
		headers: {
			'Access-Control-Allow-Origin': '*',
		},
		host,
		port,
		proxy: {
			'/backoffice': {
				target,
				changeOrigin: true,
				pathRewrite: {
					'^/backoffice': '',
				},
			},
		},
		disableHostCheck: true,
		progress: true,
	},
	pluginOptions: {
		'style-resources-loader': {
			preProcessor: 'scss',
			patterns: [],
		},
	},
	transpileDependencies: ['vuetify'],
};
