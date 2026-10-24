<template>
	<div>
		<div class="mt-4">
			<div v-if="searchResult && searchResult.root" class="pointer" @click="scrollToTop()" style="position: fixed; right: 15%; bottom: 5%; font-size: 30px">
				<v-icon left>mdi-arrow-up</v-icon>返回畫面最上層
			</div>

			<div v-if="searchResult && searchResult.root">
				<div class="eachAgentRow">
					<div class="hoverRow">
						<span class="label-layer"> 總代: </span>
						<div class="chipStyle" @click="copyTextToClipboard2(searchResult.root.copyInfo)">{{ searchResult.root.merchantID }} {{ searchResult.root.name }}</div>

						<span class="ml-2 f12 yellow-text">子代理總數: {{ searchResult.totalAgentCount - 1 }}</span>

						<v-icon @click="showCreateDialog(searchResult.root)" class="addColor" v-if="isCRUDButtonVisible"> add </v-icon>
					</div>
				</div>

				<div class="eachLayer">
					<div v-for="(agent_2, index) in searchResult.root.childs" :key="index" class="mb-2 eachAgentRow">
						<div class="hoverRow">
							<span class="label-layer"> 二級代理: </span>
							<div class="chipStyle" @click="copyTextToClipboard2(agent_2.copyInfo)">{{ agent_2.merchantID }} {{ agent_2.name }}</div>

							<v-icon @click="showCreateDialog(agent_2)" class="addColor" v-if="isAddButtonVisible(agent_2)"> add </v-icon>

							<v-icon @click="confirmDelete(searchResult.root, agent_2)" class="closeColor" v-if="isCRUDButtonVisible"> close </v-icon>
						</div>

						<div class="eachLayer mt-2 mb-4" v-if="agent_2.childs && agent_2.childs.length > 0">
							<div v-for="(agent_3, index3) in agent_2.childs" :key="index3" class="mb-2 eachAgentRow">
								<div class="hoverRow">
									<span class="label-layer"> 三級代理: </span>
									<div class="chipStyle" @click="copyTextToClipboard2(agent_3.copyInfo)">{{ agent_3.merchantID }} {{ agent_3.name }}</div>

									<v-icon @click="showCreateDialog(agent_3)" class="addColor" v-if="isAddButtonVisible(agent_3)"> add </v-icon>

									<v-icon @click="confirmDelete(agent_2, agent_3)" class="closeColor" v-if="isCRUDButtonVisible"> close </v-icon>
								</div>

								<div class="eachLayer mt-2 mb-4" v-if="agent_3.childs && agent_3.childs.length > 0">
									<div v-for="(agent_4, index4) in agent_3.childs" :key="index4" class="mb-2 eachAgentRow">
										<div class="hoverRow">
											<span class="label-layer"> 四級代理: </span>
											<div class="chipStyle" @click="copyTextToClipboard2(agent_4.copyInfo)">{{ agent_4.merchantID }} {{ agent_4.name }}</div>
											<v-icon @click="showCreateDialog(agent_4)" class="addColor" v-if="isAddButtonVisible(agent_4)"> add </v-icon>

											<v-icon @click="confirmDelete(agent_3, agent_4)" class="addColor" v-if="isCRUDButtonVisible"> close </v-icon>
										</div>

										<div class="eachLayer mt-2 mb-4" v-if="agent_4.childs && agent_4.childs.length > 0">
											<div v-for="(agent_5, index5) in agent_4.childs" :key="index5" class="mb-2 eachAgentRow">
												<div class="hoverRow">
													<span class="label-layer"> 五級代理: </span>
													<div class="chipStyle" @click="copyTextToClipboard2(agent_5.copyInfo)">{{ agent_5.merchantID }} {{ agent_5.name }}</div>
													<v-icon @click="showCreateDialog(agent_5)" class="addColor" v-if="isAddButtonVisible(agent_5)"> add </v-icon>

													<v-icon @click="confirmDelete(agent_4, agent_5)" class="addColor" v-if="isCRUDButtonVisible"> close </v-icon>
												</div>
											</div>
										</div>
									</div>
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>

		<!-- 新增下級代理 -->
		<v-dialog v-model="isShowCreateDialog" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					新增下級代理

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<v-row class="mt-4">
						<v-text-field :label="$t('TableHeader.__parent')" disabled="disabled" :value="selectedParentMerchant.merchantID + '｜' + selectedParentMerchant.name"> </v-text-field>
					</v-row>

					<v-row>
						<v-autocomplete :items="dropdownItemsMerchants" :label="$t('TableHeader.__merchant')" clearable v-model="createHierarchyForm.targetMerchantID">
							<template v-slot:item="data">
								<div :class="data.item.valueInt == null ? 'merchantCategoryText' : 'merchantValue'">
									<span>{{ data.item.text }}</span>
								</div>
							</template>
						</v-autocomplete>
					</v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveCreate" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>
	</div>
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
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class MerchantCredits extends Vue {
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_Update) >= 0;
	}

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	isShowCreateDialog: boolean = false;
	isShowCreateRootMerchantDialog: boolean = false;
	isRenderComponent: boolean = true;
	isCRUDButtonVisible: boolean = false;

	selectedRootMerchantID: number = 0;
	searchResult: MerchantCreditModel.IMerchantHierarchyTreeview = {} as MerchantCreditModel.IMerchantHierarchyTreeview;
	searchFilter: MerchantCreditModel.IMerchantHierarchyTreeviewRequest = {} as MerchantCreditModel.IMerchantHierarchyTreeviewRequest;

	selectedParentMerchant: MerchantCreditModel.IMerchantHierarchyTreeviewItem = {} as MerchantCreditModel.IMerchantHierarchyTreeviewItem;
	createHierarchyForm: MerchantCreditModel.IMerchantHierarchyCreateRequest = {} as MerchantCreditModel.IMerchantHierarchyCreateRequest;

	panel: number[] = [0];

	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];

	searchByFilter(filter: MerchantCreditModel.IMerchantHierarchyTreeviewRequest) {
		this.searchFilter = filter;
		this.refresh();
	}

	searchByRootMerchantID(rootMerchantID: number) {
		this.searchFilter.rootMerchantID = rootMerchantID;
		this.refresh();
	}

	setCRUDButtonVisible() {
		this.isCRUDButtonVisible = true;
	}

	refresh() {
		this.registerReloadComponent();

		EventBus.$emit('show_mask', null);
		MerchantCreditApi.getMerchantHierarchy(this.searchFilter)
			.then((res) => {
				this.searchResult = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showCreateDialog(item: MerchantCreditModel.IMerchantHierarchyTreeviewItem) {
		this.isShowCreateDialog = true;
		this.selectedParentMerchant = item;
		this.createHierarchyForm.parentMerchantID = item.merchantID;
	}

	saveCreate() {
		EventBus.$emit('show_mask', null);

		MerchantCreditApi.createMerchantHierarchy(this.createHierarchyForm)
			.then((res) => {
				this.refresh();
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	confirmDelete(parent: MerchantCreditModel.IMerchantHierarchyTreeviewItem, child: MerchantCreditModel.IMerchantHierarchyTreeviewItem) {
		if (confirm('是否確定清除下級代理關係? 【' + parent.name + '】>【' + child.name + '】')) {
			const deleteFrom = {} as MerchantCreditModel.IMerchantHierarchyDeleteRequest;
			deleteFrom.targetMerchantID = child.merchantID;

			EventBus.$emit('show_mask', null);
			MerchantCreditApi.deleteMerchantHierarchy(deleteFrom)
				.then((res) => {
					this.refresh();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	closeAllModal() {
		this.isShowCreateDialog = false;
		this.isShowCreateRootMerchantDialog = false;
	}

	isAddButtonVisible(item: MerchantCreditModel.IMerchantHierarchyTreeviewItem) {
		// 目前不開放三層以上
		return item.layer < this.searchResult.hierarchyMaxLayerLimit && this.isCRUDButtonVisible;
	}

	showAddRootAgent() {
		this.selectedRootMerchantID = 0;
		this.isShowCreateRootMerchantDialog = true;
	}

	saveAddRootAgent() {
		MerchantCreditApi.addRootAgent(this.selectedRootMerchantID)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
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

	scrollToTop() {
		window.scrollTo(0, 0);
	}

	mounted() {
		DropdownApi.getMerchants(true).then((res) => {
			this.dropdownItemsMerchants = res;
		});
	}
}
</script>

<style scoped>
.eachLayer {
	padding: 5px 5px 5px 50px;
}

.label-layer {
	color: #aaa;
	font-size: 12px;
}

.chipStyle {
	color: #fff;
	display: inline-block;
	margin-left: 5px;
	background: #4a4a4a;
	font-size: 14px;
	border-radius: 10px;
	padding: 3px 10px;
	cursor: pointer;
}

.eachAgentRow {
	border-bottom: solid 1px #333;
	padding-bottom: 2px;
}

.hoverRow {
	padding-left: 10px;
	width: 650px;
}

.hoverRow:hover {
	background: #222;
}

.hoverRow .v-icon {
	float: right;
}

.addColor {
	color: #888;
}

.closeColor {
	color: #995050;
}
</style>
