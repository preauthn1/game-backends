import Vue from 'vue';
import Router from 'vue-router';
import Vuex from 'vuex';
import { checkAuth } from '@/router/auth';
import PermissionApi from '@/api/commons/permission';
import * as Model from '@/models/interfaces/commons/permission';
import store from '@/store/index';
Vue.use(Router);
Vue.use(Vuex);
const routes = [
	{
		path: '/',
		name: 'home',
		component: () => import('@/views/others/Home.vue'),
	},
	{
		path: '/home',
		name: 'home',
		component: () => import('@/views/others/Home.vue'),
	},
	{
		path: '/error',
		name: 'error',
		component: () => import('@/views/others/Error.vue'),
	},	
	
	{
		path: '/members-management/list',
		name: '',
		component: () => import('@/views/members-management/List.vue'),
	},
	{
		path: '/members-management/detail/:playerid?',
		name: '',
		component: () => import('@/views/members-management/Detail.vue'),
	},
	{
		path: '/members-management/money-record',
		name: '',
		component: () => import('@/views/members-management/Money.vue'),
	},
	{
		path: '/members-management/bet-record',
		name: '',
		component: () => import('@/views/members-management/Bet.vue'),
	},
	{
		path: '/members-management/lock',
		name: '',
		component: () => import('@/views/members-management/Lock.vue'),
	},
	{
		path: '/members-management/transfer-record',
		name: '',
		component: () => import('@/views/members-management/TransferRecord.vue'),
	},

	{
		path: '/members-management/game-vendor-transfer-record',
		name: '',
		component: () => import('@/views/members-management/TransferRecordOfGameVendorPage.vue'),
	},

	{
		path: '/members-management/third-game-login-logs',
		name: '',
		component: () => import('@/views/members-management/ThirdPartyPlayerLoginLogsPage.vue'),
	},

	{
		path: '/games-management/record/:perGameLogID?',
		name: '',
		component: () => import('@/views/game-management/Record.vue'),
	},

	{
		path: '/games-management/union-bet-record',
		name: '',
		component: () => import('@/views/game-management/UnionBetRecordPage.vue'),
	},
	{
		path: '/games-management/servers-health-summary',
		name: '',
		component: () => import('@/views/game-management/ServersHealthSummary.vue'),
	},


	{
		path: '/customized-columns/dynamic-query',
		name: '',
		component: () => import('@/views/game-management/CustomizedColumnsQueryPage.vue'),
	},

	{
		path: '/games-management/bet-record-search',
		name: '',
		component: () => import('@/views/game-management/BetRecordSearchPage.vue'),
	},

	{
		path: '/games-management/notify-queue-list',
		name: '',
		component: () => import('@/views/game-management/CallbackNotifyQueueListPage.vue'),
	},


	{
		path: '/games-management/agg-jp-amount',
		name: '',
		component: () => import('@/views/game-management/AGGJPAmountPage.vue'),
	},

	{
		path: '/game-setting/switch',
		name: '',
		component: () => import('@/views/others/Switch.vue'),
	},

	{
		path: '/operator-management/accounts',
		name: '',
		component: () => import('@/views/system-setting/Accounts.vue'),
	},

	{
		path: '/system-setting/IMChannelList',
		name: '',
		component: () => import('@/views/others/IMChannelListPage.vue'),
	},

	{
		path: '/statistics/merchant-summary',
		name: '',
		component: () => import('@/views/statistics/MerchantSummary.vue'),
	},

	{
		path: '/statistics/game',
		name: '',
		component: () => import('@/views/statistics/Game.vue'),
	},

	{
		path: '/statistics/player-game',
		name: '',
		component: () => import('@/views/statistics/PlayerGameSummaryPage.vue'),
	},

	{
		path: '/statistics/free-spin',
		name: '',
		component: () => import('@/views/statistics/FreespinReportPage.vue'),
	},

	{
		path: '/statistics/free-spin-status-list',
		name: '',
		component: () => import('@/views/statistics/FreespinStatusListPage.vue'),
	},

	{
		path: '/statistics/agent-hierarchy-daily-report',
		name: '',
		component: () => import('@/views/statistics/AgentHierarchyDailyReportPage.vue'),
	},

	{
		path: '/statistics/agent-hierarchy-daily-game-report',
		name: '',
		component: () => import('@/views/statistics/AgentHierarchyDailyGameReportPage.vue'),
	},

	{
		path: '/statistics/agent-special-report',
		name: '',
		component: () => import('@/views/statistics/AgentSpecialReportPage.vue'),
	},

	{
		path: '/statistics/agent-hierarchy-total-report',
		name: '',
		component: () => import('@/views/statistics/AgentTotalSummaryReportPage.vue'),
	},


	{
		path: '/statistics/merchant-daily-report',
		name: '',
		component: () => import('@/views/statistics/MerchantDailyReportPage.vue'),
	},

	{
		path: '/statistics/blank-template-xxxxx',
		name: '',
		component: () => import('@/views/statistics/BlankTemplateReportPage.vue'),
	},
	
	{
		path: '/statistics/player-winLoss-ranking-report',
		name: '',
		component: () => import('@/views/statistics/PlayerWinLossRankingReportPage.vue'),
	},

	{
		path: '/statistics/retention-rate',
		name: '',
		component: () => import('@/views/statistics/RetentionReportPage.vue'),
	},

	{
		path: '/statistics/finance-compare-report',
		name: '',
		component: () => import('@/views/statistics/MerchantSummaryFinanceComparePage.vue'),
	},

	{
		path: '/statistics/merchant-all-player-winloss-report',
		name: '',
		component: () => import('@/views/statistics/MerchantAllPlayerWinLossReportPage.vue'),
	},

	{
		path: '/statistics/eazy-query-for-partner',
		name: '',
		component: () => import('@/views/statistics/EazyQueryForPartnerPage.vue'),
	},

	{
		path: '/system-setting/organizations',
		name: '',
		component: () => import('@/views/system-setting/Organizations.vue'),
	},

	{
		path: '/system-setting/roles',
		name: '',
		component: () => import('@/views/system-setting/Roles.vue'),
	},

	{
		path: '/system-setting/merchants/compare',
		name: '',
		component: () => import('@/views/others/MerchantComparePage.vue'),
	},

	{
		path: '/system-setting/merchants',
		name: '',
		component: () => import('@/views/system-setting/Merchants.vue'),
	},	

	{
		path: '/system-setting/OneTimeJobManagement',
		name: '',
		component: () => import('@/views/system-setting/OneTimeJobPage.vue'),
	},	

	{
		path: '/system-setting/merchants-settings',
		name: '',
		component: () => import('@/views/system-setting/MerchantCustomizedSettingListPage.vue'),
	},	

	{
		path: '/system-setting/language-text',
		name: '',
		component: () => import('@/views/system-setting/LanguageTextPage.vue'),
	},
	{
		path: '/system-setting/gameList',
		name: '',
		component: () => import('@/views/system-setting/GameList.vue'),
	},

	{
		path: '/system-setting/game-vendor-rtp',
		name: '',
		component: () => import('@/views/system-setting/GameVendorRTPPage.vue'),
	},

	{
		path: '/system-setting/AccountingInvoice',
		name: '',
		component: () => import('@/views/system-setting/AccountingInvoicePage.vue'),
	},

	{
		path: '/system-setting/events',
		name: '',
		component: () => import('@/views/system-setting/EventListPage.vue'),
	},

	{
		path: '/system-setting/event-prize-record',
		name: '',
		component: () => import('@/views/system-setting/EventPrizeRecordPage.vue'),
	},

	{
		path: '/system-setting/event-detail',
		name: '',
		component: () => import('@/views/system-setting/EventDetailPage.vue'),
	},

	{
		path: '/system-setting/event-draw-prize',
		name: '',
		component: () => import('@/views/system-setting/EventDrawPrizePage.vue'),
	},


	{
		path: '/system-setting/announcement',
		name: '',
		component: () => import('@/views/system-setting/AnnouncementPage.vue'),
	},

	{
		path: '/system-setting/gamePayTableImage',
		name: '',
		component: () => import('@/views/system-setting/GamePayTableImagePage.vue'),
	},

	{
		path: '/system-setting/servers',
		name: '',
		component: () => import('@/views/system-setting/Servers.vue'),
	},
	{
		path: '/system-setting/batch-edit-servers-merchant',
		name: '',
		component: () => import('@/views/system-setting/BatchEditServerMerchant.vue'),
	},
	{
		path: '/system-setting/server-robot-config',
		name: '',
		component: () => import('@/views/system-setting/ServersRobotConfig.vue'),
	},

	{
		path: '/system-setting/ipAddress-template',
		name: '',
		component: () => import('@/views/system-setting/IPAddressTemplateListPage.vue'),
	},

	{
		path: '/system-setting/system-config-setting',
		name: '',
		component: () => import('@/views/system-setting/SystemConfigSettingPage.vue'),
	},

	{
		path: '/system-setting/OneToManyCustomizedSettingPage',
		name: '',
		component: () => import('@/views/system-setting/OneToManyCustomizedSettingPage.vue'),
	},

	{
		path: '/system-setting/CustomerServiceTool',
		name: '',
		component: () => import('@/views/system-setting/CustomerServiceToolPage.vue'),
	},

	{
		path: '/system-setting/generalTags',
		name: '',
		component: () => import('@/views/system-setting/GeneralTagPage.vue'),
	},

	{
		path: '/system-setting/TGAnnouncement',
		name: '',
		component: () => import('@/views/system-setting/TGAnnouncementPage.vue'),
	},


	{
		path: '/system-setting/vendor-maintenance',
		name: '',
		component: () => import('@/views/system-setting/VendorMaintenancePage.vue'),
	},

	{
		path: '/system-setting/vendor-config',
		name: '',
		component: () => import('@/views/system-setting/VendorConfigPage.vue'),
	},

	{
		path: '/system-logs/ExternalApiRequestLog',
		name: '',
		component: () => import('@/views/others/ExternalApiRequestLogPage.vue'),
	},

	{
		path: '/system-logs/SingleWalletPlatformReqeustLog',
		name: '',
		component: () => import('@/views/others/SingleWalletPlatformReqeustLogPage.vue'),
	},

	{
		path: '/system-logs/systemLogApiRequestLog',
		name: '',
		component: () => import('@/views/others/SystemLogApiRequestLogPage.vue'),
	},	

	{
		path: '/Other/OrganizationAccountManagement',
		name: '',
		component: () => import('@/views/others/OrganizationAccountManagementPage.vue'),
	},

	{
		path: '/Agent/MerchantCredits',
		name: '',
		component: () => import('@/views/others/MerchantCreditsPage.vue'),
	},

	{
		path: '/Agent/MerchantCreditChangeLog',
		name: '',
		component: () => import('@/views/others/MerchantCreditsChangeLogPage.vue'),
	},

	{
		path: '/Agent/MerchantHierarchyTreeView',
		name: '',
		component: () => import('@/views/others/MerchantHierarchyTreeViewPage.vue'),
	},

	{
		path: '/merchant-self-update/game-switch',
		name: '',
		component: () => import('@/views/others/MerchantSelfUpdateGameSwitchPage.vue'),
	},

	{
		path: '/merchant-self-update/BOAnnouncementPage',
		name: '',
		component: () => import('@/views/others/BOAnnouncementPage.vue'),
	},

	{
		path: '/merchant-self-update/DownloadResource',
		name: '',
		component: () => import('@/views/others/DownloadResourcePage.vue'),
	},
	

	{
		path: '/records/user-operate-records',
		name: '',
		component: () => import('@/views/others/UserOperateRecordPage.vue'),
	},


	/* path不要超過兩層 不然底層的router似乎會一直顯示 沒有訪問頁面的權限 */
	/* path不要超過兩層 不然底層的router似乎會一直顯示 沒有訪問頁面的權限 */
	/* path不要超過兩層 不然底層的router似乎會一直顯示 沒有訪問頁面的權限 */
	/* path不要超過兩層 不然底層的router似乎會一直顯示 沒有訪問頁面的權限 */
	/* path不要超過兩層 不然底層的router似乎會一直顯示 沒有訪問頁面的權限 */
	/* path不要超過兩層 不然底層的router似乎會一直顯示 沒有訪問頁面的權限 */

	{
		path: '/debug/debug',
		name: 'XXXXXXXXXXXXXXXXX',
		component: () => import('@/views/others/DebugPage.vue'),
	},	

];
const host = window.location.hostname.split('.');

