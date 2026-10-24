<template>
	<v-app v-if="renderComponent" class="app">
		<div :class="'hide-in-PC'" v-if="hasCustomerServicePermission">
			<div class="text-c" style="font-size: 40px">⚠️</div>

			<div class="text-c">
				<div class="f18">This page can only be operated using the computer (PC). Please switch the device.</div>
				<div class="mt-2" style="color: #aaa">本功能此能使用電腦版操作，請切換裝置</div>
			</div>
		</div>

		<div :class="hasCustomerServicePermission ? 'hide-in-phone' : ''">
			<v-expansion-panels v-model="panel" multiple>
				<v-expansion-panel>
					<v-expansion-panel-content>
						<v-flex>
							<v-row>
								<v-col cols="12" sm="3" md="3">
									<v-text-field v-model="searchFilter.playerID" clearable :label="$t('TableHeader.__playerID')"> </v-text-field>
								</v-col>
								<v-col cols="12" sm="3" md="3">
									<v-text-field v-model="searchFilter.playerName" clearable :label="$t('TableHeader.__playerAccount')"> </v-text-field>
								</v-col>

								<v-col cols="12" sm="6" md="6">
									<div class="item height1-phone">
										<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
											<v-icon left>search</v-icon>
											{{ $t('Button.__search') }} </v-btn
										>&#9;
										<v-btn color="pink lighten-3 ml-1" text class="btn search" @click.stop="clear">
											<v-icon left>clear</v-icon>
											{{ $t('Button.__reset') }}
										</v-btn>

										<v-btn color="light-blue lighten-3 ml-1" text class="btn search" @click.stop="exportFiles">
											<v-icon left>cloud_download</v-icon>
											{{ $t('Button.__downloadFile') }} </v-btn
										>&#9;

										<v-btn color="purple lighten-3 ml-1" text class="btn search" v-if="hasSuperPermission || hasCustomerServicePermission" @click.stop="showSuspiciousCheckDialog">
											<v-icon left>stars</v-icon>
											檢查玩家數據是否異常 </v-btn
										>&#9;

										<v-btn color="pink lighten-3 ml-1" text class="btn search" v-if="hasSuperPermission" @click.stop="showLiveCasinoSuspiciousCheckDialog">
											<v-icon left>stars</v-icon>
											檢查玩家視訊(Live)數據 </v-btn
										>&#9;
									</div>
								</v-col>

								<div style="clear: both"></div>

								<v-col cols="12" sm="3" md="3">
									<div class="minus-mt-2">
										<date-picker
											v-model="datetimeRangePicker.searchStartTimeRange"
											type="datetime"
											v-bind:clearable="false"
											class="force-w100"
											value-type="timestamp"
											range
											:placeholder="$t('TableHeader.__time')"
										>
										</date-picker>
									</div>
								</v-col>

								<v-col cols="12" sm="6" md="6" class="quick-time">
									<div class="minus-mt-2">
										<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }" @click="globalQuicklyPickTimeRange('today')">
											<v-icon left>mdi-alarm-check</v-icon>
											{{ $t(`TableHeader.__today`) }}
										</v-chip>
										<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }" @click="globalQuicklyPickTimeRange('yesterday')">
											<v-icon left>mdi-alarm-check</v-icon>
											{{ $t(`TableHeader.__yesterday`) }}
										</v-chip>
										<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }" @click="globalQuicklyPickTimeRange('thisWeek')">
											<v-icon left>mdi-alarm-check</v-icon>
											{{ $t(`TableHeader.__thisWeek`) }}
										</v-chip>

										<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }" @click="globalQuicklyPickTimeRange('lastWeek')">
											<v-icon left>mdi-alarm-check</v-icon>
											{{ $t(`TableHeader.__lastWeek`) }}
										</v-chip>
										<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }" @click="globalQuicklyPickTimeRange('thisMonth')">
											<v-icon left>mdi-alarm-check</v-icon>
											{{ $t(`TableHeader.__thisMonth`) }}
										</v-chip>
										<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }" @click="globalQuicklyPickTimeRange('lastMonth')">
											<v-icon left>mdi-alarm-check</v-icon>
											{{ $t(`TableHeader.__lastMonth`) }}
										</v-chip>
									</div>
								</v-col>
							</v-row>
						</v-flex>
					</v-expansion-panel-content>
				</v-expansion-panel>
			</v-expansion-panels>

			<div class="hint-row">
				<v-icon> error </v-icon>
				{{ $t('Message.__dataDelay30Minutes') }}

				<v-icon class="ml-3"> visibility </v-icon>
				{{ $t('TableHeader.__playerViewPoint') }}

				<v-chip class="ml-2" color="grey darken-3" small v-if="searchResult && searchResult.merchantID > 0"> {{ $t('TableHeader.__merchantID') }}：{{ searchResult.merchantID }} </v-chip>

				<v-chip class="ml-2" color="grey darken-3" small v-if="searchResult && searchResult.playerID > 0"> {{ $t('TableHeader.__playerID') }}：{{ searchResult.playerID }} </v-chip>

				<v-icon class="ml-2" small>mdi-clock</v-icon>
				GMT+{{ userProfile.timeZone }}

				<span class="f12 grey-text ml-3" style="display: inline-block" v-if="searchResult.serverTimeTW"> {{ $t('TableHeader.__lastSearchTime') }}: {{ searchResult.serverTimeTW }} </span>
			</div>

			<v-row>
				<v-col cols="12">
					<v-data-table :headers="headers" :items="searchResult.playerGameSummary" sort :items-per-page="1000" hide-default-footer>
						<template v-slot:item.merchantName="{ item }">
							<span :title="item.merchantID"> {{ item.merchantName }} </span>
						</template>

						<template v-slot:item.gameName="{ item }">
							<div>{{ item.gameName }}</div>
						</template>

						<template v-slot:item.totalRound="{ item }">
							{{ item.totalRound | countDisplay }}
						</template>

						<template v-slot:item.winLossAmount="{ item }">
							<span :class="item.winLossAmount < 0 ? 'minus-red ' : ''">
								{{ item.winLossAmount | amountDisplay }}
							</span>
						</template>

						<template v-slot:item.betAmount="{ item }">
							{{ item.betAmount | amountDisplay }}
						</template>
						<template v-slot:item.validAmount="{ item }">
							{{ item.validAmount | amountDisplay }}
						</template>
						<template v-slot:item.payoutAmount="{ item }">
							{{ item.payoutAmount | amountDisplay }}
						</template>

						<template v-slot:item.rtp="{ item }">
							<span :class="item.rtp > 100 ? 'h-yellow  ' : ''">
								{{ item.rtp | amountDisplay }}
							</span>
						</template>
						<template v-slot:item.taxAmount="{ item }">
							{{ item.taxAmount | amountDisplay }}
						</template>

						<template slot="body.append" v-if="searchResult.totalSummary">
							<tr>
								<td>
									{{ $t('TableHeader.__total') }}
									:
								</td>
								<td></td>
								<td></td>
								<td></td>

								<td class="text-r">
									{{ searchResult.totalSummary.totalRound | countDisplay }}
								</td>
								<td class="text-r">
									{{ searchResult.totalSummary.betAmount | amountDisplay }}
								</td>
								<td class="text-r">
									{{ searchResult.totalSummary.validAmount | amountDisplay }}
								</td>
								<td class="text-r">
									{{ searchResult.totalSummary.payoutAmount | amountDisplay }}
								</td>
								<td class="text-r">
									<span :class="searchResult.totalSummary.winLossAmount < 0 ? 'minus-red ' : ''">
										{{ searchResult.totalSummary.winLossAmount | amountDisplay }}
									</span>
								</td>
								<td class="text-r" :class="searchResult.totalSummary.rtp > 100 ? 'h-yellow   ' : ''">
									{{ searchResult.totalSummary.rtp | amountDisplay }}
								</td>

								<!-- 
							<td class='text-r'>
								{{searchResult.totalSummary.taxAmount | amountDisplay}}
							</td> -->
							</tr>
						</template>
					</v-data-table>
				</v-col>
			</v-row>

			<v-row v-if="searchResult && searchResult.eventBonusSummary != null && searchResult.eventBonusSummary.length > 0">
				<v-col cols="12">
					<h2>
						{{ $t('TableHeader.__promotionBonus') }}
					</h2>

					<v-data-table :headers="headersOfEventSummary" :items="searchResult.eventBonusSummary" sort :items-per-page="1000" hide-default-footer>
						<template v-slot:item.totalCount="{ item }">
							{{ item.totalCount | countDisplay }}
						</template>

						<template v-slot:item.totalAmount="{ item }">
							{{ item.totalAmount | amountDisplay }}
						</template>

						<template slot="body.append" v-if="searchResult.totalEventSummary">
							<tr>
								<td>
									{{ $t('TableHeader.__total') }}
									:
								</td>

								<td></td>

								<td class="text-r">
									{{ searchResult.totalEventSummary.totalCount | countDisplay }}
								</td>
								<td class="text-r">
									{{ searchResult.totalEventSummary.totalAmount | amountDisplay }}
								</td>
							</tr>
						</template>
					</v-data-table>
				</v-col>
			</v-row>

			<!-- 檢查玩家數據異常 -->
			<v-dialog v-model="isShowSuspiciousCheckDialog" max-width="1000px" scrollable>
				<v-card v-if="searchResultOfSuspiciousCheck && searchResultOfSuspiciousCheck.playerID > 0">
					<v-card-title class="grey darken-2">
						檢查玩家數據異常

						<v-chip small class="ml-2" color="brown darken-3"> {{ searchResultOfSuspiciousCheck.playerName }}({{ searchResultOfSuspiciousCheck.playerID }}) </v-chip>

						<v-chip small class="ml-2" color="brown darken-3"> {{ searchResultOfSuspiciousCheck.merchantName }}({{ searchResultOfSuspiciousCheck.merchantID }}) </v-chip>

						<v-icon color="yellow" class="ml-4" @click="copyPlayerInfo()"> file_copy </v-icon>

						<v-icon color="yellow lighten-3" class="ml-2 pointer" v-clipboard:copy="copyTextOfPlayerInfo" v-clipboard:success="successHint"> file_copy </v-icon>

						<div class="top-right-item" @click="closeAllModal">
							<v-btn icon dark>
								<v-icon>close</v-icon>
							</v-btn>
						</div>
					</v-card-title>

					<v-card-text v-if="searchResultOfSuspiciousCheck">
						<v-row>
							<v-col cols="12" md="12" sm="12">
								<div class="text-c mt-2">
									<div v-if="searchResultOfSuspiciousCheck.isValidateOK">
										<v-icon color="green" style="font-size: 48px">check</v-icon>
										<div class="f16">確認無異常</div>
									</div>
									<div v-else>
										<v-icon color="red" style="font-size: 48px">close</v-icon>
										<div class="f16 minus-red"><v-icon small color="red">error</v-icon> 需技術人員確認 (點擊右上方複製資訊)</div>
									</div>
								</div>
							</v-col>
						</v-row>

						<v-row style="">
							<v-col cols="12" md="2" sm="2" class="text-c"> </v-col>

							<v-col cols="12" md="2" sm="2" class="text-c">
								<div class="rtpBlock">
									<div class="rtpText">
										<span :class="rtpExtraStyle(searchResultOfSuspiciousCheck.rtP_Player)">
											{{ searchResultOfSuspiciousCheck.rtP_Player }}
										</span>
									</div>
									<div class="sub-title-text">RTP ({{ $t('TableHeader.__player') }})</div>
								</div>
							</v-col>

							<v-col cols="12" md="2" sm="2" class="text-c">
								<div class="rtpBlock">
									<div :class="'rtpText '">
										<span :class="rtpExtraStyle(searchResultOfSuspiciousCheck.rtP_Merchant)">
											{{ searchResultOfSuspiciousCheck.rtP_Merchant }}
										</span>
									</div>
									<div class="sub-title-text">RTP ({{ $t('TableHeader.__merchant') }})</div>
								</div>
							</v-col>

							<v-col cols="12" md="2" sm="2" class="text-c">
								<div class="rtpBlock">
									<div class="rtpText">
										<span :class="rtpExtraStyle(searchResultOfSuspiciousCheck.rtP_Game)">
											{{ searchResultOfSuspiciousCheck.rtP_Game }}
										</span>
									</div>
									<div class="sub-title-text">RTP (單款遊戲)</div>
								</div>
							</v-col>

							<v-col cols="12" md="2" sm="2" class="text-c">
								<div class="rtpBlock">
									<div class="rtpText">
										<span :class="rtpExtraStyle(searchResultOfSuspiciousCheck.rtP_GameServerAll)">
											{{ searchResultOfSuspiciousCheck.rtP_GameServerAll }}
										</span>
									</div>
									<div class="sub-title-text">RTP (遊戲服務器)</div>
								</div>
							</v-col>

							<v-col cols="12" md="2" sm="2" class="text-c"> </v-col>

							<v-col cols="12" md="12" sm="12" class="text-c" v-if="searchResultOfSuspiciousCheck.tags && searchResultOfSuspiciousCheck.tags.length > 0">
								<v-chip v-for="(tag, index) in searchResultOfSuspiciousCheck.tags" :key="index" color="pink lighten-1" class="mr-1">
									<b>{{ tag }}</b>
								</v-chip>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" md="12" sm="12" class="text-c mt-4">
								<div class="rtpText">
									<div v-if="hasSuperPermission || searchResultOfSuspiciousCheck.isValidateOK">
										<v-btn color="purple lighten-3" text class="btn search" @click.stop="copyTextToClipboard(searchResultOfSuspiciousCheck.copyShortMsgCN)">
											<v-icon left>file_copy</v-icon>
											複製無異常文案(中文) </v-btn
										>&#9;

										<v-btn color="purple lighten-3" text class="btn search" @click.stop="copyTextToClipboard(searchResultOfSuspiciousCheck.copyShortMsgEN)">
											<v-icon left>file_copy</v-icon>
											複製無異常文案(EN) </v-btn
										>&#9;
									</div>

									<div class="mt-1" v-if="hasSuperPermission">
										<v-btn color="blue lighten-1" text class="btn search" @click.stop="copyTextToClipboard(searchResultOfSuspiciousCheck.copyMsgCN)">
											<v-icon left>file_copy</v-icon>
											複製文案(中文) </v-btn
										>&#9;

										<v-btn color="blue lighten-1" text class="btn search" @click.stop="copyTextToClipboard(searchResultOfSuspiciousCheck.copyMsgEN)">
											<v-icon left>file_copy</v-icon>
											複製文案(EN) </v-btn
										>&#9;
									</div>
								</div>
							</v-col>

							<v-col cols="12" md="12" sm="12" class="text-c mt-4" style="margin-top: 35px">
								<h2>
									{{ $t('TableHeader.__gameWinLoss') }}
								</h2>

								<div class="f12 grey-text">
									{{ searchResultOfSuspiciousCheck.beginText }} ~
									{{ searchResultOfSuspiciousCheck.endText }}
								</div>

								<table class="ez-table" style="margin-top: 25px; font-size: 14px">
									<tr class="smallTextHeader">
										<!-- <th class="text-l">總代</th> -->
										<th class="text-l" style="">{{ $t('TableHeader.__gameID') }}</th>
										<th class="text-l" style="">{{ $t('TableHeader.__game') }}</th>
										<th class="text-l">服務器配置</th>
										<th class="text-r">{{ $t('TableHeader.__totalRound') }}</th>
										<th class="text-r">{{ $t('TableHeader.__betAmount') }}</th>
										<th class="text-r">{{ $t('TableHeader.__playerWinLoss') }}</th>
										<th class="text-r">RTP</th>
										<th class="text-c"></th>
									</tr>

									<tr v-for="(item, index2) in searchResultOfSuspiciousCheck.gameSummary" :key="index2" class="hover">
										<td class="text-l">
											{{ item.gameID }}
										</td>
										<td class="text-l">
											{{ item.gameName }}
										</td>
										<td class="text-l f12">{{ item.vendorConfigName }} ({{ item.vendorConfigID }})</td>
										<td class="text-r">
											{{ item.totalRound | countDisplay }}
										</td>
										<td class="text-r">
											{{ item.betAmount | amountDisplay }}
										</td>
										<td class="text-r">
											{{ item.playerWinLoss | amountDisplay }}
										</td>
										<td class="text-r">
											<span :class="item.rtp > 100 ? 'yellow-text' : ''">
												{{ item.rtp | amountDisplay }}
											</span>
										</td>

										<td class="text-c">
											<v-icon color="grey" small @click="copyTextToClipboard(item.extraMemo)">file_copy</v-icon>
										</td>
									</tr>
								</table>
							</v-col>

							<v-col cols="12" md="12" sm="12" class="text-c mt-4">
								<h2>大獎注單</h2>

								<table class="ez-table" style="margin-top: 25px; font-size: 14px">
									<tr class="smallTextHeader">
										<th class="text-l">{{ $t('TableHeader.__time') }}</th>
										<th class="text-l">{{ $t('TableHeader.__orderNo') }}</th>
										<th class="text-l">{{ $t('TableHeader.__gameID') }}</th>
										<th class="text-r">{{ $t('TableHeader.__betAmount') }}</th>
										<th class="text-r">{{ $t('TableHeader.__payoutAmount') }}</th>
										<th class="text-r">{{ $t('TableHeader.__playerWinLoss') }}</th>
										<th class="text-r"></th>
									</tr>

									<tr v-for="(item, index2) in searchResultOfSuspiciousCheck.bigWinPerGameLogs" :key="index2" class="hover">
										<td class="text-l">
											{{ item.gameTime | timeWithAddTimezone(userProfile.timeZone) }}
										</td>
										<td class="text-l f12">
											{{ item.transactionNo }}
										</td>
										<td class="text-l">
											{{ item.gameID }}
										</td>
										<td class="text-r">
											{{ item.betAmount | amountDisplay }}
										</td>
										<td class="text-r">
											{{ item.payoutAmount | amountDisplay }}
										</td>
										<td class="text-r">
											{{ item.updateAmount | amountDisplay }}
										</td>

										<td class="text-r">
											<v-icon @click="showExternalLinkDialog(item.transactionNo)" small color="yellow lighten-2"> mdi-eye </v-icon>
										</td>
									</tr>
								</table>
							</v-col>
						</v-row>
					</v-card-text>
				</v-card>
			</v-dialog>

			<!-- 檢查玩家數據異常 (Live視訊遊戲) -->
			<v-dialog v-model="isShowLiveCasinoSuspiciousCheckDialog" max-width="1000px" scrollable>
				<v-card>
					<v-card-title class="grey darken-2">
						視訊簡易版檢查

						<v-chip small class="ml-2" color="brown darken-3"> {{ searchResultOfLiveCasinoSuspiciousCheck.playerName }}({{ searchResultOfLiveCasinoSuspiciousCheck.playerID }}) </v-chip>

						<v-chip small class="ml-2" color="brown darken-3">
							{{ searchResultOfLiveCasinoSuspiciousCheck.merchantName }}({{ searchResultOfLiveCasinoSuspiciousCheck.merchantID }})
						</v-chip>

						<v-chip small class="ml-2" color="pink darken-3">
							{{ searchResultOfLiveCasinoSuspiciousCheck.currency }}
						</v-chip>

						<v-tooltip bottom color="orange darken-4">
							<template v-slot:activator="{ on, attrs }">
								<div v-bind="attrs" v-on="on" style="color: #e89247; text-decoration: underline">
									<span class="f14 ml-3"> <v-icon color="orange" small class="f14">info</v-icon> 注意事項 </span>
								</div>
							</template>
							<span>
								<p>本功能只能做很基礎的查詢。若查到【明顯有異常】可以提供給客戶</p>
								<p>但若查無異常、且注單數不少。還是要請【原廠】再協助確認一次</p>
								<p>若可疑局數沒有超過一半。建議還是請【原廠】再確認清楚</p>
							</span>
						</v-tooltip>

						<v-btn color="grey lighten-2" small text class="btn search ml-4" @click.stop="copyTextNoSuspicious()">
							<v-icon left>clear</v-icon>
							複製文案(無異常)
						</v-btn>

						<div class="top-right-item" @click="closeAllModal">
							<v-btn icon dark>
								<v-icon>close</v-icon>
							</v-btn>
						</div>
					</v-card-title>

					<v-card-text style="padding-top: 15px; min-height: 400px">
						<div class="mt-4">
							<div class="f14">
								<span class="f12">玩家:</span>
								<span class="white-text f20 mr-4">
									{{ searchResultOfLiveCasinoSuspiciousCheck.playerName }}
									({{ searchResultOfLiveCasinoSuspiciousCheck.playerID }})
								</span>

								<span class="f12">玩家總局數:</span>
								<span class="f20 yellow-text">
									{{ searchResultOfLiveCasinoSuspiciousCheck.totalRoundOfMe }}
								</span>

								<span class="f12 ml-4">玩家RTP:</span>
								<span class="f20 yellow-text">
									{{ searchResultOfLiveCasinoSuspiciousCheck.rtP_Player }}
								</span>

								<span class="f12 ml-4">遊戲RTP:</span>
								<span class="f20 yellow-text">
									{{ searchResultOfLiveCasinoSuspiciousCheck.rtP_Game }}
								</span>
							</div>
							<div class="f14 mt-2">
								標籤:
								<v-chip v-for="(tag, index) in searchResultOfLiveCasinoSuspiciousCheck.tags" small class="ml-2" :key="index">
									{{ tag }}
								</v-chip>
							</div>

							<v-divider class="mt-2"></v-divider>
						</div>

						<div v-if="searchResultOfLiveCasinoSuspiciousCheck.sameRoundPlayers == null || searchResultOfLiveCasinoSuspiciousCheck.sameRoundPlayers.length === 0">
							<div class="mb-4 mt-4">⚠️ <span class="f16 yellow-text">沒有與其他玩家同局的紀錄</span></div>
						</div>

						<div class="mt-4" v-if="searchResultOfLiveCasinoSuspiciousCheck.sameRoundPlayers && searchResultOfLiveCasinoSuspiciousCheck.sameRoundPlayers.length > 0">
							<h3 class="white-text">有同一局的玩家</h3>
						</div>

						<div class="myThinTable" v-if="searchResultOfLiveCasinoSuspiciousCheck.sameRoundPlayers && searchResultOfLiveCasinoSuspiciousCheck.sameRoundPlayers.length > 0">
							<v-data-table
								:items="searchResultOfLiveCasinoSuspiciousCheck.sameRoundPlayers"
								style="width: 550px"
								:footer-props="{ 'items-per-page-options': [50] }"
								hide-default-footer
								:headers="headersOfRoundCompare"
							>
							</v-data-table>
						</div>

						<div
							class="myThinTable"
							style="margin-bottom: 30px; margin-top: 35px"
							v-for="(item, index) in searchResultOfLiveCasinoSuspiciousCheck.playerBaseAndEachRoundCompare"
							:key="index"
						>
							<div class="white-text f20" style="position: relative; background: #333; padding: 10px 5px">
								<span class="grey-text f14">玩家: </span>{{ item.playerName }}
								<span class="f14">({{ item.playerID }})</span>

								<div style="display: inline-block; position: absolute; right: 1%">
									<span class="f14 ml-2">可疑局數/總局數:</span>
									<span class="f18 yellow-text ml-1">{{ item.totalRoundOfSuspicious }}/{{ item.totalRound }} </span>
									<span class="f14 grey-text">({{ item.suspiciousRatePercentage }} %)</span>
									<v-icon color="yellow lighten-2" class="ml-4" @click="copyTextToClipboard2(item.copyText_CN)">file_copy</v-icon>
								</div>
							</div>

							<div>
								<v-data-table :items="item.eachRoundCompareItems" :headers="headersOfAmountCompare" :footer-props="{ 'items-per-page-options': [50] }" hide-default-footer>
									<template v-slot:item.totalWinLossOfSuspiciousPlayer="{ item }">
										<div :class="item.totalWinLossOfSuspiciousPlayer < 0 ? 'minus-red' : ''">
											{{ item.totalWinLossOfSuspiciousPlayer }}
										</div>
									</template>

									<template v-slot:item.totalWinLossOfOtherPlayer="{ item }">
										<div :class="item.totalWinLossOfOtherPlayer < 0 ? 'minus-red' : ''">
											{{ item.totalWinLossOfOtherPlayer }}
										</div>
									</template>

									<template v-slot:item.isSuspicious="{ item }">
										<v-chip small color="red darken-2" v-if="item.isSuspicious">可疑</v-chip>
									</template>
								</v-data-table>
							</div>
						</div>
					</v-card-text>
				</v-card>
			</v-dialog>

			<!-- 三方注單資訊 & 產生外部遊戲快照連結 -->
			<v-dialog v-model="isShowExternalLinkDialog" max-width="900px" scrollable>
				<PerGameThirdPartyDetailPopup ref="refPerGameThirdPartyDetailPopup" @closeParentAllModal="closeMyPopup"> </PerGameThirdPartyDetailPopup>
			</v-dialog>
		</div>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import DropdownApi from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, toDateStringMMDDByDateText } from '@/utilities/datetime-format';
