<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchfilter.keyword" :label="$t('TableHeader.__keyword')" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="currencySelectItems" class="letter-size" v-model="searchfilter.currency" :label="$t('TableHeader.__currency')">
										<template v-slot:item="data">
											<div class="letter-size-equal">
												<span>{{ data.item.text }}</span>
											</div>
										</template>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="walletModeSelectItems" class="letter-size" v-model="searchfilter.walletMode" :label="$t('TableHeader.__walletMode')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="merchantTypesDropdownItems" class="letter-size" v-model="searchfilter.type" :label="$t('TableHeader.__category')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete
										clearable
										:items="gamesDropdownWithKeywordFilter"
										class="letter-size"
										no-filter
										:no-data-text="$t('Message.__pleaseInputKeywordToSearchDropdown')"
										:search-input.sync="globalKeywordSearchGamesDropdown"
										v-model="searchfilter.enableSpecificGameID"
										:label="'有開通此款遊戲'"
									>
										<template slot="selection" slot-scope="data">
											{{ data.item.text }}
										</template>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete
										clearable
										:items="hierarchyMerchantsDropdownItems"
										class="letter-size"
										v-model="searchfilter.hierarchyOfTargetMerchant"
										:label="$t('TableHeader.__merchantAndAllChilds')"
									>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="hierarchyMerchantsDropdownItems" class="letter-size" v-model="searchfilter.parentMerchantID" :label="$t('TableHeader.__parent')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="imChannelsDropdownItems" class="letter-size" v-model="searchfilter.IMChannelMappingID" :label="'對接IM群組'"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete
										clearable
										:items="filterSettingKeysDropdownItems"
										no-filter
										:search-input.sync="keywordSearchSettingKey"
										@change="whenDropdownSettingKeyChanged()"
										class="letter-size myDropdown"
										v-model="searchfilter.searchExtraSettingKey"
										:label="'查詢額外欄位'"
									>
										<template slot="selection" slot-scope="data">
											<div class="max-width-selections">
												{{ data.item.text }}
											</div>
										</template>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete
										clearable
										:items="merchantSpecialFilterTypeDropdownItems"
										class="letter-size"
										v-model="searchfilter.specialMerchantFilterType"
										:label="'特殊搜尋標籤'"
									>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<!-- <div class="item right"> -->
								<div class="mt-3 search-row-padding text-r">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="refresh">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;

									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }} </v-btn
									>&#9;
									<!-- <v-btn color="blue lighten-3" text class="btn search"
										@click.stop="showMerchantCompare">
										<v-icon left>visibility</v-icon>
										商戶比對
									</v-btn>&#9; -->

									<v-btn color="yellow lighten-3" text class="btn search" @click.stop="showCopyNewOneDiaolog">
										<v-icon left>file_copy</v-icon>
										複製建立商戶 </v-btn
									>&#9;
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table :headers="headers" id="table" :items="merchantsData" :search="search" multi-sort @current-items="getFilterItems">
			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title>
						<h4>{{ $t('TableHeader.__merchant') }}</h4>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>

					<div>
						<a class="link-text-grey f14 mr-3" @click.stop="showMerchantCompare"> 商戶比對 </a>

						<a class="link-text-grey f14 mr-3" @click.stop="showDataTableSearchExtraSetting"> 查詢額外設置(技術用) </a>

						<a class="link-text-grey f14 mr-3" @click.stop="showDeleteMerchantDialog"> 刪資料(技術用) </a>
					</div>

					<v-spacer></v-spacer>
					<v-card-title>
						<div class="f14 yellow-text mr-2">
							<v-icon small color="yellow">error</v-icon>
							預設最多搜尋{{ dataTableFetchMaxSize }}筆。若查無資料請加入【篩選條件】
						</div>

						<v-text-field v-model="search" append-icon="search" :label="'清單內關鍵字查詢(僅針對500筆內搜尋)'" style="width: 350px" single-line hide-details clearable> </v-text-field>

						<v-btn icon dark @click="addItem" v-show="hasPermissionCreate" style="margin-left: 15px">
							<v-icon>add</v-icon>
						</v-btn>

						<!-- <v-btn icon dark @click="refresh">
							<v-icon>refresh</v-icon>
						</v-btn> -->

						<v-tooltip top>
							<template v-slot:activator="{ on, attrs }">
								<v-btn icon dark v-bind="attrs" v-on="on" @click="copyAllMerchantIDs">
									<v-icon>file_copy</v-icon>
								</v-btn>
							</template>
							<span>複製當前列表所有商戶ID</span>
						</v-tooltip>

						<v-tooltip top>
							<template v-slot:activator="{ on, attrs }">
								<v-btn icon dark v-bind="attrs" v-on="on" @click="showExportSeleteColumns">
									<v-icon>file_copy</v-icon>
								</v-btn>
							</template>
							<span>選擇欄位並複製</span>
						</v-tooltip>
					</v-card-title>
				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">
				<v-tooltip top v-if="hasPermissionUpdate">
					<template v-slot:activator="{ on, attrs }">
						<v-icon color="" small dark class="mr-2" v-bind="attrs" v-on="on" @click="editItem(item)"> edit </v-icon>
					</template>
					<span>編輯</span>
				</v-tooltip>

				<v-tooltip top v-if="hasPermissionUpdate">
					<template v-slot:activator="{ on, attrs }">
						<v-icon color="" small class="mr-2" dark v-bind="attrs" v-on="on" @click="showMerchantAdvanceSetting(item)"> settings </v-icon>
					</template>
					<span>進階設置</span>
				</v-tooltip>

				<v-tooltip top v-if="hasPermissionUpdate">
					<template v-slot:activator="{ on, attrs }">
						<v-icon color="" small class="mr-2" dark v-bind="attrs" v-on="on" @click="showEditMerchantVendorConfig(item.merchantID)"> subject </v-icon>
					</template>
					<span>遊戲商配置設定</span>
				</v-tooltip>

				<v-tooltip top v-if="hasPermissionUpdate">
					<template v-slot:activator="{ on, attrs }">
						<v-icon color="" small class="mr-2" dark v-bind="attrs" v-on="on" @click="showCopyHintDialog(item)"> visibility </v-icon>
					</template>
					<span>預覽金鑰資訊</span>
				</v-tooltip>
			</template>

			<template v-slot:item.merchantID="{ item }">
				<div>
					<span class="link-text-grey" @click="showMerchantFullInfo(item.merchantID)">{{ item.merchantID }} </span>
				</div>
			</template>

			<template v-slot:item.name="{ item }">
				<div>
					<span class="pointer" @click="copyTextToClipboard(item.merchantID.toString() + ' ' + item.name)">
						{{ item.name }}
					</span>
				</div>

				<div class="sub-info">
					{{ item.memo }}
				</div>
			</template>

			<template v-slot:item.keywordForJsSearch="{ item }">
				<!-- 此欄只是為了讓keyword可以搜尋到 -->
			</template>

			<template v-slot:item.searchExtraSettingValue="{ item }">
				<span v-if="searchfilter.searchExtraSettingKey == 777777" class="f12" style="display: inline-block; width: 300px; white-space: pre-wrap">
					{{ item.memo2 }}
				</span>
				<span
					class="link-text-white"
					@click="showSingleSettingKeyUpdateDialog(item.merchantID, searchfilter.searchExtraSettingKey)"
					v-else-if="item.searchExtraSettingValue != null && item.searchExtraSettingValue.length > 0"
				>
					{{ item.searchExtraSettingValue }}
				</span>
				<span v-else class="link-text-white" @click="showSingleSettingKeyUpdateDialog(item.merchantID, searchfilter.searchExtraSettingKey)"> 默认值 </span>
			</template>

			<template v-slot:item.currencyText="{ item }">
				<div v-if="item.currencyText == item.chargeCurrencyText">
					<v-chip class="mr-1">
						{{ item.currencyText }}
					</v-chip>
				</div>
				<div v-else>
					<v-chip class="mr-1" color="green darken-3">
						{{ item.currencyText }}
					</v-chip>
					<v-chip class="" color="blue darken-4">
						{{ item.chargeCurrencyText }}
					</v-chip>
				</div>
			</template>

			<template v-slot:item.parentMerchantID="{ item }">
				<div :title="item.parentMerchantID">
					{{ item.parentMerchantName }}
				</div>
			</template>

			<template v-slot:item.currency="{ item }">{{ $t(`Currency.${currency[item.currency]}`) }} </template>

			<template v-slot:item.chargeCurrency="{ item }">{{ $t(`Currency.${currency[item.chargeCurrency]}`) }} </template>

			<template v-slot:item.imChannelName="{ item }">
				<div v-if="isEmptyString(item.imChannelName)" class="link-text-grey" @click="showSingleSettingKeyUpdateDialog(item.merchantID, 3088)">未设置</div>

				<div class="link-text-DDD" @click="showSingleSettingKeyUpdateDialog(item.merchantID, 3088)">
					{{ item.imChannelName }}
				</div>

				<div class="sub-info" s>
					{{ item.imSoftwareType }}
				</div>
			</template>

			<template v-slot:item.defaultLanguage="{ item }">{{ $t(`Lang.${item.defaultLanguage}`) }}</template>

			<template v-slot:item.status="{ item }">
				<div @click="showEditStatus(item)" :class="item.status == 1 ? 'link-text-green' : 'link-text-red'">
					{{ item.statusText }}
				</div>
				<!-- 
				<v-chip v-show="status.PermanentDelete === item.status" class="ma-2">
					{{ $t(`Status.${status[item.status]}`) }}</v-chip>
				<v-chip v-show="status.Disable === item.status" class="ma-2" color="grey">
					{{ $t(`Status.${status[item.status]}`) }}</v-chip>
				<v-chip v-show="status.Enable === item.status" class="ma-2" color="green">
					{{ $t(`Status.${status[item.status]}`) }}</v-chip> -->
			</template>

			<template v-slot:item.memo="{ item }">
				<div v-if="item.defaultBalance == null || item.defaultBalance == 0">-</div>
				<div v-else>
					{{ item.defaultBalance | amountDisplay }}
				</div>
			</template>

			<template v-slot:item.merchantTypeName="{ item }">
				<span :class="'type_' + item.type">
					{{ item.merchantTypeName }}
				</span>
			</template>

			<template v-slot:item.walletMode="{ item }">{{ $t(`MerchantsStatus.${WalletMode[item.walletMode]}`) }} </template>

			<template v-slot:item.defaultBalance="{ item }">
				<div v-if="item.defaultBalance > 0">
					{{ item.defaultBalance | countDisplay }}
				</div>
			</template>

			<template v-slot:item.createTime="{ item }">
				<div>
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</div>

				<div class="sub-title-text">
					{{ item.createBy }}
				</div>
			</template>
			<template v-slot:item.updateTime="{ item }">
				{{ item.updateTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>
		</v-data-table>

		<!-- 新增/編輯 -->
		<v-dialog v-model="dialogVisible.isShowCreateOrEdit" :max-width="isCreateFrom ? '950px' : '500px'" persistent>
			<v-card class="createForm">
				<v-card-title class="grey darken-2">
					{{ formTitle }}
					<span v-if="isEdit" class="f12 ml-2 grey-text"> ({{ this.editedItem.merchantID }}) </span>
					<div class="top-right-item" @click="close">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-row>
						<v-col cols="12" :sm="isCreateFrom ? '6' : '12'" :md="isCreateFrom ? '6' : '12'">
							<v-flex style="padding: 0px 10px 0px 10px">
								<v-row>
									<v-text-field class="required" v-model="editedItem.name" :label="$t('TableHeader.__merchantName')" autofocus required clearable> </v-text-field>
								</v-row>
								<v-row>
									<v-text-field v-model="editedItem.memo" :label="$t('TableHeader.__memo')" clearable> </v-text-field>
								</v-row>

								<v-row>
									<v-select
										class="required"
										:disabled="editedIndex !== -1"
										attach
										chips
										:label="$t('TableHeader.__currency')"
										v-model="editedItem.currency"
										:items="currencySelectItems"
									>
										<template v-slot:item="data">
											<div class="letter-size-equal">
												<span>{{ data.item.text }}</span>
											</div>
										</template>
									</v-select>
								</v-row>

								<v-row>
									<v-select class="required" attach chips :label="$t('TableHeader.__category')" style="width: 200px" v-model="editedItem.type" :items="merchantTypesDropdownItems">
									</v-select>
								</v-row>

								<v-row>
									<v-select class="required" attach chips :label="$t('TableHeader.__lang')" v-model="editedItem.defaultLanguage" :items="langSelectItems"></v-select>
								</v-row>

								<v-row>
									<v-text-field class="required" v-model="editedItem.apiSecretKey" label="API Key" :disabled="!isCreateFrom" required clearable> </v-text-field>
								</v-row>

								<v-row v-if="!isCreateFrom">
									<div class="w100">
										<div class="common-info-label">GUID</div>
										<div class="common-info-value" style="white-space: nowrap; color: #999">
											{{ editedItem.merchantGuid }}
										</div>
									</div>
								</v-row>

								<v-row class="mt-4">
									<div class="v-label v-label--active theme--dark">
										{{ $t('TableHeader.__walletMode') }}
									</div>
								</v-row>

								<v-row>
									<v-radio-group row v-model="editedItem.walletMode" :disabled="editedIndex !== -1 && !adminSysFavoritSetting.isEnableRDAdminMode">
										<v-radio :label="$t('MerchantsStatus.TransferMode')" :value="WalletMode.TransferMode"> </v-radio>

										<v-radio :label="$t('MerchantsStatus.SingleWalletMode')" :value="WalletMode.SingleWalletMode"> </v-radio>
									</v-radio-group>
								</v-row>
							</v-flex>
						</v-col>

						<v-col cols="12" sm="6" md="6" v-if="isCreateFrom && editedItem.extraSettings">
							<div class="extra-setting-create">
								<h3 class="mb-1" style="color: #fff">額外配置</h3>

								<v-divider></v-divider>

								<div>
									<div>
										<v-autocomplete :label="'上級代理'" :items="hierarchyMerchantsDropdownItems" v-model="editedItem.extraSettings.parentMerchantID" clearable> </v-autocomplete>
									</div>

									<div>
										<v-autocomplete
											:label="'從現有商戶複製遊戲開關設定'"
											:items="merchantsWithCategoryDropdownItems"
											v-model="editedItem.extraSettings.copyGameSettingFromMerchantID"
											clearable
										>
											<template v-slot:item="data">
												<div :class="data.item.valueInt == null ? 'merchantCategoryText' : 'merchantValue'">
													<span>{{ data.item.text }}</span>
												</div>
											</template>
										</v-autocomplete>
									</div>

									<div>
										<div class="groupItem" v-if="editedItem.extraSettings && editedItem.extraSettings.customizedSettings && editedItem.extraSettings.customizedSettings.length > 0">
											<div v-for="(item, index2) in editedItem.extraSettings.customizedSettings" style="margin-bottom: -8px" :key="index2">
												<v-row>
													<v-col cols="12" sm="12" md="12" :title="item.settingKey">
														<v-text-field v-if="!item.isDropdownDataType" :label="item.settingKeyDisplayName" v-model="item.value"> </v-text-field>

														<v-autocomplete v-if="item.isDropdownDataType" attach :label="item.settingKeyDisplayName" v-model="item.value" :items="item.dataDropdowns">
														</v-autocomplete>
													</v-col>
												</v-row>
											</div>
										</div>
									</div>
								</div>
							</div>
						</v-col>
					</v-row>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="close" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="save" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 複製商戶: 建立一個相同設置的商戶  -->
		<v-dialog v-model="dialogVisible.isShowCopyDialog" max-width="850px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					複製建立相同設置的商戶

					<span class="mr-3"></span>

					<v-tooltip top color="yellow darken-4" class="ml-3">
						<template v-slot:activator="{ on, attrs }">
							<v-icon color="yellow darken-1" small class="ml-1">info</v-icon>
							<span v-bind="attrs" v-on="on" class="f12 yellow-text" style="text-decoration: underline"> 同步資料說明 </span>
						</template>
						<span>
							<ul>
								<li>商戶基本設置</li>
								<li>遊戲開關</li>
								<li>伺服器配置</li>
								<li>進階配置</li>
								<li>遊戲商配置(VendorConfig)</li>
							</ul>
						</span>
					</v-tooltip>

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="min-height: 600px" class="copyMerchantByTemplate">
					<v-row>
						<v-col cols="12" sm="7" md="7">
							<div style="padding-right: 20px">
								<v-row>
									<v-autocomplete
										:items="merchantsWithCategoryDropdownItems"
										chips
										clearable
										@change="whenSourceMerchantChanged()"
										:label="'以選擇的商戶作為範本複製'"
										autofocus
										v-model="copyNewOneItem.sourceMerchantID"
									>
										<template v-slot:item="data">
											<div :class="data.item.valueInt == null ? 'merchantCategoryText' : 'merchantValue'">
												<span>{{ data.item.text }}</span>
											</div>
										</template>
									</v-autocomplete>

									<div class="copySourceExtraTags">
										<div v-if="copyNewOneItem.sourceMerchantID > 0 && copySourceMerchant && example.invalid && example.invalid.merchantID > 0">
											<v-chip x-small color="brown darken-3" class="mr-1">
												{{ example.invalid.merchantTypeName }}
											</v-chip>

											<v-chip x-small class="mr-1" color="brown darken-3">
												{{ example.invalid.walletModeText }}
											</v-chip>

											<v-chip x-small class="mr-1" v-if="copySourceMerchant.isMerchantHierarchy" color="brown darken-3">代理體系 </v-chip>

											<!-- <v-chip x-small class='mr-1'
											v-for="(item, index) in copySourceMerchant.enableGameVendorGames"
											:key="index">
											{{item.gameVendorName}}
										</v-chip> -->
										</div>
									</div>
								</v-row>

								<v-row>
									<div class="minus-mt-2 w100">
										<v-autocomplete
											class="copyMerchantInputFrom"
											attach
											small-chips
											:label="$t('TableHeader.__currency')"
											v-model="copyNewOneItem.currency"
											:items="currencySelectItems"
										>
										</v-autocomplete>
									</div>
								</v-row>
								<v-row>
									<v-text-field
										class="copyMerchantInputFrom"
										v-model="copyNewOneItem.name"
										clearable
										@keydown="syncMerchantNameToAccount()"
										:label="$t('TableHeader.__merchantName')"
									>
									</v-text-field>
								</v-row>

								<v-row>
									<v-autocomplete
										class="copyMerchantInputFrom"
										:items="hierarchyMerchantsDropdownItems"
										:label="'上級代理(*幣別需相同)'"
										clearable
										small-chips
										v-model="copyNewOneItem.parentMerchantID"
									>
									</v-autocomplete>
								</v-row>

								<v-row>
									<v-autocomplete
										class="copyMerchantInputFrom"
										multiple
										:items="copyMerchantExtraSettingFlagDropdownItems"
										:label="'特殊設置(多選)'"
										clearable
										small-chips
										v-model="copyNewOneItem.extraSettingFlag"
									>
									</v-autocomplete>
								</v-row>

								<v-row>
									<v-autocomplete
										:items="imChannelsDropdownItems"
										:label="'IM頻道(TG/Skype)'"
										:disabled="copyNewOneItem.isLockEditIMChannel"
										clearable
										small-chips
										v-model="copyNewOneItem.imChannelID"
									>
									</v-autocomplete>

									<!-- <v-text-field class="copyMerchantInputFrom" :label="'新增一個IM頻道名稱(TG/Skype)'"
										:disabled='copyNewOneItem.isLockEditIMChannel ' clearable
										v-model="copyNewOneItem.IMChannelID">
									</v-text-field> -->

									<div class="w100 text-r minus-mt-2">
										<div class="link-text f12" style="position: relative; z-index: 99999" @click="showCreateIMChannelDialog()">新增IM頻道(TG/Skype)</div>
									</div>
								</v-row>
							</div>
						</v-col>

						<v-col cols="12" sm="5" md="5" style="padding: 0px" v-if="copyNewOneItem && copyNewOneItem.createAccountInfo">
							<div class="extra-setting-create-2">
								<v-row class="">
									<v-checkbox :label="'同時新增後台帳號'" v-model="isCreateUserAccountPanel" @change="setAccountFormDefaultValue()"> </v-checkbox>
								</v-row>

								<v-row v-show="isCreateUserAccountPanel" style="margin-top: 30px !important">
									<v-autocomplete
										:items="organizationsDropdownItems"
										:label="'所屬組織'"
										:disabled="!isCreateUserAccountPanel"
										v-model="copyNewOneItem.createAccountInfo.organizationID"
									>
									</v-autocomplete>
								</v-row>

								<v-row v-show="isCreateUserAccountPanel">
									<v-text-field :label="'登入帳號(不含組織前綴)'" v-model="copyNewOneItem.createAccountInfo.account" :disabled="!isCreateUserAccountPanel"> </v-text-field>
								</v-row>
								<v-row v-show="isCreateUserAccountPanel" style="margin-top: 10px !important">
									<v-text-field :label="'登入密碼(默認會產生亂數密碼)'" v-model="copyNewOneItem.createAccountInfo.password" :disabled="!isCreateUserAccountPanel"> </v-text-field>
								</v-row>

								<v-row v-show="isCreateUserAccountPanel">
									<v-autocomplete :items="rolesDropdownItems" :label="'角色'" chips :disabled="!isCreateUserAccountPanel" v-model="copyNewOneItem.createAccountInfo.roleID">
									</v-autocomplete>
								</v-row>

								<v-row v-show="isCreateUserAccountPanel">
									<v-autocomplete
										:items="yesOrNoDropdownItems"
										:label="'採用樹狀模式瀏覽商戶'"
										chips
										:disabled="!isCreateUserAccountPanel"
										v-model="copyNewOneItem.createAccountInfo.isHierarchyMode"
									>
									</v-autocomplete>
									<!-- 
									<v-checkbox v-model="copyNewOneItem.createAccountInfo.isHierarchyMode"
										:label="'採用樹狀模式瀏覽商戶'">

									</v-checkbox> -->
								</v-row>
							</div>
						</v-col>

						<v-col cols="12" md="12" sm="12">
							<div style="padding-right: 20px; width: 75%">
								<v-row class="mt-2">
									<div
										style="width: 100%; padding: 10px 10px 10px 10px"
										v-if="adminSysFavoritSetting && adminSysFavoritSetting.copyMerchantFavoritMerchants && adminSysFavoritSetting.copyMerchantFavoritMerchants.length > 0"
									>
										<div class="yellow-text"><v-icon small color="yellow ligthen-2">error</v-icon> 複製常用商戶</div>

										<table class="ez-table">
											<tbody style="color: #fff">
												<tr v-for="(item, index) in adminSysFavoritSetting.copyMerchantFavoritMerchants" :key="index">
													<td>
														<div>
															{{ item.merchantName }}
														</div>
														<div class="sub-title-text" style="font-size: 10px">
															{{ item.merchantID }}
														</div>
													</td>
													<td>
														<div :class="item.type == 20 ? 'type_20' : ''">
															{{ item.typeText }}
														</div>
														<div class="sub-title-text" style="font-size: 10px">{{ item.memo }} &nbsp;</div>
													</td>

													<td>
														<div>
															<span style="display: inline-block">
																{{ item.walletModeText }}
															</span>
														</div>
													</td>
												</tr>
											</tbody>
										</table>
									</div>
								</v-row>
							</div>
						</v-col>
					</v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="close" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn color="yellow darken-1" text @click="saveCopyNewOne" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 商戶比對 -->
		<v-dialog v-model="isShowMerchantCompare" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					商戶配置比對

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="refresh">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<v-flex style="padding: 10px">
						<v-row>
							<v-autocomplete :items="merchantsDropdownItems" v-model="compareLeftMerchantID" class="merchantDropdownItems" :label="$t('TableHeader.__merchant')"> </v-autocomplete>
						</v-row>

						<v-row>
							<v-autocomplete :items="merchantsDropdownItems" v-model="compareRightMerchantID" class="merchantDropdownItems" :label="$t('TableHeader.__merchant')"> </v-autocomplete>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="confirmCompareMerchants" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- API金鑰、資訊預覽/preivew／確認 -->
		<v-dialog v-model="dialogVisible.isShowPreviewDialog" max-width="700px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					複製前確認
					<span style="font-size: 14px" class="ml-2" v-if="currentSelecedToCopyMerchant && example.invalid"> ({{ example.invalid.merchantID }}) </span>

					<div class="top-right-item">
						<v-checkbox :label="'顯示額外信息'" v-model="isShowExtraInfoForPreview" class="mt-0" style="display: inline-block; vertical-align: sub; margin-right: 25px"> </v-checkbox>

						<v-btn icon dark @click="closeAllModal">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text class="advancePanelContent advancePanelContent group-settings">
					<div style="padding: 0px 15px">
						<div>
							<div style="margin-bottom: 10px; font-size: 14px; color: #fff">
								<div>
									<div class="reviewMerchant" v-if="currentSelecedToCopyMerchant && example.invalid">
										<v-row class="">
											<v-col cols="12" sm="6" md="6">
												<div class="">
													<div class="common-info-label">商戶名稱</div>
													<div class="common-info-value" style="height: 24px">
														{{ example.invalid.name }}

														<span style="font-size: 14px; color: #aaa" class="ml-1"> ({{ example.invalid.merchantID }}) </span>
													</div>
												</div>
											</v-col>

											<v-col cols="12" sm="6" md="6">
												<div class="">
													<div class="common-info-label">幣別</div>
													<div class="common-info-value" style="height: 24px">
														{{ example.invalid.currency }}
													</div>
												</div>
											</v-col>

											<v-col cols="12" sm="6" md="6">
												<div class="">
													<div class="common-info-label">錢包模式</div>
													<div class="common-info-value" style="height: 24px">
														{{ example.invalid.walletModeText }}
													</div>
												</div>
											</v-col>

											<v-col cols="12" sm="6" md="6">
												<div class="">
													<div class="common-info-label">分類</div>
													<div class="common-info-value" style="height: 24px">
														{{ example.invalid.merchantTypeName }}
													</div>
												</div>
											</v-col>

											<v-col cols="12" sm="12" md="12" v-if="isShowExtraInfoForPreview">
												<div class="">
													<div class="common-info-label">額外信息</div>
													<div class="" style="min-height: 24px">
														<div
															style="display: inline-block; background: #383838; border-radius: 8px; padding: 1px 5px"
															class="f12 mr-2 pointer mb-1"
															v-for="(item, index) in currentSelecedToCopyMerchant.extraInfoItemsDefaultHidden"
															@click="showSingleSettingKeyUpdateDialog(example.invalid.merchantID, item.customizedSettingKey)"
															:key="index"
														>
															<v-icon small color="yellow lighten-2" v-if="item.iconStyle == 'money'"> mdi-coin </v-icon>
															<v-icon small color="green darken-2" v-if="item.iconStyle == 'ok'"> check </v-icon>
															<v-icon small color="red darken-2" v-if="item.iconStyle == 'close'"> close </v-icon>
															<v-icon small color="pink lighten-2" class="" v-if="item.iconStyle == 'person'"> person </v-icon>
															{{ item.text }}
														</div>
													</div>
												</div>
											</v-col>

											<v-col cols="12" sm="12" md="12" v-if="currentSelecedToCopyMerchant.memo_CN && currentSelecedToCopyMerchant.memo_CN.length > 0">
												<div class="">
													<div class="common-info-label">備註信息</div>
													<div class="common-info-value" style="height: 24px">
														<span v-for="(memo, index) in currentSelecedToCopyMerchant.memo_CN" :key="index">
															{{ memo }}
														</span>
													</div>
												</div>
											</v-col>

											<v-col cols="12" sm="12" md="12">
												<div class="">
													<div class="common-info-label">其他檢查項目</div>
													<div class="" style="height: 18px">
														<div class="hint-warning" v-if="currentSelecedToCopyMerchant.isCodeNotSetup">
															<v-icon> error </v-icon>
															未配置商戶代碼(code)
														</div>

														<div class="hint-warning ml-2" v-if="currentSelecedToCopyMerchant.isEnableGameEmpty">
															<v-icon> error </v-icon>
															未配置遊戲
														</div>

														<div class="hint-warning ml-2" v-if="currentSelecedToCopyMerchant.isAnyPgsGameNotMappingServer">
															<v-icon> error </v-icon>
															部分天馬遊戲尚未配置伺服器
														</div>

														<div class="hint-warning ml-2" v-if="currentSelecedToCopyMerchant.isSingleWalletDoaminNotSetup">
															<v-icon> error </v-icon>
															單錢包endpoint未配置
														</div>

														<div class="hint-warning ml-2" v-if="currentSelecedToCopyMerchant.isW88OperatorCodeNotSetup">
															<v-icon> error </v-icon>
															W88商戶代碼未配置
														</div>

														<div class="hint-warning ml-2" v-for="(tag, index) in currentSelecedToCopyMerchant.extraHintTags" :key="index">
															<v-icon> error </v-icon>
															{{ tag }}
														</div>
													</div>
												</div>
											</v-col>

											<v-col cols="12" sm="6" md="6" v-if="currentSelecedToCopyMerchant.enableThirdPartyGames">
												<div class="">
													<!-- <div class="common-info-label">
														VendorConfig配置
													</div> -->
													<!-- 													
													<div style="height: 26px;font-size:14px;padding-left:2px;"
														v-for="(eachVendorConfig ,index ) in currentSelecedToCopyMerchant.vendorConfigMappings">

														<span
															:class=" (eachVendorConfig.isWarning ? 'hint-warning' : '') + ' mr-3 '">

															{{eachVendorConfig.vendorConfigText}}

															<v-icon v-if="eachVendorConfig.isWarning" class="">error
															</v-icon>

															<span v-if=" eachVendorConfig.isWarning" style="font-size:12px;"
																class="hint-warning">非正式服務器
															</span>

														</span>
													</div> -->

													<div class=""></div>
												</div>
											</v-col>

											<v-col cols="12" sm="12" md="12">
												<div class="" style="margin-top: 5px; margin-bottom: 5px">
													<v-divider></v-divider>

													<div class="" style="margin-top: 10px">
														<table class="brief-table">
															<thead>
																<tr>
																	<th>遊戲商</th>
																	<th>啟用遊戲數</th>
																	<th>轉換比例</th>
																	<th>對應遊戲商配置</th>
																</tr>
															</thead>

															<tbody>
																<tr v-for="(myItem, index) in currentSelecedToCopyMerchant.enableGameVendorGames" :key="index">
																	<td>
																		{{ myItem.gameVendorName }}
																	</td>
																	<td class="link-text-white" @click="showMerchantEnableGames(example.invalid.merchantID)">
																		{{ myItem.enableGames }}
																	</td>
																	<td>{{ myItem.convertRate }}</td>
																	<td>
																		<span :class="(myItem.isWarning ? 'hint-warning' : '') + ' mr-3 '">
																			{{ myItem.vendorConfigName }}

																			<v-icon v-if="myItem.isWarning" small class="ml-1">error </v-icon>

																			<span v-if="myItem.isWarning" style="font-size: 12px" class="hint-warning">服務器不匹配(環境/幣別) </span>
																		</span>
																	</td>
																</tr>
															</tbody>
														</table>

														<div style="clear: both"></div>
													</div>
												</div>
											</v-col>
										</v-row>
									</div>
								</div>

								<div></div>
							</div>

							<v-divider></v-divider>

							<!-- <div style="color:#FFF;margin-top:15px;">
								<b>
									提供金鑰給對方前，請確認以下項目已設置
								</b>
							</div> -->
							<ul style="padding: 7px 35px" v-if="currentSelecedToCopyMerchant && example.invalid">
								<!-- 
								<li> [商戶開放遊戲] </li> -->
								<!-- 
								<li v-if="currentSelecedToCopyMerchant.enablePgsGames > 0">
									[伺服器分配]
								</li> -->

								<!-- <li v-show="isSingleWalletMode(example.invalid.walletMode)">
									[進階設置] (單錢包模式)
								</li> -->

								<!-- <li> [進階設置] (轉換比例)</li> -->

								<!-- <li v-if="currentSelecedToCopyMerchant.enableThirdPartyGames > 0">
									[遊戲商設置] (若有開放三方遊戲)
									<a target="_blank" class="link-text" href="/system-setting/vendor-config">
										遊戲商設置
									</a>
								</li> -->
							</ul>
						</div>

						<div class="text-c" style="margin-top: 20px">
							<div class="text-c color-label" v-if="currentSelecedToCopyMerchant.isProvideComplete">
								<v-checkbox
									v-model="isDisabledCopyButton"
									:label="'複製按鈕已鎖定 (此商戶已提供過API金鑰、是否點錯商戶了?)'"
									style="display: inline-block; font-size: 12px; color: #ff0000"
									color="red"
								>
								</v-checkbox>
							</div>

							<v-btn text color="teal lighten-2" v-clipboard:copy="copyText" :disabled="isDisabledCopyButton" v-clipboard:success="copySuccess" class="btn">
								<v-icon left>file_copy</v-icon>
								{{ $t('Button.__copyToClipboard') }} (中文)
							</v-btn>

							<v-btn text color="blue lighten-1" v-clipboard:copy="copyTextEN" :disabled="isDisabledCopyButton" style="margin-left: 10px" v-clipboard:success="copySuccess" class="btn">
								<v-icon left>file_copy</v-icon>
								{{ $t('Button.__copyToClipboard') }} (英文)
							</v-btn>
						</div>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 進階設置 -->
		<v-dialog v-model="dialogVisible.isShowAdvanceInfo" content-class="no-scroll-y" max-width="850px">
			<MerchantsAdvanceSettingV2 ref="refMerchantsAdvanceSettingV2" @closeModalAndRefreshParentPage="closeMerchantAdvanceSetting"> </MerchantsAdvanceSettingV2>
		</v-dialog>

		<!-- 進階設置(單一編輯)) -->
		<v-dialog v-model="dialogVisible.isShowAdvanceInfo2" content-class="no-scroll-y" max-width="700px">
			<MerchantsAdvanceSetting ref="refMerchantsAdvanceSettingForSingleKey" @closeModalAndRefreshParentPage="closeMerchantAdvanceSetting"> </MerchantsAdvanceSetting>
		</v-dialog>

		<!-- 商戶比對 -->
		<v-dialog v-model="dialogVisible.isShowFullInfo" max-width="1200px">
			<v-card>
				<v-card-text>
					<MerchantCompare ref="refMerchantCompare"></MerchantCompare>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 匯出選擇欄位 -->
		<v-dialog v-model="dialogVisible.isShowExport" max-width="450px">
			<v-card>
				<v-card-title class="grey darken-2">
					選擇匯出哪些欄位
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div class="mySelectColumnPanel mt-4">
						<div>
							<a @click="removeAllSelectedColumns">清除選擇</a>
						</div>

						<div class="mt-2">
							<v-checkbox v-model="selectedExportColumns.isExportMerchantID" :label="'商戶ID'"></v-checkbox>
						</div>
						<div>
							<v-checkbox v-model="selectedExportColumns.isExportMerchantName" :label="'商戶名稱'"></v-checkbox>
						</div>
						<div>
							<v-checkbox v-model="selectedExportColumns.isExportApiKey" :label="'API金鑰'"></v-checkbox>
						</div>
						<div>
							<v-checkbox v-model="selectedExportColumns.isExportGuid" :label="'商戶GUID'"></v-checkbox>
						</div>
						<div>
							<v-checkbox v-model="selectedExportColumns.isExportWalletMode" :label="'錢包類型'"></v-checkbox>
						</div>
						<div>
							<v-checkbox v-model="selectedExportColumns.isExportCurrency" :label="'幣別'"></v-checkbox>
						</div>
						<div>
							<v-checkbox v-model="selectedExportColumns.isExportIMChannel" :label="'對接頻道'"></v-checkbox>
						</div>
						<div>
							<v-checkbox v-model="selectedExportColumns.isExportTestLinkDev" :label="'測試連結_測試環境(test_001)'"> </v-checkbox>
						</div>

						<div>
							<v-checkbox v-model="selectedExportColumns.isExportTestLinkPro" :label="'測試連結_正式環境(test_001)'"> </v-checkbox>
						</div>

						<div v-if="searchfilter.searchExtraSettingKey > 0">
							<v-checkbox
								v-model="selectedExportColumns.isExportExtraSettingValue"
								:label="'進階配置 (' + filterSettingKeysDropdownItems.filter((x) => x.valueInt == searchfilter.searchExtraSettingKey)[0].text + ')'"
							>
							</v-checkbox>
						</div>

						<v-divider></v-divider>

						<div class="mt-2">
							<h3 class="green-text">額外勾選</h3>

							<div class="mt-3">
								<v-checkbox v-model="selectedExportColumns.isIncludeOtherPage" :label="'匯出包含其他頁(若不勾選=只匯出本頁10筆)'"> </v-checkbox>
							</div>
						</div>
					</div>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="yellow darken-1" text @click="confrimAndExport" class="btn">
						<v-icon left>file_copy</v-icon>
						複製選擇的欄位至剪貼簿
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 查詢額外設置 -->
		<v-dialog v-model="isShowSearchExtraSettingDialog" max-width="600px">
			<v-card>
				<v-card-title class="grey darken-2">
					查詢額外設置
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div style="min-height: 300px; padding: 10px">
						<v-row>
							<v-col cols="12" md="12" sm="12">
								<v-text-field v-model="dataTableFetchMaxSize" :label="'單次查詢筆數上限(技術人員使用)'"> </v-text-field>
							</v-col>
						</v-row>

						<div class="mt-4">
							<div><v-icon small>error</v-icon> 若要批次創建各貨幣商戶(某代理)</div>
							<div>請搭配 /merchants/batch-copy-by-each-parents-and-prefixName</div>
						</div>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 複製商戶金鑰後 額外操作 -->
		<v-dialog v-model="dialogVisible.isShowConfirm" max-width="750px">
			<v-card>
				<v-card-title class="grey darken-2">
					複製完成 & 額外操作

					<span class="f12 grey-text ml-2">
						{{ currentSelecedToCopyMerchantListData.name }}
						({{ currentSelecedToCopyMerchantListData.merchantID }})
					</span>
					<div class="top-right-item" @click="closeConfirmDialog">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="min-height: 400px">
					<div class="text-c" style="padding: 20px">
						<div>
							<v-icon color="green" x-large>check</v-icon>
						</div>
						<h2 class="green-text">商戶資訊已複製至剪貼簿</h2>
						<div class="grey-text">請確認是否需進行以下操作</div>
					</div>

					<div style="padding: 5px 35px">
						<v-row>
							<v-col cols="12" md="9" sm="9" class="v-col-padding-3"> 標記API金鑰已提供 (防呆機制) </v-col>
							<v-col cols="12" md="3" sm="3" class="v-col-padding-3">
								<v-btn
									color="grey darken-1"
									:disabled="isTargetActionAlreadyExecute(1011)"
									class="specialActionBtn"
									@click="triggerExtraOperateByType(currentSelecedToCopyMerchantListData.merchantID, 1011)"
								>
									{{ isTargetActionAlreadyExecute(1011) ? '已執行' : '執行此動作' }}
								</v-btn>
							</v-col>

							<v-col cols="12" md="9" sm="9" class="v-col-padding-3"> 標記下拉選單組別 (近期商戶-即將上線) </v-col>
							<v-col cols="12" md="3" sm="3" class="v-col-padding-3">
								<v-btn
									color="grey darken-1"
									:disabled="isTargetActionAlreadyExecute(1044)"
									class="specialActionBtn"
									@click="triggerExtraOperateByType(currentSelecedToCopyMerchantListData.merchantID, 1044)"
								>
									{{ isTargetActionAlreadyExecute(1044) ? '已執行' : '執行此動作' }}
								</v-btn>
							</v-col>

							<v-col cols="12" md="9" sm="9" class="v-col-padding-3">
								TG告知客服新頻道
								<span class="f10 yellow-text"> ({{ currentSelecedToCopyMerchantListData.imSoftwareType }}｜{{ currentSelecedToCopyMerchantListData.imChannelName }}) </span>
							</v-col>
							<v-col cols="12" md="3" sm="3" class="v-col-padding-3">
								<v-btn
									color="grey darken-1"
									:disabled="isTargetActionAlreadyExecute(1099)"
									class="specialActionBtn"
									@click="triggerExtraOperateByType(currentSelecedToCopyMerchantListData.merchantID, 1099)"
								>
									{{ isTargetActionAlreadyExecute(1099) ? '已執行' : '執行此動作' }}
								</v-btn>
							</v-col>

							<!-- <v-col cols="12" md="9" sm="9" class="v-col-padding-3">
								14天後檢查是否有數據 (未實作*)
							</v-col>
							<v-col cols="12" md="3" sm="3" class="v-col-padding-3">
								<v-btn color="grey darken-1" :disabled="isTargetActionAlreadyExecute(2222)"
									class="specialActionBtn"
									@click="triggerExtraOperateByType(currentSelecedToCopyMerchantListData.merchantID , 2222)">
									{{isTargetActionAlreadyExecute(2222) ? '已執行': '執行此動作'}}
								</v-btn>
							</v-col> -->
						</v-row>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 編輯商戶狀態 -->
		<v-dialog v-model="dialogVisible.isShowEditBriefInfo" max-width="450px">
			<MerchantStatusEditPopup ref="refMerchantStatusEditPopup" @parentCloseAllModal="closeAllModalAndRefresh()"> </MerchantStatusEditPopup>
		</v-dialog>

		<!-- 新增IM頻道 -->
		<v-dialog v-model="dialogVisible.isShowEditOther" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					新增IM頻道 (TG/Skype)

					<div class="top-right-item" @click="closeCreateIMDialog">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<v-row>
						<v-autocomplete :items="dropdownOfIMChannelType" :label="'IMChannel類型'" chips v-model="IMChannelForm.type"> </v-autocomplete>
					</v-row>

					<v-row>
						<v-text-field v-model="IMChannelForm.channelName" :label="'頻道名稱'"> </v-text-field>
					</v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="yellow darken-1" text @click="saveCreateIMChannel()" class="btn">
						<v-icon left>file_copy</v-icon>
						儲存送出
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 刪除商戶 -->
		<v-dialog v-model="dialogVisible.isShowDelete" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					刪除商戶

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<v-row>
						<v-col cols="12" md="12" sm="12">
							<v-autocomplete :items="merchantsDropdownItems" v-model="deleteMerchantID" :label="'商戶'"> </v-autocomplete>
						</v-col>

						<v-col cols="12" md="12" sm="12">
							<v-text-field :items="merchantsDropdownItems" v-model="deleteMerchantMemo" :label="'備註'"> </v-text-field>
						</v-col>
					</v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<span class="f12 mr-2 yellow-text"> <v-icon color="yellow" small>error</v-icon> 後台帳號也要另外刪除 </span>

					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn color="yellow darken-1" text @click="confirmDeleteMerchant" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 遊戲商VendorConfig配置對應編輯 -->
		<MerchantVendorConfigUpdate ref="refMerchantVendorConfigUpdate"></MerchantVendorConfigUpdate>

		<NotificationWarning
			v-show="showNotificationWarning"
			:title="`${$t('Message.__warning')}`"
			:max="1"
			:text="`${$t('Message.__isConfirmDelete')}#${deleteIndex}?`"
			:confirmed-action="confirmedAction"
			:canceled-action="canceledAction"
		/>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import MerchantsApi from '@/api/system-setting/merchants';
import DropdownApi from '@/api/commons/dropdown';
import oneToManyCustomizedSettingApi from '@/api/system-setting/oneToManyCustomizedSetting';
import { commonMethodMixin } from '@/utilities/common-methods';
import * as ResponseModel from '@/models/interfaces/commons/response';
import { Locale, Currency, Status } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';

import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import MerchantCompare from '@/components/system-setting/MerchantCompare.vue';
import MerchantVendorConfigUpdate from '@/components/system-setting/MerchantVendorConfigUpdate.vue';
import MerchantsAdvanceSetting from '@/components/system-setting/MerchantsAdvanceSetting.vue';
import MerchantsAdvanceSettingV2 from '@/components/system-setting/MerchantsAdvanceSettingV2.vue';
import MerchantStatusEditPopup from '@/components/system-setting/MerchantStatusEditPopup.vue';

import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';
import internalTest from '@/api/commons/internalTest';

@Component({
	components: { NotificationWarning, MerchantCompare, MerchantVendorConfigUpdate, MerchantsAdvanceSetting, MerchantsAdvanceSettingV2, MerchantStatusEditPopup },
	mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin],
})
export default class Merchants extends Vue {
	locale: typeof Locale = Locale;
	currency: typeof Currency = Currency;
	status: typeof Status = Status;
	WalletMode: typeof WalletMode = WalletMode;

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();
	isRenderComponent2: boolean = false;
	isShowMerchantCompare: boolean = false;
	isShowExtraInfoForPreview: boolean = false;
	isShowSearchExtraSettingDialog: boolean = false;
	isCreateUserAccountPanel: boolean = false;

