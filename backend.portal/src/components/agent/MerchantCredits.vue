<template>
	<v-app v-if="isRenderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.keyword" :label="$t('TableHeader.__keywordSearch') + ' (e.g.' + $t('TableHeader.__merchantName') + ')'" clearable>
									</v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownItemsAgentMerchants" v-model="searchFilter.parentMerchantID" :label="$t('Message.__searchChildsByParentAgent')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div class="mt-4 search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search mt-1 mr-1" @click.stop="search">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }}
									</v-btn>

									<v-btn color="pink lighten-3" text class="btn search mt-1 mr-1" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }}
									</v-btn>

									<v-btn color="light-blue lighten-3" text class="btn search mt-1 mr-1" v-if="hasBatchExportPermission" @click.stop="exportAllApiKey">
										<v-icon left>cloud_download</v-icon>
										{{ $t('Button.__exportApiKey') }} ({{ $t('TableHeader.__searchResult') }})
									</v-btn>

									<v-btn color="light-blue lighten-3" text class="btn search mt-1 mr-1" @click.stop="exportBasicInfo">
										<v-icon left>cloud_download</v-icon>
										{{ $t('Button.__downloadFile') }} ({{ $t('TableHeader.__basicInfo') }})
									</v-btn>

									<v-btn color="light-green lighten-3" text class="btn search mt-1 mr-1" v-if="hasPermissionToCreateChild" @click.stop="showCreateChildAgentDialog">
										<v-icon left>add</v-icon>
										{{ $t('Menu.Agent_MerchantCredit_CreateChildAgent') }}
									</v-btn>

									<v-btn color="light-green lighten-3" text class="btn search mt-1 mr-1" v-if="hasPermission_CreateDevMerchantPermission" @click.stop="showCreateDevMerchantDialog">
										<v-icon left>add</v-icon>
										{{ $t('Menu.Agent_MerchantCredit_CreateDevMerchant') }}
									</v-btn>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt-2">
								<div class="minus-mt-4">
									<v-autocomplete
										clearable
										:items="dropdownItemsAgentMerchants"
										chips
										v-model="searchFilter.hierarchyOfTargetMerchant"
										:label="$t('Message.__searchHierarchyOfTargetAgent')"
									>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt-2">
								<div class="minus-mt-4">
									<v-autocomplete :items="dropdownItemsGames" chips clearable :label="$t('TableHeader.__enableGamesFilter')" v-model="searchFilter.enableSpecificGameID">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt-2">
								<div class="minus-mt-4">
									<v-autocomplete :items="dropdownItemsCurrency" v-model="searchFilter.currency" clearable :label="$t('TableHeader.__currency')" chips> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt-2">
								<div class="minus-mt-4">
									<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime" value-type="timestamp" range :placeholder="$t('TableHeader.__createTime')">
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt-2" v-if="hasSuperPermission || hasPermission_Special_AMPermission">
								<div class="minus-mt-4">
									<v-autocomplete :items="dropdownItemsIMChannels" v-model="searchFilter.IMChannelMappingID" clearable :label="$t('TableHeader.__imChannelName')" chips>
									</v-autocomplete>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div class="hint-row">
			<div v-if="!isAllowEditCreditByAccountType && isShowComponentAfterFewDely"><v-icon small class="mr-1">error</v-icon> {{ $t('Message.__canNotEditCreditThisAccount') }}</div>
		</div>

		<v-data-table
			:headers="headers"
			id="table"
			:items="searchList"
			class=""
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps"
			@update:page="changePage"
			@update:items-per-page="changeItemPerPage"
		>
			<template v-slot:item.action="{ item }">
				<v-tooltip top v-if="hasExportApiKeyPermission">
					<template v-slot:activator="{ on, attrs }">
						<v-icon class="ml-1" color="" dark v-bind="attrs" v-on="on" small @click.stop="exportSingleApiKey(item.merchantID, item.name)"> cloud_download </v-icon>
					</template>
					<span>{{ $t('Button.__exportApiKey') }}</span>
				</v-tooltip>

				<v-tooltip top v-if="hasTriggerSwValidateTestPermission">
					<template v-slot:activator="{ on, attrs }">
						<v-icon
							color="yellow darken-2"
							class="ml-1"
							:disabled="!item.isSingleWallet"
							dark
							v-bind="attrs"
							v-on="on"
							small
							@click.stop="showTriggerSwTestDialog(item.merchantID, item.name)"
						>
							mdi-star-circle
							<!-- mdi-white-balance-sunny -->
						</v-icon>
					</template>
					<span>Trigger SW Validate Test</span>
				</v-tooltip>
			</template>

			<template v-slot:item.parentMerchantName="{ item }">
				<span :title="item.parentMerchantID">
					{{ item.parentMerchantName }}
				</span>
			</template>

			<template v-slot:item.isCreditModeEnable="{ item }">
				<v-chip v-if="item.isCreditModeEnable" small>
					{{ $t('Button.__yes') }}
				</v-chip>
			</template>

			<template v-slot:item.merchantID="{ item }">
				<div>
					{{ item.merchantID }}
				</div>
			</template>

			<template v-slot:item.name="{ item }">
				<div>
					{{ item.name }}
				</div>
				<div class="sub-title-text">
					{{ item.memoDisplay }}
				</div>
			</template>

			<template v-slot:item.currencyText="{ item }">
				<div>
					{{ item.currencyText }}
				</div>
				<div class="grey-text f12" style="margin-top: -3px">{{ item.walletModeText }} <v-chip x-small class="ml-1" color="purple" v-if="item.isSingleWallet">SW</v-chip></div>
			</template>

			<template v-slot:item.isUnlimit="{ item }">
				<div class="">
					<v-chip color="red darken-2" v-if="item.isUnlimit" small>
						{{ $t('TableHeader.__unlimitCredit') }}
					</v-chip>

					<v-chip color="red darken-2" v-if="item.isSingleWalletDoaminNotSetup" small> {{ $t('Message.__notSetValueHint') }} endpoint </v-chip>

					<v-chip v-for="(tag, index) in item.extraTags" :key="index" color="red darken-2" small>
						{{ tag }}
					</v-chip>

					<v-chip v-for="(tag, index) in item.extraTagsBlackColor" :key="index" color="grey darken-3" small>
						{{ tag }}
					</v-chip>
				</div>
			</template>

			<template v-slot:item.enableGamesCount="{ item }">
				<div @click="showEnableGames(item.merchantID)" class="link-text-grey-gameCount">
					<span v-if="item.parentMerchantID <= 0">{{ item.enableGamesCount }}</span>
					<span v-else-if="item.notEnableGamesCount <= 0">{{ item.enableGamesCount }}</span>
					<span v-else> {{ item.enableGamesCount }} ({{ item.notEnableGamesCount }}) </span>
				</div>
			</template>

			<template v-slot:item.extra="{ item }">
				<div @click="showAdvanceSettingEditDialog(item)" class="link-text-white">
					{{ $t('Button.__advanceSetting') }}
				</div>
			</template>

			<template v-slot:item.creditAmount="{ item }">
				<span
					v-if="hasPermissionUpdate && !item.isSelfCredit && !item.isSingleWallet && item.isCreditModeEnable && isAllowEditCreditByAccountType"
					class="link-text extra-underline"
					@click="showPreview(item)"
				>
					{{ item.creditAmount | amountDisplay }}
				</span>
				<span v-else-if="item.isSingleWallet"> - </span>
				<span v-else>
					{{ item.creditAmount | amountDisplay }}
				</span>
			</template>

			<template v-slot:item.statusText="{ item }">
				<div v-if="hasPermissionToEditStatus && !item.isSelfMerchant" @click="showEditStatus(item)">
					<span :class="item.status == 1 ? 'link-text-green' : 'link-text-red'">
						{{ item.statusText }}
					</span>
				</div>
				<div v-else>
					{{ item.statusText }}
				</div>
			</template>

			<template v-slot:item.numberOfAccount="{ item }">
				<div class="link-text" v-if="item.numberOfAccount > 0" @click="showUserAccounts(item.merchantID)">
					{{ item.numberOfAccount }}
				</div>
				<div v-else>
					{{ item.numberOfAccount }}
				</div>
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>
		</v-data-table>

		<!-- 調整額度 -->
		<v-dialog v-model="dialogVisible.isShowPreviewDialog" max-width="400px">
			<v-card style="border: solid 1px #444">
				<v-card-title>
					{{ $t('TableHeader.__adjustCreditAmount') }}

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text v-if="searchPreview.myCredit">
					<div class="leftAndRight mb-4">
						<div class="flexItem">
							<div class="label-bg">
								<v-chip small color="green darken-3" class="abs-left">
									<span v-if="!searchPreview.myCredit.isSuperAdmin && !searchPreview.myCredit.isMultipleHierarchy"> {{ searchPreview.myCredit.merchantID }}｜ </span
									>{{ searchPreview.myCredit.merchantName }}
								</v-chip>

								{{ $t('TableHeader.__yourRemainingCreditAmount') }}
							</div>

							<div class="wallet-amount">
								<div v-if="searchPreview.myCredit.isUnlimit">
									<!-- 無限額度∞ -->
									{{ searchPreview.myCredit.creditAmount | amountDisplay }}
								</div>
								<div v-else>
									{{ searchPreview.myCredit.creditAmount | amountDisplay }}
								</div>
							</div>
						</div>
					</div>

					<div style="padding-left: 10px; margin-top: 25px">
						<div>
							<v-text-field
								:label="$t('TableHeader.__agent')"
								:value="searchPreview.targetMerchantCredit.merchantName + ' (' + searchPreview.targetMerchantCredit.merchantID + ')'"
								disabled="disabled"
							>
							</v-text-field>
						</div>

						<div>
							<v-text-field
								:label="$t('TableHeader.__agentRemainingCreditAmount') + ' (' + searchPreview.targetMerchantCredit.currency + ')'"
								:value="searchPreview.targetMerchantCredit.creditAmount"
								class="text-r"
								disabled="disabled"
							>
							</v-text-field>
						</div>

						<div>
							<v-autocomplete :items="getAddOrMinusDropdwon" v-model="updateCreditForm.plusType" chips :label="$t('TableHeader.__operationType')"> </v-autocomplete>
						</div>

						<div>
							<v-text-field
								:label="$t('TableHeader.__adjustValue') + ' (' + searchPreview.targetMerchantCredit.currency + ')'"
								v-model="updateCreditForm.updateAmount"
								placeholder="0.00"
							>
							</v-text-field>
						</div>

						<div>
							<v-text-field :label="$t('TableHeader.__memo')" v-model="updateCreditForm.memo" maxlength="50"> </v-text-field>
						</div>
					</div>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveUpdateCredit" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 代理自行新增下級代理 -->
		<v-dialog v-model="dialogVisible.isShowCreate" max-width="900px" persistent>
			<v-card style="border: solid 1px #444">
				<v-card-title class="grey darken-2">
					{{ $t('Menu.Agent_MerchantCredit_CreateChildAgent') }}
					<span class="red-text f13 ml-3 mt-1">*{{ $t('Message.__allFieldRequired') }}</span>

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text class="padding-row">
					<div>
						<v-row>
							<v-col cols="12" md="6" sm="6" class="my-padding-by-device">
								<v-row>
									<div class="categoryTitle">
										<v-icon>settings</v-icon>
										{{ $t('TableHeader.__merchantOrAgent') }}
									</div>
								</v-row>

								<v-row>
									<v-autocomplete
										:items="dropdownItemsLayer2Agents"
										:label="$t('TableHeader.__parent')"
										@change="whenChangeTemplateParentMerchant()"
										small-chips
										v-model="copyAgentForm.parentMerchantID"
									>
									</v-autocomplete>
								</v-row>

								<v-row>
									<v-autocomplete
										:items="walletModeSelectItems"
										:disabled="!hasSelectWalletModePermission"
										:label="$t('TableHeader.__walletMode')"
										small-chips
										v-model="copyAgentForm.walletMode"
									>
									</v-autocomplete>
								</v-row>

								<v-row v-if="hasSelectIMChannelPermission">
									<v-autocomplete :items="dropdownItemsIMChannels" :label="$t('TableHeader.__imChannelName') + '(TG/Skype)'" small-chips v-model="copyAgentForm.imChannelID">
									</v-autocomplete>
								</v-row>

								<v-row v-show="!isOrgDropdownInvisible">
									<v-autocomplete
										:items="dropdownItemsOrganization"
										small-chips
										@change="organizationDropdownChanged()"
										:label="$t('TableHeader.__organization')"
										v-model="copyAgentForm.organizationID"
									>
									</v-autocomplete>
								</v-row>

								<v-row>
									<v-text-field :label="$t('TableHeader.__merchantName') + ' (' + $t('TableHeader.__agent') + ')'" v-model="copyAgentForm.name" maxlength="20"> </v-text-field>
								</v-row>

								<v-row v-if="availableGamesForCopyAgent.availableGamesDropdown && availableGamesForCopyAgent.availableGamesDropdown.length > 0">
									<div style="width: 100%">
										<v-checkbox :label="$t('Message.__enableGameByAgentDefault')" v-model="isLockEditGameIDsForCopyAgent"> </v-checkbox>
										<v-chip color="blue darken-4 mr-2" small>{{ $t('TableHeader.__slots') }}: {{ getNumberOfTargetGameType(1000) }}</v-chip>
										<v-chip color="blue darken-4 mr-2" small>{{ $t('TableHeader.__liveCasino') }}: {{ getNumberOfTargetGameType(2500) }}</v-chip>
										<v-chip color="blue darken-4 mr-2" small>{{ $t('TableHeader.__others') }}: {{ getNumberOfOtherGameType() }}</v-chip>
									</div>

									<v-autocomplete
										:label="$t('TableHeader.__game')"
										v-model="copyAgentForm.gameIDs"
										@blur="whenChangeSelectedGames()"
										v-show="!isLockEditGameIDsForCopyAgent"
										clearable
										deletable-chips
										small-chips
										multiple
										:items="availableGamesForCopyAgent.availableGamesDropdown"
									>
									</v-autocomplete>
								</v-row>

								<v-row>
									<div class="textarea-bg w100p mt-3">
										<v-textarea v-model="copyAgentForm.whiteListIPAddress" :label="$t('Message.__pleaseInputAgentWhiteListIP')" style="padding-left: 3px" class="w100p">
										</v-textarea>
									</div>
								</v-row>
							</v-col>

							<v-col cols="12" md="6" sm="6" class="my-padding-by-device">
								<v-row class="">
									<div class="categoryTitle">
										<v-icon>settings</v-icon>
										{{ $t('TableHeader.__BOAccount') }}
									</div>
								</v-row>

								<v-row>
									<v-text-field
										name="merchant_credit_account"
										:label="$t('TableHeader.__BOAccount') + '(' + $t('Message.__notIncludePrefix') + ')'"
										autocomplete="off"
										v-model="copyAgentForm.account"
										@keydown="accountTextChanged()"
										@change="accountTextChanged()"
										maxlength="30"
									>
									</v-text-field>

									<div style="" class="hint-msg f13" v-if="isRenderComponent2" v-show="copyAgentForm.account && copyAgentForm.account.length > 0">
										<v-icon small color="grey ">error</v-icon>
										{{ $t('Message.__adminSysLoginHint1') }}
										<span style="color: #ff4e4e; font-weight: 700; font-size: 15px">{{ accountPrefix }}-{{ copyAgentForm.account }} </span> {{ $t('Message.__adminSysLoginHint2') }}
									</div>
									<div style="clear: both"></div>
								</v-row>

								<v-row>
									<v-text-field
										name="merchant_credit_password"
										:label="$t('TableHeader.__password')"
										v-model="copyAgentForm.password"
										autocomplete="new-password"
										type="password"
										maxlength="30"
									>
									</v-text-field>
								</v-row>

								<v-row>
									<v-text-field :label="$t('TableHeader.__password2')" autocomplete="off" v-model="copyAgentForm.confirmPassword" type="password" maxlength="30"> </v-text-field>
								</v-row>

								<v-row>
									<v-autocomplete :items="dropdownItemsRoles" :label="$t('TableHeader.__roleAndPermission')" chips v-model="copyAgentForm.roleID"> </v-autocomplete>
								</v-row>

								<!-- 
								<v-row>
									<v-text-field :label="$t('TableHeader.__memo') + ' (Memo / Domain)'" v-model="copyAgentForm.agentMemo"> </v-text-field>
								</v-row> -->
							</v-col>
						</v-row>
					</div>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<div>
						<v-btn text @click="closeAllModal" class="btn">
							<v-icon left>mdi-cancel</v-icon>
							{{ $t('Button.__cancel') }}
						</v-btn>
						<v-btn color="yellow darken-1" text @click="saveCreateChildAgent" class="btn">
							<v-icon left>mdi-check-circle</v-icon>
							{{ $t('Button.__submit') }}
						</v-btn>
					</div>
				</v-card-actions>

				<v-card-actions>
					<div class="w100">
						<div style="font-size: 12px; text-align: right; width: 100%; color: #aaa">* {{ $t('Message.__limitSubAgentMaxCount1000') }}</div>
					</div>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 建立 Dev 商戶 -->
		<v-dialog v-model="dialogVisible.isShowExtra1" max-width="450px" persistent>
			<v-card style="border: solid 1px #444">
				<v-card-title class="grey darken-2">
					{{ $t('Menu.Agent_MerchantCredit_CreateDevMerchant') }}

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark><v-icon>close</v-icon></v-btn>
					</div>
				</v-card-title>
				<v-card-text class="padding-row">
					<v-row>
						<v-col cols="12">
							<v-text-field :label="$t('TableHeader.__merchantName')" v-model="createDevMerchantForm.displayName" maxlength="20"> </v-text-field>
						</v-col>
						<v-col cols="12">
							<v-autocomplete :items="walletModeSelectItems" :label="$t('TableHeader.__walletMode')" chips v-model="createDevMerchantForm.type"> </v-autocomplete>
						</v-col>
						<v-col cols="12">
							<v-autocomplete :items="dropdownItemsIMChannels" :label="$t('TableHeader.__imChannelName') + '(TG/Skype)'" chips v-model="createDevMerchantForm.IMChannelID">
							</v-autocomplete>
						</v-col>
					</v-row>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn color="yellow darken-1" text @click="saveCreateDevMerchant" class="btn"> <v-icon left>mdi-check-circle</v-icon>{{ $t('Button.__submit') }} </v-btn>
				</v-card-actions>

				<v-card-actions>
					<!-- <div class="f14 yellow-text">⚠️ Reverse商戶，需由技術人員來創建</div> -->
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 查看某個代理開了多少個帳號&子帳號 -->
		<v-dialog v-model="dialogVisible.isShowPopup_1" max-width="600px">
			<v-card style="border: solid 1px #444">
				<v-card-title class="grey darken-2">
					{{ $t('TableHeader.__BOAccount') }}
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text class="padding-row mt-2">
					<div class="yellow-text">
						<v-icon small color="yellow">info</v-icon>
						{{ $t('Message.__toDisableAccountHint') }}
						<b class="orange-text">【{{ $t('Menu.Merchant_SelfUpdateSetting') }}】</b> >
						<b class="orange-text">【{{ $t('Menu.Other_AccountManagement') }}】</b>
					</div>

					<div style="padding: 10px; color: #eee; font-size: 16px; line-height: 30px; min-height: 300px">
						<v-row>
							<ul>
								<li v-for="(item, index) in searchUserList">
									{{ item.userName }}
								</li>
							</ul>
						</v-row>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 代理編輯遊戲開關 -->
		<v-dialog v-model="dialogVisible.isShowAdvanceInfo2" max-width="750px" persistent style="z-index: 50">
			<v-card>
				<v-card-title class="grey darken-2">
					{{ $t('TableHeader.__flagGameEnable') }}
					<v-chip color="brown darken-3 ml-2 " small>{{ gameOpenFlagSelfEdit.merchantID }}｜{{ gameOpenFlagSelfEdit.merchantName }}</v-chip>

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div class="text-l">
						<v-checkbox
							:label="
								$t('Button.__enableEditMode') + (hasEditGameOpenFlagPermission || hasEditGameOpenFlagButEnableOnlyPermission ? '' : ' (' + $t('Message.__permissionNotEnough') + ')')
							"
							v-model="isSwitchToEditGameFlagMode"
							:disabled="!hasEditGameOpenFlagPermission && !hasEditGameOpenFlagButEnableOnlyPermission"
							style="display: inline-block"
						>
						</v-checkbox>
					</div>

					<div style="min-height: 50px">
						<div v-if="isSwitchToEditGameFlagMode" class="text-r" style="padding-right: 10px">
							<div style="float: left; padding-top: 5px" class="text-l">
								<div v-if="!gameOpenFlagSelfEdit.isDisableAllEditGameOpenFlag">
									<v-tooltip bottom color="red">
										<template v-slot:activator="{ on, attrs }">
											<div v-bind="attrs" v-on="on" class="link-text-grey mr-2" @click="enableAllGameType(1000)" v-if="getNumberOfGameType(1000) > 0">
												<v-icon small>check</v-icon>
												{{ $t('Button.__enableAll') }} ({{ $t('TableHeader.__slots') }})
											</div>
										</template>
										<span>
											<div>
												{{ $t('Message.__areYouSureOpenAllGameByType') }}
												({{ $t('TableHeader.__slots') }})｜{{ $t('TableHeader.__totalCount') }}:
												{{ getNumberOfGameType(1000) }}
											</div>
										</span>
									</v-tooltip>

									<v-tooltip bottom color="red">
										<template v-slot:activator="{ on, attrs }">
											<div v-bind="attrs" v-on="on" class="link-text-grey mr-2" @click="enableAllGameType(2500)" v-if="getNumberOfGameType(2500) > 0">
												<v-icon small>check</v-icon>
												{{ $t('Button.__enableAll') }} ({{ $t('TableHeader.__liveCasino') }})
											</div>
										</template>
										<span>
											<div>
												{{ $t('Message.__areYouSureOpenAllGameByType') }}
												({{ $t('TableHeader.__liveCasino') }})｜{{ $t('TableHeader.__totalCount') }}:
												{{ getNumberOfGameType(2500) }}
											</div>
										</span>
									</v-tooltip>
								</div>
							</div>

							<div style="float: left; padding-top: 5px; margin-left: 5px" @click="showSelectChildAgentsDialog()">
								<span class="link-text-grey" v-if="hasSyncSubAgentPermission"> 同步開關至子代理 ({{ gameOpenFlagSelfEdit.syncSubAgentIDs.length }}) </span>
							</div>

							<v-icon small color="green">error</v-icon>
							<span class="green-text f12">{{ $t('Message.__afterUpdateYouNeedSubmit') }}</span>

							<v-btn color="green" class="btn search ml-3 mt-1" @click.stop="saveEnableGamesByAgentSelfEdit()">
								<v-icon left>check</v-icon>
								{{ $t('Button.__save') }} </v-btn
							>&#9;
						</div>
					</div>

					<div style="clear: both"></div>

					<div class="agentSelfEdit">
						<v-data-table
							:items="gameOpenFlagSelfEdit.gameSettingItems"
							hide-default-footer
							class="mt-2"
							height="600px"
							fixed-header
							:footer-props="{ 'items-per-page-options': [500] }"
							:headers="gameOpenFlagSelfEditHeaders"
						>
							<template v-slot:item.gameID="{ item }">
								<div>
									{{ item.gameName }}
								</div>
								<div class="sub-title-text">
									{{ item.gameID }}
								</div>
							</template>

							<template v-slot:item.merchantGameEnableFlag="{ item }">
								<div>
									<v-switch v-model="item.merchantGameEnableFlag" :disabled="item.isGameOpenFlagDisableEdit || item.isSpecialReasonDisableEdit || !isSwitchToEditGameFlagMode">
									</v-switch>
								</div>
							</template>

							<template v-slot:item.subAgentDefaultEnableGameFlag="{ item }">
								<div>
									<v-switch v-model="item.subAgentDefaultEnableGameFlag" :disabled="!item.merchantGameEnableFlag || !isSwitchToEditGameFlagMode"> </v-switch>
								</div>
							</template>

							<template v-slot:item.isLobbyVisibleFlag="{ item }">
								<div>
									<v-switch v-model="item.isLobbyVisibleFlag" v :disabled="!item.merchantGameEnableFlag || !isSwitchToEditGameFlagMode"> </v-switch>
								</div>
							</template>
						</v-data-table>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 代理編輯遊戲開關-選擇同步哪些子代理 -->
		<v-dialog v-model="dialogVisible.isShowPopup_2" max-width="800px">
			<v-card style="border: solid 1px #444">
				<v-card-title class="grey darken-2">
					同步給以下子代理
					<div class="top-right-item" @click="closeSelectChildAgentsDialog()">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="min-height: 200px">
					<v-row>
						<v-col cols="12" sm="12" md="12">
							<v-autocomplete
								clearable
								:items="gameOpenFlagSelfEdit.childAgentDropdowns"
								multiple
								small-chips
								deletable-chips
								v-model="gameOpenFlagSelfEdit.syncSubAgentIDs"
								:label="$t('TableHeader.__merchant')"
							>
							</v-autocomplete>
						</v-col>
					</v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="yellow darken-1" text @click="closeSelectChildAgentsDialog()" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 觸發sw Test  -->
		<v-dialog v-model="dialogVisible.isShowFormConfirm" max-width="600px">
			<v-card style="border: solid 1px #444">
				<v-card-title class="grey darken-2">
					Trigger Seamless Test (触发无缝钱包测试)
					<div class="top-right-item" @click="closeAllModal()">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text class="mt-1" style="padding: 20px">
					<v-row>
						<v-col cols="12" sm="12" md="12">
							<v-text-field disabled :value="backendButtonForm.merchantID + '｜' + backendButtonForm.merchantName" :label="$t('TableHeader.__merchant')"></v-text-field>
						</v-col>

						<v-col cols="12" sm="12" md="12">
							<v-text-field v-model="backendButtonForm.text" class="required" ref="refSwTestPlayer" :label="$t('TableHeader.__playerAccount')"></v-text-field>
						</v-col>

						<v-col cols="12" sm="12" md="12">
							<div class="f14">
								<div class="orange-text">⚠️{{ $t('Message.__swValidateTest_Hint') }}</div>
								<ul style="margin-left: 15px; line-height: 25px">
									<li>#A1 {{ $t('Message.__swValidateTest_Rule1') }}</li>
									<li>#A2 {{ $t('Message.__swValidateTest_Rule2') }}</li>
									<li>#A3 {{ $t('Message.__swValidateTest_Rule3') }}</li>
								</ul>

								<div class="mt-4">❓{{ $t('Message.__thisMayTakeMuchTimeWatting') }} (30s+)</div>
								<div class="mt-1">❓{{ $t('Message.__swValidateTest_Hint2') }}</div>
							</div>
						</v-col>
					</v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="grey lighten-1" text @click="copyExcelResultText()" v-if="hasSuperPermission" class="btn">
						<v-icon left>file_copy</v-icon>
						複製文案(請對方確認Excel)
					</v-btn>

					<v-btn color="green darken-1" text v-if="hasTriggerSwValidateTestPermission" @click="triggerSwTest()" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }} & {{ $t('Button.__downloadFile') }}
					</v-btn>
				</v-card-actions>

				<v-card-actions style="margin-top: -5px; padding-bottom: 25px">
					<v-spacer></v-spacer>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<v-dialog v-model="dialogVisible.isShowCopyDialog" max-width="650px">
			<v-card>
				<v-card-title class="grey darken-2">
					💡{{ $t('Message.__PleaseCheckDownloadFile') }}

					<div class="top-right-item" @click="dialogVisible.isShowCopyDialog = false">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="background: #111">
					<div style="padding: 15px; white-space: pre-line">
						{{ downloadAndCheckExcelMessage }}
					</div>
				</v-card-text>

				<v-card-actions style="background: #111; padding-bottom: 20px">
					<div class="text-c w100p">
						<v-btn color="yellow darken-1" text @click="copyTextToClipboard(downloadAndCheckExcelMessage)" class="btn">
							<v-icon left>file_copy</v-icon>
							{{ $t('Button.__copyToClipboard') }}
						</v-btn>
					</div>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 編輯商戶狀態 -->
		<v-dialog v-model="dialogVisible.isShowEditBriefInfo" max-width="450px">
			<MerchantStatusEditPopup ref="refMerchantStatusEditPopup" @parentCloseAllModal="closeAllModalAndRefresh()"> </MerchantStatusEditPopup>
		</v-dialog>

		<!-- 進階設置 -->
		<v-dialog v-model="dialogVisible.isShowAdvanceInfo" content-class="no-scroll-y" max-width="700px">
			<MerchantsAdvanceSetting ref="refMerchantsAdvanceSetting" @closeModalAndRefreshParentPage="closeAllModal"> </MerchantsAdvanceSetting>
		</v-dialog>

		<NotificationWarning
			v-show="dialogVisible.isShowConfirm"
			:title="`${$t('Message.__warning')}`"
			:max="5"
			:text="confirmText"
			:confirmed-action="doSubmitEnableGamesByAgentSelfEdit"
			:canceled-action="closeAllModal"
		/>
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

