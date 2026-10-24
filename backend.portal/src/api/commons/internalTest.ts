import * as Model from '@/models/interfaces/commons/permission';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler from '@/utilities/api-handler.lib';
import * as Qs from 'qs';
import Cookies from 'js-cookie';
import { Md5 } from 'ts-md5';

export default {

	async getGameLaunchLink(payload : ResponseModel.IInternalTestRequest): Promise<ResponseModel.IResponseData<ResponseModel.IInternalTestResult>> {
				
		const config = {
			url: `/internal-test/game-launch?${Qs.stringify(payload)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IInternalTestResult>>result.data;
	},

	async getDynamicQuerySetting(querySqlID : number | null): Promise<ResponseModel.IResponseData<ResponseModel.IDynamicQueryCommandSettingResult>> {
		
		const config = {
			url: `/customized-columns/dynamicQuery/setting/${querySqlID}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IDynamicQueryCommandSettingResult>>result.data;
	},


	async getDynamicQuerySelectDropdowns(): Promise<ResponseModel.IResponseData<ResponseModel.IDynamicQueryCategoryDropdowns>> {
		
		const config = {
			url: `/customized-columns/dynamicQuery/selectItems`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IDynamicQueryCategoryDropdowns>>result.data;
	},


	async getCustomizedColumns(requestModel : ResponseModel.ICustomizedColumnRequest): Promise<ResponseModel.IResponseData<ResponseModel.ICustomizedColumnDisplayResult<ResponseModel.ICustomizedColumnResult>>> {
		
		const config = {
			url: `/customized-columns?${Qs.stringify(requestModel)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICustomizedColumnDisplayResult<ResponseModel.ICustomizedColumnResult>>>result.data;
	},

	async downloadCsvByDynamicQuery(requestModel : ResponseModel.ICustomizedColumnRequest): Promise<void> {
		
		const config = {
			url: `/customized-columns/csv/result?${Qs.stringify(requestModel)}`,
			method: 'get',
		};

		const result = await Handler.httpGetDownloadCsv(config.url , 'pgs_csv_download');
		return;
	},


	async checkPipelineID(): Promise<void> {		

		// 若是打production環境API的話，此API裡面做的事情 可能會有防火牆的問題
		const url = (process.env.VUE_APP_API_RAW + '/internal-test/pipeline-id-check').replace(/([^:]\/)\/+/g, '$1');
		const result = await Handler.requestTargetUrl(url);
		return;

	},

	
	async getAdminSysFavoritSetting(): Promise<ResponseModel.IResponseData<ResponseModel.IAdminSysFavoritSetting>> {
		
		const config = {
			url: `/internal-utility/favorit-setting`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IAdminSysFavoritSetting>>result.data;
	},

	
	async getBackendActionButtons(): Promise<ResponseModel.IResponseData<ResponseModel.IBackendButtonResponse>> {
		
		const config = {
			url: `/internal/backend-buttons`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IBackendButtonResponse>>result.data;
	},



	async getCustomerServiceUtilityInfo(): Promise<ResponseModel.IResponseData<ResponseModel.ICustomerServiceUtilityInfo>> {
		
		const config = {
			url: `/internal-test/customer-service-utility-info`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICustomerServiceUtilityInfo>>result.data;
	},


	async changeGameUrlDomainByCS(): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
		
		const config = {
			url: `/internal-test/update-game-url-domain-by-cs`,
			method: 'post',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},


	async getCommonAdminSysFavoritSettings(): Promise<ResponseModel.IResponseData<any>> {
		
		const config = {
			url: `/internal-utility/common-favorit-setting`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	},

	async downloadFileByTargetUrl(targetUrl: string): Promise<void> {	
		return this.httpPostDownloadFile(targetUrl);
	},


	async downloadFileByTargetUrlAndBody(targetUrl: string , body : any): Promise<void> {	
		return this.httpPostDownloadFile(targetUrl, body);
	},

	async downloadApiDocumentPDF(type: number , langCode: string , myToken: [REDACTED] Promise<void> {	
		const targetUrl = process.env.VUE_APP_API_RAW  + `internal-test/download-target-api-document?type=${type}&Language=${langCode}&token=[REDACTED];
		this.httpPostDownloadFile(targetUrl);
	},


	async downloadGameListFIle(): Promise<void> {	
		const targetUrl = process.env.VUE_APP_API_RAW  + "gameList-download";
		this.httpPostDownloadFile(targetUrl);
	},

	async executeBackendButtonAction(payload: ResponseModel.IBackendButtonActionRequest): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
		
		const config = {
			url: `/internal/backend-buttons/action`,
			method: 'post',
			isHash: true,
			data: payload,
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},

	async executeBackendButtonActionForCS(payload: ResponseModel.IBackendButtonActionRequest): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
		
		const config = {
			url: `/internal/backend-buttons/cs-action`,
			method: 'post',
			data: payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},


	async generateInsertSQLByID(targetID : number , tableName : string): Promise<ResponseModel.IResponseData<DropdownModel.ISQLInsertCommandGenerateResult>> {
		
		const payload: DropdownModel.ILazyCommonParameterReqeust = {} as DropdownModel.ILazyCommonParameterReqeust;
		payload.targetID = targetID;
		payload.table = tableName;

		return this.generateInsertSQL(payload);
	},


	async generateInsertSQL(payload: DropdownModel.ILazyCommonParameterReqeust): Promise<ResponseModel.IResponseData<DropdownModel.ISQLInsertCommandGenerateResult>> {
		
		const config = {
			url: `/internal-utility/table-sql-gen`,
			method: 'post',
			data: payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<DropdownModel.ISQLInsertCommandGenerateResult>>result.data;
	},


	async parseTextToListIDs(payload: DropdownModel.ILazyCommonParameterReqeust): Promise<ResponseModel.IResponseData<DropdownModel.ITextParseToListInt>> {
		
		const config = {
			url: `/internal-test/parse-text-to-IDs`,
			method: 'post',
			data: payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<DropdownModel.ITextParseToListInt>>result.data;
	},


	async showErrorMessageInPromise(response: any , isShowErrorAlert: boolean)
	{
		try
		{
			const responseText = await response.text();
			const parseObj = JSON.parse(responseText);

			if(isShowErrorAlert)
			{						
				alert('Error! '+ parseObj.message);
			}

			localStorage.setItem("download_error_msg" , parseObj.message);
		}
		catch(ex)
		{
			
		}
	},


	async httpPostDownloadFile(targetUrl: string, postBody : any = null, isShowErrorAlert: boolean = true)
	{
		const myHeaders: any = {
			"Content-Type": "application/json",
			'access_token': Cookies.get('token'),
			'locale': Cookies.get('locale') ? Cookies.get('locale') : 'en-us',
		};

		let payload = JSON.stringify('');

		if(postBody != null) {

			payload = JSON.stringify(postBody);

			// 額外加密Hash
			const hashSource = Object.keys(postBody || {})
				.sort()
				.map(key => {
					const value = postBody[key];
					const hashValue = value !== null && typeof value === 'object'
						? JSON.stringify(value)
						: String(value);
					return `${key}=${hashValue}`;
				})
				.join('&')
				.toLowerCase();

			myHeaders['X-target-POST-HASH'] = Md5.hashStr(hashSource + "3d1a6d06c246");
		}
	
		return fetch(targetUrl, {
			method: "POST",
			headers: myHeaders,
			body: payload
		})
		.then((response) => {
			if (!response.ok) {			
				this.showErrorMessageInPromise(response , isShowErrorAlert);
				throw new Error(`Failed to download ${targetUrl}`);
			}
	
			return response.blob().then((blob) => {
				// 处理下载的文件，例如保存到本地
				console.log(`Downloaded ${targetUrl}`);
				console.log(blob);

				// 檔案名稱格式為base64。避免中文亂碼問題
				// @ts-ignore 
				const fileName = decodeURIComponent(escape(atob(response.headers.get('X-target-File-Name'))));
	
				console.log(fileName);
				console.log(response.headers.get('Content-Disposition'));
	
				const a = document.createElement('a');
				a.href = window.URL.createObjectURL(blob);
				a.download = fileName;
				a.style.display = 'none';
				document.body.appendChild(a);
				a.click();
				window.URL.revokeObjectURL(a.href);
				document.body.removeChild(a);

				// 讓async後面識別此下載已完成
				localStorage.setItem(targetUrl , "1");
			});
		})
		.catch((error) => {
			// alert('There has been a problem with your fetch operation:' + error);
		})
		.then((blobObject) => {
			console.log('blobObject');
			console.log(blobObject);
		});
	},
		

};