	renderComponent: boolean = true;

	compareLeftMerchantID: number = 0;
	compareRightMerchantID: number = 0;
	dataTableFetchMaxSize: number = 500;

	selectedExportColumns: Model.IMerchantExportColumns = {} as Model.IMerchantExportColumns;
	searchfilter: Model.IMerchantFilterRequest = {} as Model.IMerchantFilterRequest;
	search: string = '';

	copyText: string = '';
	copyTextEN: string = '';
	panel: number[] = [0];

	merchantsData: Model.IMerchant[] = [];
	merchantsDataWithKeyword: Model.IMerchant[] = [];
	keepSpecialActionHistory: number[] = [];

	currentSelecedToCopyMerchantListData: Model.IMerchant = {} as Model.IMerchant;
	currentSelecedToCopyMerchant: Model.IMerchantCopyPreviewInfo = {} as Model.IMerchantCopyPreviewInfo;
	isDisabledCopyButton: boolean = false;

	isEdit: boolean = false;
	IMChannelForm: any = {};
	editedIndex: number = -1;
	editedItem: Model.IMerchantRequest = {} as Model.IMerchantRequest;
	deleteMerchantID: number = 0;
	deleteMerchantMemo: string = '';

	copyNewOneItem: Model.IMerchantCopyRequest = {} as Model.IMerchantCopyRequest;
	lastCopyNewOneItem: Model.IMerchantCopyRequest = {} as Model.IMerchantCopyRequest;
	copySourceMerchant: Model.IMerchantCopyPreviewInfo = {} as Model.IMerchantCopyPreviewInfo;

