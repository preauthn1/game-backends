<template>
	<v-app v-if="isRenderComponent" class="app">

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-autocomplete clearable :items="dropdownItemsRootMerchants"
										v-model="searchFilter.rootMerchantID" :label="'總代'">
									</v-autocomplete>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-autocomplete clearable :items="dropdownItemsHierarchyMerchants"
										v-model="searchFilter.searchHierarchyMemberMerchantID"
										:label="'商戶 (總代或商戶必填其中一種)'">
									</v-autocomplete>

								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{$t('Button.__search')}}
									</v-btn>&#9;

									<v-btn color="red lighten-1" text class="btn search"
										@click.stop="showAddRootAgent">
										<v-icon left>add</v-icon>
										新增總代
									</v-btn>&#9;

									<v-btn color="grey lighten-1" text class="btn search"
										@click.stop="showMigrateHierarchy">
										<v-icon left>redo</v-icon>
										代理移轉
									</v-btn>&#9;

									<v-btn color="purple lighten-3" text class="btn search"
										@click.stop="checkHierarchyTreeError()">
										<v-icon left>redo</v-icon>
										檢查樹狀異常
									</v-btn>&#9;

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">

							</v-col>

						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<!-- 新增總代-->
		<v-dialog v-model="isShowCreateRootMerchantDialog" max-width="500px">

			<v-card>
				<v-card-title class="grey darken-2">
					新增總代
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>

				</v-card-title>

				<v-card-text>

					<v-row>

						<div v-if="!isSimplifyDropdownItems" class="w100p">
							<v-autocomplete :items="dropdownItemsMerchants" chips
								:label="$t('TableHeader.__merchant') + '(*注意選單簡化)'"
								v-model="selectedRootMerchantID">

								<template v-slot:item="data">
									<div
										:class="data.item.valueInt == null  ? 'merchantCategoryText': 'merchantValue'">
										<span>{{data.item.text}}</span>
									</div>
								</template>

							</v-autocomplete>
						</div>

						<div v-else class="w100p">
							<v-autocomplete clearable :items="dropdownMerchantsWithKeyword" chips
								:search-input.sync="merchantsSearchKeyword" v-model="selectedRootMerchantID"
								class="merchantDropdownItems" :label="$t('TableHeader.__merchant')">

							</v-autocomplete>
						</div>

					</v-row>

				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveAddRootAgent" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>

			</v-card>

		</v-dialog>

		<!-- 代理移轉-->
		<v-dialog v-model="isShowMigrateMerchantDialog" max-width="500px">

			<v-card>
				<v-card-title class="grey darken-2">
					代理移轉 (更換上級代理)
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>

				</v-card-title>

				<v-card-text>

					<v-row>

						<v-autocomplete :items="dropdownItemsHierarchyMerchants" :label="'要調整哪位商戶'" clearable
							v-model="migrateForm.targetMerchantID">

						</v-autocomplete>

					</v-row>

					<v-row>
						<v-autocomplete :items="dropdownItemsHierarchyMerchants" :label="'新的上級代理'" clearable
							v-model="migrateForm.newParentMerchantID">

						</v-autocomplete>
					</v-row>

					<v-row>

						<v-icon color="red">error</v-icon> <span class="minus-red">此功能包含下級代理都會受到影響，請謹慎使用</span>
					</v-row>

				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveMigrateHierarchy" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>

			</v-card>

		</v-dialog>

		<MerchantHierarchyTreeViewPartial ref="refMerchantHierarchyTreeViewPartial">
		</MerchantHierarchyTreeViewPartial>

	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import * as MerchantCreditModel from '@/models/interfaces/agent/merchantCredit';

import MerchantsApi from '@/api/system-setting/merchants';
import EventsApi from '@/api/system-setting/events';
import MerchantCreditApi from '@/api/agent/merchantCredit';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';

import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';

import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';

import MerchantHierarchyTreeViewPartial from '@/components/agent/MerchantHierarchyTreeViewPartial.vue';

