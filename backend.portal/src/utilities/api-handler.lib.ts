import axios, { AxiosResponse, AxiosRequestConfig } from 'axios';
import Cookies from 'js-cookie';
import { Md5 } from 'ts-md5';
import { requestSuccess, requestFail, responseSuccess, responseFail } from './error-handler.lib';
import * as Model from '@/models/interfaces/commons/response';

class HttpModel {
	constructor() {}
	/**
	 * axios API handler
	 * @author rourou
	 * @description 一律透過此method呼叫api
	 */
	async request<T>(cfg: AxiosRequestConfig | any, isFromLoginAPI: boolean = false , tempToken: [REDACTED] = '') {
		cfg.baseURL = process.env.VUE_APP_API;
		cfg.baseURL = '/backoffice/';

		const envType = process.env.VUE_APP_HOST;
		const isLocal = envType == 'local';
		const isDebug = Cookies.get('is_debug');

		// 增加debug資訊
		if (isDebug) {
			console.log(process.env.VUE_APP_API);

			if (cfg.data) {
				console.log(cfg.data);
			}

			console.log(cfg);
		}


		if(tempToken != null && tempToken.length > 1)
		{
			cfg.headers = {
				HDCode: localStorage.getItem('deviceID'),
				access_token: [REDACTED],
				locale: Cookies.get('locale') ? Cookies.get('locale') : 'en-us',
				"X-target-device-width" : this.getDeviceWidth(),
				"X-target-device-height" : this.getDeviceHeight(),				
				'content-type': 'application/json',
			};
		}
		else if (isFromLoginAPI) {
			cfg.headers = {
				HDCode: localStorage.getItem('deviceID'),
				'content-type': 'application/json',
				locale: Cookies.get('locale') ? Cookies.get('locale') : 'en-us',
				"X-target-device-width" : this.getDeviceWidth(),
				"X-target-device-height" : this.getDeviceHeight(),				
			};
		} 	
		else {
			cfg.headers = {
				HDCode: localStorage.getItem('deviceID'),
				access_token: [REDACTED]'token'),
				locale: Cookies.get('locale') ? Cookies.get('locale') : 'en-us',
				"X-target-device-width" : this.getDeviceWidth(),
				"X-target-device-height" : this.getDeviceHeight(),				
				'content-type': 'application/json',
			};
		}

		if (cfg.isHash != null && cfg.isHash === true) {
			const hashSource = Object.keys(cfg.data || {})
				.sort()
				.map(key => {
					const value = cfg.data[key];
					const hashValue = value !== null && typeof value === 'object'
						? JSON.stringify(value)
						: String(value);
					return `${key}=${hashValue}`;
				})
				.join('&')
				.toLowerCase();

			if(isLocal) {
				console.log('before hash ------');
				console.log(hashSource);
			}

			cfg.headers['X-target-POST-HASH'] = Md5.hashStr(hashSource + "3d1a6d06c246");
		}

		const instance = axios.create();
		instance.interceptors.request.use(
			(config: any) => requestSuccess(config),
			(err: any) => requestFail(err)
		);
		instance.interceptors.response.use(
			(response: any) => responseSuccess(response),
			(error: any) => responseFail(error)
		);
		let result;
		try {
			result = await instance.request<T>(cfg);
			return Promise.resolve(result);
		} catch (throwError) {
			return Promise.reject(throwError);
		}
	}

	async OpenNewTabToUrl(url : string , isAddPrefix : boolean = false) {		

		if(isAddPrefix)
		{
			url = '/backoffice/' + url;
		}
		
		window.open(url);
	}


	async requestTargetUrl(url : string , isAddPrefix : boolean = false) {		

		if(isAddPrefix)
		{
			url = '/backoffice/' + url;
		}

		const postConfig: any = {};
		axios
			.get(url, {
				headers: {
					HDCode: localStorage.getItem('deviceID'),
					access_token: [REDACTED]'token'),
					locale: Cookies.get('locale') ? Cookies.get('locale') : 'en-us',
				},
			})
			.then(response => {
				
			});
	}


	async httpGetDownloadCsv<T>(url: string, fileName: string) {
		const postConfig: any = {};

		if (url.charAt(0) === '/') {
			url = url.substring(1, url.length);
		}		

		let apiDomain = process.env.VUE_APP_API_RAW;

		if(!apiDomain || apiDomain === '' || apiDomain.length <= 0)
		{
			apiDomain = process.env.VUE_APP_API;
		}

		console.log(apiDomain);

		const myUrl = apiDomain + url;

		try {			
			const result = axios
			.get(myUrl, {
				headers: {
					HDCode: localStorage.getItem('deviceID'),
					access_token: [REDACTED]'token'),
					locale: Cookies.get('locale') ? Cookies.get('locale') : 'en-us',
				},
				timeout : 1000 * 180,
				responseType: 'text',
			})
			.then(response => {

				const excelUrl: any = URL.createObjectURL(
					new Blob([ "\uFEFF" + response.data], {
						type: 'text/csv;charset=utf8;',
					})
				);

				const link = document.createElement('a');
				link.href = excelUrl;
				link.setAttribute('download', fileName + '.csv');
				document.body.appendChild(link);
				link.click();
			});
			return Promise.resolve(result);
		} catch (throwError) {
			return Promise.reject(throwError);
		}
	}

	getDeviceWidth() {
		try {
			return (window.innerWidth > 0) ? window.innerWidth : screen.width;
		} catch (e) {
			return 0;
		}
	}
	
	getDeviceHeight() {
		try {
			return (window.innerHeight > 0) ? window.innerHeight : screen.height;
		} catch (e) {
			return 0;
		}
	}

	async downloadExcel<T>(url: string, fileName: string) {
		const postConfig: any = {};

		if (url.charAt(0) === '/') {
			url = url.substring(1, url.length);
		}

		axios
			.post(process.env.VUE_APP_API + url, postConfig, {
				headers: {
					HDCode: localStorage.getItem('deviceID'),
					access_token: [REDACTED]'token'),
					locale: Cookies.get('locale') ? Cookies.get('locale') : 'en-us',
				},
				responseType: 'blob',
			})
			.then(response => {
				const excelUrl: any = URL.createObjectURL(
					new Blob([response.data], {
						type: 'application/vnd.ms-excel',
					})
				);
				const link = document.createElement('a');
				link.href = excelUrl;
				link.setAttribute('download', fileName + '.xls');
				document.body.appendChild(link);
				link.click();
			});
	}
}

const model = new HttpModel();

export default model;

export const pageData = {
	pageIndex: 1,
	pageSize: 500,
};

export const time = 300;
