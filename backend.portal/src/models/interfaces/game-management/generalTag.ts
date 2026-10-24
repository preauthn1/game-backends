
export interface IGeneralTagRequest {
		
	pageIndex: number;
	pageSize: number;
	status: number;
	orderByColumn: string | null;
	isASC: boolean | null;

}

export interface IGeneralTagCreateRequest {
	tagID: number;
	type: number;
	name: string;	
	memo : string;
}

export interface IGeneralTagAssignerUpdateRequest {
	tagID: number;
	assignTargetIDs: number[];	

	// 前端顯示用
	type: number;
	name: string;
}

export interface IGeneralTagResponse {
	tagID: number;
	type: number;
	typeText: string;	
	name: string;	
	createTime: string;	
	assignTargetIDs: number[];	
}