import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker, MerchantHierarchyTreeViewPartial }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class MerchantCredits extends Vue {
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_Update) >= 0;
	}

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	isShowCreateRootMerchantDialog: boolean = false;
	isShowMigrateMerchantDialog: boolean = false;
	isRenderComponent: boolean = true;

	merchantsSearchKeyword: string = '';
	pendingTimeoutTask: number = 0;

	selectedRootMerchantID: number = 0;
	searchFilter: MerchantCreditModel.IMerchantHierarchyTreeviewRequest = {} as MerchantCreditModel.IMerchantHierarchyTreeviewRequest;

	migrateForm: MerchantCreditModel.IMerchantHierarchyMigrateRequest = {} as MerchantCreditModel.IMerchantHierarchyMigrateRequest;

	panel: number[] = [0];

	dropdownMerchantsWithKeyword: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsHierarchyMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsRootMerchants: DropdownModel.IDropdownItems[] = [];

	@Watch('merchantsSearchKeyword')
	whenMerchantKeywordChange() {
		clearTimeout(this.pendingTimeoutTask);
		this.pendingTimeoutTask = setTimeout(() => {
			this.searchDropdownByKeywordAPI();
		}, 600);
	}

	searchDropdownByKeywordAPI() {
		const myFilter = {} as DropdownModel.IMerchantDropdownRequest;
		myFilter.keyword = this.merchantsSearchKeyword;
		myFilter.isSimplifyItems = false;

		DropdownApi.getMerchantsByFilter(myFilter, false)
			.then((res) => {
				this.dropdownMerchantsWithKeyword = res;
			})
			.finally(() => {});

		return;
	}

	isSimplifyDropdownItems() {
		const vm: any = this;
		return vm.isEnableSimplifyDropdown();
	}

	search() {
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		this.$nextTick(() => {
			const myRef: any = this.$refs.refMerchantHierarchyTreeViewPartial;
			myRef.searchByFilter(this.searchFilter);
			myRef.setCRUDButtonVisible();
		});

		// EventBus.$emit('show_mask', null);
		// MerchantCreditApi.getMerchantHierarchy(this.searchFilter)
		// 	.then((res) => {
		// 		this.searchResult = res.data;
		// 	})
		// 	.finally(() => {
		// 		EventBus.$emit('hide_mask', null);
		// 	});
	}

	closeAllModal() {
		this.isShowCreateRootMerchantDialog = false;
		this.isShowMigrateMerchantDialog = false;
	}

	isAddButtonVisible(item: MerchantCreditModel.IMerchantHierarchyTreeviewItem) {
		// 目前不開放三層以上
		return item.layer <= 2;
	}

	showAddRootAgent() {
		this.selectedRootMerchantID = 0;
		this.isShowCreateRootMerchantDialog = true;
		this.loadRootMerchantDropdown();
	}

	showMigrateHierarchy() {
		this.migrateForm.targetMerchantID = 0;
		this.isShowMigrateMerchantDialog = true;
		this.loadRootMerchantDropdown();
	}

	saveAddRootAgent() {
		EventBus.$emit('show_mask', null);
		MerchantCreditApi.addRootAgent(this.selectedRootMerchantID)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.loadRootMerchantDropdown();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	checkHierarchyTreeError() {
		if (confirm('是否進行檢查? 可能需花費一點時間。若有異常頻道會發Notify通知')) {
			EventBus.$emit('show_mask', null);
			MerchantCreditApi.checkMerchantTreeError()
				.then((res) => {})
				.finally(() => {
					setTimeout(() => {
						EventBus.$emit('hide_mask', null);
					}, 5000);
				});
		}
	}

	saveMigrateHierarchy() {
		EventBus.$emit('show_mask', null);
		MerchantCreditApi.migrateHierarchy(this.migrateForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.loadRootMerchantDropdown();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	clear() {
		this.searchFilter = {} as MerchantCreditModel.IMerchantHierarchyTreeviewRequest;
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

	loadRootMerchantDropdown() {
		EventBus.$emit('show_mask', null);

		setTimeout(() => {
			EventBus.$emit('hide_mask', null);
		}, 500);

		DropdownApi.getRootAgents().then((res) => {
			this.dropdownItemsRootMerchants = res;
		});
		DropdownApi.getMerchants(true).then((res) => {
			this.dropdownItemsMerchants = res;
			this.dropdownMerchantsWithKeyword = this.dropdownItemsMerchants;
		});
		DropdownApi.getHierarchyMerchants().then((res) => {
			this.dropdownItemsHierarchyMerchants = res;
		});
	}

	mounted() {
		this.loadRootMerchantDropdown();
	}
}
</script>



<style scoped>
</style>