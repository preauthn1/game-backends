import { IActionContextBasic } from '@/store/index';

export interface IState {
	errorHistory: any[];
}
const ERROR = 'ERROR';

const initState: IState = {
	errorHistory: [],
};

const getters = {
	getErrorHistory: (state: IState) => state.errorHistory,
};

const actions = {
	getError(context: IActionContextBasic, error: any) {
		context.commit(ERROR, error);
	},
};

const mutations = {
	[ERROR](state: IState, payload: any) {
		state.errorHistory.splice(0, 0, payload);
	},
};

export default {
	namespaced: true,
	state: initState,
	getters,
	actions,
	mutations,
};
