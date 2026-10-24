import * as ResponseModel from '@/models/interfaces/commons/response';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import Handler from '@/utilities/api-handler.lib';
import * as Qs from 'qs';
import Cookies from 'js-cookie';
import { Locale } from '@/models/status/common';

export default {

	async simpleUpdate(payload: DropdownModel.ISimpleUpdateReqeust): Promise<ResponseModel.IResponseData<void>> {
		const config = {
			url: `/simgle-update`,
			method: 'post',
			data: payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	}
	

};