import EventsApi from '@/api/system-setting/events';
import MerchantCreditApi from '@/api/agent/merchantCredit';
import MerchantApi from '@/api/system-setting/merchants';

import DropdownApi from '@/api/commons/dropdown';
import UsersApi from '@/api/operator-management/users';
import DatePicker from 'vue2-datepicker';
import * as UserModel from '@/models/interfaces/operator-management/users';
import { WalletMode } from '@/models/status/merchant';
import InternalTestApi from '@/api/commons/internalTest';

import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';

import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import MerchantStatusEditPopup from '@/components/system-setting/MerchantStatusEditPopup.vue';

import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';
import { nextTick } from 'vue/types/umd';

import MerchantsAdvanceSetting from '@/components/system-setting/MerchantsAdvanceSetting.vue';

// @ts-ignore
@Component({
	components: { NotificationWarning, DatePicker, MerchantStatusEditPopup, MerchantsAdvanceSetting },
	mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin],
})
export default class MerchantCredits extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	WalletMode: typeof WalletMode = WalletMode;
	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();
	isShowPreview: boolean = false;
	isRenderComponent: boolean = true;
	isRenderComponent2: boolean = false;
	isAllowEditCreditByAccountType: boolean = false;
	isShowComponentAfterFewDely: boolean = false;
	isOrgDropdownInvisible: boolean = true;
	isLockEditGameIDsForCopyAgent: boolean = true;

	backendButtonForm: any = {};

	confirmText: string = '';

	// @ts-ignore
	downloadAndCheckExcelMessage = '';

	isSwitchToEditGameFlagMode: boolean = false;

	accountPrefix: string = '';
	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchList: MerchantCreditModel.IMerchantCredit[] = [];
	searchFilter: MerchantCreditModel.IQueryMerchantCreditApiRequestParameter = {} as MerchantCreditModel.IQueryMerchantCreditApiRequestParameter;
	exportFilter: MerchantCreditModel.IQueryMerchantCreditApiRequestParameter = {} as MerchantCreditModel.IQueryMerchantCreditApiRequestParameter;
	searchUserList: UserModel.IUser[] = [];

	updateStatusForm: DropdownModel.ILazyCommonParameterReqeust = {} as DropdownModel.ILazyCommonParameterReqeust;
	targetEditStatus: MerchantCreditModel.IMerchantCredit = {} as MerchantCreditModel.IMerchantCredit;

	searchPreview: MerchantCreditModel.IMerchantCreditUpdatePreview = {} as MerchantCreditModel.IMerchantCreditUpdatePreview;
	updateCreditForm: MerchantCreditModel.IMerchantCreditUpdateFormRequest = {} as MerchantCreditModel.IMerchantCreditUpdateFormRequest;
	copyAgentForm: MerchantCreditModel.IMerchantCopyByAgentRequest = {} as MerchantCreditModel.IMerchantCopyByAgentRequest;
	createDevMerchantForm: MerchantCreditModel.ICreateDevMerchantRequest = {} as MerchantCreditModel.ICreateDevMerchantRequest;

	availableGamesForCopyAgent: any = {};
	gameOpenFlagSelfEdit: any = {};

	panel: number[] = [0];

	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsAgentMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsOrganization: DropdownModel.IDropdownItems[] = [];
	dropdownItemsRoles: DropdownModel.IDropdownItems[] = [];
	dropdownItemsLayer2Agents: DropdownModel.IDropdownItems[] = [];
	dropdownItemsStatus: DropdownModel.IDropdownItems[] = [];
	dropdownItemsCurrency: DropdownModel.IDropdownItems[] = [];
	dropdownItemsGames: DropdownModel.IDropdownItems[] = [];
	dropdownItemsIMChannels: DropdownModel.IDropdownItems[] = [];

	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_Update) >= 0;
	}

	get hasPermissionToCreateChild() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_CreateChildAgent) >= 0;
	}

	get hasPermissionToEditStatus() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_EditStatus) >= 0;
	}

	get hasAdvanceEditPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_EditAdvanceSetting) >= 0;
	}

	get hasSuperPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get hasExportApiKeyPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_ExportApiKey) >= 0;
	}

	get hasEditGameOpenFlagPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_UpdateGameOpenFlag) >= 0;
	}

	get hasEditGameOpenFlagButEnableOnlyPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_GameOpenFlag_EnableOnly) >= 0;
	}

	get hasSelectWalletModePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_AllowSetWalletModeWhenCreate) >= 0;
	}

	get hasSelectIMChannelPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_AllowSetIMChannelID) >= 0;
	}

	get hasBatchExportPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_BatchExportApiKey) >= 0;
	}

	get hasSyncSubAgentPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_SyncSubAgent) >= 0;
	}

	get hasTriggerSwValidateTestPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_TriggerSwValidateTest) >= 0;
	}

	get hasPermission_Special_AMPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_AM_Permission) >= 0;
	}

	get hasPermission_CreateDevMerchantPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_CreateDevMerchant) >= 0;
	}

	get walletModeSelectItems() {
		const dropdowns = [];
		dropdowns.push({ value: WalletMode.TransferMode, text: this.$t('MerchantsStatus.TransferMode') });
		dropdowns.push({ value: WalletMode.SingleWalletMode, text: this.$t('MerchantsStatus.SingleWalletMode') });
		return dropdowns;
	}

	get getAddOrMinusDropdwon() {
		return [
			{ text: this.$t('Button.__addCredit'), value: 1 },
			{ text: this.$t('Button.__reduceCredit'), value: -1 },
		];
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'action', width: '4%', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__merchantID'), value: 'merchantID', sortable: false, width: '7%' },
			{ text: this.$t('TableHeader.__merchantName'), value: 'name', sortable: false, width: '10%' },
			{ text: this.$t('TableHeader.__parent'), value: 'parentMerchantName', sortable: false, width: '10%' },
			{ text: this.$t('TableHeader.__BOAccount'), value: 'numberOfAccount', sortable: false, width: '7%' },
			{ text: this.$t('TableHeader.__enableGamesCount'), value: 'enableGamesCount', sortable: false, width: '7%' },
			{ text: this.$t('TableHeader.__status'), value: 'statusText', sortable: false, width: '7%' },
		];

		if (this.hasAdvanceEditPermission) {
			data.push({ text: this.$t('Button.__advanceSetting'), value: 'extra', sortable: false, width: '7%' });
		}

		data.push({ text: this.$t('TableHeader.__currency'), value: 'currencyText', sortable: false, width: '7%' });
		data.push({ text: this.$t('TableHeader.__creditAmount'), value: 'creditAmount', sortable: false, align: 'end', width: '10%' });
		data.push({ text: this.$t('TableHeader.__createTime'), value: 'createTime', sortable: false, align: 'end', width: '5%' });
		data.push({ text: this.$t('TableHeader.__tag'), value: 'isUnlimit', sortable: false, width: '15%', align: 'start' });

		// data.push({ text: this.$t('TableHeader.__memo'), value: 'agentMemo', sortable: false, width: '10%', align: 'start' });

		return data;
	}

	get gameOpenFlagSelfEditHeaders() {
		const data = [
			{ text: this.$t('TableHeader.__merchantID'), value: 'gameID', sortable: false },
			{ text: this.$t('TableHeader.__flagGameEnable'), value: 'merchantGameEnableFlag', sortable: false },
			{ text: this.$t('TableHeader.__flagSubAgentDefaultEnableGame'), value: 'subAgentDefaultEnableGameFlag', sortable: false },
			{ text: this.$t('TableHeader.__lobbyVisible'), value: 'isLobbyVisibleFlag', sortable: false },
		];

		return data;
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.refresh();
	}

	search() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	getNumberOfTargetGameType(type: number) {
		let myCounter = 0;

		if (this.availableGamesForCopyAgent && this.availableGamesForCopyAgent.availableGamesDropdown && this.availableGamesForCopyAgent.availableGamesDropdown.length > 0) {
			for (const item of this.availableGamesForCopyAgent.availableGamesDropdown) {
				console.log(item);

				if (this.copyAgentForm.gameIDs.includes(item.value) && item.gameType === type) {
					myCounter++;
				}
			}
		}

		return myCounter;
	}

	getNumberOfOtherGameType() {
		let myCounter = 0;

		if (this.availableGamesForCopyAgent && this.availableGamesForCopyAgent.availableGamesDropdown && this.availableGamesForCopyAgent.availableGamesDropdown.length > 0) {
			for (const item of this.availableGamesForCopyAgent.availableGamesDropdown) {
				console.log(item);

				if (this.copyAgentForm.gameIDs.includes(item.value) && item.gameType !== 1000 && item.gameType !== 2500) {
					myCounter++;
				}
			}
		}

		return myCounter;
	}

	whenChangeSelectedGames() {
		this.dialogVisible.isShowCreate = false;
		this.$nextTick(() => {
			this.dialogVisible.isShowCreate = true;
		});
	}

	whenChangeTemplateParentMerchant() {
		if (this.copyAgentForm.parentMerchantID == null || this.copyAgentForm.parentMerchantID <= 0) {
			return;
		}
		EventBus.$emit('show_mask', null);

		MerchantApi.getEnableGamesForSubAgent(this.copyAgentForm.parentMerchantID)
			.then((res) => {
				this.availableGamesForCopyAgent = res.data;
				this.availableGamesForCopyAgent.availableGamesDropdown = res.data.availableGamesDropdown.map((item: any) => {
					return {
						text: item.text,
						value: item.valueInt,
						gameType: item.extraProperty.gameType,
					};
				});

				this.copyAgentForm.walletMode = res.data.walletMode;

				// 部分頻道不要預設勾選
				if (!res.data.isDisableIMChannelAutoComplete) {
					this.copyAgentForm.imChannelID = res.data.imChannelID;
				} else {
					this.copyAgentForm.imChannelID = 0;
				}

				// 10008是richmond的總代預設頻道
				if (this.copyAgentForm.imChannelID === 10008) {
					this.copyAgentForm.imChannelID = 0;
				}
				// 沒權限就直接塞null
				if (!this.hasSelectIMChannelPermission) {
					this.copyAgentForm.imChannelID = null;
				}

				this.copyAgentForm.gameIDs = [];

				res.data.defaultEnableGameIDs.forEach((id: any) => {
					this.copyAgentForm.gameIDs.push(id);
				});

				this.dialogVisible.isShowCreate = false;
				this.$nextTick(() => {
					this.dialogVisible.isShowCreate = true;
				});
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	refresh() {
		this.registerReloadComponent();
		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;
		this.searchFilter.isASC = false;

		if (this.datetimeRangePicker.searchStartTimeRange && this.datetimeRangePicker.searchStartTimeRange.length > 1) {
			this.searchFilter.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
			this.searchFilter.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		}

		EventBus.$emit('show_mask', null);
		MerchantCreditApi.getMerchantCredit(this.searchFilter)
			.then((res) => {
				this.searchList = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	checkMyCredit() {
		MerchantCreditApi.getMyCredit()
			.then((res) => {
				this.isAllowEditCreditByAccountType = res.data.creditAmount !== -1;
			})
			.finally(() => {});
	}

	showAdvanceSettingEditDialog(item: MerchantCreditModel.IMerchantCredit) {
		this.dialogVisible.isShowAdvanceInfo = true;

		this.$nextTick(() => {
			const ref: any = this.$refs.refMerchantsAdvanceSetting;
			const settingKeys: number[] = [];

			if (item.isSingleWallet) {
				settingKeys.push(7001);
			}

			settingKeys.push(9125);

			ref.getMerchantAdvanceSettingByMultipleKey(item.merchantID, settingKeys);
		});
	}

	showPreview(item: MerchantCreditModel.IMerchantCredit) {
		this.dialogVisible.isShowPreviewDialog = true;
		this.updateCreditForm = {} as MerchantCreditModel.IMerchantCreditUpdateFormRequest;
		this.updateCreditForm.merchantID = item.merchantID;
		this.updateCreditForm.plusType = 1;

		this.searchPreview = {} as MerchantCreditModel.IMerchantCreditUpdatePreview;

		EventBus.$emit('show_mask', null);
		MerchantCreditApi.getMerchantCreditPreview(item.merchantID)
			.then((res) => {
				this.searchPreview = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	accountTextChanged() {
		this.isRenderComponent2 = false;
		this.isRenderComponent2 = true;
	}

	saveUpdateCredit(item: MerchantCreditModel.IMerchantCredit) {
		EventBus.$emit('show_mask', null);
		MerchantCreditApi.updateMerchantCredit(this.updateCreditForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showCreateChildAgentDialog() {
		this.dialogVisible.isShowCreate = true;

		if (this.copyAgentForm.walletMode == null || this.copyAgentForm.walletMode <= 0) {
			this.copyAgentForm.walletMode = 1;
		}

		this.copyAgentForm.name = '';
		this.copyAgentForm.account = '';
		this.copyAgentForm.password = '';
		this.copyAgentForm.confirmPassword = '';

		if (this.dropdownItemsOrganization.length === 1) {
			this.isOrgDropdownInvisible = true;
			this.copyAgentForm.organizationID = this.dropdownItemsOrganization[0].valueInt;
			this.organizationDropdownChanged();
		} else {
			this.isOrgDropdownInvisible = false;
		}
	}

	showCreateDevMerchantDialog() {
		this.createDevMerchantForm = {} as MerchantCreditModel.ICreateDevMerchantRequest;
		this.createDevMerchantForm.displayName = '';
		this.createDevMerchantForm.type = 0;
		this.createDevMerchantForm.IMChannelID = null;
		this.dialogVisible.isShowExtra1 = true;
	}

	saveCreateDevMerchant() {
		EventBus.$emit('show_mask', null);
		MerchantCreditApi.createDevMerchant(this.createDevMerchantForm)
			.then(() => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	saveCreateChildAgent() {
		const vm: any = this;
		EventBus.$emit('show_mask', null);

		if (!this.hasSelectWalletModePermission) {
			this.copyAgentForm.walletMode = null;
		}

		MerchantApi.copyCreateChildAgent(this.copyAgentForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	exportAllApiKey() {
		this.exportFilter = {} as MerchantCreditModel.IQueryMerchantCreditApiRequestParameter;
		this.exportFilter = this.searchFilter;

		const vm: any = this;

		let fileNamePrefix: string = '_All';

		if (!vm.isEmptyString(this.searchFilter.keyword)) {
			fileNamePrefix = '_' + this.searchFilter.keyword.trim();
		}

		this.exportApiKey(fileNamePrefix);
	}

	exportSingleApiKey(merchantID: number, merchantName: string = '') {
		this.exportFilter = {} as MerchantCreditModel.IQueryMerchantCreditApiRequestParameter;
		this.exportFilter.merchantID = merchantID;

		const nameLetter: string = merchantName
			.replace('測試用)', 'DEVKEY')
			.replace('*Dev)', 'DEVKEY')
			.replace(/[^a-zA-Z0-9 ]/g, '')
			.replace(' ', '')
			.replace('DEVKEY', 'DevKey_');

		const prefixFileName: string = '_' + merchantID.toString() + '_' + nameLetter;
		this.exportApiKeyV2(prefixFileName);
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	async exportApiKey(fileNamePrefix: string = '') {
		const excelHeaders: any = [
			{ text: this.geti18nTextAsString('TableHeader.__merchantName'), width: 25 },
			{ text: this.geti18nTextAsString('TableHeader.__type'), width: 15 },
			{ text: this.geti18nTextAsString('TableHeader.__parent'), width: 18 },
			{ text: 'BO Url', width: 25 },
			{ text: this.geti18nTextAsString('TableHeader.__account'), width: 25 },
			{ text: this.geti18nTextAsString('TableHeader.__password'), width: 30 },
			{ text: this.geti18nTextAsString('TableHeader.__merchantID'), width: 15 },
			{ text: 'API Key (MerchantSecret)', width: 25 },
			{ text: 'API Url', width: 35 },
			{ text: 'X-PROVIDER-IDENTITY (Seamless)', width: 25 },
			{ text: this.geti18nTextAsString('TableHeader.__memo'), width: 25 },
		];

		const excelData: any = [];

		if (this.datetimeRangePicker.searchStartTimeRange && this.datetimeRangePicker.searchStartTimeRange.length > 1) {
			this.exportFilter.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
			this.exportFilter.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		}

		EventBus.$emit('show_mask', null);

		const result = await MerchantCreditApi.getMerchantCreditExportColumns(this.exportFilter).finally(() => {
			setTimeout(() => {
				EventBus.$emit('hide_mask', null);
			}, 500);
		});

		result.data.forEach((value) => {
			const eachRow: any = [];

			eachRow.push(value.name);
			eachRow.push(value.walletModeText);
			eachRow.push(value.parentMerchantName);
			eachRow.push(value.backOfficeUrl);
			eachRow.push(value.userAccount);
			eachRow.push(value.userPassword);
			eachRow.push(value.merchantID);
			eachRow.push(value.apiSecretKey);
			eachRow.push(value.apiUrl);
			eachRow.push(value.singleWalletPgsIdentity);

			const memo: string = value.memo;

			eachRow.push(memo);
			excelData.push(eachRow);
		});

		const config = {
			headersWithConfig: excelHeaders,
			rowData: excelData,
			fileName: 'Export_ApiKey' + fileNamePrefix + '.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);

		// 提示一些警告訊息
		if (result.data.length === 1) {
			const isEndpointNotSetup = result.data[0].isEndpointNotSetup;

			const msg = this.geti18nTextAsString('TableHeader.__merchant') + ':' + result.data[0].merchantID + ', ' + result.data[0].memo;

			if (isEndpointNotSetup) {
				setTimeout(() => {
					EventBus.$emit(`event-warning`, msg);
				}, 500);
			}
		}
	}

	async exportBasicInfo(fileNamePrefix: string = '') {
		const excelHeaders: any = [
			{ text: this.geti18nTextAsString('TableHeader.__merchantID'), width: 15 },
			{ text: this.geti18nTextAsString('TableHeader.__merchantName'), width: 25 },
			{ text: this.geti18nTextAsString('TableHeader.__walletMode'), width: 15 },
			{ text: this.geti18nTextAsString('TableHeader.__parent'), width: 25 },

			{ text: this.geti18nTextAsString('TableHeader.__enableGamesCount'), width: 12 },
			{ text: this.geti18nTextAsString('TableHeader.__status'), width: 12 },
			{ text: this.geti18nTextAsString('TableHeader.__currency'), width: 12 },
			{ text: this.geti18nTextAsString('TableHeader.__createTime'), width: 18 },
			{ text: this.geti18nTextAsString('TableHeader.__creditAmount'), width: 18 },
			{ text: this.geti18nTextAsString('TableHeader.__tag'), width: 25 },
			{ text: this.geti18nTextAsString('TableHeader.__memo'), width: 25 },
		];

		const excelData: any = [];

		if (this.searchList.length === 0) {
			alert('Please search first before exporting. (请先进行查询再汇出)');
			return;
		}

		EventBus.$emit('show_mask', null);

		setTimeout(() => {
			EventBus.$emit('hide_mask', null);
		}, 1000);

		this.searchList.forEach((value) => {
			const eachRow: any = [];

			eachRow.push(value.merchantID);
			eachRow.push(value.name);
			eachRow.push(value.walletModeText);
			eachRow.push(value.parentMerchantName);
			eachRow.push(value.enableGamesCount);
			eachRow.push(value.statusText);
			eachRow.push(value.currency);
			eachRow.push(timeWithTimezone(value.createTime, this.userProfile.timeZone));
			eachRow.push(value.creditAmount);

			let tagText = '';

			if (value.extraTagsBlackColor.length > 0) {
				value.extraTagsBlackColor.forEach((text) => {
					tagText += text + ' ';
				});
				eachRow.push(tagText);
			}
			eachRow.push(value.memo2);

			excelData.push(eachRow);
		});

		// 下方有另一個 exportApiKeyV2 那種特殊版型的Excel
		// 下方有另一個 exportApiKeyV2 那種特殊版型的Excel
		// 下方有另一個 exportApiKeyV2 那種特殊版型的Excel

		const config = {
			headersWithConfig: excelHeaders,
			rowData: excelData,
			fileName: 'Export_SearchAgent' + '.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	// V2版本。excel版型稍作調整
	async exportApiKeyV2(fileNamePrefix: string = '') {
		const vm: any = this;
		const excelData: any = [];

		if (this.datetimeRangePicker.searchStartTimeRange && this.datetimeRangePicker.searchStartTimeRange.length > 1) {
			this.exportFilter.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
			this.exportFilter.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		}

		EventBus.$emit('show_mask', null);

		const result = await MerchantCreditApi.getMerchantCreditExportColumns(this.exportFilter).finally(() => {
			setTimeout(() => {
				EventBus.$emit('hide_mask', null);
			}, 500);
		});

		const targetMerchant = result.data[0];

		const eachRow1: any = [];
		eachRow1.push({ text: this.geti18nTextAsString('TableHeader.__merchantID'), style: 1 });
		eachRow1.push({ text: this.geti18nTextAsString('TableHeader.__merchantName'), style: 1 });
		eachRow1.push({ text: this.geti18nTextAsString('TableHeader.__type'), style: 1 });
		eachRow1.push({ text: this.geti18nTextAsString('TableHeader.__currency'), style: 1 });
		eachRow1.push({ text: this.geti18nTextAsString('TableHeader.__parentLine'), style: 1 });
		excelData.push(eachRow1);

		const eachRow2: any = [];
		eachRow2.push({ text: targetMerchant.merchantID, style: 0 });
		eachRow2.push({ text: targetMerchant.name, style: 0 });
		eachRow2.push({ text: targetMerchant.walletModeText, style: 0 });
		eachRow2.push({ text: targetMerchant.currency, style: 0 });
		eachRow2.push({ text: targetMerchant.parentMerchantName, style: 0 });
		excelData.push(eachRow2);

		excelData.push([{ text: '', style: 0 }]); // 空格行
		excelData.push([{ text: '', style: 0 }]); // 空格行

		const eachRow3: any = [];
		eachRow3.push({ text: 'BO Url', style: 1 });
		eachRow3.push({ text: this.geti18nTextAsString('TableHeader.__account') + '(BO)', style: 1 });
		eachRow3.push({ text: this.geti18nTextAsString('TableHeader.__password') + '(BO)', style: 1 });
		eachRow3.push({ text: this.geti18nTextAsString('TableHeader.__environment'), style: 1 });
		excelData.push(eachRow3);

		const eachRow4: any = [];
		eachRow4.push({ text: targetMerchant.backOfficeUrl, style: 0 });
		eachRow4.push({ text: targetMerchant.userAccount, style: 0 });
		eachRow4.push({ text: targetMerchant.userPassword, style: 0 });
		eachRow4.push({ text: targetMerchant.merchantType === 20 ? 'Production (正式环境)' : 'Dev/Staging ', style: 0 });
		excelData.push(eachRow4);

		excelData.push([{ text: '', style: 0 }]); // 空格行
		excelData.push([{ text: '', style: 0 }]); // 空格行

		const eachRow5: any = [];
		eachRow5.push({ text: 'API Url', style: 1 });
		eachRow5.push({ text: 'API MerchantID', style: 1 });
		eachRow5.push({ text: 'API MerchantSecret', style: 1 });
		if (targetMerchant.isSingleWallet) {
			eachRow5.push({ text: 'X-target-Identity (Seamless mode)', style: 1 });
			eachRow5.push({ text: 'VendorCode', style: 1 });
		}
		excelData.push(eachRow5);

		const eachRow6: any = [];
		eachRow6.push({ text: targetMerchant.apiUrl, style: 0 });
		eachRow6.push({ text: targetMerchant.merchantID, style: 0 });
		eachRow6.push({ text: targetMerchant.apiSecretKey, style: 0 });
		if (targetMerchant.isSingleWallet) {
			eachRow6.push({ text: targetMerchant.singleWalletPgsIdentity, style: 0 });
			eachRow6.push({ text: targetMerchant.vendorCode, style: 0 });
		}
		excelData.push(eachRow6);

		excelData.push([{ text: '', style: 0 }]); // 空格行
		excelData.push([{ text: '', style: 0 }]); // 空格行

		excelData.push([{ text: this.geti18nTextAsString('TableHeader.__memo'), style: 2 }]);

		const extraHintTags: string[] = [];

		if (targetMerchant.isSingleWalletDoaminNotSetup) {
			extraHintTags.push('⚠️' + this.geti18nTextAsString('Message.__notSetValueHint') + ' endpoint (Seamless mode)');
		}

		if (!targetMerchant.isSingleWalletDoaminNotSetup && targetMerchant.creditAmount <= 0 && targetMerchant.isCreditModeEnable) {
			extraHintTags.push('⚠️' + this.geti18nTextAsString('Message.__yourCreditAmountNotEnough'));
		}

		if (targetMerchant.isProductionEnvType) {
			extraHintTags.push('⚠️' + this.geti18nTextAsString('Message.__productionMercahntWillCalculateBilling'));
		}

		if (targetMerchant.isHintTransferConvertRate) {
			extraHintTags.push('⚠️' + this.geti18nTextAsString('Message.__hintConvertRate') + ' (Rate: 1:' + targetMerchant.transferConvertRate + ')');
		}

		if (!vm.isEmptyString(targetMerchant.memo_ThisIsDevKey)) {
			extraHintTags.push(targetMerchant.memo_ThisIsDevKey);
		}

		if (!vm.isEmptyString(targetMerchant.memo_DevBETACoin)) {
			extraHintTags.push(targetMerchant.memo_DevBETACoin);
		}

		if (!vm.isEmptyString(targetMerchant.memo_WhereToDownloadDocument)) {
			extraHintTags.push(targetMerchant.memo_WhereToDownloadDocument);
		}

		for (const tag of extraHintTags) {
			excelData.push([{ text: tag, style: 0 }]);
		}

		excelData.push([{ text: '', style: 0 }]); // 空格行
		excelData.push([{ text: '', style: 0 }]); // 空格行

		// IP白名單
		if (targetMerchant.isSingleWallet) {
			excelData.push([{ text: this.geti18nTextAsString('Message.__pleaseAddIPToWhiteList'), style: 2 }]);

			const myServerIPs: string[] = targetMerchant.whiteListServerIPs;

			for (const ip of myServerIPs) {
				excelData.push([{ text: ip, style: 0 }]);
			}

			excelData.push([{ text: '', style: 0 }]); // 空格行
			excelData.push([{ text: '', style: 0 }]); // 空格行
		}

		const config = {
			rowData: excelData,
			fileName: 'Export_ApiKey' + fileNamePrefix + '.xlsx',
		};

		// @ts-ignore
		this.downloadExcelAndSetEachCellStyle(config);
	}

	closeAllModalAndRefresh() {
		this.closeAllModal();
		this.refresh();
	}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
	}

	clear() {
		this.searchFilter = {} as MerchantCreditModel.IQueryMerchantCreditApiRequestParameter;
		this.exportFilter = {} as MerchantCreditModel.IQueryMerchantCreditApiRequestParameter;
		this.datetimeRangePicker.searchStartTimeRange = [];
		this.datetimeRangePicker.quicklyPickTimeRangeText = '';
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	showUserAccounts(merchantID: number) {
		this.searchUserList = [];
		this.dialogVisible.isShowPopup_1 = true;

		const myFilter: UserModel.IUserFilter = {} as UserModel.IUserFilter;
		myFilter.referenceCreditMerchantID = merchantID;

		EventBus.$emit('show_mask', null);
		UsersApi.getUsers(myFilter)
			.then((x) => {
				this.searchUserList = x.data.items;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showEditStatus(item: MerchantCreditModel.IMerchantCredit) {
		this.dialogVisible.isShowEditBriefInfo = true;

		this.$nextTick(() => {
			const myRef: any = this.$refs.refMerchantStatusEditPopup;
			myRef.getPreviewInfo(item.merchantID);
		});
	}

	saveEditStatus(item: MerchantCreditModel.IMerchantCredit) {
		EventBus.$emit('show_mask', null);
		MerchantApi.updateMerchantStatusByAgent(this.updateStatusForm)
			.then((x) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	getNumberOfGameType(gameType: number) {
		try {
			if (!this.gameOpenFlagSelfEdit) {
				return 0;
			}
			if (gameType === 1000) {
				return this.gameOpenFlagSelfEdit.gameIDs_Slots.length;
			}
			if (gameType === 2500) {
				return this.gameOpenFlagSelfEdit.gameIDs_Live.length;
			}
		} catch {}
		return 0;
	}

	enableAllGameType(gameType: number) {
		let gameTypeText = '';
		const totalCount = this.getNumberOfGameType(gameType);

		if (gameType === 1000) {
			gameTypeText = this.geti18nTextAsString('TableHeader.__slots');
		} else if (gameType === 2500) {
			gameTypeText = this.geti18nTextAsString('TableHeader.__liveCasino');
		}

		if (totalCount <= 0) {
			alert('not found this gameType');
			return;
		}

		const confirmMsg =
			this.geti18nTextAsString('Message.__areYouSureOpenAllGameByType') + ' (' + gameTypeText + ')? ' + this.geti18nTextAsString('TableHeader.__totalCount') + ':' + totalCount.toString();

		// const isConfirm = confirm(confirmMsg);

		// if (!isConfirm) {
		// 	return;
		// }

		EventBus.$emit('show_mask', null);

		if (gameType === 1000) {
			this.gameOpenFlagSelfEdit.gameSettingItems.forEach((item: any) => {
				if (this.gameOpenFlagSelfEdit.gameIDs_Slots.includes(item.gameID)) {
					item.merchantGameEnableFlag = true;
				}
			});
		} else if (gameType === 2500) {
			this.gameOpenFlagSelfEdit.gameSettingItems.forEach((item: any) => {
				if (this.gameOpenFlagSelfEdit.gameIDs_Live.includes(item.gameID)) {
					item.merchantGameEnableFlag = true;
				}
			});
		} else {
			alert('not implement');
		}

		setTimeout(() => {
			EventBus.$emit('hide_mask', null);
		}, 500);
	}

	showEnableGames(merchantID: number) {
		EventBus.$emit('show_mask', null);

		this.isSwitchToEditGameFlagMode = false;
		this.gameOpenFlagSelfEdit = {};

		MerchantApi.getGameEnableSettingsForAgentSelfEdit(merchantID)
			.then((res) => {
				this.dialogVisible.isShowAdvanceInfo2 = true;
				this.gameOpenFlagSelfEdit = res.data;
				this.gameOpenFlagSelfEdit.syncSubAgentIDs = [];
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	saveEnableGamesByAgentSelfEdit() {
		// 如果只能開不能關
		if (this.hasEditGameOpenFlagButEnableOnlyPermission) {
			const newEnableGames = this.gameOpenFlagSelfEdit.gameSettingItems.filter((item: any) => !item.isGameOpenFlagDisableEdit && item.merchantGameEnableFlag && !item.isSpecialReasonDisableEdit);

			console.log(newEnableGames);

			if (newEnableGames.length > 0) {
				const msg = this.$t('Message.__confirmYourCanOnlyEnableGame').toString();
				if (confirm(msg)) {
					this.doSubmitEnableGamesByAgentSelfEdit();
				}
			} else {
				this.doSubmitEnableGamesByAgentSelfEdit();
			}

			// this.confirmText = msg;
			// this.dialogVisible.isShowConfirm = true;

			// this.$notify({
			// 	group: 'confirm',
			// 	type: 'warn',
			// 	duration: -1,
			// 	speed: 10,
			// });
		} else {
			this.doSubmitEnableGamesByAgentSelfEdit();
		}
	}

	showTriggerSwTestDialog(myMerchantID: number, myMerchantName: string) {
		this.dialogVisible.isShowFormConfirm = true;

		this.backendButtonForm = {
			merchantID: myMerchantID,
			merchantName: myMerchantName,
			text: '',
		};

		setTimeout(() => {
			const refInput: any = this.$refs.refSwTestPlayer;
			refInput.focus();
		}, 250);
	}

	triggerSwTest() {
		EventBus.$emit('show_mask', null);

		const url = '/backoffice/merchants/sw_test';

		const vm: any = this;

		const myBody: any = {
			BackendButtonActionID: 3222,
			ExtraParameter: this.backendButtonForm,
		};

		localStorage.removeItem(url);
		localStorage.removeItem('download_error_msg');

		InternalTestApi.httpPostDownloadFile(url, myBody, false)
			.then((res) => {
				const successFlag = localStorage.getItem(url);

				if (successFlag === '1') {
					const finalMsg = this.generateExcelResultText() + '\n\n' + this.generateExcelResultTextCN();
					this.downloadAndCheckExcelMessage = finalMsg;
					this.dialogVisible.isShowCopyDialog = true;
				} else {
					setTimeout(() => {
						const errorMsg = localStorage.getItem('download_error_msg');
						if (errorMsg != null && errorMsg.length > 0) {
							EventBus.$emit(`event-error`, errorMsg);
						}
					}, 500);
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showSelectChildAgentsDialog() {
		this.dialogVisible.isShowPopup_2 = true;
	}

	closeSelectChildAgentsDialog() {
		this.dialogVisible.isShowPopup_2 = false;
	}

	copyExcelResultText() {
		const vm: any = this;
		const myMsg = this.generateExcelResultText();
		vm.copyTextToClipboard2(myMsg);
	}

	generateExcelResultText() {
		let myMsg = 'Hi, please check the above excel file. \n';
		myMsg += 'And check those column' + '\n';
		myMsg += '- IsValid' + '\n';
		myMsg += '- Result Description' + '\n' + '\n';
		myMsg += '※ If IsValid is `❌`' + '\n';
		myMsg += 'Possible reasons are your side response error or invalid code. (Result Description will show which error code should use.)' + '\n\n';
		myMsg += '※ If IsValid is `Refer Description (Check Manually)`' + '\n';
		myMsg += 'Please refer the description and check your system implement correctly. (confirm by yourself)' + '\n' + '\n';
		myMsg += 'If every case is OK. please apply production account with our AM.' + '\n';

		return myMsg;
	}

	generateExcelResultTextCN() {
		let myMsg = '你好，请参考这份测试结果Excel \n';
		myMsg += '并确认挡案中的这几个栏位' + '\n';
		myMsg += '- IsValid' + '\n';
		myMsg += '- Result Description' + '\n' + '\n';
		myMsg += '※ 若 IsValid 结果为 `❌`' + '\n';
		myMsg += '可能原因是贵方API没有回应符合预期的code代码、或出现特定错误 (Result Description 栏位会说明需要返回哪个指定Code)' + '\n\n';
		myMsg += '※ 若 IsValid 结果为 `Refer Description (Check Manually)`' + '\n';
		myMsg += '需要由贵方自行检查是否有正确处理描述中的注意事项 (e.g. 避免重复派钱、由贵方自行检查)' + '\n' + '\n';
		myMsg += '若每个项目都通过，请向我方AM申请正式环境账号' + '\n';

		return myMsg;
	}

	doSubmitEnableGamesByAgentSelfEdit() {
		EventBus.$emit('show_mask', null);

		MerchantApi.saveEnableGamesByAgentSelfEdit(this.gameOpenFlagSelfEdit)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.isSwitchToEditGameFlagMode = false;

				// 因為gameDisableFlag 要強制刷新頁面
				if (this.hasEditGameOpenFlagButEnableOnlyPermission) {
					this.closeAllModal();
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	organizationDropdownChanged() {
		try {
			console.log(this.dropdownItemsOrganization.filter((item) => item.valueInt === this.copyAgentForm.organizationID));

			this.accountPrefix = this.dropdownItemsOrganization.filter((item) => item.valueInt === this.copyAgentForm.organizationID)[0].extraProperty.organizationCode;
		} catch (e) {}
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
		this.refresh();
		this.checkMyCredit();

		this.DataTablePagingParameter.footerProps = { 'items-per-page-options': [10, 50, 100] };

		setTimeout(() => {
			this.isShowComponentAfterFewDely = true;
		}, 3000);

		DropdownApi.getHierarchyMerchants().then((res) => {
			this.dropdownItemsAgentMerchants = res;
		});

		DropdownApi.getMerchants().then((res) => {
			this.dropdownItemsMerchants = res;
		});

		DropdownApi.getOrganizations().then((res) => {
			this.dropdownItemsOrganization = res;
		});

		const maxLayer = 5;
		DropdownApi.getHierarchyMerchants(maxLayer).then((res) => {
			this.dropdownItemsLayer2Agents = res;
		});

		DropdownApi.getRoles().then((res) => {
			this.dropdownItemsRoles = res;
		});

		DropdownApi.getNormalStatus().then((res) => {
			this.dropdownItemsStatus = res.filter((x) => x.valueInt !== 999);
		});

		DropdownApi.getGames().then((res) => {
			this.dropdownItemsGames = res;
		});

		DropdownApi.getCurrency().then((res) => {
			this.dropdownItemsCurrency = res;
		});

		DropdownApi.getIMChannels().then((res) => {
			this.dropdownItemsIMChannels = res;
		});

		// 鍵盤事件監聽
		const vm = this;
		document.addEventListener('keydown', (event) => {
			const key = event.key; // const {key} = event; in ES6+
			if (key === 'Escape') {
				vm.closeAllModal();
			}
		});
	}
}
</script>

<style scoped>
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

.hint-warning {
	color: #ef4c4c;
	font-size: 14px;
}

.hint-warning i {
	color: #ef4c4c !important;
	font-size: 16px !important;
}

.eventType_32 {
	/* 紅包 */
	background: #863f3f !important;
}

.eventType_51 {
	/* 甜蜜期 (顏色太多反而炫目) */
	/* background: #9c719e !important; */
}

.hint-msg {
	margin-top: -8px;
	margin-bottom: 5px;
	background: #444;
	padding: 5px;
	border-radius: 5px;
}

.categoryTitle {
	font-size: 18px;
	margin-left: -12px;
	margin-bottom: 7px;
}

.categoryTitle,
.categoryTitle i {
	color: #6aa2dc;
}

.padding-row .row {
	padding-left: 10px;
}

.my-padding-by-device {
	padding-right: 40px;
}

.agentSelfEdit .v-input--is-disabled,
.agentSelfEdit .v-input--is-disabled .primary--text {
	opacity: 0.3 !important;
}

.link-text-grey-gameCount,
.link-text-white-gameCount {
	display: inline-block;
	color: #ccc !important;
	border-bottom: solid 1px #ccc;
	line-height: 14px;
	cursor: pointer;
}

.link-text-white-gameCount,
.link-text-white {
	color: #ccc !important;
}

.hint-box {
	background: rgb(73 71 67);
	padding: 4px;
	border-radius: 5px;
}

@media only screen and (max-width: 480px) {
	.my-padding-by-device {
		padding-right: 20px;
	}
}
</style>
