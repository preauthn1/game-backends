<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="gameVendorDropdownItems" class="required" chips v-model="searchFilter.gameVendorID" :label="$t('TableHeader.__gameVendorType')">
										<template #selection="{ item }">
											<v-chip color="teal darken-3">{{ item.text }}</v-chip>
										</template>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete :items="searchTargetFieldItems" :label="'搜尋欄位'" small-chips v-model="searchTargetField"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="myInputKeyword" :label="'查詢文字'" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.playerID" :label="$t('TableHeader.__playerID')" append-icon="perm_identity" clearable> </v-text-field>
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
									<v-autocomplete
										clearable
										:items="gamesDropdownWithKeywordFilter"
										no-filter
										@focus="resetDropdwonKeyword()"
										:no-data-text="$t('Message.__pleaseInputKeywordToSearchDropdown')"
										:search-input.sync="globalKeywordSearchGamesDropdown"
										@change="whenGameSelectChange()"
										v-model="searchFilter.gameID"
										:label="$t('TableHeader.__game')"
									>
										<template slot="selection" slot-scope="data">
											{{ data.item.text }}
										</template>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="merchantDropdownItems" class="merchantDropdownItems" v-model="searchFilter.merchantID" :label="$t('TableHeader.__merchant')">
										<template v-slot:item="data">
											<div :class="data.item.valueInt == null ? 'merchantCategoryText' : 'merchantValue'">
												<span>{{ data.item.text }}</span>
											</div>
										</template>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="betStatusDropdownItems" v-model="searchFilter.status" :label="$t('TableHeader.__status')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.playerName" :label="$t('TableHeader.__playerAccount')" append-icon="person" clearable> </v-text-field>
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
									<date-picker
										v-model="datetimeRangePicker.searchStartTimeRange"
										type="datetime"
										v-bind:clearable="false"
										value-type="timestamp"
										range
										:placeholder="$t('TableHeader.__time')"
									>
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="quick-time">
								<!-- <div class="item"> -->
								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }" @click="globalQuicklyPickTimeRange('today')">
									<v-icon left small>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__today`) }}
								</v-chip>
								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }" @click="globalQuicklyPickTimeRange('yesterday')">
									<v-icon left small>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__yesterday`) }}
								</v-chip>
								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }" @click="globalQuicklyPickTimeRange('thisWeek')">
									<v-icon left small>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__thisWeek`) }}
								</v-chip>

								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }" @click="globalQuicklyPickTimeRange('lastWeek')">
									<v-icon left small>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__lastWeek`) }}
								</v-chip>
								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }" @click="globalQuicklyPickTimeRange('thisMonth')">
									<v-icon left small>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__thisMonth`) }}
								</v-chip>
								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }" @click="globalQuicklyPickTimeRange('lastMonth')">
									<v-icon left small>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__lastMonth`) }}
								</v-chip>
								<!-- </div> -->
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="tagsDropdownItems" v-model="searchFilter.tagID" :label="$t('TableHeader.__tag')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="clickSearch">
										<v-icon left small>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;

									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left small>clear</v-icon>
										{{ $t('Button.__reset') }}
									</v-btn>

									<v-btn color="light-blue lighten-3" text class="btn search ml-1" v-if="hasExportPermission" @click.stop="exportFiles">
										<v-icon left small>cloud_download</v-icon>
										{{ $t('Button.__downloadFile') }}
									</v-btn>

									<v-btn color="purple lighten-3" text class="btn search ml-1" v-if="hasExportPermission" @click.stop="copyAllTransactionNo">
										<v-icon left small>cloud_download</v-icon>
										複製單號
									</v-btn>

									<!-- <v-btn color="purple lighten-3" text class="btn mt-1" v-if="hasDevelopeSuperPermission" @click.stop="showConfirmNotSettleRecord">
										<v-icon left>cloud_download</v-icon>
										檢查未結算單 </v-btn
									>&#9; -->

									<v-btn color="grey lighten-1" small text class="btn mt-2" v-if="hasDevelopeSuperPermission" @click.stop="showBatchRefundDialog">
										<v-icon left>cloud_download</v-icon>
										批次Refund
									</v-btn>

									<v-btn color="green lighten-3" small text class="btn mt-2" v-if="hasDevelopeSuperPermission" @click.stop="showBatchWritePerGameDialog">
										<v-icon left>cloud_download</v-icon>
										批次補perGame
									</v-btn>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div>
			<div style="padding: 5px">
				<v-icon color="yellow darken-2" class="icon-m-size">error</v-icon>
				<span class="f12 yellow-text"> AGG遊戲商默認不得查詢超過23小時 </span>

				<span class="ml-4 f14 red-text">{{ searchHintText }}</span>
			</div>
		</div>

		<v-data-table
			:items="queryRecords"
			:headers="headers"
			id="table"
			:disable-sort="true"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps"
			@update:page="changePage"
			@update:items-per-page="getItemPerPage"
		>
			<template v-slot:item.action="{ item }">
				<div class="operate_icons">
					<v-tooltip top v-if="hasDetailPagePermission">
						<template v-slot:activator="{ on, attrs }">
							<v-icon color="blue lighten-3" dark v-bind="attrs" v-on="on" @click="openBetRecordDetail(item.betRecordNoPGS)"> description </v-icon>
						</template>
						<span>查看詳細</span>
					</v-tooltip>

					<v-tooltip top v-if="hasDevelopeSuperPermission">
						<template v-slot:activator="{ on, attrs }">
							<v-icon color="" dark v-bind="attrs" v-on="on" @click="showDebugSQLDialog(item.betRecordNoPGS)"> stars </v-icon>
						</template>
						<span>查看Log</span>
					</v-tooltip>

					<v-tooltip top v-if="hasAdminPermission || hasCSPermission">
						<template v-slot:activator="{ on, attrs }">
							<v-icon color="yellow lighten-1" dark v-bind="attrs" v-on="on" @click="showBetRecordCopyInfo(item.betRecordNoPGS)"> file_copy </v-icon>
						</template>
						<span>複製資訊</span>
					</v-tooltip>

					<v-tooltip top v-if="item.status == 0 && hasPermissionManualRefund">
						<template v-slot:activator="{ on, attrs }">
							<v-icon color="red " dark v-bind="attrs" v-on="on" @click="confirmManualRefund(item)"> close </v-icon>
						</template>
						<span>取消注單退還金幣</span>
					</v-tooltip>
				</div>
			</template>

			<template v-slot:item.betRecordNoPGS="{ item }">
				<div class="">
					<div class="copyTextTo" @click="copyText(item.betRecordNoPGS)">
						{{ item.betRecordNoPGS }}
					</div>
					<div class="copyTextTo f12" @click="copyText(item.betRecordNo)" style="color: #999; margin-top: -3px">
						{{ item.betRecordNo }}
					</div>
				</div>
			</template>

			<template v-slot:item.merchantNotifyQueueID="{ item }">
				<div v-if="!item.isSingleWallet || !hasDetailPagePermission">
					<span></span>
				</div>

				<div v-else-if="item.merchantNotifyQueueID == 0" class="text-c">
					<a
						:href="'/system-logs/SingleWalletPlatformReqeustLog?referenceTransactionNo=' + item.betRecordNoPGS + '&begin=' + toDisplayTime(item.createTime)"
						target="_blank"
						class="link-text-grey"
					>
						成功
					</a>
				</div>

				<div v-else-if="item.merchantNotifyQueueID > 0 && hasPermissionManualRefund" class="text-c">
					<a
						:href="'/system-logs/SingleWalletPlatformReqeustLog?referenceTransactionNo=' + item.betRecordNoPGS + '&begin=' + toDisplayTime(item.createTime)"
						target="_blank"
						class="link-text-grey"
					>
						成功
					</a>
				</div>

				<div v-else class="minus-red">異常</div>
			</template>

			<template v-slot:item.gameName="{ item }">
				<span :title="item.gameID">
					{{ item.gameName }}
				</span>

				<div style="font-size: 12px; color: #999">{{ item.gameID }}｜{{ item.vendorConfigID }}</div>
			</template>

			<template v-slot:item.statusText="{ item }">
				<span :class="'status_' + item.status">{{ item.statusText }}</span>
			</template>

			<template v-slot:item.gameVendorName="{ item }">
				<v-chip>{{ item.gameVendorName }}</v-chip>
			</template>

			<template v-slot:item.merchantName="{ item }">
				<div>
					<v-tooltip top color="teal darken-1">
						<template v-slot:activator="{ on }">
							<a v-on="on" @click="openPlayerDetail(item.playerID)" :title="item.playerID">
								{{ item.playerName }}
							</a>
						</template>
						<span>{{ $t('Button.__openLink') }} ({{ item.playerID }}) </span>
					</v-tooltip>
				</div>

				<div style="font-size: 12px; color: #999" :title="item.merchantID">
					{{ item.merchantName }}
					<v-chip x-small color="purple darken-2" v-if="item.isSingleWallet"> SW </v-chip>
				</div>
			</template>

			<template v-slot:item.tags="{ item }">
				<v-tooltip top color="blue darken-1" v-for="(tag, index) in item.tagDetails" :key="index">
					<template v-slot:activator="{ on }">
						<span v-on="on" small class="mr-1 specialTag">
							{{ tag.text }}
						</span>
					</template>
					<span>{{ tag.key }}／{{ tag.text }}</span>
				</v-tooltip>
			</template>

			<template v-slot:item.betAmount="{ item }">
				{{ item.betAmount | amountDisplay }}
			</template>
			<template v-slot:item.payoutAmount="{ item }">
				{{ item.payoutAmount | amountDisplay }}
			</template>
			<template v-slot:item.winLossAmount="{ item }">
				<span :class="item.winLossAmount < 0 ? 'minus-red' : ''">
					{{ item.winLossAmount | amountDisplay }}
				</span>
			</template>

			<template v-slot:item.createTime="{ item }">
				<span class="pointer" @click="globalPopupCopyDateTime(item.createTime)">
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>
			</template>

			<template slot="body.append" v-if="queryRecordsTotalSum && queryRecordsTotalSum != null && queryRecords && queryRecords.length > 0">
				<tr>
					<td colspan="2">
						<div class="text-l">{{ $t('TableHeader.__total') }} ({{ $t('TableHeader.__currentPageOnly') }}) :</div>
					</td>
					<td></td>

					<td></td>
					<td></td>

					<td class="text-r">
						{{ queryRecordsTotalSum.betAmount | amountDisplay }}
					</td>
					<td class="text-r">
						{{ queryRecordsTotalSum.payoutAmount | amountDisplay }}
					</td>

					<td class="text-r">
						<span :class="queryRecordsTotalSum.winLossAmount < 0 ? 'minus-red' : ''">
							{{ queryRecordsTotalSum.winLossAmount | amountDisplay }}
						</span>
					</td>

					<td></td>

					<td></td>

					<td></td>
				</tr>
			</template>
		</v-data-table>

		<!-- 是否確認進行【注單取消】? -->
		<v-dialog v-model="isShowConfirm" max-width="500px">
			<v-card class="notify-queue-info">
				<v-card-title>
					是否確認進行【注單取消】?
					<div class="top-right-item">
						<div class="close" @click="closeAllDialog">
							<v-icon>close</v-icon>
						</div>
					</div>
				</v-card-title>
				<v-card-text class="">
					<v-row style="padding: 15px 35px">
						<v-col cols="12" sm="12" md="12">
							<div class="common-info-label">{{ $t('TableHeader.__betRecordNoPGS') }}</div>
							<div class="common-info-value">
								{{ refundTarget.betRecordNoPGS }}
							</div>
						</v-col>

						<v-col cols="12" sm="12" md="12">
							<div class="common-info-label">{{ $t('TableHeader.__merchant') }}</div>
							<div class="common-info-value">
								{{ refundTarget.merchantName }}
							</div>
						</v-col>

						<v-col cols="12" sm="12" md="12">
							<div class="common-info-label">{{ $t('TableHeader.__game') }}</div>
							<div class="common-info-value">
								{{ refundTarget.gameName }}
							</div>
						</v-col>

						<v-col cols="12" sm="12" md="12">
							<div class="common-info-label">{{ $t('TableHeader.__betAmount') }}</div>
							<div class="common-info-value">
								{{ refundTarget.betAmount }}
							</div>
						</v-col>

						<v-col cols="12" sm="12" md="12">
							<div class="common-info-label">{{ $t('TableHeader.__tag') }}</div>
							<div class="common-info-value">
								<v-tooltip top color="blue darken-1" v-for="(tag, index) in refundTarget.tags" :key="index">
									<template v-slot:activator="{ on }">
										<span v-on="on" small class="mr-1 specialTag" style="max-width: 150px; padding: 2px 12px">
											{{ tag }}
										</span>
									</template>
									<span>{{ tag }}</span>
								</v-tooltip>
							</div>
						</v-col>
					</v-row>
				</v-card-text>

				<v-row>
					<v-col cols="12" sm="12" md="12">
						<div class="text-c" style="margin-top: 5px">
							<v-spacer></v-spacer>

							<div style="padding: 0px 5px 25px 5px">
								<v-btn color="red lighten-1" text @click="manualRefund(refundTarget.betRecordNoPGS)" class="btn">
									<v-icon left>close</v-icon>
									取消注單並退還金幣
								</v-btn>
							</div>

							<div style="padding: 2px; display: inline-block">
								<v-checkbox v-model="isSkipConfirm" :label="'不要再顯示確認視窗'"> </v-checkbox>
							</div>
						</div>
					</v-col>
				</v-row>
			</v-card>
		</v-dialog>

		<!-- 搜尋未結算注單 -->
		<v-dialog v-model="isFindNotSettleRecordConfirm" max-width="500px" persistent>
			<v-card>
				<v-card-title class="grey darken-2">
					檢查未結算注單
					<div class="top-right-item">
						<div class="close" @click="closeAllDialog">
							<v-icon>close</v-icon>
						</div>
					</div>
				</v-card-title>

				<v-card-text style="padding: 5px 25px">
					<v-row>
						<v-col cols="12" md="8" sm="8">
							<v-autocomplete clearable :items="gameVendorDropdownItems" class="required" chips v-model="findRecordConfirm.gameVendorID" :label="$t('TableHeader.__gameVendorType')">
							</v-autocomplete>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="10" md="10">
							<div style="margin-bottom: -15px">{{ $t('TableHeader.__beginTime') }} - {{ $t('TableHeader.__endTime') }}</div>

							<date-picker
								v-model="findRecordConfirm.searchStartTimeRange"
								type="datetime"
								@change="refreshDialog()"
								value-type="timestamp"
								range
								:placeholder="$t('TableHeader.__time')"
							>
							</date-picker>
						</v-col>
					</v-row>
				</v-card-text>

				<v-card-actions class="mt-4">
					<v-spacer></v-spacer>

					<v-btn color="green lighten-1" text @click="searchNotSettleRecord()" class="btn">
						<v-icon left>check</v-icon>
						確認
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!--顯示debugSQL -->
		<v-dialog v-model="isShowDebugSQLDialog" max-width="850px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					DebugSQL & Log紀錄

					<div class="top-right-item">
						<div class="close" @click="closeAllDialog">
							<v-icon>close</v-icon>
						</div>
					</div>
				</v-card-title>

				<v-card-text>
					<h2 class="yellow-text mt-4">
						基本SQL
						<v-icon @click="copyTextToClipboard2(debugSQLInfo.debugSQL)">file_copy</v-icon>
					</h2>

					<div style="white-space: break-spaces; padding: 5px 15px; background: #2e2e2e; margin-top: 5px">
						{{ debugSQLInfo.debugSQL }}
					</div>

					<h2 class="yellow-text mt-4 mb-4">Log紀錄</h2>

					<div v-for="(detailInfo, index) in debugSQLInfo.logItems" :key="index">
						<div style="padding: 5px 15px; margin-bottom: 50px; position: relative">
							<div style="position: absolute; left: -5px; top: -10px">
								<div style="background: rgb(120 76 240); width: 25px; height: 25px; border-radius: 50%; text-align: center; display: inline-block">
									{{ index + 1 }}
								</div>

								<div class="pointer" @click="copyTextToClipboard(detailInfo.uniqueID)" style="display: inline-block; margin-left: 5px; color: #888">
									{{ detailInfo.uniqueID }}
								</div>

								<v-chip v-if="detailInfo.isErrorLog" small color="red darken-4" class="ml-2"> ErrorLog </v-chip>
							</div>

							<v-btn style="position: absolute; right: 30px; top: -10px" color="purple darken-1" @click="copyRequestInfo(detailInfo)"> 複製資訊 </v-btn>

							<v-flex>
								<v-row>
									<v-col cols="12" sm="12" md="12">
										<div class="lineDiv info-column-name">請求網址</div>
										<div style="color: #2196f3">
											{{ detailInfo.requestUrl }}
										</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="12" md="12">
										<div class="lineDiv info-column-name">請求時間</div>
										<div>
											{{ detailInfo.createTime | timeWithAddTimezone(userProfile.timeZone) }}
										</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="12" md="12">
										<div class="lineDiv info-column-name">請求參數 (request body)</div>
										<div class="info-code-panel">
											{{ detailInfo.requestBody }}
										</div>
									</v-col>
								</v-row>

								<v-row v-if="!isEmptyString(detailInfo.requestHeader)">
									<v-col cols="12" sm="12" md="12">
										<div class="lineDiv info-column-name">請求表頭 (request header)</div>
										<div class="info-code-panel">
											{{ detailInfo.headers }}
										</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="12" md="12">
										<div class="lineDiv info-column-name">API回應 (response)</div>
										<div class="info-code-panel">
											{{ detailInfo.responseText }}
										</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="12" md="12">
										<div class="lineDiv info-column-name">執行時間 (executionTime)</div>
										<div>
											{{ detailInfo.executionTime }}
										</div>
									</v-col>
								</v-row>
							</v-flex>
						</div>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!--批次補perGame資料 -->
		<v-dialog v-model="isShowBatchWritePerGame" max-width="650px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					批次補上PerGame

					<div class="top-right-item">
						<div class="close" @click="closeAllDialog">
							<v-icon>close</v-icon>
						</div>
					</div>
				</v-card-title>

				<v-card-text>
					<v-row>
						<v-col cols="12" sm="12" md="12">
							<div class="">
								<div class="targetHistoryValue">
									<v-checkbox v-model="batchWritePerGameForm.isReplaceGameTimeToNowTime" :label="'是否使用當前時間做為遊玩時間(對帳日期)'"> </v-checkbox>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="12" md="12">
							<div class="textarea-bg h300">
								<div class="targetHistoryValue">
									<v-textarea v-model="batchWritePerGameForm.CommaSeperateTransactionNoPGS" :label="'貼上單號(換行或逗號分開)'"> </v-textarea>
								</div>
							</div>
						</v-col>
					</v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<span class="f12 yellow-text mr-4"> <v-icon small color="yellow darken-2">info</v-icon> 根據狀況，可能需要重跑統計數據 </span>

					<v-btn color="green lighten-1" text @click="confrimBatchWritePerGameSubmit()" class="btn">
						<v-icon left>check</v-icon>
						確認
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!--批次Refund資料 -->
		<v-dialog v-model="isShowBatchRefund" max-width="650px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					批次Refund (Cancel取消單)

					<div class="top-right-item">
						<div class="close" @click="closeAllDialog">
							<v-icon>close</v-icon>
						</div>
					</div>
				</v-card-title>

				<v-card-text>
					<v-row>
						<v-col cols="12" sm="12" md="12" class="mt-4">
							<v-text-field v-model="batchRefundForm.memo" :label="'備註'"></v-text-field>
						</v-col>

						<v-col cols="12" sm="12" md="12">
							<div class="textarea-bg h300">
								<div class="targetHistoryValue">
									<v-textarea v-model="batchRefundForm.allTransactionNoText" :label="'貼上單號(換行或逗號分開)'"> </v-textarea>
								</div>
							</div>
						</v-col>
					</v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="green lighten-1" text @click="confrimBatchRefundSubmit()" class="btn">
						<v-icon left>check</v-icon>
						確認
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<PopupCopyDatetime ref="refCopyDateTime"></PopupCopyDatetime>

		<NotifyQueueDetailPopup ref="refNotifyQueueDetailPopup"></NotifyQueueDetailPopup>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