	editedDefaultBalanaceForm: Model.IMerchantEditDefaultBalance = {} as Model.IMerchantEditDefaultBalance;

	deleteIndex: number = -1;
	showNotificationWarning: boolean = false;
	adminSysFavoritSetting: ResponseModel.IAdminSysFavoritSetting = {} as ResponseModel.IAdminSysFavoritSetting;
	currencySelectItems: any[] = [];
	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantsWithCategoryDropdownItems: DropdownModel.IDropdownItems[] = [];
	hierarchyMerchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantTypesDropdownItems: DropdownModel.IDropdownItems[] = [];
	rootMerchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	organizationsDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantSettingKeysDropdownItems: DropdownModel.IDropdownItems[] = [];
	keywordSearchSettingKey: string = '';
	imChannelsDropdownItems: DropdownModel.IDropdownItems[] = [];
	rolesDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantSpecialFilterTypeDropdownItems: DropdownModel.IDropdownItems[] = [];
	copyMerchantExtraSettingFlagDropdownItems: DropdownModel.IDropdownItems[] = [];

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

	get specialMerchantFilterType() {
		return [
			{ value: 0, text: '不查詢三級代理' },
			{ value: 1, text: '搜尋全部' },
			{
				value: Locale.ENUS,
				text: this.$t(`Lang.${Locale.ENUS}`),
			},
		];
	}

