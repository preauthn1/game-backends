<template>
	<v-card>
		<v-card-title class="grey darken-2">{{ $t('TableHeader.__loginRecord') }} {{ playerID }}</v-card-title>
		<v-spacer></v-spacer>
		<v-card-text>
			<v-flex>
				<v-data-table
					:items="loginDetail"
					:headers="headers"
					id="table"
					:disable-sort="true"
					:page="pageIndex"
					:items-per-page="itemsPerPage"
					:server-items-length="totalCount"
					:footer-props="footerProps"
					@update:page="changePage"
					@update:items-per-page="getItemPerPage"
				>
					<template v-slot:item.hdType="{ item }">{{ $t(`HDTypeEnum.${hDTypeEnum[item.hdType]}`) }}</template>
					<template v-slot:item.createTime="{ item }">{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}</template>
				</v-data-table>
			</v-flex>
		</v-card-text>
		<v-card-actions>
			<v-spacer></v-spacer>
			<v-btn text @click="$emit('close')" class="btn">
				<v-icon left>mdi-cancel</v-icon>
				{{ $t('Button.__close') }}
			</v-btn>
		</v-card-actions>
	</v-card>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as Model from '@/models/interfaces/members-management/players';
import PlayersApi from '@/api/members-management/players';
import { HDTypeEnum } from '@/models/status/players';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
@Component({ mixins: [datetimeMixin] })
export default class LoginDetail extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IProfileResponse;
	@Prop(Number) readonly playerID!: number;
	hDTypeEnum: typeof HDTypeEnum = HDTypeEnum;
	totalCount: number = 0;
	pageIndex: number = 1;
	footerProps: any = { 'items-per-page-options': [10, 30, 50, 100] };
	itemsPerPage: number = 10;
	loginDetail: Model.ILoginDetailResponse[] = [];
	get headers() {
		return [
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID' },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName' },
			{ text: this.$t('TableHeader.__time'), value: 'createTime' },
			{ text: 'IP', value: 'createIPAddress' },
			{ text: this.$t('TableHeader.__hdType'), value: 'hdType' },
			{ text: this.$t('TableHeader.__hdBrowser'), value: 'hdBrowser' },
		];
	}
	@Watch('playerID', { immediate: true, deep: true })
	onPlayerIDChange() {
		this.search();
	}
	changePage(index: number = 1) {
		this.pageIndex = index;
		if (this.loginDetail.length > 0) {
			this.search();
		}
	}

	getItemPerPage(val: number = 10) {
		this.itemsPerPage = val;
		if (this.loginDetail.length > 0) {
			this.search();
		}
	}

	search() {
		PlayersApi.getPlayerLoginDetail(<Model.ILoginDetailRequest>{
			playerID: this.playerID,
			pageIndex: this.pageIndex,
			pageSize: this.itemsPerPage,
			// createTimeBegin: '2010-01-01 00:00:00',
			// createTimeEnd: '2030-01-01 00:00:00',
			orderByColumn: 'createTime',
			isASC: false,
		}).then((res) => {
			this.loginDetail = res.data.items;
			this.totalCount = res.data.totalCount;
		});
	}
}
</script>