const playersModule = namespace('Players');
import EventBus from '@/utilities/event-bus';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin, toLocalDateMMdd } from '@/utilities/datetime-format';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import * as Model from '@/models/interfaces/game-management/per-game';
import GameManagementApi from '@/api/game-management/per-game';
import DropdownApi, { convertLang } from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import * as Permission from '@/models/status/permission';
import PopupCopyDatetime from '@/components/PopupCopyDatetime.vue';
import NotifyQueueDetailPopup from '@/components/game-management/NotifyQueueDetailPopup.vue';

import axios, { AxiosResponse, AxiosRequestConfig } from 'axios';

const authModule = namespace('Auth');

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, formatMixin, commonMethodMixin], components: { DatePicker, NotifyQueueDetailPopup, PopupCopyDatetime } })
export default class List extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@playersModule.State('transactionNoPGS') transactionNoPGS!: string;

	renderComponent: boolean = true;
	panel: number[] = [0];
	isShowConfirm: boolean = false;
	isShowBatchWritePerGame: boolean = false;
	isShowBatchRefund: boolean = false;
	isSkipConfirm: boolean = false;

	isShowDebugSQLDialog: boolean = false;
	isFindNotSettleRecordConfirm: boolean = false;
	searchHintText: string = '';

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchFilter: Model.IUnionBetRecordSearchRequest = {} as Model.IUnionBetRecordSearchRequest;
	debugSQLInfo: any = {};

	searchTargetField: string = '';
	myInputKeyword: string = '';

	betStatusDropdownItems: DropdownModel.IDropdownItems[] = [];
	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantDropdownItems: DropdownModel.IDropdownItems[] = [];
	gameVendorDropdownItems: DropdownModel.IDropdownItems[] = [];
	tagsDropdownItems: DropdownModel.IDropdownItems[] = [];

	batchWritePerGameForm: any = {};
	batchRefundForm: any = {};

	findRecordConfirm: any = {};

	queryRecordsTotalSum: any = {};
	queryRecords: Model.IUnionBetRecordResponse[] = [];
	queryNotifyQueueInfo: Model.IBetRecordNotifyQueue = {} as Model.IBetRecordNotifyQueue;
	refundTarget: Model.IUnionBetRecordResponse = {} as Model.IUnionBetRecordResponse;

	@Action('Players/setTransactionNoPGS') private setTransactionNoPGS!: any;
	@Action('Players/setPlayerID') private setPlayerID!: any;

	get headers() {
		return [
			{ text: this.$t('TableHeader.__action'), value: 'action' },
			{ text: this.$t('TableHeader.__time'), value: 'createTime', width: '8%' },
			{ text: this.$t('TableHeader.__betRecordNoPGS') + '／' + this.$t('TableHeader.__betRecordNo'), value: 'betRecordNoPGS', width: '12%' },
			{ text: this.$t('TableHeader.__game'), value: 'gameName' },
			{ text: this.$t('TableHeader.__merchant') + '／' + this.$t('TableHeader.__playerAccount'), value: 'merchantName' },
			{ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end' },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', align: 'end' },
			{ text: this.$t('TableHeader.__winLossAmount'), value: 'winLossAmount', align: 'end' },
			{ text: this.$t('TableHeader.__status'), value: 'statusText', align: 'end' },
			{ text: this.$t('TableHeader.__tag'), value: 'tags', align: 'end' },
			{ text: '通知狀態', value: 'merchantNotifyQueueID', align: 'end' },
		];
	}
	get searchTargetFieldItems() {
		return [
			{ text: 'PGS單號 (TransactionNo)', value: '1' },
			{ text: '三方遊戲商原廠單號', value: '2' },
			{ text: '遊戲局號', value: '3' },
		];
	}

	get hasCSPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_PGS_CustomerService) >= 0;
	}

	get hasDevelopeSuperPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_AllowToUseDevelopeFeature) >= 0;
	}

	get hasAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagementAdminPermission) >= 0;
	}

	get hasPermissionManualRefund() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagement_BetRecord_ManualRefund) >= 0;
	}

	get hasDetailPagePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagement_BetRecordSearch) >= 0;
	}

	get hasExportPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagement_BetRecordSearch_Export) >= 0;
	}

	get gamesDropdownWithKeywordFilter() {
		const vm: any = this;
		return vm.filterDropdownItemsByKeywordSearch(vm.gamesDropdownItems, vm.globalKeywordSearchGamesDropdown, this.searchFilter.gameID);
	}

	openBetRecordDetail(transactionNo: string) {
		this.setTransactionNoPGS(transactionNo);
		this.$router.push({ path: '/games-management/bet-record-search' });
	}

	showBetRecordCopyInfo(betRecordNoPGS: string) {
		EventBus.$emit('show_mask', null);
		GameManagementApi.getBetRecordCopyInfo(betRecordNoPGS)
			.then((res) => {
				const myConfig: any = {
					copyText: res.data.message,
				};
				EventBus.$emit(`globalShowCopyTextDialog`, myConfig);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	openPlayerDetail(playerID: number) {
		this.setPlayerID(playerID);
		this.$router.push({ path: '/members-management/detail' });
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		if (this.queryRecords.length > 0) {
			this.search();
		}
	}

	getItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		if (this.queryRecords.length > 0) {
			this.search();
		}
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	toDisplayTime(isoTime: string) {
		return timeWithTimezone(isoTime, this.userProfile.timeZone);
	}

	clickSearch() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.search();
	}

	refreshDialog() {
		this.isFindNotSettleRecordConfirm = false;
		this.isFindNotSettleRecordConfirm = true;
	}

	showDebugSQLDialog(transactionNo: string) {
		this.isShowDebugSQLDialog = true;
		this.debugSQLInfo = {};

		EventBus.$emit('show_mask', null);
		GameManagementApi.getDebugSQLAndLog(transactionNo)
			.then((res) => {
				this.debugSQLInfo = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showConfirmNotSettleRecord() {
		this.isFindNotSettleRecordConfirm = true;
		this.findRecordConfirm = {};
		this.findRecordConfirm.gameVendorID = 290000;

		this.findRecordConfirm.beginTime = monthsAgo(0)[0];
		this.findRecordConfirm.endTime = monthsAgo(0)[1] - 86400 * 1000;
		this.findRecordConfirm.searchStartTimeRange = [this.findRecordConfirm.beginTime, this.findRecordConfirm.endTime];
	}

	showBatchWritePerGameDialog() {
		this.batchWritePerGameForm.isReplaceGameTimeToNowTime = false;
		this.batchWritePerGameForm.commaSeperateTransactionNoPGS = '';
		this.batchWritePerGameForm.isUpdatePayoutAmount = true;
		this.batchWritePerGameForm.isWriteSingleWalletNotifyQueue = true;

		this.isShowBatchWritePerGame = true;
	}

	showBatchRefundDialog() {
		this.batchRefundForm.memo = '手動Cancel';
		this.batchRefundForm.delayMilliseconds = 10;
		this.batchRefundForm.allTransactionNoText = '';
		this.batchRefundForm.ListOfTransactionNo = [];

		this.isShowBatchRefund = true;
	}

	confrimBatchWritePerGameSubmit() {
		EventBus.$emit('show_mask', null);

		GameManagementApi.batchWritePerGame(this.batchWritePerGameForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	confrimBatchRefundSubmit() {
		EventBus.$emit('show_mask', null);

		GameManagementApi.batchRefund(this.batchRefundForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	searchNotSettleRecord() {
		EventBus.$emit('show_mask', null);
		const myForm: Model.IFindNotSettleRecordRequest = {} as Model.IFindNotSettleRecordRequest;
		myForm.gameVendorID = this.findRecordConfirm.gameVendorID;
		myForm.beginTime = startEndDate(this.findRecordConfirm.searchStartTimeRange[0]);
		myForm.endTime = startEndDate(this.findRecordConfirm.searchStartTimeRange[1]);

		GameManagementApi.findNotSettleRecords(myForm)
			.then((res) => {
				EventBus.$emit(`globalPrintTextPopupDialog`, res.data.transactionNoLsitLineBreak);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	whenGameSelectChange() {
		if (this.searchFilter.gameID) {
			const match = this.gamesDropdownItems.filter((element) => {
				return element.valueInt === Number(this.searchFilter.gameID);
			});

			// 自动更换gameVendor选单
			if (match.length > 0) {
				this.searchFilter.gameVendorID = match[0].extraProperty.gameVendor;
			}
		}
	}

	search() {
		const request = this.searchFilter;
		request.pageIndex = this.DataTablePagingParameter.pageIndex;
		request.pageSize = this.DataTablePagingParameter.itemsPerPage;
		request.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		request.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		request.orderByColumn = this.searchFilter.orderByColumn;
		request.isASC = this.searchFilter.isASC;

		const vm: any = this;
		vm.globalKeywordSearchGamesDropdown = '';

		request.transactionNoPGS = '';
		request.betRecordNo = '';
		request.roundID = '';

		if (this.searchTargetField === '1') {
			request.transactionNoPGS = this.myInputKeyword;
		} else if (this.searchTargetField === '2') {
			request.betRecordNo = this.myInputKeyword;
		} else if (this.searchTargetField === '3') {
			request.roundID = this.myInputKeyword;
		}

		EventBus.$emit('show_mask', null);

		GameManagementApi.getUnionBetRecords(<Model.IUnionBetRecordSearchRequest>request)
			.then((res) => {
				this.queryRecords = res.data.items;
				this.queryRecordsTotalSum = res.data.extraObject.totalSummary;

				if (res.data.extraObject != null) {
					this.searchHintText = res.data.extraObject.hintText;
				}

				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}
	async exportFiles() {
		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__time'),
			this.geti18nTextAsString('TableHeader.__betRecordNoPGS'),
			this.geti18nTextAsString('TableHeader.__betRecordNo'),
			this.geti18nTextAsString('TableHeader.__game'),
			this.geti18nTextAsString('TableHeader.__merchant'),
			this.geti18nTextAsString('TableHeader.__currency'),
			'PlayerID',
			this.geti18nTextAsString('TableHeader.__playerAccount'),
			this.geti18nTextAsString('TableHeader.__betAmount'),
			this.geti18nTextAsString('TableHeader.__payoutAmount'),
			this.geti18nTextAsString('TableHeader.__winLossAmount'),
			this.geti18nTextAsString('TableHeader.__status'),
		];

		const excelData: any = [];

		const request = this.searchFilter;
		request.pageIndex = 1;
		request.pageSize = 10000;

		if (!this.hasDevelopeSuperPermission) {
			request.pageSize = 2000;
		}

		request.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		request.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);

		setTimeout(() => {
			EventBus.$emit('hide_mask', null);
		}, 1000 * 15);

		const result = await GameManagementApi.getUnionBetRecords(request);

		EventBus.$emit('hide_mask', null);

		result.data.items.forEach((value) => {
			const eachRow: any = [];
			eachRow.push(timeWithTimezone(value.createTime, this.userProfile.timeZone));
			eachRow.push(value.betRecordNoPGS);
			eachRow.push(value.betRecordNo);
			eachRow.push(value.gameName);
			eachRow.push(value.merchantName);
			eachRow.push(value.currency);
			eachRow.push(value.playerID);
			eachRow.push(value.playerName);
			eachRow.push(value.betAmount);
			eachRow.push(value.payoutAmount);
			eachRow.push(value.winLossAmount);
			eachRow.push(value.statusText);

			excelData.push(eachRow);
		});

		const fileNameTimeTextStart = toLocalDateMMdd(this.datetimeRangePicker.searchStartTimeRange[0], 8, '');
		const fileNameTimeTextEnd = toLocalDateMMdd(this.datetimeRangePicker.searchStartTimeRange[1], 8, '');

		const fileNameSuffix = '_' + this.searchFilter.gameVendorID?.toString() + '_' + fileNameTimeTextStart + '_' + fileNameTimeTextEnd;

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_GameVendorWagers' + fileNameSuffix + '.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	closeAllDialog() {
		this.isShowConfirm = false;
		this.isFindNotSettleRecordConfirm = false;
		this.isShowDebugSQLDialog = false;
		this.isShowBatchWritePerGame = false;
		this.isShowBatchRefund = false;
	}

	confirmManualRefund(item: Model.IUnionBetRecordResponse) {
		this.refundTarget = item;
		if (this.isSkipConfirm) {
			this.manualRefund(item.betRecordNoPGS);
		} else {
			this.isShowConfirm = true;
		}
	}
	copyRequestInfo(item: any) {
		const vm: any = this;

		const timeTextUtc8: any = vm.toTimeWithAddTimezone(item.createTime);

		const requestTime = '．发送时间(RequestTime)\n`' + timeTextUtc8 + '  (UTC+8)`\n\n';
		const infoUrl = '．请求网址(RequestUrl)\n`' + item.requestUrl + '`\n\n';
		const infoBody = '．请求参数(RequestBody)\n```\n' + item.requestBody + '\n```\n\n';
		const infoRes = '．API回应(Response)\n```\n' + item.responseText + '\n```\n\n';

		const statusCode = '';

		let executionTimeText = '';

		if (item.responseTime > 3000) {
			executionTimeText = '执行时间(ExecutionTime)\n```' + (item.executionTime / 1000).toFixed(1) + 's ```\n\n';
		}

		const myCopyText = requestTime + infoUrl + infoBody + statusCode + executionTimeText + infoRes;
		vm.copyTextToClipboard2(myCopyText);
	}

	manualRefund(betRecordNoPGS: string) {
		const data = {} as Model.IBetRecordManualRefundRequestModel;
		data.transactionNo = betRecordNoPGS;

		EventBus.$emit('show_mask', null);
		GameManagementApi.manualRefund(data)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllDialog();
				this.search();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	copyAllTransactionNo() {
		let myText = '';

		if (this.queryRecords != null && this.queryRecords.length > 0) {
			this.queryRecords.forEach((item) => {
				myText = myText + item.betRecordNoPGS + '\n';
			});
		}

		const vm: any = this;
		vm.copyTextToClipboard(myText);
	}

	mounted() {
		this.init();

		const route: any = this.$route;
		this.searchFilter.playerName = route.query.playerName;
		this.searchTargetField = '1';

		this.registerReloadComponent();

		// 這頁loading很慢 不要自動search了
		// this.search();
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	init() {
		const vm: any = this;

		DropdownApi.getGames().then((res) => {
			this.gamesDropdownItems = res;
		});

		DropdownApi.getMerchants(true).then((res) => {
			this.merchantDropdownItems = res;
		});

		DropdownApi.getBetStatus().then((res) => {
			this.betStatusDropdownItems = res;
		});

		DropdownApi.getSystemTags(3329).then((res) => {
			this.tagsDropdownItems = res;
		});

		DropdownApi.getGameVendors().then((res) => {
			this.gameVendorDropdownItems = res;

			if (!vm.hasDetailPagePermission) {
				this.searchFilter.gameVendorID = -1;
			} else if (res.some((item) => item.valueInt === 290000)) {
				this.searchFilter.gameVendorID = 290000; // 預設篩選AGG
			} else if (res.some((item) => item.valueInt === 610000)) {
				this.searchFilter.gameVendorID = 610000; // 預設篩選Vplus
			}
		});

		this.clear();
	}

	copyText(text: string) {
		const vm: any = this;
		vm.copyTextToClipboard(text);
	}

	clear() {
		const tempValue = this.searchFilter.gameVendorID;

		this.searchFilter = {} as Model.IUnionBetRecordSearchRequest;
		this.searchFilter.orderByColumn = 'gameTime';
		this.searchFilter.gameVendorID = tempValue;
		this.myInputKeyword = '';

		const vm: any = this;
		vm.globalClear();

		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-1), daysAgo(1)];
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.clear();
				this.queryRecords = [];
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

.container--fluid {
	margin-right: 5px !important;
}

.status_77,
.status_9 {
	color: #f27575;
}

.status_1 {
	color: #78b17c;
}

.common-info-value {
	min-height: 30px;
	line-height: 25px;
}

.notify-queue-info .col-12 {
	padding: 3px;
}

.notify-queue-info h3 {
	color: #71abb1;
}

.specialTag {
	text-overflow: clip;
	white-space: nowrap;
	overflow: hidden;
	max-width: 80px;
	display: inline-block;
	font-size: 12px;
	background: #4b71c5;
	border-radius: 10px;
	padding: 6px;
}
</style>