	get dropdownOfIMChannelType() {
		return [
			{ value: 10000, text: 'TG (Telegram)' },
			{ value: 20000, text: 'Skpye' },
		];
	}

	get walletModeSelectItems() {
		const dropdowns = [];
		dropdowns.push({ value: WalletMode.TransferMode, text: this.$t('MerchantsStatus.TransferMode') });
		dropdowns.push({ value: WalletMode.SingleWalletMode, text: this.$t('MerchantsStatus.SingleWalletMode') });
		return dropdowns;
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

	get yesOrNoDropdownItems() {
		const myArray = [];
		myArray.push({ text: '是', value: true });
		myArray.push({ text: '否', value: false });

		return myArray;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__merchantID'), value: 'merchantID', width: '5%' },
			{ text: this.$t('TableHeader.__merchantName'), value: 'name', sortable: false },
			{ text: this.$t('TableHeader.__category'), value: 'merchantTypeName', sortable: false },
			{ text: this.$t('TableHeader.__walletMode'), value: 'walletMode', sortable: false },
			{ text: this.$t('TableHeader.__status'), value: 'status', align: 'center' },
			{ text: this.$t('TableHeader.__currency') + '（伺服器｜對帳）', value: 'currencyText' },
			{ text: '上級', value: 'parentMerchantID', sortable: false },
			{ text: '對接頻道名稱', value: 'imChannelName', align: 'start', sortable: false },
			{ text: this.$t('TableHeader.__defaultBalanace'), value: 'defaultBalance', align: 'end', sortable: false },
		];

		if (this.searchfilter.searchExtraSettingKey > 0) {
			data.push({ text: '額外欄位 (' + this.searchfilter.searchExtraSettingKey + ')', value: 'searchExtraSettingValue', align: 'end', sortable: false });
		}

		// 故意加這欄是因為希望關鍵字可以搜尋
		data.push({ text: this.$t('TableHeader.__createTime'), value: 'createTime', sortable: false });
		data.push({ text: '', value: 'keywordForJsSearch', sortable: false });

		if (this.hasPermissionUpdate || this.hasPermissionDelete) {
			data.splice(0, 0, { text: this.$t('TableHeader.__action'), value: 'action', sortable: false });
		}
		return data;
	}

