<template>

	<div>

		<v-card class="">

			<v-card-title class="grey darken-2">

				<v-icon class="mr-2">
					history
				</v-icon>
				Fish Game Detail Records

				<div class="top-right-item">
					<div class="close pointer" @click="closeAllModal">
						<v-icon>close</v-icon>
					</div>
				</div>

			</v-card-title>

			<v-card-text style="padding:5px 35px;" class="">

				<v-row v-if="searchResult && searchResult.items.length > 0">

					<div style="width:100%; height: 600px;overflow-y: scroll;:">

						<v-data-table :items="searchResult.items" :headers="headers" id="table" style=""
							:disable-sort="true" :page="pageIndex" :items-per-page="pageSize" hide-default-footer>

							<template v-slot:item.betTime="{ item }">

								<span class="pointer" style="font-size:13px;"
									@click="globalPopupCopyDateTime(item.betTime , '')">
									{{ item.betTime | timeWithAddTimezone(userProfile.timeZone) }}
								</span>

							</template>

						</v-data-table>

					</div>
				</v-row>

				<v-row v-if="isSearchFinish && searchResult && searchResult.items.length == 0">

					<v-icon>
						info
					</v-icon>
					<div style="padding: 20px;font-size:16px;">
						查无明细纪录
					</div>

				</v-row>

			</v-card-text>
		</v-card>

	</div>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import * as Model from '@/models/interfaces/game-management/per-game';
import GameManagementApi from '@/api/game-management/per-game';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as Permission from '@/models/status/permission';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import { commonMethodMixin } from '@/utilities/common-methods';

import EventBus from '@/utilities/event-bus';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, formatMixin, commonMethodMixin] })
export default class FishGameDetailRecordPopup extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();
	externalLink: string = '';
	searchResult: Model.IFishGameDetailInfoRecord = {} as Model.IFishGameDetailInfoRecord;

	isSearchFinish: boolean = false;
	pageIndex: number = 1;
	pageSize: number = 10000;

	get hasAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagementAdminPermission) >= 0;
	}

	get hasSystenAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get headers() {
		const array = [];

		// array.push({ text: this.$t('TableHeader.__action'), value: 'perGameDetailLogID', width: '3%', align: 'center' });
		array.push({ text: this.$t('TableHeader.__time'), value: 'betTime' });
		array.push({ text: this.$t('TableHeader.__game'), value: 'playerName' });
		array.push({ text: this.$t('TableHeader.__betRecordNo'), value: 'wagerID' });
		array.push({ text: 'fishType', value: 'fishTypeText' });
		array.push({ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end' });
		array.push({ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', align: 'end' });
		array.push({ text: this.$t('TableHeader.__winLossAmount'), value: 'winLossAmount', align: 'end' });
		array.push({ text: '', value: 'extraTag', align: 'end' });

		return array;
	}

	getFishGameRecords(transctionNo: string) {
		EventBus.$emit('show_mask', null);

		const filter = {} as Model.IFishGameDetailRecordQueryRequest;
		filter.transactionNo = transctionNo;

		this.isSearchFinish = false;

		GameManagementApi.getFishGameDetailRecords(filter)
			.then((res) => {
				this.searchResult = res.data;
				this.isSearchFinish = true;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeAllModal() {
		this.$emit('closeParentAllModal');
	}
}
</script>



<style scoped>
.targetHistoryLabel {
	margin-top: 1px;
	color: #888;
}

.targetHistoryValue {
	border-bottom: dashed 1px #888;
	font-size: 16px;
	padding: 2px 0px;
}
</style>