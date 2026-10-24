<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-header>
					<!-- <template v-slot:actions> <v-icon color="primary">$expand</v-icon> </template> -->
					<template v-slot:actions>
						<v-icon color="primary">filter_list</v-icon>
					</template>
					{{$t(`Filter.__filter`)}}
				</v-expansion-panel-header>
				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="merchantsDropdownItems"
										v-model="searchData.merchantID" :label="$t('TableHeader.__merchant')">
									</v-autocomplete>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.playerID" :label="$t('TableHeader.__playerID')"
										append-icon="perm_identity" clearable></v-text-field>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.playerName"
										:label="$t('TableHeader.__playerAccount')" append-icon="person" clearable>
									</v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<!-- <div class="item right"> -->
								<div class="item">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{$t('Button.__search')}}
									</v-btn>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{$t('Button.__reset')}}
									</v-btn>&#9;
									<!-- <v-btn color="light-blue lighten-3" text class="btn search" @click.stop="deletePlayer">
										<v-icon left>delete</v-icon>封号
									</v-btn>-->
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.memo" :label="$t('TableHeader.__memo')"
										append-icon="question_answer" clearable></v-text-field>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<date-picker v-model="timeRange" type="datetime" value-type="timestamp" range
										:placeholder="$t('TableHeader.__createTime')"></date-picker>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete :items="orderByColumn" v-model="searchData.orderByColumn"
										:label="$t('Filter.__sort')"></v-autocomplete>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-checkbox v-model="searchData.isASC" :label="$t('Filter.__increasing')">
									</v-checkbox>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>
		<v-data-table :items="result" :headers="headers" id="table" :disable-sort="true"
			:page="pageIndex" :items-per-page="itemsPerPage" :server-items-length="totalCount"
			:footer-props="footerProps" @update:page="changePage" @update:items-per-page="getItemPerPage">
			<template v-slot:item.playerID="{ item }">
				<v-tooltip top color="teal darken-1">
					<template v-slot:activator="{ on }">
						<a v-on="on" v-show="hasPermissionPlayerDetail" @click="openDetail(item.playerID)">
							<v-icon color="primary" small left>mdi-link</v-icon>
							{{ item.playerID }}
						</a>
					</template>
					<span>{{$t('Button.__openLink')}}</span>
				</v-tooltip>
				<div v-show="!hasPermissionPlayerDetail">{{ item.playerID }}</div>
			</template>
			<template v-slot:item.operateType="{ item }">
				<v-chip v-show="userOperateTypeEnum.User_UserLogin === item.operateType" class="ma-2"
					color="orange">{{$t(`UserOperateTypeEnum.${userOperateTypeEnum[item.operateType]}`)}}
				</v-chip>
				<v-chip v-show="userOperateTypeEnum.Player_LockStatus === item.operateType" class="ma-2"
					color="red">{{$t(`UserOperateTypeEnum.${userOperateTypeEnum[item.operateType]}`)}}
				</v-chip>
				<v-chip v-show="userOperateTypeEnum.Player_UnlockStatus === item.operateType" class="ma-2"
					color="green">{{$t(`UserOperateTypeEnum.${userOperateTypeEnum[item.operateType]}`)}}
				</v-chip>
			</template>
			<template
				v-slot:item.createTime="{ item }">{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}</template>
		</v-data-table>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import { datetimeMixin, startEndDate } from '@/utilities/datetime-format';
import * as Model from '@/models/interfaces/members-management/players';
import PlayersApi from '@/api/members-management/players';
import DropdownApi from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import { UserOperateTypeEnum } from '@/models/status/players';
import * as PermissionModel from '@/models/interfaces/commons/permission';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ mixins: [datetimeMixin], components: { DatePicker } })
export default class LockRecord extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;
	userOperateTypeEnum: typeof UserOperateTypeEnum = UserOperateTypeEnum;
	panel: number[] = [0];
	totalCount: number = 0;
	pageIndex: number = 1;
	footerProps: any = { 'items-per-page-options': [10, 30, 50, 100] };
	itemsPerPage: number = 10;
	timeRange: number[] = [];
	searchData: Model.ILockRecordRequest = {} as Model.ILockRecordRequest;
	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	playerID: number | null = null;
	playerName: string = '';
	result: Model.ILockRecordReponse[] = [];
	get hasPermissionPlayerDetail() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementDetail) >= 0;
	}
	get headers() {
		return [
			{ text: this.$t('TableHeader.__lockTime'), value: 'createTime' },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID' },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName' },
			{ text: this.$t('TableHeader.__operateType'), value: 'operateType' },
		];
	}
	get orderByColumn() {
		return [
			{ text: this.$t('TableHeader.__lockTime'), value: 'createTime' },
			{ text: this.$t('TableHeader.__operateType'), value: 'operateType' },
			{ text: 'IP', value: 'createIPAddress' },
			{ text: this.$t('TableHeader.__memo'), value: 'memo' },
		];
	}

	@Action('Players/setPlayerID') private setPlayerID!: any;
	changePage(index: number = 1) {
		this.pageIndex = index;
		if (this.result.length > 0) {
			this.search();
		}
	}

	getItemPerPage(val: number = 10) {
		this.itemsPerPage = val;
		if (this.result.length > 0) {
			this.search();
		}
	}
	search() {
		const request = this.searchData;
		request.pageIndex = this.pageIndex;
		request.pageSize = this.itemsPerPage;
		request.createTimeBegin = this.timeRange[0] ? startEndDate(this.timeRange[0]) : null;
		request.createTimeEnd = this.timeRange[1] ? startEndDate(this.timeRange[1]) : null;
		request.orderByColumn = this.searchData.orderByColumn;
		request.isASC = this.searchData.isASC;
		PlayersApi.getLockRecord(<Model.ILockRecordRequest>request).then((res) => {
			this.result = res.data.items;
			this.totalCount = res.data.totalCount;
		});
	}

	mounted() {
		this.init();
		// this.search();
		this.registerReloadComponent();
	}

	openDetail(playerID: number) {
		this.setPlayerID(playerID);
		this.$router.push({ path: '/members-management/detail' });
	}

	init() {
		DropdownApi.getMerchants().then((res) => {
			this.merchantsDropdownItems = res;
			this.searchData.orderByColumn = 'createTime';
		});
	}
	clear() {
		this.searchData = {} as Model.ILockRecordRequest;
		this.searchData.orderByColumn = 'createTime';
		this.timeRange = [];
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.clear();
				this.result = [];
				this.renderComponent = true;
			});
		});
	}
}
</script>
<style lang="scss" scoped>
.v-expansion-panel--active > .v-expansion-panel-header {
	min-height: 1% !important;
	max-height: 1% !important;
}
.item {
	max-width: 350px;
	// width: 100%;
	// display: block;
	max-height: 20px !important;
	margin-top: 2px !important;
	margin-bottom: 3px !important;
	&.right {
		text-align: right;
	}
}
.search {
	max-width: 80px;
	margin-top: 15px !important;
}
.mx-datepicker {
	margin-top: 15px !important;
	width: 100% !important;
	width: 100% !important;
}
</style>