const router = new Router({
	mode: 'history',
	routes,
});

router.beforeEach((to, from, next) => {
	next();
	let href = to.path;
	// 路徑參數移除
	if (href.split('/').length > 3) {
		href = `/${href.split('/')[1]}/${href.split('/')[2]}`;
	}
	const temp: any = store.state;
	
	if (href === '/xxxxxxxxxxxxxxxxxxx' ||
		href === '/xxxxxxxxxxxxxxx' ||
		// href === '/statistics/blank-template-xxxxx' ||
		href === '/debug/debug' || 
		href === '/xxxxxxxxxxxxxxx' || 
		href === '/XXXXXXXXXXXXXXXXXXXXX' || 
		href === '/XXXXXXXXXXXXXXXXXXXXXXXX' || 
		href === '/xxxxxxxxxxxxxxxxxxxxxxx') {
		// 開發中的頁面不做檢查
	} else if (temp && temp.Auth.allPermissions.length > 0) {

		
		if (temp.Auth.allPermissions.findIndex((s: any) => s.pageUrl === href) > -1 || to.name === 'error' || to.name === 'home' /*|| !to.path */) {
		} else {
			window.location.href = '/error';
		}
	} else {
		
		PermissionApi.getMyPermissions().then(res => {
			store.dispatch('Auth/setAllPermissions', res.data.allPermissionsIncludeUserPermission);
			store.dispatch('Auth/setNestedPermissions', res.data.nestedPermissions);
			if (res.data.allPermissions.findIndex(s => s.pageUrl === href) > -1 || to.name === 'error' || to.name === 'home' /*|| !to.path */) {
			} else {
				
				window.location.href = '/error';
			}
		});
	}
	
	window.document.body.scrollTop = 0;
	window.document.documentElement.scrollTop = 0;
	if (store) {
		store.dispatch('Auth/setCurrentPath', to.fullPath);
	}
});

export default router;
