<template>

	<div>
		此功能已廢棄 (請改用 代理管理 > 遊戲開關)
	</div>

	<!-- 
	<div>

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-autocomplete clearable :items="dropdownItemsMerchants"
										v-model="searchMerchantID" :label="$t('TableHeader.__merchant')">
									</v-autocomplete>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{$t('Button.__search')}}
									</v-btn>&#9;
								</div>
							</v-col>

						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div class="operate_icons text-l grey-text text-l" v-if="hasSuperAdminPermission"
			style="display: inline-block;">
			<v-icon color="grey" class="mr-2">error</v-icon> <span class="">此页仅管理大厅页的游戏显示</span>

			<v-icon color="grey">error</v-icon> <span class="">若游戏禁止Launch启动。也不会显示在大厅</span>
		</div>

		<div style="width: 30%" class="mt-2"
			v-if="searchResult.lobbyVisibleGames && searchResult.lobbyVisibleGames.length > 0">

			<div>
				<div class="text-r">

				</div>

				<div class="text-r">

				</div>

			</div>

			<div class="v-data-table theme--dark">
				<div class="v-data-table__wrapper">

					<table class="">

						<thead class="v-data-table-header">

							<tr>
								<th class="column-fix-width ">
									{{$t("TableHeader.__gameID")}}
								</th>
								<th class="text-l">
									{{$t("TableHeader.__gameName")}}
								</th>
								<th class="text-r column-fix-width " style="padding-right:40px">
									{{$t("TableHeader.__status")}}
								</th>

							</tr>

						</thead>

					</table>

				</div>

				<div style="height:60vh; overflow:auto;" class="v-data-table__wrapper">

					<table class="">

						<tbody>
							<tr v-for="(item , index) in searchResult.lobbyVisibleGames" :key="index">
								<td class="column-fix-width ">
									{{item.gameID}}
								</td>
								<td>
									{{item.gameName}}
								</td>
								<td class="column-fix-width ">
									<div v-if="hasPermissionUpdate" class="text-r">
										<v-switch v-model="item.isEnable" color="green lighten-1" class="mt-3 mt-0"
											@change="switchValueAndDelaySave(item.gameID)" right>
										</v-switch>
									</div>
								</td>

							</tr>

						</tbody>

					</table>
				</div>

			</div>

		</div>

		<div style="display:none">
			<v-data-table></v-data-table>
		</div>

	</div> -->

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import * as MerchantCreditModel from '@/models/interfaces/agent/merchantCredit';
import * as GameSwitchModel from '@/models/interfaces/game-setting/switch';
import MerchantsApi from '@/api/system-setting/merchants';
import EventsApi from '@/api/system-setting/events';
import MerchantCreditApi from '@/api/agent/merchantCredit';
import GameSettingSwitchApi from '@/api/game-setting/switch';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';

import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';

import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class LobbyVisiableGameSwitch extends Vue {
	get hasSuperAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingServerRobotConfig_Update) >= 0;
	}

	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Merchant_SelfUpdateSetting_Update) >= 0;
	}

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	isRenderComponent: boolean = true;

	searchMerchantID: number = 0;
	searchResult: GameSwitchModel.IMerchantSelfUpdateSetting = {} as GameSwitchModel.IMerchantSelfUpdateSetting;
	searchFilter: MerchantCreditModel.IQueryMerchantCreditChangeLogApiRequestParameter = {} as MerchantCreditModel.IQueryMerchantCreditChangeLogApiRequestParameter;

	panel: number[] = [0];
	lastEditGameID: number = 0;

	dropdownItemsCreditChangeTypes: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];

	search() {
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		EventBus.$emit('show_mask', null);

		GameSettingSwitchApi.getMerchantSelfUpdateEnableGames(this.searchMerchantID)
			.then((res) => {
				this.searchResult = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	switchValueAndDelaySave(gameID: number) {
		this.lastEditGameID = gameID;

		// 閒置1秒後才送出儲存
		setTimeout(() => {
			if (this.lastEditGameID === gameID) {
				this.save();
			}
		}, 1000);
	}

	save() {
		EventBus.$emit('show_mask', null);

		setTimeout(() => {
			GameSettingSwitchApi.UpdateMerchantSelfUpdateEnableGames(this.searchResult)
				.then((res) => {})
				.finally(() => {
					setTimeout(() => {
						EventBus.$emit('hide_mask', null);
					}, 500);
				});
		}, 50);
	}

	closeAllModal() {}

	clear() {
		this.searchFilter = {} as MerchantCreditModel.IQueryMerchantCreditChangeLogApiRequestParameter;
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.isRenderComponent = false;
			this.$nextTick(() => {
				this.isRenderComponent = true;
			});
		});
	}

	mounted() {
		DropdownApi.getMerchants().then((res) => {
			this.dropdownItemsMerchants = res;
		});
	}
}
</script>


<style scoped>
.column-fix-width {
	width: 20%;
}
</style>