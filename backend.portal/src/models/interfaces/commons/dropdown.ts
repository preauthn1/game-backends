export interface IDropdownItems {
	extraProperty: any;
	text: string;
	text_CN: string;
	text_EN: string;
	fullTextForKeywordSearch: string;
	value: number;
	valueInt : number | null;
	valueIntOrZero: number;
}


export interface IMerchantDropdownRequest {
	categoryFirstOrderID: number | null;
	isIncludeCategory: boolean;
	isExcludeSpecificAgent: boolean;
	isSimplifyItems: boolean;
	isIncludeFavoritCopyMerchantCategory: boolean;
	keyword: string;
}


export class DataTablePagingParameter {
		
	totalCount: number = 0;
	pageIndex: number = 1;
	footerProps: any = { 'items-per-page-options': [10, 30, 50, 100] };
	itemsPerPage: number = 10;

}


/* tslint:disable:max-classes-per-file */
export class DialogVisible {
		
	// 基本上就是把比較常見的popup開關定義出來
	isShowCreateOrEdit: boolean = false;
	isShowCreate: boolean = false;
	isShowCopy: boolean = false;
	isShowChart: boolean = false;
	isShowConfirm : boolean = false;
	isShowFormConfirm : boolean = false;
	isShowEdit: boolean = false;
	isShowEdit2: boolean = false;
	isShowEditBriefInfo: boolean = false;
	isShowEditOther: boolean = false;
	isShowExport: boolean = false;
	isShowExtra1: boolean = false;
	isShowExtra2: boolean = false;
	isShowError : boolean = false;
	isShowDelete: boolean = false;
	isShowDetailInfo: boolean = false;
	isShowFullInfo : boolean = false;
	isShowAdvanceInfo: boolean = false;
	isShowAdvanceInfo2: boolean = false;
	isShowBatchEdit: boolean = false;
	isShowCopyDialog : boolean = false;
	isShowTreeInfo : boolean = false;
	isShowTreeInfo2 : boolean = false;
	isShowPreviewDialog : boolean = false;
	isShowSuccess : boolean = false;

	/* tslint:disable-next-line */
	isShowPopup_1 : boolean = false;
	
	/* tslint:disable-next-line */
	isShowPopup_2 : boolean = false;
	
	/* tslint:disable-next-line */
	isShowPopup_3 : boolean = false;


	resetAllValueToFalse() {

		const myObject : any = this;

		for (const name of Object.keys(myObject)) {
			myObject[name] = false;
		}
	}

}



/* tslint:disable:max-classes-per-file */
export class DataTablePagingParameterSize500 {
		
	totalCount: number = 0;
	pageIndex: number = 1;
	footerProps: any = { 'items-per-page-options': [500] };
	itemsPerPage: number = 500;

}

/* tslint:disable:max-classes-per-file */
export class DatetimeRangePicker {		

	quicklyPickTimeRangeText : string = '';
	searchStartTimeRange : number[] = [];	

}


export interface ISimpleUpdateReqeust {		

	callback : any;
	updatePlayerMemo : any;
	updateXXXXXXXXXXXX : any;
	
}

export interface ILazyCommonParameterReqeust {		

	hierarchyOfTargerMerchant: number;
	parentMerchantID : number;
	merchantID : number;
	status : number;
	memo: string;
	targetID : number;
	isEnableFlag1: boolean;
	categoryID : number;
	table: string;
	token : [REDACTED];
	begin : any;
	end : any;
	beginTS: number;
	endTS: number;
	type: number;
	eventTemplateType: number | null;
	vendorConfigID : number;
	rawText: string;
	beginDateTW : string;
	endDateTW : string;
	playerIDs : number[];
	playerID : number | null;
	playerName : string | null;
	displayName: string | null;
	gameIDsText: string | null;
	merchantIDsText: string | null;
	aggregativeGroup: number | null;
	templateID: number;
	gameID: number;

	merchantIDs : number[];	
	gameIDs : number[];	
	vendorConfigIDs : number[];	

	roleIDs: number[];
	isSync: boolean;
	yesOrNo: number;
}


export interface ILazyCommonResponseViewModel {
	totalCount: number;
	merchantIDs: number[];
}


export interface ICommonBriefInfoListResponse {		

	id : number;
	displayName : string;
	extraInfo : string;
}




export interface ISQLInsertCommandGenerateResult {		

	sql : string;
}


export interface ITextParseToListInt {		

	ids : number[];
}