import * as Model from '@/models/interfaces/statistics/game';
import ReportApi from '@/api/statistics/merchant-summary';
import PlayersApi from '@/api/members-management/players';
import DatePicker from 'vue2-datepicker';
import LineChartPopup from '@/components/statistics/LineChartPopup.vue';
import PerGameThirdPartyDetailPopup from '@/components/game-management/PerGameThirdPartyDetailPopup.vue';
import * as PermissionModel from '@/models/interfaces/commons/permission';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, commonMethodMixin], components: { PerGameThirdPartyDetailPopup, DatePicker, LineChartPopup } })
export default class SearchList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;

	panel: number[] = [0];
	renderComponent: boolean = true;
	isShowSuspiciousCheckDialog: boolean = false;
	isShowLiveCasinoSuspiciousCheckDialog: boolean = false;
	isShowExternalLinkDialog: boolean = false;

	copyTextOfPlayerInfo: string = '';
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchFilter: Model.IPlayerGameSummaryRequest = {} as Model.IPlayerGameSummaryRequest;
	suspiciousSearchFilter: DropdownModel.ILazyCommonParameterReqeust = {} as DropdownModel.ILazyCommonParameterReqeust;
	searchResult: Model.IPlayerGameSummary = {} as Model.IPlayerGameSummary;
	searchResultOfSuspiciousCheck: any = {};
	searchResultOfLiveCasinoSuspiciousCheck: any = {};

	get headersOfRoundCompare() {
		return [
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName', align: 'start', sortable: false },
			{ text: '共同參與局數', value: 'totalRound', sortable: true },
			{ text: '同局比率(%)', value: 'sameRoomRatePercentage', sortable: true },
		];
	}

	get headersOfAmountCompare() {
		return [
			{ text: '局號', value: 'roundID', sortable: true },
			{ text: '投注金額(主要玩家)', value: 'totalBetAmountOfSuspiciousPlayer', align: 'end' },
			{ text: '投注金額(比較玩家)', value: 'totalBetAmountOfOtherPlayer', align: 'end' },
			{ text: '輸贏金額(主要玩家)', value: 'totalWinLossOfSuspiciousPlayer', align: 'end' },
			{ text: '輸贏金額(比較玩家)', value: 'totalWinLossOfOtherPlayer', align: 'end' },
			{ text: '可疑', value: 'isSuspicious', sortable: true },
		];
	}
	get hasPermissionGameDetail() {
		return true;
	}

	get hasSuperPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get hasCustomerServicePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_PGS_CustomerService) >= 0;
	}

	get headers() {
		return [
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName', align: 'start', sortable: false },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName', align: 'start', sortable: false },
			{ text: this.$t('TableHeader.__gameID'), value: 'gameID', sortable: true },
			{ text: this.$t('TableHeader.__gameName'), value: 'gameName', sortable: false },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', align: 'end', sortable: false },
			{ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__validAmount'), value: 'validAmount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__playerWinLoss'), value: 'winLossAmount', align: 'end', sortable: true },
			{ text: 'RTP', value: 'rtp', align: 'end', sortable: true },
			// { text: this.$t('TableHeader.__taxAmount'), value: 'taxAmount', align: 'end', sortable: true },
		];
	}

	get headersOfEventSummary() {
		return [
			{ text: this.$t('TableHeader.__eventID'), value: 'eventID', sortable: true },
			{ text: this.$t('TableHeader.__eventName'), value: 'eventName', sortable: false },
			{ text: this.$t('TableHeader.__totalCount'), value: 'totalCount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__amount'), value: 'totalAmount', align: 'end', sortable: true },
		];
	}

	@Action('Players/setGameID') private setGameID!: any;
	@Action('Players/setGamePlayerID') private setGamePlayerID!: any;
	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.clear();
			this.reloadComponent();
		});
	}

	reloadComponent() {
		setTimeout(() => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		}, 250);
	}

	search() {
		EventBus.$emit('show_mask', null);
		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		ReportApi.getPlayerGameSummaryReportList(this.searchFilter)
			.then((res) => {
				this.searchResult = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showSuspiciousCheckDialog() {
		this.suspiciousSearchFilter = {} as DropdownModel.ILazyCommonParameterReqeust;
		this.suspiciousSearchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.suspiciousSearchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		this.suspiciousSearchFilter.playerID = this.searchFilter.playerID;
		this.suspiciousSearchFilter.playerName = this.searchFilter.playerName;
		this.searchResultOfSuspiciousCheck = {};

		if (this.suspiciousSearchFilter.playerID != null) {
			const idText = this.suspiciousSearchFilter.playerID.toString();
			if (idText === '' || idText === ' ' || idText.length <= 0) {
				this.suspiciousSearchFilter.playerID = null;
			}
		}

		EventBus.$emit('show_mask', null);
		PlayersApi.playerSuspiciousCheck(this.suspiciousSearchFilter)
			.then((res) => {
				this.isShowSuspiciousCheckDialog = true;
				this.searchResultOfSuspiciousCheck = res.data;
				this.setCopyText();
				console.log(res.data);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showLiveCasinoSuspiciousCheckDialog() {
		this.suspiciousSearchFilter = {} as DropdownModel.ILazyCommonParameterReqeust;
		this.suspiciousSearchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.suspiciousSearchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		this.suspiciousSearchFilter.playerID = this.searchFilter.playerID;
		this.suspiciousSearchFilter.playerName = this.searchFilter.playerName;
		this.searchResultOfLiveCasinoSuspiciousCheck = {};

		if (this.suspiciousSearchFilter.playerID != null) {
			const idText = this.suspiciousSearchFilter.playerID.toString();
			if (idText === '' || idText === ' ' || idText.length <= 0) {
				this.suspiciousSearchFilter.playerID = null;
			}
		}

		EventBus.$emit('show_mask', null);
		PlayersApi.playerLiveCasinoSuspiciousCheck(this.suspiciousSearchFilter)
			.then((res) => {
				this.isShowLiveCasinoSuspiciousCheckDialog = true;
				this.searchResultOfLiveCasinoSuspiciousCheck = res.data;
				this.setCopyText();
				console.log(res.data);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showExternalLinkDialog(betRecordNoPGS: string) {
		this.isShowExternalLinkDialog = true;

		this.$nextTick(() => {
			const myRef: any = this.$refs.refPerGameThirdPartyDetailPopup;
			myRef.getExternalLinkDialogInfoByNo(betRecordNoPGS);
		});
	}

	successHint() {
		EventBus.$emit(`successfully-copy`);
	}

	clear() {
		this.searchFilter = {} as Model.IPlayerGameSummaryRequest;
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	rtpExtraStyle(myRTP: number) {
		if (myRTP >= 100) {
			return 'yellow-text';
		}

		return '';
	}

	mounted() {
		// this.searchFilter.playerID = 1017184;

		this.reloadComponent();
		this.registerReloadComponent();
		this.init();
	}

	closeAllModal() {
		this.isShowSuspiciousCheckDialog = false;
		this.isShowLiveCasinoSuspiciousCheckDialog = false;
	}

	closeMyPopup() {
		this.isShowExternalLinkDialog = false;
	}

	copyTextNoSuspicious() {
		const myText: string = '您好，玩家帐号:' + this.searchResultOfLiveCasinoSuspiciousCheck.playerName + ' 我方查看后无发现异常';
		const vm: any = this;
		vm.copyTextToClipboard(myText);
	}

	init() {
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-7), daysAgo(1)];
	}

	setCopyText() {
		let msg: string = '';
		msg += '您好。玩家RTP數據檢查異常。請幫忙確認下 (' + this.searchResultOfSuspiciousCheck.beginText + ' ~ ' + this.searchResultOfSuspiciousCheck.endText + ')\n';
		msg += '玩家: ' + this.searchResultOfSuspiciousCheck.playerName + ' (' + this.searchResultOfSuspiciousCheck.playerID + ' *可能對應遊戲端 玩家暱稱、玩家帳號)\n';
		msg += '商戶: ' + this.searchResultOfSuspiciousCheck.merchantName + ' (' + this.searchResultOfSuspiciousCheck.merchantID + ')\n';
		msg += '頻道: ' + this.searchResultOfSuspiciousCheck.imChannelName + '\n';
		msg += this.searchResultOfSuspiciousCheck.vendorConfigInfo;

		let memoText: string = '';

		if (this.searchResultOfSuspiciousCheck.tags != null && this.searchResultOfSuspiciousCheck.tags.length > 0) {
			this.searchResultOfSuspiciousCheck.tags.forEach((tag: string) => {
				memoText = memoText + '【' + tag + '】　';
			});
		}

		msg += '備註: ' + memoText + '\n';

		this.copyTextOfPlayerInfo = msg;
	}

	copyPlayerInfo() {
		const vm: any = this;
		vm.copyTextToClipboard(this.copyTextOfPlayerInfo);
	}

	async exportFiles() {
		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__merchant'),
			this.geti18nTextAsString('TableHeader.__playerAccount'),
			this.geti18nTextAsString('TableHeader.__gameID'),
			this.geti18nTextAsString('TableHeader.__gameName'),
			this.geti18nTextAsString('TableHeader.__totalRound'),
			this.geti18nTextAsString('TableHeader.__validAmount'),
			this.geti18nTextAsString('TableHeader.__betAmount'),
			this.geti18nTextAsString('TableHeader.__payoutAmount'),
			this.geti18nTextAsString('TableHeader.__playerWinLoss'),
			'rtp',
			// this.geti18nTextAsString('TableHeader.__taxAmount'),
		];

		const excelData: any = [];

		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);
		const result = await ReportApi.getPlayerGameSummaryReportList(this.searchFilter).finally(() => {
			EventBus.$emit('hide_mask', null);
		});

		result.data.playerGameSummary.forEach((value) => {
			const eachRow: any = [];

			eachRow.push(value.merchantName);
			eachRow.push(value.playerName);

			eachRow.push(value.gameID);
			eachRow.push(value.gameName);
			eachRow.push(value.totalRound);
			eachRow.push(value.validAmount);
			eachRow.push(value.betAmount);
			eachRow.push(value.payoutAmount);
			eachRow.push(value.winLossAmount);
			eachRow.push(value.rtp);
			// eachRow.push(value.taxAmount);
			excelData.push(eachRow);
		});

		const datePrefix = '_' + toDateStringMMDDByDateText(this.searchFilter.begin, this.userProfile.timeZone) + '_' + toDateStringMMDDByDateText(this.searchFilter.end, this.userProfile.timeZone);

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_PlayerGameReport' + datePrefix + '.xlsx',
		};
		// @ts-ignore
		this.downloadExcel(config);
	}
}
</script>
<style lang="scss" scoped>
.rtpText {
	font-size: 18px;
	margin-bottom: 0px;
	color: #fff;
}

.rtpBlock {
	background: #333;
	max-width: 150px;
	margin: 0 auto;
	padding: 10px;
	border-radius: 5px;
}

@media only screen and (max-width: 480px) {
	.height1-phone {
		min-height: 125px;
	}
}
</style>
