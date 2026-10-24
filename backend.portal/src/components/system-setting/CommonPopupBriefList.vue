<template>

	<v-dialog v-model="isShowPopup" max-width="600px">

		<v-card>

			<v-card-title>
				顯示清單 <span class="f12 ml-2">(最多顯示500筆)</span>

				<v-btn text color="orange lighten-2" @click="copyTextToClipboard2(copyText)"
					class="btn ml-4">
					<v-icon left>file_copy</v-icon>
					複製所有id
				</v-btn>
			</v-card-title>

			<v-divider></v-divider>
			<v-card-text style="min-height:400px;">

				<div style="padding:15px 10px;">

					<div v-for="(item , index) in searchResult" :key="index"
						style="padding: 3px;font-size: 16px;">

						<div v-if="isPlayerLayout">
							{{item.id}}｜
							<span class="link-text" @click="openPlayerDetail(item.id)">{{item.displayName}}</span>
						</div>
						<div v-else>
							{{item.id}}｜{{item.displayName}}
						</div>
					</div>
				</div>

			</v-card-text>

		</v-card>

	</v-dialog>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import BriefListApi from '@/api/commons/briefListApi';
import { commonMethodMixin } from '@/utilities/common-methods';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';

import NotificationWarning from '@/components/notification/NotificationWarning.vue';

import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

@Component({ components: { NotificationWarning }, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class CommonPopupBriefList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	isShowPopup: boolean = false;
	isPlayerLayout: boolean = false;
	copyText: string = '';
	searchFilter: DropdownModel.ILazyCommonParameterReqeust = {} as DropdownModel.ILazyCommonParameterReqeust;
	searchResult: DropdownModel.ICommonBriefInfoListResponse[] = [];

	@Action('Players/setPlayerID') private setPlayerID!: any;

	closeAllModal() {
		this.isShowPopup = false;
	}

	resetAllValues() {
		this.searchResult = [];
	}

	getPlayers(playerIDs: number[]) {
		this.searchFilter.playerIDs = playerIDs;
		this.isShowPopup = true;
		this.copyText = '';
		this.isPlayerLayout = true;
		this.resetAllValues();

		EventBus.$emit('show_mask', null);

		playerIDs.forEach((text) => {
			this.copyText += text + '\n';
		});

		BriefListApi.getBriefPlayers(this.searchFilter)
			.then((res) => {
				this.searchResult = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	openPlayerDetail(playerID: number) {
		this.setPlayerID(playerID);
		this.$router.push({ path: '/members-management/detail' });
	}

	mounted() {}
}
</script>


<style>
.advancePanelContent {
	min-height: 250px;
	padding-bottom: 75px !important;
}

.reviewMerchant {
	padding: 1px;
}

.reviewMerchant > div {
	padding: 3px;
}

.reviewMerchant .label-text {
	color: #ccc;
	width: 175px;
	display: inline-block;
}

.hint-warning-color {
	color: #ef4c4c;
}

.hint-warning {
	color: #ef4c4c;
	font-size: 14px;
	display: inline-block;
}

.hint-warning i {
	color: #ef4c4c !important;
	font-size: 16px !important;
}

.type_0 {
	color: #b5b5b5;
}

.type_20 {
	color: #72cbff;
}

.type_30 {
	color: #ffa67b;
}

.type_40 {
	color: #49d49a;
}

.color-label label {
	color: #fd6262 !important;
	font-size: 12px;
}

.reviewMerchant .col-md-6,
.reviewMerchant .col-md-12 {
	padding: 4px 12px !important;
}
</style>