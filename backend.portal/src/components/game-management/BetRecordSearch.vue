<template>
	<v-app>
		<v-card class="mx-auto" :min-width="1600">
			<v-flex>
				<v-toolbar>
					<v-toolbar-title>
						<h5>
							{{ $t('Button.__search') }}
							<!-- {{ $t('TableHeader.__betRecordNo') }} -->
						</h5>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-card-title>
						<v-text-field
							v-model="searchTransactionNo"
							style="width: 350px"
							:label="$t('TableHeader.__betRecordNoPGS')"
							single-line
							hide-details
							v-on:keyup.enter="searchTransactionNo"
							clearable
						>
						</v-text-field>

						<v-icon style="cursor: pointer" @click="searchByTransactionNo"> search </v-icon>
					</v-card-title>
					<v-spacer></v-spacer>
				</v-toolbar>

				<v-row class="info-wrapper" style="padding: 5px 30px 30px 30px" v-if="queryBetRecordInfo.gameVendorID || queryBetRecordInfo.gameVendorID == 0">
					<v-col cols="12" sm="6" md="6">
						<div class="row">
							<v-col cols="12" sm="12" md="12">
								<div>
									<h3>基本資訊</h3>
									<v-divider></v-divider>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__betRecordNo') }}
									</div>
									<div class="common-info-value copyTextTo f15" @click="copyText(queryBetRecordInfo.generalInfo.betRecordNo)">
										{{ queryBetRecordInfo.generalInfo.betRecordNo }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__betRecordNoPGS') }}
									</div>
									<div class="common-info-value copyTextTo f15" style="white-space: nowrap" @click="copyText(queryBetRecordInfo.generalInfo.betRecordNoPGS)">
										{{ queryBetRecordInfo.generalInfo.betRecordNoPGS }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__gameVendorType') }}
									</div>
									<div class="common-info-value" :title="queryBetRecordInfo.gameVendorID">
										{{ queryBetRecordInfo.gameVendorName }}

										<span class="ml-2 grey-text">
											(
											{{ queryBetRecordInfo.vendorConfigID }}｜{{ queryBetRecordInfo.vendorConfigAssignMerchantID }}
											)
										</span>

										<v-icon color="yellow" @click="copyServerInfo(queryBetRecordInfo)">file_copy</v-icon>
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__merchant') }}
									</div>
									<div class="common-info-value">
										{{ queryBetRecordInfo.generalInfo.merchantName }}
										({{ queryBetRecordInfo.generalInfo.merchantID }})
										<span class="grey-text">{{ queryBetRecordInfo.currency }}</span>
										<v-chip small color="purple" v-if="queryBetRecordInfo.isSingleWalletMode"> SW </v-chip>
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__gameID') }}
									</div>
									<div class="common-info-value">
										{{ queryBetRecordInfo.generalInfo.gameID }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__game') }}
									</div>
									<div class="common-info-value">
										{{ queryBetRecordInfo.generalInfo.gameName }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__playerID') }}
									</div>
									<div class="common-info-value">
										{{ queryBetRecordInfo.generalInfo.playerID }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__playerAccount') }}
									</div>
									<div class="common-info-value">
										<a target="_blank" :href="'/members-management/detail?playerID=' + queryBetRecordInfo.generalInfo.playerID">
											{{ queryBetRecordInfo.generalInfo.playerName }}
										</a>
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__status') }}
									</div>
									<div class="common-info-value">
										<span style="color: #dbba41">
											{{ queryBetRecordInfo.generalInfo.statusText }}
										</span>
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__createTime') }}
									</div>
									<div class="common-info-value" style="height: 24px">
										<span v-if="queryBetRecordInfo.generalInfo.createTime">
											{{ toDisplayTime(queryBetRecordInfo.generalInfo.createTime) }}
										</span>
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__settleTime') }}
									</div>
									<div class="common-info-value" style="height: 24px">
										<span v-if="queryBetRecordInfo.generalInfo.settlementTime">
											{{ toDisplayTime(queryBetRecordInfo.generalInfo.settlementTime) }}
										</span>
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__betAmount') }}
									</div>
									<div class="common-info-value">
										{{ queryBetRecordInfo.generalInfo.betAmount | amountDisplay }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__payoutAmount') }}
									</div>
									<div class="common-info-value">
										{{ queryBetRecordInfo.generalInfo.payoutAmount | amountDisplay }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__finalScore') }}
									</div>
									<div class="common-info-value">
										{{ queryBetRecordInfo.generalInfo.winLossAmount | amountDisplay }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__validAmount') }}
									</div>
									<div class="common-info-value">
										{{ queryBetRecordInfo.generalInfo.validAmount | amountDisplay }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__taxAmount') }}
									</div>
									<div class="common-info-value">
										{{ queryBetRecordInfo.generalInfo.taxAmount | amountDisplay }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__convertRate') }}
									</div>
									<div class="common-info-value">
										{{ queryBetRecordInfo.generalInfo.convertRate | amountDisplay }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">{{ $t('TableHeader.__extraInfo') }} (局號/其他)</div>
									<div class="common-info-value">{{ queryBetRecordInfo.generalInfo.extraInfo1 }} &nbsp;</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">{{ $t('TableHeader.__settleTime') }} (三方)</div>
									<div class="common-info-value">{{ toDisplayTime(queryBetRecordInfo.generalInfo.thirdSettleTime) }} &nbsp;</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div>
									<div class="common-info-label">結算日 (三方)</div>
									<div class="common-info-value">
										{{ toDate(queryBetRecordInfo.generalInfo.thirdSettleDate) }}
										&nbsp;
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="12" md="12">
								<div>
									<div class="common-info-label">額外備註</div>
									<div class="common-info-value">
										{{ queryBetRecordInfo.extraInfoText }}
										&nbsp;
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="12" md="12">
								<div>
									<div class="common-info-label">標籤</div>
									<div class="common-info-value">
										<v-chip v-for="(item, index) in queryBetRecordInfo.tagMapping" :key="index" class="ml-1" small>
											{{ item.text }}
										</v-chip>
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<div style="padding: 25px"></div>
							</v-col>
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div>
									<h3>遊戲歷程</h3>
									<v-divider></v-divider>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6" v-if="queryBetRecordInfo.perGameLogInfo">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__perGameLogID') }}
									</div>
									<div class="common-info-value" @click="openPerGameLogList(queryBetRecordInfo.generalInfo.betRecordNoPGS)">
										<span class="link-text">
											{{ queryBetRecordInfo.generalInfo.betRecordNoPGS }}
										</span>

										<!-- ({{queryBetRecordInfo.perGameLogInfo.perGameLogID}}) -->
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6" v-if="queryBetRecordInfo.perGameLogInfo">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__betRecordNo') }}
									</div>
									<div class="common-info-value">
										{{ queryBetRecordInfo.perGameLogInfo.perGameDetailLogID }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6" v-if="queryBetRecordInfo.perGameLogInfo && queryBetRecordInfo.generalInfo.isManualSettlement">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__memo') }}
									</div>
									<div class="common-info-value" style="color: #e77171">
										{{ $t('Button.__ManualSettlement') }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6" v-if="queryBetRecordInfo.perGameLogInfo && queryBetRecordInfo.generalInfo.isManualSettlement">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__updateBy') }}
									</div>
									<div class="common-info-value">
										{{ queryBetRecordInfo.generalInfo.manualSettlementUserName }}
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6" v-if="isUnexpectedData(queryBetRecordInfo) || isMissingPerGameWhenSettlement(queryBetRecordInfo)">
								<div>
									<div class="common-info-label">
										{{ $t('TableHeader.__othersMemo') }}
									</div>
									<div class="common-info-value">
										<div v-if="queryBetRecordInfo.isVirtualItemCard" style="line-height: 25px">
											<div>⭐此注單為免費投注 (虛寶卡／Freespin)</div>
											<div>⭐不會有perGame資料</div>
										</div>

										<div v-else>
											<span class="minus-red">資料異常*</span>
											<v-icon small color="red lighten-1" class="ml-1" @click="copyDebugSQL_EazyTemp(queryBetRecordInfo.generalInfo.betRecordNoPGS)"> file_copy </v-icon>
										</div>
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="12" md="12">
								<div class="text-l" style="margin-top: 30px">
									<v-spacer></v-spacer>

									<div style="padding: 0px 25px" v-if="isThirdPartyGameVendor(queryBetRecordInfo.gameVendorID)">
										<div>
											{{ $t('Message.__mustToExternalLinkWatchInfo') }}
										</div>
									</div>
									<div style="padding: 15px 25px">
										<v-btn
											color="red lighten-3"
											text
											class="btn link mb-2 mr-1"
											v-if="hasManualRefundPermission && queryBetRecordInfo.generalInfo.status == 0"
											@click.stop="manualRefund(queryBetRecordInfo.generalInfo.betRecordNoPGS, queryBetRecordInfo.generalInfo.betAmount)"
										>
											<v-icon left>settings_backup_restore</v-icon>
											{{ $t('Menu.GamesManagement_BetRecord_ManualRefund') }}
										</v-btn>

										<v-btn
											color="green lighten-2"
											text
											class="btn link mb-2 mr-1"
											v-if="hasManualSettlementPermission && queryBetRecordInfo.generalInfo.status == 0"
											@click.stop="showManualSettleDialog"
											:disabled="isUnexpectedData(queryBetRecordInfo)"
										>
											<v-icon left>monetization_on</v-icon>
											{{ $t('Button.__ManualSettlement') }}
										</v-btn>

										<v-btn
											color="orange lighten-1"
											text
											class="btn link mb-2 mr-1"
											v-if="hasManualSettlementPermission && queryBetRecordInfo.generalInfo.status == 1"
											@click.stop="showChangeSettleInfoDialog"
											:disabled="isUnexpectedData(queryBetRecordInfo)"
										>
											<v-icon left>edit</v-icon>
											修改已結算注單金額
										</v-btn>

										<div style="display: inline-block" v-if="isThirdPartyGameVendor(queryBetRecordInfo.gameVendorID)">
											<v-btn v-if="!queryBetRecordInfo.isSupportExternalLink" disabled>
												<v-icon left>link</v-icon>
												{{ $t('Message.__notSupport') }}
											</v-btn>

											<v-btn
												color="blue lighten-1"
												text
												class="btn link mb-2"
												v-if="queryBetRecordInfo.isSupportExternalLink"
												@click.stop="getBetRecordExternalLink(queryBetRecordInfo.generalInfo.betRecordNoPGS)"
											>
												<v-icon left>link</v-icon>
												{{ $t('Button.__openExternalLink') }}
											</v-btn>
										</div>

										<v-btn
											color="blue darken-1"
											text
											class="btn link mb-2 mr-1"
											v-if="hasManualSettlementPermission && isMissingPerGameWhenSettlement(queryBetRecordInfo)"
											@click.stop="showManualWritePerGameDialog"
										>
											<v-icon left>link</v-icon>
											手動補上遊戲歷程
										</v-btn>

										<v-btn color="yellow darken-2" text class="btn link mb-2 mr-1" @click.stop="copyServerInfo(queryBetRecordInfo)" style="margin-left: 5px">
											<v-icon left>file_copy</v-icon>
											{{ $t('Button.__copyToClipboard') }}
										</v-btn>

										<v-btn color="brown lighten-3" text class="btn link mb-2 mr-1" @click.stop="searchRawDetailByThirdApi">
											<v-icon left>visibility</v-icon>
											API查看明細(僅部分遊戲商支援)
										</v-btn>

										<v-btn
											color="brown lighten-3"
											text
											class="btn link mb-2 mr-1"
											v-if="queryBetRecordInfo.isSingleWalletMode"
											@click.stop="showMappingUserAccountsByRoleID(queryBetRecordInfo.generalInfo.betRecordNoPGS)"
										>
											<v-icon left>visibility</v-icon>
											查看 SW notify queue
										</v-btn>

										<v-btn color="yellow lighten-3" text class="btn link mb-2 mr-1" @click.stop="goToMoneyRecordPage">
											<v-icon left>visibility</v-icon>
											查看金幣紀錄
										</v-btn>

										<v-btn color="yellow lighten-3" text class="btn link mb-2 mr-1" @click.stop="recoverNotifyQueueData(queryBetRecordInfo.generalInfo.betRecordNoPGS)">
											<v-icon left>visibility</v-icon>
											重建單錢包notifyQueue
										</v-btn>

										<a
											:href="
												'/system-logs/SingleWalletPlatformReqeustLog?referenceTransactionNo=' +
												queryBetRecordInfo.generalInfo.betRecordNoPGS +
												'&begin=' +
												toDisplayTime(queryBetRecordInfo.generalInfo.createTime)
											"
											v-if="queryBetRecordInfo.isSingleWalletMode"
											target="_blank"
										>
											<v-btn color="pink lighten-3" text class="btn link mb-2 mr-1">
												<v-icon left>visibility</v-icon>
												查看 SW callback logs
											</v-btn>
										</a>
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="12" md="12" class="mt-4" v-if="queryBetRecordInfo && queryBetRecordInfo.raiseBetInfo && queryBetRecordInfo.raiseBetInfo.length > 0">
								<div>
									<h3 class="yellow-light-text">
										<v-icon small class="yellow-light-text">mdi-format-float-left</v-icon>
										加注紀錄
									</h3>

									<div class="v-data-table theme--dark">
										<div class="v-data-table__wrapper">
											<table class="">
												<thead class="v-data-table-header">
													<tr>
														<th>三方单号</th>
														<th>金额</th>
														<th>时间</th>
														<th>状态</th>
													</tr>
												</thead>

												<tbody>
													<tr v-for="(item, index) in queryBetRecordInfo.raiseBetInfo" :key="index">
														<td>{{ item.raiseTransactionNo }}</td>
														<td>
															{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
														</td>
														<td>{{ item.status }}</td>
														<td class="text-r">
															{{ item.amount }}
														</td>
													</tr>
												</tbody>
											</table>
										</div>
									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="12" md="12" class="mt-4" v-if="queryBetRecordInfo && queryBetRecordInfo.extraAmountDetail && queryBetRecordInfo.extraAmountDetail.length > 0">
								<div>
									<h3 class="yellow-light-text">
										<v-icon small class="yellow-light-text">mdi-format-float-left</v-icon>
										mapping紀錄
									</h3>

									<div class="v-data-table theme--dark">
										<div class="v-data-table__wrapper">
											<table class="">
												<thead class="v-data-table-header">
													<tr>
														<th>三方单号</th>
														<th>时间</th>
														<th>金额</th>
														<th>備註</th>
													</tr>
												</thead>

												<tbody>
													<tr v-for="(item, index) in queryBetRecordInfo.extraAmountDetail" :key="index">
														<td>{{ item.thirdTransactionNo }}</td>
														<td>
															{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
														</td>
														<td class="text-r">
															<span v-if="item.amount > 0">
																{{ item.amount }}
															</span>
														</td>
														<td>{{ item.memo }}</td>
													</tr>
												</tbody>
											</table>
										</div>
									</div>
								</div>
							</v-col>
						</v-row>
					</v-col>
				</v-row>
			</v-flex>
		</v-card>

		<!-- 手動結算 -->
		<v-dialog v-model="isShowManualSettleDialog" max-width="600px">
			<v-card class="">
				<v-card-title>
					{{ $t('Button.__ManualSettlement') }}
					<div class="top-right-item">
						<div class="close" @click="closeAllDialog">
							<v-icon>close</v-icon>
						</div>
					</div>
				</v-card-title>
				<v-card-text class="manualSettle">
					<v-row>
						<v-col cols="12" sm="8" md="8">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field :label="$t('TableHeader.__betRecordNo')" v-model="queryBetRecordInfo.generalInfo.betRecordNoPGS" :disabled="true"> </v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="8" md="8">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field :label="$t('TableHeader.__playerAccount')" v-model="queryBetRecordInfo.generalInfo.playerName" :disabled="true"> </v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="8" md="8">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field :label="$t('TableHeader.__betAmount')" v-model="queryBetRecordInfo.generalInfo.betAmount" :disabled="true"> </v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="8" md="8">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field :label="$t('TableHeader.__payoutAmount')" ref="payoutAmount" :placeholder="$t('Message.__pleaseInput')" v-model="manualSettleEdit.payoutAmount">
									</v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="8" md="8">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field :label="$t('TableHeader.__validAmount')" :placeholder="$t('Message.__pleaseInput')" v-model="manualSettleEdit.validAmount"> </v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="8" md="8">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field
										:label="$t('TableHeader.__winLossAmount') + ' (自動計算)'"
										disabled="disabled"
										:value="manualSettleEdit.payoutAmount ? manualSettleEdit.payoutAmount - queryBetRecordInfo.generalInfo.betAmount : '-'"
									>
									</v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="8" md="8">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field :label="$t('TableHeader.__memo')" :placeholder="$t('Message.__pleaseInput')" v-model="manualSettleEdit.memo"> </v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="8" md="8">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field :label="'三方結算日'" :placeholder="'填寫範例: 2024-01-01'" v-model="manualSettleEdit.thirdSettleDate"> </v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="12" md="12">
							<div class="text-c" style="margin-top: 30px">
								<v-spacer></v-spacer>

								{{ $t('Message.__mustToExternalLinkWatchInfo') }}

								<div style="padding: 25px">
									<v-btn color="yellow darken-1" text @click="submitManualSettle" class="btn">
										<v-icon left>mdi-check-circle</v-icon>
										{{ $t('Button.__submit') }}
									</v-btn>
								</div>
							</div>
						</v-col>
					</v-row>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 修改已結算注單金額 -->
		<v-dialog v-model="isShowChangeSettleInfo" max-width="500px" persistent>
			<v-card class="">
				<v-card-title class="grey darken-2">
					修改已結算注單金額
					<div class="top-right-item">
						<div class="close" @click="closeAllDialog">
							<v-icon>close</v-icon>
						</div>
					</div>
				</v-card-title>
				<v-card-text class="manualSettle mt-4" style="padding-right: 25px">
					<v-row class="">
						<v-col cols="12" sm="12" md="12">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field :label="$t('TableHeader.__betRecordNo')" v-model="queryBetRecordInfo.generalInfo.betRecordNoPGS" :disabled="true"> </v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row class="">
						<v-col cols="12" sm="12" md="12">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field :label="$t('TableHeader.__playerAccount')" v-model="queryBetRecordInfo.generalInfo.playerName" :disabled="true"> </v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>
					<v-row class="">
						<v-col cols="12" sm="12" md="12">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field :label="$t('TableHeader.__betAmount')" v-model="queryBetRecordInfo.generalInfo.betAmount" :disabled="true"> </v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row class="">
						<v-col cols="12" sm="12" md="12">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field :label="$t('TableHeader.__payoutAmount')" ref="payoutAmount" :placeholder="$t('Message.__pleaseInput')" v-model="manualChangeSettleInfo.payoutAmount">
									</v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row class="">
						<v-col cols="12" sm="12" md="12">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field :label="$t('TableHeader.__validAmount')" :placeholder="$t('Message.__pleaseInput')" v-model="manualChangeSettleInfo.validAmount"> </v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row class="">
						<v-col cols="12" sm="12" md="12">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field
										:label="$t('TableHeader.__winLossAmount') + ' (自動計算)'"
										disabled="disabled"
										:value="manualChangeSettleInfo.payoutAmount ? manualChangeSettleInfo.payoutAmount - queryBetRecordInfo.generalInfo.betAmount : '-'"
									>
									</v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row class="">
						<v-col cols="12" sm="12" md="12">
							<div class="">
								<div class="targetHistoryValue">
									<v-text-field :label="$t('TableHeader.__memo')" :placeholder="$t('Message.__pleaseInput')" v-model="manualChangeSettleInfo.memo"> </v-text-field>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row class="" v-if="queryBetRecordInfo.isSingleWalletMode">
						<v-col cols="12" sm="12" md="12">
							<div class="">
								<div class="targetHistoryValue">
									<v-checkbox v-model="manualChangeSettleInfo.isTriggerCallback" label="觸發單錢包callback (settle change)"></v-checkbox>
								</div>
							</div>
						</v-col>
					</v-row>

					<v-row class="">
						<v-col cols="12" sm="12" md="12">
							<div class="text-c" style="margin-top: 30px">
								<v-spacer></v-spacer>

								<div style="padding: 25px">
									<v-btn color="yellow darken-1" text @click="submitChangeSettleInfo" class="btn">
										<v-icon left>mdi-check-circle</v-icon>
										{{ $t('Button.__submit') }}
									</v-btn>
								</div>
							</div>
						</v-col>
					</v-row>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!--手動補上遊戲歷程 -->
		<v-dialog v-model="isShowManualWritePerGame" max-width="500px">
			<v-card class="">
				<v-card-title class="grey darken-2">
					手動補上遊戲歷程
					<div class="top-right-item">
						<div class="close" @click="closeAllDialog">
							<v-icon>close</v-icon>
						</div>
					</div>
				</v-card-title>
				<v-card-text class="manualSettle">
					<v-flex>
						<v-row class="mt-3">
							<v-col cols="12" sm="12" md="12">
								<div class="">
									<div class="targetHistoryValue">
										<v-text-field :label="$t('TableHeader.__betRecordNo')" v-model="queryBetRecordInfo.generalInfo.betRecordNoPGS" :disabled="true"> </v-text-field>
									</div>
								</div>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="">
									<div class="targetHistoryValue">
										<v-text-field :label="$t('TableHeader.__playerAccount')" v-model="queryBetRecordInfo.generalInfo.playerName" :disabled="true"> </v-text-field>
									</div>
								</div>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="">
									<div class="targetHistoryValue">
										<v-text-field :label="$t('TableHeader.__betAmount')" v-model="queryBetRecordInfo.generalInfo.betAmount" :disabled="true"> </v-text-field>
									</div>
								</div>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="">
									<div class="targetHistoryValue">
										<v-text-field :label="$t('TableHeader.__payoutAmount')" v-model="queryBetRecordInfo.generalInfo.payoutAmount" :disabled="true"> </v-text-field>
									</div>
								</div>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="">
									<div class="targetHistoryValue">
										<v-text-field :label="$t('TableHeader.__memo')" :placeholder="$t('Message.__pleaseInput')" v-model="manualWritePerGameEdit.memo"> </v-text-field>
									</div>
								</div>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="">
									<div class="targetHistoryValue">
										<v-checkbox v-model="manualWritePerGameEdit.isReplaceGameTimeToNowTime" :label="'是否使用當前時間做為遊玩時間(對帳日期)'"> </v-checkbox>
									</div>
								</div>
							</v-col>
						</v-row>

						<v-row v-if="queryBetRecordInfo.isSingleWalletMode">
							<v-col cols="12" sm="12" md="12">
								<div class="">
									<div class="targetHistoryValue">
										<v-checkbox v-model="manualWritePerGameEdit.isWriteSingleWalletNotifyQueue" :label="'是否建立單錢包商戶補發通知佇列'"> </v-checkbox>
									</div>
								</div>
							</v-col>
						</v-row>

						<v-row v-if="!queryBetRecordInfo.isSingleWalletMode">
							<v-col cols="12" sm="12" md="12">
								<div class="">
									<div class="targetHistoryValue">
										<v-checkbox v-model="manualWritePerGameEdit.isUpdatePayoutAmount" :label="'是否派發彩金至錢包'"> </v-checkbox>
									</div>
								</div>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="text-c" style="margin-top: 30px">
									<v-spacer></v-spacer>

									<div style="padding: 25px">
										<div>
											<v-btn color="yellow darken-1" text @click="submitManualWritePerGame" class="btn">
												<v-icon left>mdi-check-circle</v-icon>
												{{ $t('Button.__submit') }}
											</v-btn>
										</div>

										<div class="mt-2 orange-text">
											<v-icon small color="orange">info</v-icon>
											当补完历程。根据当下时间、可能需重跑统计报表
										</div>
									</div>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-card-text>
			</v-card>
		</v-dialog>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/game-management/per-game';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import { datetimeMixin, timeWithTimezone } from '@/utilities/datetime-format';
import DeletePlayersPopup from '@/components/members-management/DeletePlayersPopup.vue';
import DetailLoginRecord from '@/components/members-management/DetailLoginRecord.vue';
const playersModule = namespace('Players');
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import GameManagementApi from '@/api/game-management/per-game';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
@Component({ components: { DeletePlayersPopup, DetailLoginRecord }, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class Detail extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;
	@playersModule.State('transactionNoPGS') transactionNoPGS!: string;

	manualWritePerGameEdit: Model.IManualWritePerGameRequest = {} as Model.IManualWritePerGameRequest;
	manualSettleEdit: Model.IManualSettleRequest = {} as Model.IManualSettleRequest;
	manualChangeSettleInfo: Model.IManualChangeSettleInfoRequest = {} as Model.IManualChangeSettleInfoRequest;

	queryBetRecordInfo: Model.IBetRecordInfo = {} as Model.IBetRecordInfo;
	searchTransactionNo: string = '';
	isShowManualSettleDialog: boolean = false;
	isShowManualWritePerGame: boolean = false;
	isShowChangeSettleInfo: boolean = false;

	get hasManualRefundPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagement_BetRecord_ManualRefund) >= 0;
	}

	get hasManualSettlementPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagement_BetRecord_ManualSettleBet) >= 0;
	}

	@Action('Players/setMoneyRecordReferenceNo') private setMoneyRecordReferenceNo!: any;
	@Action('Players/setWalletPlayerID') private setWalletPlayerID!: any;
	@Action('Players/setPerGameLogID') private setPerGameLogID!: any;
	@Action('Players/setTransactionNo') private setTransactionNo!: any;

	@Watch('transactionNoPGS')
	onTransactionNoPGSChange(newVal: string) {
		this.searchTransactionNo = newVal;
		this.search();
	}

	isUnexpectedData(item: Model.IBetRecordInfo) {
		// 未結算、卻又有perGame
		return item.perGameLogInfo && item.generalInfo.status === 0;
	}

	isMissingPerGameWhenSettlement(item: Model.IBetRecordInfo) {
		// 已結算、卻缺少perGame
		return !item.perGameLogInfo && item.generalInfo.status === 1;
	}

	showManualSettleDialog() {
		this.manualSettleEdit = {} as Model.IManualSettleRequest;
		this.isShowManualSettleDialog = true;

		setTimeout(() => {
			const refInput: any = this.$refs.payoutAmount;
			refInput.focus();
		}, 300);
	}

	showManualWritePerGameDialog() {
		this.manualWritePerGameEdit = {} as Model.IManualWritePerGameRequest;
		this.manualWritePerGameEdit.isReplaceGameTimeToNowTime = true;
		this.manualWritePerGameEdit.isWriteSingleWalletNotifyQueue = true;
		this.manualWritePerGameEdit.isUpdatePayoutAmount = false;

		this.isShowManualWritePerGame = true;
	}

	showChangeSettleInfoDialog() {
		this.manualChangeSettleInfo = {} as Model.IManualChangeSettleInfoRequest;
		this.manualChangeSettleInfo.isTriggerCallback = true;
		this.isShowChangeSettleInfo = true;
	}

	closeAllDialog() {
		this.isShowManualSettleDialog = false;
		this.isShowManualWritePerGame = false;
		this.isShowChangeSettleInfo = false;
	}

	searchByTransactionNo() {
		this.search();
	}

	isThirdPartyGameVendor(gameID: number) {
		return gameID > 0;
	}

	submitManualSettle() {
		const formBody = this.manualSettleEdit;

		if (
			formBody.payoutAmount === '' ||
			formBody.payoutAmount === undefined ||
			formBody.payoutAmount === null ||
			formBody.validAmount === '' ||
			formBody.validAmount === undefined ||
			formBody.validAmount === null
		) {
			alert(this.$t('Message.__pleaseFillFormContent'));
			return;
		}

		this.manualSettleEdit.transactionNo = this.queryBetRecordInfo.generalInfo.betRecordNoPGS;
		EventBus.$emit('show_mask', null);
		GameManagementApi.manualSettle(this.manualSettleEdit)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllDialog();
				this.search();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	submitChangeSettleInfo() {
		const formBody = this.manualChangeSettleInfo;

		if (
			formBody.payoutAmount === '' ||
			formBody.payoutAmount === undefined ||
			formBody.payoutAmount === null ||
			formBody.validAmount === '' ||
			formBody.validAmount === undefined ||
			formBody.validAmount === null
		) {
			alert(this.$t('Message.__pleaseFillFormContent'));
			return;
		}

		this.manualChangeSettleInfo.transactionNo = this.queryBetRecordInfo.generalInfo.betRecordNoPGS;
		EventBus.$emit('show_mask', null);
		GameManagementApi.manualChangeSettleInfo(this.manualChangeSettleInfo)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllDialog();
				this.search();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	submitManualWritePerGame() {
		this.manualWritePerGameEdit.transactionNoPGS = this.queryBetRecordInfo.generalInfo.betRecordNoPGS;
		EventBus.$emit('show_mask', null);
		GameManagementApi.manualWritePerGame(this.manualWritePerGameEdit)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllDialog();
				this.search();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	recoverNotifyQueueData(betRecordNoPGS: string) {
		this.manualWritePerGameEdit.transactionNoPGS = this.queryBetRecordInfo.generalInfo.betRecordNoPGS;

		if (confirm('是否確定建立單錢包callback notify queue?')) {
			EventBus.$emit('show_mask', null);
			GameManagementApi.recoverNotifyQueueData(betRecordNoPGS)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.search();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	copyServerInfo(queryBetRecordInfo: Model.IBetRecordInfo) {
		const vm: any = this;
		const myText = queryBetRecordInfo.copyInfoForGameVendor;
		vm.copyTextToClipboard(myText);
	}

	manualRefund(transactionNo: string, amount: number) {
		const data = {} as Model.IBetRecordManualRefundRequestModel;
		data.transactionNo = transactionNo;

		const msg: any = this.$t('Message.__isConfirmOperate') + '? ' + this.$t('Menu.GamesManagement_BetRecord_ManualRefund') + '＄' + amount + '';
		if (confirm(msg)) {
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
	}

	copyDebugSQL_EazyTemp(betRecordNoPGS: string) {
		const msg = 'SELECT TOP 55 * FROM [PGS_Master].[dbo].[EazyTempValue] WHERE BetRecordNoPGS = ' + "'" + betRecordNoPGS + "'";
		const vm: any = this;
		vm.copyTextToClipboard2(msg);
	}

	search() {
		this.initialEmptyObject();

		if (!this.searchTransactionNo) {
			return;
		}

		EventBus.$emit('show_mask', null);
		GameManagementApi.getBetRecordByTransactionNo(this.searchTransactionNo)
			.then((res) => {
				this.queryBetRecordInfo = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	searchRawDetailByThirdApi() {
		EventBus.$emit('show_mask', null);
		GameManagementApi.getBetRecordRawResponseByApi(this.searchTransactionNo)
			.then((res) => {
				if (res.data) {
					EventBus.$emit(`globalShowSelfDefineColumnsDisplayPopupDialog`, res.data);
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showMappingUserAccountsByRoleID(betRecordNoPGS: string) {
		const myFilter: any = {
			querySqlID: 900851,
			transactionNo: betRecordNoPGS,
		};

		const myConfig: any = {
			maxWidth: 1000,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	initialEmptyObject() {
		this.queryBetRecordInfo = {} as Model.IBetRecordInfo;
		this.queryBetRecordInfo.generalInfo = {} as Model.IBetRecordGeneralInfo;
		this.queryBetRecordInfo.perGameLogInfo = {} as Model.IBetRecordPerGameInfo;
	}

	openPerGameLogList(myTransactionNo: string) {
		setTimeout(() => {
			this.setTransactionNo(myTransactionNo);
		}, 500);

		this.$router.push({ path: '/games-management/record', query: { transactionNo: myTransactionNo } });
	}

	toDisplayTime(isoTime: string) {
		return timeWithTimezone(isoTime, this.userProfile.timeZone);
	}

	toDate(isoTime: string) {
		if (isoTime == null || isoTime.length <= 0) {
			return '';
		}

		return isoTime.substring(0, 10);
	}

	getBetRecordExternalLink(pgsTransactionNo: string) {
		EventBus.$emit('show_mask', null);
		GameManagementApi.getExternalBetRecordDetailUrlByTransactionNo(pgsTransactionNo)
			.then((res) => {
				// 請參考 ThirdPartyVendorInfoTypeEnum.cs
				if (res.data.infoType === 1) {
					window.open(res.data.url, '_blank');
				} else {
					const win: any = window.open('', 'Title');
					win.document.body.innerHTML = '<h1>遊戲商提供以下原始內容參考</h1>' + '<div style="white-space: pre;">' + res.data.rawJson + '</div>';
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	copyText(text: string) {
		const vm: any = this;
		vm.copyTextToClipboard(text);
	}

	goToMoneyRecordPage() {
		this.setWalletPlayerID(this.queryBetRecordInfo.generalInfo.playerID);

		setTimeout(() => {
			this.setMoneyRecordReferenceNo(this.queryBetRecordInfo.generalInfo.betRecordNoPGS);
		}, 500);

		// 不稍微延遲似乎會有異常
		setTimeout(() => {
			this.$router.push({ path: '/members-management/money-record' });
		}, 200);
	}

	mounted() {
		this.initialEmptyObject();

		// const debugTransactionNo = '7d9eb1fd-a448-4568-97af-75924a3a9857';
		// this.searchTransactionNo = debugTransactionNo;
		// this.transactionNoPGS = debugTransactionNo;

		// 如果state有值
		if (this.transactionNoPGS) {
			this.searchTransactionNo = this.transactionNoPGS;
			this.search();
		}
	}
}
</script>

<style>
.v-list-item__subtitle {
	padding: 3px;
}

.info-wrapper .col-md-6,
.info-wrapper .col-md-8,
.info-wrapper .col-md-12 {
	padding: 5px 30px;
	clear: both;
}

.manualSettle .col-12 {
	padding: 0px 12px !important;
}
</style>