	get formTitle() {
		return this.isCreateFrom ? this.$t('Popup.__create') : this.$t('Popup.__update');
	}
	get isCreateFrom() {
		return this.editedIndex === -1 ? true : false;
	}

	get gamesDropdownWithKeywordFilter() {
		const vm: any = this;
		return vm.filterDropdownItemsByKeywordSearch(vm.gamesDropdownItems, vm.globalKeywordSearchGamesDropdown, vm.searchfilter.enableSpecificGameID);
	}

	get filterSettingKeysDropdownItems() {
		const myItems: DropdownModel.IDropdownItems[] = this.merchantSettingKeysDropdownItems;

		const myTagIDText = '777777';

		// @ts-ignore
		myItems.push({ text: myTagIDText + '｜查詢特殊備註(memo2)', value: myTagIDText, valueInt: 777777, fullTextForKeywordSearch: myTagIDText } as DropdownModel.IDropdownItems);

		if (this.keywordSearchSettingKey == null || this.keywordSearchSettingKey.length <= 0) {
			return myItems;
		}

		const myKeyword: string = this.keywordSearchSettingKey.toLowerCase();

		const filterItems = myItems.filter((item) => {
			return item.fullTextForKeywordSearch.includes(myKeyword);
		});

		return filterItems;
	}

