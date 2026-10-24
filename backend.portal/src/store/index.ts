import Vue from 'vue';
import Vuex, { Commit, Dispatch } from 'vuex';
import Auth from '@/store/auth';
import Error from '@/store/error';
import Players from '@/store/players';

Vue.use(Vuex);

export default new Vuex.Store({
	modules: {
		Auth,
		Error,
		Players,
	},
});

export interface IActionContextBasic {
	commit: Commit;
	dispatch: Dispatch;
}
