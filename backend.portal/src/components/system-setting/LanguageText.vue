<template>
	<v-app v-if="renderComponent" class="app">

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-text-field v-model="searchFilter.languageCodeLikeQuery"
										:label="$t('TableHeader.__code')" clearable>
									</v-text-field>

								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">

								<v-spacer></v-spacer>

								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{$t('Button.__search')}}
									</v-btn>&#9;

									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{$t('Button.__reset')}}
									</v-btn>&#9;

									<v-btn color="red lighten-3" text class="btn search"
										@click.stop="triggetCreateNotExistGameNameLanguageText">
										<v-icon left>note_add</v-icon>
										檢查並新增【GameList】文字
									</v-btn>&#9;

								</div>

							</v-col>

						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table id="table" :headers="headers" :page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps" :items="searchDataLanguageTexts"
			@update:page="changePage" @update:items-per-page="changeItemPerPage" multi-sort>

			<template v-slot:item.action="{ item }">

				<!-- <v-icon small class="mr-2" @click="showMerchantAdvanceSetting(item)">settings
				</v-icon> -->

			</template>

			<template v-slot:item.allTextList="{ item }">

				<v-chip v-for="(item , index) in item.allTextList" :key="index">

					{{item}}

				</v-chip>

			</template>

			<template v-slot:item.totalCount="{ item }">

				<div class="link-text" @click="showEditModal(item.languageCode)">
					{{item.totalCount}}
				</div>

			</template>

		</v-data-table>

		<v-form ref="loginForm">
			<v-dialog v-model="isShowEditDialog" max-width="500px">
				<v-card>
					<v-card-title class="grey darken-2">

						<span>
							新增／編輯
						</span>
						<div class="top-right-item" @click="closeAllDialog">
							<v-btn icon dark @click="closeAllDialog">
								<v-icon>close</v-icon>
							</v-btn>
						</div>
					</v-card-title>

					<v-card-text>
						<v-flex>
							<v-row v-for="(item , index) in searchDataAllText" :key="index">

								<div class="text-xs-center" style="text-align:center">
									<v-chip
										style="margin-top:20px;margin-right:15px;min-width:100px;text-align:center"
										color="primary">
										{{item.languageText}}
									</v-chip>
								</div>

								<v-text-field v-model="item.text">

								</v-text-field>

							</v-row>
						</v-flex>
					</v-card-text>
					<v-card-actions>
						<v-spacer></v-spacer>

						<v-btn text color="teal lighten-2" v-if="copyInsertSQL && copyInsertSQL != ''"
							v-clipboard:copy="copyInsertSQL" v-clipboard:success="copySuccess" class="btn">
							<v-icon left>file_copy</v-icon>
							{{$t("Button.__copyToClipboard")}}(insert語法)
						</v-btn>

						<v-btn color="yellow darken-1" text @click="save" class="btn">
							<v-icon left>mdi-check-circle</v-icon>
							{{$t('Button.__submit')}}
						</v-btn>
					</v-card-actions>
				</v-card>
			</v-dialog>
		</v-form>

	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import LanguageTextApi from '@/api/system-setting/languageText';
import DropdownApi from '@/api/commons/dropdown';

import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
// import Validation from '@/utilities/validation';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

@Component({ components: { NotificationWarning }, mixins: [commonDisplayFiltersMixin, datetimeMixin] })
export default class Merchants extends Vue {
	locale: typeof Locale = Locale;
	panel: number[] = [0];

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;
	dialog: boolean = false;
	isShowEditDialog: boolean = false;

	selectedLanguageCode: string = '';

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	searchFilter: Model.ILanguageTextPagingRequest = {} as Model.ILanguageTextPagingRequest;
	searchDataLanguageTexts: Model.ILanguageText[] = [];
	searchDataAllText: Model.ILanguageTextItemModel[] = [];
	copyInsertSQL: string = '';

	get langSelectItems() {
		return [
			{ value: Locale.ZHCN, text: this.$t(`Lang.${Locale.ZHCN}`) },
			{ value: Locale.ZHTW, text: this.$t(`Lang.${Locale.ZHTW}`) },
			{
				value: Locale.ENUS,
				text: this.$t(`Lang.${Locale.ENUS}`),
			},
		];
	}
	get currencySelectItems() {
		const vm = this;
		return ALL_CURRENCY.map((item, index, array) => {
			return {
				value: item.value,
				text: vm.$t(item.i18nKey),
			};
		});
	}

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Update) >= 0;
	}
	get hasPermissionDelete() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Delete) >= 0;
	}

	get headers() {
		const data = [
			{ text: '操作', value: 'action', width: '10%', align: 'center' },
			{ text: '多語系代碼', value: 'languageCode', sortable: false, width: '15%' },
			{ text: '已設置語系數', value: 'totalCount', width: '10%' },
			{ text: '文字', value: 'allTextList', width: '35%' },
			{ text: '', value: 'other', width: '20%' },
		];

		return data;
	}

	clear() {
		this.searchFilter = {} as Model.ILanguageTextPagingRequest;
	}

	search() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;

		LanguageTextApi.getLanguageCodes(this.searchFilter).then((res) => {
			this.searchDataLanguageTexts = res.data.items;
			this.DataTablePagingParameter.totalCount = res.data.totalCount;
		});
	}

	triggetCreateNotExistGameNameLanguageText() {
		EventBus.$emit('show_mask', null);

		// 建立未產生多語系的遊戲
		LanguageTextApi.createNotExistGameNameLanguageText()
			.then((res) => {
				alert('檢查並新增完成');
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	getAllTextByLanguageCode(languageCode: string) {
		this.registerReloadComponent();
		LanguageTextApi.getAllTextByLanguageCode(languageCode).then((res) => {
			this.searchDataAllText = res.data.items;
			this.copyInsertSQL = res.data.insertSQL;
		});
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	save() {
		const post: Model.ILanguageBatchUpdateModel = {} as Model.ILanguageBatchUpdateModel;

		post.languageCode = this.selectedLanguageCode;
		post.items = this.searchDataAllText;

		LanguageTextApi.batchUpdate(post).then((res) => {
			EventBus.$emit(`successfully-modified`);
			this.closeAllDialog();
			this.refresh();
		});
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.refresh();
	}

	closeAllDialog() {
		this.isShowEditDialog = false;
		this.copyInsertSQL = '';
	}

	showEditModal(languageCode: string) {
		this.isShowEditDialog = true;
		this.selectedLanguageCode = languageCode;
		this.getAllTextByLanguageCode(languageCode);
	}

	mounted() {
		this.refresh();
	}
}
</script>


<style>
.advancePanelContent {
	min-height: 250px;
	padding-bottom: 75px !important;
}

.v-chip .v-chip__content {
	margin: 0 auto;
}
</style>