	whenDropdownSettingKeyChanged() {
		this.keywordSearchSettingKey = '';
		this.refresh();
	}

	isDisableCodeEdit(item: any) {
		if (this.isCreateFrom) {
			return false;
		}

		const vm: any = this;
		return vm.isEmptyString(item.code) ? false : true;
	}

	showDataTableSearchExtraSetting() {
		this.isShowSearchExtraSettingDialog = true;
	}

	showDeleteMerchantDialog() {
		this.deleteMerchantID = 0;
		this.dialogVisible.isShowDelete = true;
	}

	confirmDeleteMerchant() {
		EventBus.$emit('show_mask', null);
		MerchantsApi.deleteMerchant(this.deleteMerchantID, this.deleteMerchantMemo)
			.then(() => {
				EventBus.$emit(`successfully-modified`);
				this.close();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	whenSourceMerchantChanged() {
		const selectValue = this.merchantsDropdownItems.filter((x) => x.valueInt === this.copyNewOneItem.sourceMerchantID);
		if (selectValue && selectValue.length > 0) {
			this.copyNewOneItem.currency = selectValue[0].extraProperty.currency;

			const merchantID = this.copyNewOneItem.sourceMerchantID;

			EventBus.$emit('show_mask', null);
			MerchantsApi.getMerchantCopyPreviewInfo(merchantID)
				.then((x) => {
					this.copySourceMerchant = x.data;
					this.copyNewOneItem.parentMerchantID = x.data.parentMerchantID;

					this.copyNewOneItem.imChannelID = x.data.imChannelID;
					this.copyNewOneItem.isLockEditIMChannel = false;

					// 還是有修改的可能性 因此不要lock了
					// if (x.data.imChannelID > 0) {
					// 	this.copyNewOneItem.isLockEditIMChannel = true;
					// }

					// 重新render component
					this.dialogVisible.isShowCopyDialog = false;
					this.dialogVisible.isShowCopyDialog = true;
				})
				.finally(() => {
					setTimeout(() => {
						EventBus.$emit('hide_mask', null);
					}, 500);
				});
		}
	}

	showExtraOperateDialog() {
		// 只有正式商戶才彈出
		if (example.invalid.type !== 20) {
			return;
		}
		this.dialogVisible.isShowConfirm = true;
		this.keepSpecialActionHistory = [];
	}

	showEditMerchantVendorConfig(merchantID: number) {
		const targetRef: any = this.$refs.refMerchantVendorConfigUpdate;
		targetRef.getSettings(merchantID);
	}

	showEditStatus(item: Model.IMerchant) {
		this.dialogVisible.isShowEditBriefInfo = true;

		this.$nextTick(() => {
			const myRef: any = this.$refs.refMerchantStatusEditPopup;
			myRef.getPreviewInfo(item.merchantID);
		});
	}

	showMerchantFullInfo(merchantID: number) {
		this.dialogVisible.isShowFullInfo = true;

		this.$nextTick(() => {
			const refInput: any = this.$refs.refMerchantCompare;
			refInput.setMerchantID(merchantID, merchantID);
			refInput.getCompareInfo();
		});
	}

	showMerchantCompare() {
		this.isShowMerchantCompare = true;
		this.compareLeftMerchantID = 0;
		this.compareRightMerchantID = 0;
	}

	confirmCompareMerchants() {
		if (this.compareLeftMerchantID === 0 || this.compareRightMerchantID === 0) {
			alert('請選擇商戶!');
			return;
		}

		const url = '/system-setting/merchants/compare?leftMerchantID=' + this.compareLeftMerchantID + '&rightMerchantID=' + this.compareRightMerchantID;

		// @ts-ignore
		window.open(url, '_blank').focus();
	}

	refresh() {
		this.registerReloadComponent();
		this.searchfilter.orderByColumn = 'type,merchantID';
		this.searchfilter.isASC = true;
		this.searchfilter.isSpecialOrder = true;
		this.searchfilter.dataTableFetchMaxSize = this.dataTableFetchMaxSize;

		const vm: any = this;
		vm.globalKeywordSearchGamesDropdown = '';

		this.search = '';

		EventBus.$emit('show_mask', null);

		MerchantsApi.getMerchants(this.searchfilter)
			.then((res) => {
				this.merchantsData = res.data.items;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});

		this.editedItem.status = Status.Enable;
	}

	getCreateAvailableSettings() {
		MerchantsApi.getCreateAvailableSettings()
			.then((res) => {
				this.editedItem.extraSettings.customizedSettings = res.data;
			})
			.finally(() => {
				this.renderComponent = false;
				this.renderComponent = true;
			});
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.search = '';
				this.renderComponent = true;
			});
		});
	}

	showCopyHintDialog(item: Model.IMerchant) {
		this.currentSelecedToCopyMerchant = {} as Model.IMerchantCopyPreviewInfo;
		this.dialogVisible.isShowPreviewDialog = true;

		EventBus.$emit('show_mask', null);

		MerchantsApi.getMerchantCopyPreviewInfo(item.merchantID)
			.then((res) => {
				this.currentSelecedToCopyMerchantListData = item;
				this.currentSelecedToCopyMerchant = res.data;
				this.isDisabledCopyButton = res.data.isProvideComplete;
				this.copyMerchantInfoToClipboard_CN(res.data);
				this.copyMerchantInfoToClipboard_EN(res.data);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	isTargetActionAlreadyExecute(type: number) {
		return this.keepSpecialActionHistory.includes(type);
	}

	triggerExtraOperateByType(merchantID: number, type: number) {
		EventBus.$emit('show_mask', null);
		MerchantsApi.doExtraOperateForMerchant(merchantID, type)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.keepSpecialActionHistory.push(type);
			})
			.finally(() => {
				setTimeout(() => {
					EventBus.$emit('hide_mask', null);
				}, 250);
			});

		return;
	}

	getEnvTextByMerchantType(type: number, isCN: boolean) {
		let envText = 'XXXXXXXXXXXXXXXX';
		if (type === 20) {
			envText = isCN ? '正式' : 'Production';
		} else if (type === 40) {
			envText = isCN ? '測試' : 'Dev/Staging';
		}
		return envText;
	}

	copyMerchantInfoToClipboard_CN(model: Model.IMerchantCopyPreviewInfo) {
		this.copyText = model.credentialMessage_CN;
	}

	copyMerchantInfoToClipboard_EN(model: Model.IMerchantCopyPreviewInfo) {
		this.copyTextEN = model.credentialMessage_EN;
	}

	showSingleSettingKeyUpdateDialog(merchantID: number, settingKey: number = 3088) {
		if (settingKey <= 0) {
			return;
		}

		this.dialogVisible.isShowAdvanceInfo2 = true;

		// 3088 = IM頻道名稱
		this.$nextTick(() => {
			const ref: any = this.$refs.refMerchantsAdvanceSettingForSingleKey;
			ref.getMerchantAdvanceSetting(merchantID, settingKey);
		});
	}

	isSingleWalletMode(walletMode: WalletMode | null) {
		return walletMode === WalletMode.SingleWalletMode;
	}

	showMerchantAdvanceSetting(item: Model.IMerchant) {
		this.dialogVisible.isShowAdvanceInfo = true;

		this.$nextTick(() => {
			const ref: any = this.$refs.refMerchantsAdvanceSettingV2;
			ref.getMerchantAdvanceSetting(item.merchantID);
		});
	}

	showCreateIMChannelDialog() {
		this.dialogVisible.isShowEditOther = true;
		this.IMChannelForm = {};
	}

	saveCreateIMChannel() {
		EventBus.$emit('show_mask', null);
		oneToManyCustomizedSettingApi
			.createNewIMChannel(this.IMChannelForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.reloadIMChannelDropdown();
				this.dialogVisible.isShowEditOther = false;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeMerchantAdvanceSetting() {
		// *不要全關
		this.dialogVisible.isShowAdvanceInfo2 = false;
		this.dialogVisible.isShowAdvanceInfo = false;
	}

	closeConfirmDialog() {
		this.dialogVisible.isShowConfirm = false;
	}

	closeAllModalAndRefresh() {
		this.closeAllModal();
		this.refresh();
	}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
		this.isShowMerchantCompare = false;
		this.isShowSearchExtraSettingDialog = false;

		try {
			const myRef: any = this.$refs.refMerchantVendorConfigUpdate;
			myRef.closeAllModal();
		} catch (ex) {}
	}

	clear() {
		this.searchfilter = {} as Model.IMerchantFilterRequest;
	}

	addItem() {
		setTimeout(() => {
			this.dialogVisible.isShowCreateOrEdit = true;
		}, 150);

		this.isEdit = false;
		this.editedIndex = -1;
		this.editedItem = {} as Model.IMerchantRequest;
		this.editedItem.status = Status.Enable;
		this.editedItem.walletMode = 1;
		this.editedItem.defaultLanguage = Locale.ENUS;
		this.editedItem.currency = Currency.BETA;

		const text = uuid.v4().toString().replace(/-/g, '');
		this.editedItem.apiSecretKey = text.substring(0, 15);
		this.editedItem.extraSettings = {};

		this.getCreateAvailableSettings();
	}
	editItem(item: Model.IMerchant) {
		this.addItem();

		MerchantsApi.getMerchant(item.merchantID)
			.then((res) => {
				this.editedItem.merchantID = res.data.merchantID;
				this.editedItem.type = res.data.type;
				this.editedItem.name = res.data.name;
				this.editedItem.memo = res.data.memo;
				this.editedItem.defaultLanguage = res.data.defaultLanguage;
				this.editedItem.currency = res.data.currency;
				this.editedItem.status = res.data.status;
				this.editedItem.apiSecretKey = res.data.apiSecretKey;
				this.editedItem.apiValidatePlayerSessionUrl = res.data.apiValidatePlayerSessionUrl;
				this.editedItem.apiSingleWalletGetAmountUrl = res.data.apiSingleWalletGetAmountUrl;
				this.editedItem.walletMode = res.data.walletMode;
				this.editedItem.merchantGuid = res.data.merchantGuid;
				this.editedItem = { ...this.editedItem };
				this.editedItem.code = res.data.code;
			})
			.finally(() => {});

		this.isEdit = true;
		this.editedIndex = this.merchantsData.indexOf(item);
	}

	systemDefaultPassword() {
		const vm: any = this;
		return vm.getDefaultPasswordV2026();
	}

	showCopyNewOneDiaolog() {
		this.dialogVisible.isShowCopyDialog = true;
		this.copySourceMerchant = {} as Model.IMerchantCopyPreviewInfo;
		this.copyNewOneItem = {} as Model.IMerchantCopyRequest;
		this.copyNewOneItem.createAccountInfo = {} as Model.IMerchantCopyCreateUserAccountRequest;
		this.copyNewOneItem.createAccountInfo.organizationID = 0;
		this.copyNewOneItem.createAccountInfo.roleID = 0;
		this.copyNewOneItem.createAccountInfo.password = [REDACTED];
		this.copyNewOneItem.createAccountInfo.isHierarchyMode = true;

		// 如果連續操作 有部分欄位會帶入上一次填寫的
		if (this.lastCopyNewOneItem && this.lastCopyNewOneItem.createAccountInfo) {
			this.copyNewOneItem.sourceMerchantID = this.lastCopyNewOneItem.sourceMerchantID;
			this.copyNewOneItem.parentMerchantID = this.lastCopyNewOneItem.parentMerchantID;
			this.copyNewOneItem.currency = this.lastCopyNewOneItem.currency;
			this.copyNewOneItem.createAccountInfo.organizationID = this.lastCopyNewOneItem.createAccountInfo.organizationID;
			this.copyNewOneItem.createAccountInfo.roleID = this.lastCopyNewOneItem.createAccountInfo.roleID;
			this.copyNewOneItem.createAccountInfo.isHierarchyMode = this.lastCopyNewOneItem.createAccountInfo.isHierarchyMode;
			this.copyNewOneItem.imChannelID = this.lastCopyNewOneItem.imChannelID;
		}
	}

	saveCopyNewOne() {
		const vm: any = this;

		if (this.isCreateUserAccountPanel) {
			if (this.copyNewOneItem.createAccountInfo.organizationID <= 0 || this.copyNewOneItem.createAccountInfo.roleID <= 0 || vm.isEmptyString(this.copyNewOneItem.createAccountInfo.account)) {
				alert('帳戶新增表單有部分欄位未填寫');
				return;
			}
		}

		EventBus.$emit('show_mask', null);

		this.copyNewOneItem.isCreateUserAccount = this.isCreateUserAccountPanel;

		MerchantsApi.copyAndCreateNewMerchant(this.copyNewOneItem)
			.then((res) => {
				// 如果有部分項目失敗
				if (!vm.isEmptyString(res.data.debugMessage)) {
					alert('商戶建立成功、但' + res.data.debugMessage);
				}

				this.lastCopyNewOneItem = { ...this.copyNewOneItem };
				EventBus.$emit(`successfully-modified`);
				this.close();
				this.refresh();
				this.reloadMerchantDropdown();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	syncMerchantNameToAccount() {
		// 有些BUG 因為不sycn了 省得麻煩
		// this.copyNewOneItem.createAccountInfo.account = this.copyNewOneItem.name;
	}

	setAccountFormDefaultValue() {
		console.log(this.copyNewOneItem);
		console.log(this.copyNewOneItem.isCreateUserAccount);

		if (!this.copyNewOneItem.createAccountInfo && this.isCreateUserAccountPanel) {
			this.copyNewOneItem.createAccountInfo = {} as Model.IMerchantCopyCreateUserAccountRequest;
		}

		// if (this.copyNewOneItem.isCreateUserAccount) {
		// 	this.copyNewOneItem.isCreateUserAccount = false;
		// 	this.copyNewOneItem.isCreateUserAccount = false;
		// }

		this.isRenderComponent2 = false;
		this.isRenderComponent2 = true;
	}

	showExportSeleteColumns() {
		this.dialogVisible.isShowExport = true;
		this.removeAllSelectedColumns();
	}

	removeAllSelectedColumns() {
		this.selectedExportColumns = {} as Model.IMerchantExportColumns;
		this.selectedExportColumns.isExportMerchantID = true;
		this.selectedExportColumns.isIncludeOtherPage = true;
	}

	confrimAndExport() {
		this.closeAllModal();

		let myText = '';

		EventBus.$emit('show_mask', null);

		let targetItems: any[] = this.merchantsDataWithKeyword;

		if (this.selectedExportColumns.isIncludeOtherPage) {
			targetItems = this.merchantsData;
		}

		targetItems.forEach((item) => {
			let concateLineText = '';

			// ps. 順序應該要跟畫面勾選看到的一樣
			if (this.selectedExportColumns.isExportMerchantID) {
				concateLineText = concateLineText + item.merchantID + '\t';
			}
			if (this.selectedExportColumns.isExportMerchantName) {
				concateLineText = concateLineText + item.name + '\t';
			}
			if (this.selectedExportColumns.isExportApiKey) {
				concateLineText = concateLineText + item.apiSecretKey + '\t';
			}
			if (this.selectedExportColumns.isExportGuid) {
				concateLineText = concateLineText + item.merchantGuid + '\t';
			}
			if (this.selectedExportColumns.isExportWalletMode) {
				concateLineText = concateLineText + item.walletModeText + '\t';
			}
			if (this.selectedExportColumns.isExportCurrency) {
				concateLineText = concateLineText + item.currency + '\t';
			}
			if (this.selectedExportColumns.isExportIMChannel) {
				concateLineText = concateLineText + item.imChannelName + '\t';
			}
			if (this.selectedExportColumns.isExportTestLinkDev) {
				concateLineText = concateLineText + item.externalAPI_TestAccountLink_Dev + '\t';
			}
			if (this.selectedExportColumns.isExportTestLinkPro) {
				concateLineText = concateLineText + item.externalAPI_TestAccountLink_Pro + '\t';
			}
			if (this.selectedExportColumns.isExportExtraSettingValue) {
				concateLineText = concateLineText + item.searchExtraSettingValue + '\t';
			}
			concateLineText = concateLineText + '\n';

			myText = myText + concateLineText;
		});

		const vm: any = this;
		vm.copyTextToClipboard(myText);

		setTimeout(() => {
			EventBus.$emit('hide_mask', null);
		}, 500);
	}

	copyAllMerchantIDs() {
		let myText = '';

		if (this.search != null && this.search.length > 0) {
			this.merchantsDataWithKeyword.forEach((item) => {
				myText = myText + item.merchantID.toString() + '\n';
			});
		} else {
			this.merchantsData.forEach((item) => {
				myText = myText + item.merchantID.toString() + '\n';
			});
		}

		const vm: any = this;
		vm.copyTextToClipboard(myText);
	}

	close() {
		this.closeAllModal();

		setTimeout(() => {
			this.editedItem.status = Status.Enable;
			this.editedIndex = -1;
		}, 300);
	}

	closeCreateIMDialog() {
		this.dialogVisible.isShowEditOther = false;
	}

	save() {
		const request = <Model.IMerchantRequest>{ ...this.editedItem };
		request.status = request.status;

		EventBus.$emit('show_mask', null);

		if (this.editedIndex > -1) {
			MerchantsApi.updateMerchant(<Model.IMerchantRequest>request)
				.then(() => {
					EventBus.$emit(`successfully-modified`);
					this.close();
					this.refresh();
					this.reloadMerchantDropdown();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		} else {
			MerchantsApi.createMerchant(<Model.IMerchantRequest>request)
				.then(() => {
					EventBus.$emit(`successfully-modified`);
					this.close();
					this.refresh();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	getFilterItems(items: any) {
		this.merchantsDataWithKeyword = items;
	}

	deleteItem(item: Model.IMerchantRequest) {
		this.deleteIndex = item.merchantID;
		this.showNotificationWarning = true;
		this.$notify({
			group: 'confirm',
			type: 'warn',
			duration: -1,
			speed: 10,
		});
	}

	confirmedAction() {
		MerchantsApi.getMerchant(this.deleteIndex).then((res) => {
			const request = {} as Model.IMerchantRequest;
			request.merchantID = res.data.merchantID;
			request.name = res.data.name;
			request.memo = res.data.memo;
			request.defaultLanguage = res.data.defaultLanguage;
			request.currency = res.data.currency;
			request.status = Status.PermanentDelete;
			request.apiSecretKey = res.data.apiSecretKey;
			request.apiValidatePlayerSessionUrl = res.data.apiValidatePlayerSessionUrl;
			request.apiSingleWalletGetAmountUrl = res.data.apiSingleWalletGetAmountUrl;
			request.walletMode = res.data.walletMode;

			MerchantsApi.updateMerchant(<Model.IMerchantRequest>request).then(() => {
				EventBus.$emit(`successfully-modified`);
				this.close();
				this.refresh();
			});
		});

		this.showNotificationWarning = false;
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
		this.showExtraOperateDialog();
	}
	canceledAction() {
		this.showNotificationWarning = false;
	}

	showMerchantEnableGames(myMerchantID: number) {
		const myFilter: any = {
			querySqlID: 900151,
			merchantID: myMerchantID,
		};

		const myConfig: any = {
			maxWidth: 500,
			isShowCloseButton: true,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	reloadMerchantDropdown() {
		DropdownApi.getMerchants().then((res) => {
			this.merchantsDropdownItems = res;
		});

		const merchantFiler = {} as DropdownModel.IMerchantDropdownRequest;
		merchantFiler.isIncludeCategory = true;
		merchantFiler.isIncludeFavoritCopyMerchantCategory = true;

		DropdownApi.getMerchantsByFilter(merchantFiler).then((res) => {
			this.merchantsWithCategoryDropdownItems = res;
		});
		DropdownApi.getHierarchyMerchants().then((res) => {
			this.hierarchyMerchantsDropdownItems = res;
		});
	}

	reloadIMChannelDropdown() {
		DropdownApi.getIMChannels().then((res) => {
			this.imChannelsDropdownItems = res;
		});
	}

	mounted() {
		this.reloadMerchantDropdown();
		this.reloadIMChannelDropdown();

		DropdownApi.getMerchantTypes().then((res) => {
			this.merchantTypesDropdownItems = res;
		});

		DropdownApi.getCurrency().then((res) => {
			this.currencySelectItems = res;
		});

		DropdownApi.getRootAgents().then((res) => {
			this.rootMerchantsDropdownItems = res;
		});

		DropdownApi.getOrganizations().then((res) => {
			this.organizationsDropdownItems = res;
		});

		DropdownApi.getRoles().then((res) => {
			this.rolesDropdownItems = res;
		});

		DropdownApi.getGames().then((res) => {
			this.gamesDropdownItems = res;
		});

		DropdownApi.getMerchantSettingKeys().then((res) => {
			res.forEach((item) => {
				item.text = item.value.toString() + '｜' + item.text;
			});

			this.merchantSettingKeysDropdownItems = res;
		});

		DropdownApi.getMerchantSpecialFilterType().then((res) => {
			this.merchantSpecialFilterTypeDropdownItems = res;

			// 再額外插入tag提供篩選
			DropdownApi.getGeneralTags(20000).then((myRes) => {
				const arrayItems = myRes;
				if (arrayItems != null && arrayItems.length > 0) {
					arrayItems.forEach((item) => {
						item.text = '【標籤】' + item.text;
						this.merchantSpecialFilterTypeDropdownItems.push(item);
					});
				}
			});
		});

		DropdownApi.getCopyMerchantExtaSettings().then((res) => {
			this.copyMerchantExtraSettingFlagDropdownItems = res;
		});

		internalTest.getAdminSysFavoritSetting().then((res) => {
			this.adminSysFavoritSetting = res.data;
		});

		// 鍵盤事件監聽
		const vm = this;
		document.addEventListener('keydown', (event) => {
			const key = event.key; // const {key} = event; in ES6+
			if (key === 'Escape') {
				vm.closeAllModal();
			}
		});

		this.refresh();
	}
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

.extra-checkbox .v-messages {
	display: none;
}

.reviewMerchant .col-md-6,
.reviewMerchant .col-md-12 {
	padding: 4px 12px !important;
}

.extra-setting-create-2 {
	background: #222;
	border-radius: 5px;
	height: 100%;
	padding: 5px 30px;
}

.extra-setting-create {
	background: #222;
	border-radius: 5px;
	height: 95%;
	padding: 10px 20px;
}

.extra-setting-create > div {
	width: 75%;
}

.copySourceExtraTags {
	width: 100%;
	margin-top: -10px;
	text-align: right;
	min-height: 22px;
}

.copySourceExtraTags .v-chip {
	padding: 3px 6px !important;
}

.specialActionBtn {
	min-width: 100px !important;
}

.copyMerchantInputFrom {
	/* max-width: 350px; */
}

.mySelectColumnPanel .v-label {
	font-size: 18px;
	line-height: 28px;
}

.mySelectColumnPanel .v-label:hover {
	opacity: 0.8;
}

.mySelectColumnPanel .v-input--selection-controls {
	margin-top: -10px;
}

.mySelectColumnPanel .v-input--selection-controls:hover {
	opacity: 0.8;
}
</style>
