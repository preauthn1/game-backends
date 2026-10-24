import EventBus from '@/utilities/event-bus';

/**
 * axios error handler
 * @author rourou
 * @description response success
 */
const responseSuccess = (response: any) => {
	return Promise.resolve(response);
};

/**
 * axios error handler
 * @author rourou
 * @description response fail
 */
const responseFail = (errorData: any) => {
	const res = errorData.response;

	// 只有login需要特製下 
	if (res != null && res.config != null) {
		const url = res.config.url;
		if (url === '/backoffice/users/login' || url === '/users/login') {

			if(res.data.code !== 403200) {

				const myObj = {
					response : {
						data: {
							code: 500001,
							message : '(*'+ res.status + ') System response an error. The possible reason is that system under maintenance. We recommend that you try again later. 系统回应错误，可能原因为当前系统维护中。建议您稍后再做尝试'
						}
					}
				};

				if(res.status !== 400) {
					EventBus.$emit('login-error', myObj);
				}
				else {
					EventBus.$emit('login-error', errorData);
				}				
			}

			return Promise.reject(res);
		}
	}

	// 登入頁API 不會走到 EventBus.$on('api-error') 
	EventBus.$emit('api-error', errorData);
	return Promise.reject(res);
};

/**
 * axios error handler
 * @author rourou
 * @description request success
 */

const requestSuccess = (config: any) => {
	return config;
};

/**
 * axios error handler
 * @author rourou
 * @description request fail
 */
const requestFail = (error: any) => {
	err.error.message = '[token null] request fail';
	EventBus.$emit('api-error', err);
	// '请求超时!'
	return Promise.reject(error);
};

const err: any = {};

export { requestSuccess, requestFail, responseSuccess, responseFail };
