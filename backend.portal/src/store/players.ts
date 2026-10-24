import { IActionContextBasic } from '@/store/index';

export interface IState {
	perGameLogID : number | null;
	playerID: number | null;
	gameID: number | null;
	gamePlayerID: number | null;
	serverID: number | null;
	walletPlayerID: number | null;
	prizeRecordPlayerID: number | null;
	transactionNoPGS: string | null;
	transactionNo:  string | null;
	eventID: number | null;
	beginTS: number | null;
	endTS: number | null;
	moneyRecordReferenceNo : string | null;
}

const initState: IState = {
	perGameLogID: null,
	playerID: null,
	gameID: null,
	gamePlayerID: null,
	serverID: null,
	walletPlayerID: null,
	prizeRecordPlayerID: null,
	transactionNoPGS : null,
	transactionNo: null,
	eventID : null,
	beginTS: null,
	endTS: null,	
	moneyRecordReferenceNo : null
};

// 步驟1. 定義 getter、action、mutations
// 步驟2. 觸發端 呼叫 setXXXPlayerID
// 步驟3. 接收端需要定義變數 @playersModule.State('prizeRecordPlayerID') prizeRecordPlayerID!: number;。並且watch監控onChanged事件

const SET_PER_GAME_TRAN_NO = 'SET_PER_GAME_TRAN_NO';
const SET_PER_GAME_LOG_ID = 'SET_PER_GAME_LOG_ID';
const SET_PLAYER_ID = 'SET_PLAYER_ID';
const SET_GAME_ID = 'SET_GAME_ID';
const SET_GAME_PLAYER_ID = 'SET_GAME_PLAYER_ID';
const SET_SERVER_ID = 'SET_SERVER_ID';
const SET_WALLET_PLAYER_ID = 'SET_WALLET_PLAYER_ID';
const SET_PRIZE_RECORD_PLAYER_ID = 'SET_PRIZE_RECORD_PLAYER_ID';
const SET_TRABSACTION_NO = 'SET_TRABSACTION_NO';
const SET_EVENT_ID = 'SET_EVENT_ID';
const SET_REFERENCE_NO = 'SET_REFERENCE_NO';

const getters = {
	getPerGameLogID: (state: IState) => state.playerID,
	getPlayerID: (state: IState) => state.playerID,
	getGameID: (state: IState) => state.gameID,
	getGamePlayerID: (state: IState) => state.gamePlayerID,
	getServerID: (state: IState) => state.serverID,
	getWalletPlayerID: (state: IState) => state.walletPlayerID,
	getPrizeRecordPlayerID: (state: IState) => state.prizeRecordPlayerID,
	getTransactionNo: (state: IState) => state.transactionNo,
	getTransactionNoPGS: (state: IState) => state.transactionNoPGS,
	getMoneyRecordReferenceNo: (state: IState) => state.moneyRecordReferenceNo,
	getEventID: (state: IState) => state.eventID,
};

const actions = {

	async setPerGameLogID(context: IActionContextBasic, data: number) {
		context.commit(SET_PER_GAME_LOG_ID, data);
	},	
	async setPlayerID(context: IActionContextBasic, data: number) {
		context.commit(SET_PLAYER_ID, data);
	},
	async setGameID(context: IActionContextBasic, data: number) {
		context.commit(SET_GAME_ID, data);
	},
	async setGamePlayerID(context: IActionContextBasic, data: number) {
		context.commit(SET_GAME_PLAYER_ID, data);
	},
	async setServerID(context: IActionContextBasic, data: number) {
		context.commit(SET_SERVER_ID, data);
	},
	async setWalletPlayerID(context: IActionContextBasic, data: number) {
		context.commit(SET_WALLET_PLAYER_ID, data);
	},	
	async setPrizeRecordPlayerID(context: IActionContextBasic, data: number) {
		context.commit(SET_PRIZE_RECORD_PLAYER_ID, data);
	},	
	async setTransactionNo(context: IActionContextBasic, data: string) {
		context.commit(SET_PER_GAME_TRAN_NO, data);
	},
	async setTransactionNoPGS(context: IActionContextBasic, data: number) {
		context.commit(SET_TRABSACTION_NO, data);
	},
	async setEventID(context: IActionContextBasic, data: number) {
		context.commit(SET_EVENT_ID, data);
	},	
	async setMoneyRecordReferenceNo(context: IActionContextBasic, data: number) {
		context.commit(SET_REFERENCE_NO, data);
	},
};

const mutations = {
	[SET_PER_GAME_LOG_ID](state: IState, payload: number) {
		state.perGameLogID = payload;
	},
	[SET_PLAYER_ID](state: IState, payload: number) {
		state.playerID = payload;
	},
	[SET_GAME_ID](state: IState, payload: number) {
		state.gameID = payload;
	},
	[SET_GAME_PLAYER_ID](state: IState, payload: number) {
		state.gamePlayerID = payload;
	},
	[SET_SERVER_ID](state: IState, payload: number) {
		state.serverID = payload;
	},
	[SET_WALLET_PLAYER_ID](state: IState, payload: number) {
		state.walletPlayerID = payload;
	},
	[SET_PRIZE_RECORD_PLAYER_ID](state: IState, payload: number) {
		state.prizeRecordPlayerID = payload;
	},
	[SET_TRABSACTION_NO](state: IState, payload: string) {
		state.transactionNoPGS = payload;
	},	
	[SET_PER_GAME_TRAN_NO](state: IState, payload: string) {
		state.transactionNo = payload;
	},	
	[SET_EVENT_ID](state: IState, payload: number) {
		state.eventID = payload;
	},	
	[SET_REFERENCE_NO](state: IState, payload: string) {
		state.moneyRecordReferenceNo = payload;
	},	
};

export default {
	namespaced: true,
	state: initState,
	getters,
	actions,
	mutations,
};
