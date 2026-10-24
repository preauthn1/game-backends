<template>
	<v-app v-if="renderComponent" class="app">

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-autocomplete clearable :items="dropdownVendorConfig" chips
										v-model="currentSelectVendorConfigID" :label="$t('TableHeader.__vendorConfig')">
									</v-autocomplete>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<!-- <div class="item right"> -->
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="searchTable">
										<v-icon left>search</v-icon>
										{{$t('Button.__search')}}
									</v-btn>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{$t('Button.__reset')}}
									</v-btn>&#9;

								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table :headers="headers" id="table" :items="searchResult.items" :search="search"
			:page.sync="pageIndex" :items-per-page="itemsPerPage" :footer-props="footerProps" multi-sort>
			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title>

					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>

					<v-card-title>
						<v-text-field v-model="search" append-icon="search"
							:label="`${$t('TableHeader.__gameID')} / ${$t('TableHeader.__gameName')}`" single-line
							hide-details clearable></v-text-field>
					</v-card-title>

					<v-card-title>
						<v-btn icon dark @click="addItem" v-if="hasPermissionCreate">
							<v-icon>add</v-icon>
						</v-btn>
					</v-card-title>
				</v-toolbar>
			</template>

			<template v-slot:item.groupName="{ item }">
				<v-chip v-if="item.groupName && item.groupName !== ''"
					:color="item.isOddSequence ? 'grey darken-2' : 'blue-grey darken-1'">
					{{item.groupName}}
				</v-chip>
			</template>

			<template v-slot:item.jpAmount="{ item }">

				<span :class="item.isOddSequence ? '' : ''">
					{{item.jpAmount | amountDisplay}}
				</span>
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.gameType="{ item }">
				<v-chip class="ma-2">{{$t(`GameType.${gameTypeDic[item.gameType]}`)}}</v-chip>
			</template>

			<template v-slot:item.gameVendorTypeText="{ item }">

				{{item.gameVendorTypeText}}
			</template>

			<template v-slot:item.numberOfMerchants="{ item }">

				<span class="link-text" @click="showGameMerchantDialog(item)">
					{{item.numberOfMerchants}}
				</span>

			</template>

		</v-data-table>

	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import GameLaunchInternalTest from '@/components/system-setting/GameLaunchInternalTest.vue';
import GameMerchantInfoDialog from '@/components/system-setting/GameMerchantInfoDialog.vue';
import EventBus from '@/utilities/event-bus';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import GameListApi from '@/api/system-setting/gameList';
import DropdownApi from '@/api/commons/dropdown';
import InternalTestApi from '@/api/commons/internalTest';
import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { commonMethodMixin } from '@/utilities/common-methods';
import GameManagementApi from '@/api/game-management/per-game';
import * as Model from '@/models/interfaces/game-management/per-game';

import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

@Component({ components: { NotificationWarning }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class AGGJPAmount extends Vue {
	locale: typeof Locale = Locale;
	currency: typeof Currency = Currency;
	status: typeof Status = Status;
	gameTypeDic: typeof GameType = GameType;
	WalletMode: typeof WalletMode = WalletMode;
	CRUDActionEnums: typeof CRUDAction = CRUDAction;
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;

	search: string = '';
	panel: number[] = [0];

	pageIndex: number = 1;
	itemsPerPage: number = 50;
	footerProps: any = { 'items-per-page-options': [10, 20, 50, 100, -1] };

	searchResult: Model.IAGGJackpotAmount = {} as Model.IAGGJackpotAmount;

	currentSelectVendorConfigID: number = 0;
	dropdownVendorConfig: any[] = [];

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_GameList_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_GameList_Update) >= 0;
	}

	get headers() {
		const data = [
			{ text: '彩池群組ID', value: 'groupID', sortable: false, align: 'center', width: '10%' },
			{ text: '彩池群組名稱', value: 'groupName', sortable: false, align: 'left', width: '10%' },
			{ text: '彩池金額', value: 'jpAmount', sortable: false, align: 'end', width: '10%' },
			{ text: '彩池分類名稱', value: 'jpAmountName', sortable: false, align: 'center', width: '15%' },
			{ text: '', value: 'xxxxxxx', sortable: false, align: 'center', width: '50%' },
		];

		return data;
	}

	searchTable() {
		this.pageIndex = 1;
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		EventBus.$emit('show_mask', null);

		GameManagementApi.getAGGJackpotCurrentAmount(this.currentSelectVendorConfigID)
			.then((res) => {
				this.searchResult = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.initialDropdown();
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	initialDropdown() {
		DropdownApi.getVendorConfigSupportAggFeature().then((res) => {
			this.dropdownVendorConfig = res;
		});
	}

	mounted() {
		this.initialDropdown();
	}
}
</script>

<style>
#getGameLinkDialog .v-text-field__details {
	display: none;
}

.statusChips .v-chip {
	min-width: 70px;
	text-align: center;
}

.statusChips .v-chip .v-chip__content {
	margin: auto;
}
</style>