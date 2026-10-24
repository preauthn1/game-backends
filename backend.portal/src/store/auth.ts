import { IActionContextBasic } from '@/store/index';
import * as Model from '@/models/interfaces/commons/permission';

export interface IState {
	apiPaths: Model.INestedPermission[];
	userProfile: any;
	currentPath: string;
	allPermissions: Model.IAllPermission[];
	nestedPermissions: Model.INestedPermission[];
	defaultLangTimes: number;
}
const SET_AUTH_ROLE_STRING = 'SET_AUTH_ROLE_STRING';
const SET_USER_PROFILE = 'SET_USER_PROFILE';
const SET_CURRENT_PATH = 'SET_CURRENT_PATH';
const SET_ALL_PERMISSION = 'SET_ALL_PERMISSION';
const SET_NESTED_PERMISSIONS = 'SET_NESTED_PERMISSIONS';
const SET_DEFAULT_LANG_TIMES = 'SET_DEFAULT_LANG_TIMES';

const initState: IState = {
	apiPaths: [],
	userProfile: {},
	currentPath: '',
	allPermissions: [],
	nestedPermissions: [],
	defaultLangTimes: 0,
};

const getters = {
	getApiPaths: (state: IState) => state.apiPaths,
	getAllPermissions: (state: IState) => state.allPermissions,
};

const actions = {
	async setApiPath(context: IActionContextBasic, data: string[]) {
		context.commit(SET_AUTH_ROLE_STRING, data);
	},
	async setUserProfile(context: IActionContextBasic, data: any) {
		context.commit(SET_USER_PROFILE, data);
	},
	async setCurrentPath(context: IActionContextBasic, data: string) {
		context.commit(SET_CURRENT_PATH, data);
	},
	async setAllPermissions(context: IActionContextBasic, data: string) {
		context.commit(SET_ALL_PERMISSION, data);
	},
	async setNestedPermissions(context: IActionContextBasic, data: string) {
		context.commit(SET_NESTED_PERMISSIONS, data);
	},
	async setDefaultLangTime(context: IActionContextBasic, times: number) {
		context.commit(SET_DEFAULT_LANG_TIMES, times);
	},
};

const mutations = {
	[SET_AUTH_ROLE_STRING](state: IState, payload: Model.INestedPermission[]) {
		state.apiPaths = payload;
	},
	[SET_USER_PROFILE](state: IState, payload: any) {
		state.userProfile = payload;
	},
	[SET_CURRENT_PATH](state: IState, payload: string) {
		state.currentPath = payload;
	},
	[SET_ALL_PERMISSION](state: IState, payload: Model.IAllPermission[]) {
		state.allPermissions = payload;
	},
	[SET_NESTED_PERMISSIONS](state: IState, payload: Model.INestedPermission[]) {
		state.nestedPermissions = payload;
	},
	[SET_DEFAULT_LANG_TIMES](state: IState) {
		state.defaultLangTimes += 1;
	},
};

export default {
	namespaced: true,
	state: initState,
	getters,
	actions,
	mutations,
};
