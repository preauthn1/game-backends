<template>
	<v-app>
		<v-card class="mx-auto" :min-width="hasPlayerData ? null : 1600">
			<v-flex>
				<v-toolbar class="my_toolbar_height">
					<v-toolbar-title>
						<h4>{{ $t('TableHeader.__playerID') }}</h4>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-card-title>
						<v-text-field
							v-model="searchPlayerID"
							:label="$t('TableHeader.__playerID') + '／' + $t('TableHeader.__playerAccount')"
							single-line
							hide-details
							v-on:keyup.enter="searchPlayer"
						>
						</v-text-field>

						<v-icon style="cursor: pointer" color="yellow darken-1" @click="searchPlayer" :title="'search'"> search </v-icon>

						<v-autocomplete
							v-if="hasSuperPermission"
							single-line
							hide-details
							class="hide-in-phone"
							v-model="extraFilterMerchantID"
							:items="merchantsDropdownItems"
							style="margin-left: 30px"
							:label="$t('TableHeader.__extraFilterType') + ' (' + $t('TableHeader.__merchant') + ')'"
						>
						</v-autocomplete>
					</v-card-title>
					<v-spacer></v-spacer>

					<div class="myButtonRows" v-if="playerDetail.playerID">
						<v-btn v-if="hasCopyAssignIDPermission" color="blue lighten-3" text class="btn search ml-1" @click="showCopyTextDialog(playerDetail.copyTextForCS)">
							<v-icon left>file_copy</v-icon>
							{{ $t('Button.__copyInfo') }}
						</v-btn>

						<v-btn v-if="hasAdvanceUpdatePermission" color="orange lighten-3" text class="btn search ml-1" @click="showCreateTagDialog()">
							<v-icon left>add</v-icon>
							標記標籤
						</v-btn>

						<v-btn v-if="hasAdvanceUpdatePermission" color="yellow lighten-4" text class="btn search ml-1" @click="showBatchUpdateRTP()">
							<v-icon left>edit</v-icon>
							批量修改RTP
						</v-btn>

						<v-btn
							v-if="hasLaunchSimulatePermission && playerDetail.walletMode !== walletMode.SingleWalletMode"
							color="orange lighten-3"
							text
							class="btn search ml-1"
							@click="showGameLaunchDialog(playerDetail)"
						>
							<v-icon left>link</v-icon>
							模擬進入遊戲
						</v-btn>

						<v-btn
							v-if="hasAdvanceUpdatePermission"
							v-show="playerDetail.walletMode !== walletMode.SingleWalletMode"
							color="green lighten-3"
							text
							class="btn search ml-1"
							@click="showDepositWallet"
						>
							<v-icon left>monetization_on</v-icon>
							{{ $t('Button.__ManualDepositWallet') }}
						</v-btn>

						<v-btn v-if="hasPermission" color="red lighten-3" text class="btn search ml-1" @click="dialogDeletePlayerPopup = true">
							<v-icon left>{{ playerDetail.status === status.Disable ? 'lock_open' : 'lock' }} </v-icon>
							{{ playerDetail.status === status.Disable ? `${$t('Popup.__unlock')}` : `${$t('Popup.__lock')}` }}
						</v-btn>

						<v-btn v-if="hasPermission" color="light-blue lighten-3" text class="btn search ml-1" @click="isShowConfirmRemoveRoomLock = true">
							<v-icon left>remove_circle_outline</v-icon>
							{{ $t('Button.__removeRoomLock') }}
						</v-btn>
					</div>
				</v-toolbar>

				<v-row v-show="hasPlayerData">
					<v-col cols="12" sm="2" md="2">
						<v-list-item two-line>
							<v-list-item-content>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__playerID')"> </v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__displayName')"></v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__status')"> </v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__walletAmount')"> </v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-if="playerDetail.isFishWalletVisible" v-text="$t('TableHeader.__walletAmount') + '(Fish)'"> </v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__walletMode')"> </v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__createTime')"> </v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__currentGameRoom')"> </v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__tag')"> </v-list-item-subtitle>
							</v-list-item-content>
						</v-list-item>
					</v-col>
					<v-col cols="12" sm="4" md="4">
						<v-list-item two-line>
							<v-list-item-content>
								<v-list-item-subtitle>{{ playerDetail.playerID }}</v-list-item-subtitle>
								<v-list-item-subtitle>{{ playerDetail.displayName }}</v-list-item-subtitle>
								<v-list-item-subtitle>
									<span v-show="status.Enable === playerDetail.status" class="green--text font-weight-medium">
										<v-icon small left color="green">check_circle_outline</v-icon>
										{{ $t(`Status.${status[playerDetail.status]}`) }}
									</span>
									<span v-show="status.Disable === playerDetail.status" class="red--text font-weight-medium">
										<v-icon small left color="red">highlight_off</v-icon>
										{{ $t(`Status.${status[playerDetail.status]}`) }}
									</span>
								</v-list-item-subtitle>

								<!-- 单一钱包不显示记录 -->
								<v-list-item-subtitle>
									<div v-if="!isNaN(playerDetail.walletAmount) && playerDetail.walletMode != walletMode.SingleWalletMode">
										<div v-if="hasAdvanceUpdatePermission">
											<v-tooltip top color="teal darken-1">
												<template v-slot:activator="{ on }">
													<a v-on="on" @click="showDepositWallet()">
														<v-icon color="primary" small left>attach_money</v-icon>
														<span>
															{{ playerDetail.walletAmount }}
														</span>
													</a>

													<v-chip small class="ml-2" v-if="playerDetail.transferConvertRate != 1" style="margin-top: -5px">
														{{ $t('TableHeader.__convertRate') }}
														1:{{ playerDetail.transferConvertRate }}
													</v-chip>
												</template>

												{{ $t('Button.__ManualDepositWallet') }}
											</v-tooltip>
										</div>
										<div v-else>
											{{ playerDetail.walletAmount }}
										</div>
									</div>

									<div v-else>-</div>
								</v-list-item-subtitle>
								<v-list-item-subtitle v-if="playerDetail.isFishWalletVisible" @click="fetchFishWallet">
									{{ playerFishWalletBalance }}
								</v-list-item-subtitle>
								<v-list-item-subtitle>
									{{ $t(`MerchantsStatus.${walletMode[playerDetail.walletMode]}`) }}
									<v-chip small v-if="isSingleWalletMode()" color="purple darken-2">SW</v-chip>
								</v-list-item-subtitle>

								<v-list-item-subtitle>
									{{ playerDetail.createTime | timeWithAddTimezone(userProfile.timeZone) }}
								</v-list-item-subtitle>

								<v-list-item-subtitle>
									<div v-if="playerDetail.playerCurrentServerID > 0" :title="playerDetail.playerCurrentServerID">
										<v-icon small left color="orange">room</v-icon>
										<span style="color: #ffa261">
											{{ playerDetail.playerCurrentGameGame }}
										</span>
									</div>
									&nbsp;
								</v-list-item-subtitle>

								<v-list-item-subtitle>
									<div style="white-space: break-spaces">
										<v-chip
											v-for="(item, index) in playerDetail.playerTags"
											:key="index"
											:title="item.tagID"
											@click="deletePlayerTag(item.tagID, item.tagName)"
											class="pointer mb-1"
										>
											{{ item.tagName }}
											<v-icon class="ml-1" small>mdi-close-circle</v-icon>
										</v-chip>
									</div>
								</v-list-item-subtitle>
							</v-list-item-content>
						</v-list-item>
					</v-col>
					<v-col cols="12" sm="2" md="2">
						<v-list-item two-line>
							<v-list-item-content>
								<!-- <v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__merchantID')"></v-list-item-subtitle> -->
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__playerAccount')"></v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__playerType')"> </v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__merchant')"> </v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__loginCount')"> </v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__updateTime')"> </v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__extraInfo')"> </v-list-item-subtitle>
								<v-list-item-subtitle class="text--primary" v-text="$t('TableHeader.__memo')"> </v-list-item-subtitle>
							</v-list-item-content>
						</v-list-item>
					</v-col>
					<v-col cols="12" sm="4" md="4">
						<v-list-item two-line>
							<v-list-item-content>
								<v-list-item-subtitle>
									{{ playerDetail.playerName }}
								</v-list-item-subtitle>
								<v-list-item-subtitle>
									{{ playerDetail.playerTypeText }}
								</v-list-item-subtitle>
								<v-list-item-subtitle>
									{{ playerDetail.merchantName }}
									<span class="ml-2"> ({{ playerDetail.merchantID }}) </span>

									<v-chip x-small class="ml-2">
										{{ playerDetail.currency }}
									</v-chip>
								</v-list-item-subtitle>
								<!-- <v-list-item-subtitle>{{playerDetail.totalSummary && playerDetail.totalSummary.totalLoginTimes}}</v-list-item-subtitle> -->

								<v-list-item-subtitle v-if="playerDetail.totalSummary && playerDetail.totalSummary.totalLoginTimes">
									{{ playerDetail.totalSummary.totalLoginTimes }}
								</v-list-item-subtitle>
								<v-list-item-subtitle v-else>0</v-list-item-subtitle>
								<v-list-item-subtitle>
									{{ playerDetail.lastLoginTime | timeWithAddTimezone(userProfile.timeZone) }}
								</v-list-item-subtitle>
								<v-list-item-subtitle>
									<div v-if="playerDetail.riskScore > 0" class="minus-mt-2">
										<v-chip small color="red darken-4">風險分數:{{ playerDetail.riskScore }}</v-chip>
									</div>
								</v-list-item-subtitle>

								<v-list-item-subtitle>
									{{ playerDetail.memo }}
									<v-icon class="ml-2 minus-mt-1" color="yellow lighten-2" v-if="hasAdvanceUpdatePermission" @click="showUpdateMemo()"> edit </v-icon>
								</v-list-item-subtitle>
							</v-list-item-content>
						</v-list-item>
					</v-col>
				</v-row>
			</v-flex>
		</v-card>

		<!-- 顯示額外資訊查詢按鈕 -->
		<v-card class="mt-1">
			<v-card-title>
				<div style="display: inline-block">
					<v-btn v-if="playerDetail.isLobbyTransferEnable" color="blue lighten-3" text class="btn search ml-1" @click="showW88TransferFailCheckDialog(playerDetail.playerID)">
						<v-icon left>visibility</v-icon>
						{{ $t('Menu.MembersManagementTransfer') }} (Lobby)
					</v-btn>
				</div>

				<!-- 超級管理員才有這些按鈕 -->
				<div style="display: inline-block" v-if="hasPlayerData && hasSuperPermission">
					<v-btn v-if="hasAdvanceUpdatePermission" color="blue lighten-3" text class="btn search ml-1" @click="showRTPChangeLog()">
						<v-icon left>visibility</v-icon>
						查看RTP異動紀錄
					</v-btn>

					<v-btn v-if="hasSuperPermission" color="yellow lighten-3" text class="btn search ml-1" @click="showSingleWalletMerchantToken()">
						<v-icon left>visibility</v-icon>
						查看單錢包商戶Token紀錄
					</v-btn>

					<v-btn v-if="hasSuperPermission" color="pink lighten-3" text class="btn search ml-1" @click="showVendorSessionToken()">
						<v-icon left>visibility</v-icon>
						查看遊戲商Token紀錄
					</v-btn>

					<v-btn v-if="hasSuperPermission" color="yellow lighten-3" text class="btn search ml-1" @click="showLobbyToken()">
						<v-icon left>visibility</v-icon>
						查看LobbyToken紀錄
					</v-btn>

					<v-btn v-if="hasSuperPermission" color="purple lighten-2" text class="btn search ml-1" @click="openSingleWalletLogHistoryPage()" :disabled="!isSingleWalletMode()">
						<v-icon left>visibility</v-icon>
						查看SW callback紀錄
					</v-btn>

					<v-btn v-if="hasSuperPermission" color="pink lighten-2" text class="btn search ml-1" @click="showExternalLogs()">
						<v-icon left>visibility</v-icon>
						查看三方EX logs
					</v-btn>
				</div>
			</v-card-title>
		</v-card>

		<!-- 顯示用戶事件紀錄 -->
		<div class="mt-2" v-if="playerDetail.playerBehaviorEvents != null && playerDetail.playerBehaviorEvents.length > 0 && hasSuperPermission">
			<v-card>
				<v-data-table
					style="width: 600px"
					:page="DataTablePagingParameter.pageIndex"
					:items-per-page="DataTablePagingParameter.itemsPerPage"
					:footer-props="DataTablePagingParameter.footerProps"
					:headers="playerBehaviorEventHeaders"
					id="playerBehaviorEvents"
					:items="playerDetail.playerBehaviorEvents"
					hide-default-footer
				>
					<template v-slot:top>
						<v-toolbar height="40px">
							<v-toolbar-title>
								<div class="f18">
									<v-icon class="mr-1"> book </v-icon>
									重要事件紀錄
								</div>
							</v-toolbar-title>
						</v-toolbar>
					</template>

					<template v-slot:item.createTime="{ item }">
						{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
					</template>
				</v-data-table>
			</v-card>
		</div>

		<!-- 遊戲統計訊息 -->
		<v-card class="mt-2 mx-auto" :min-width="playerDetail.playerID ? null : 1600" style="width: 100%">
			<v-data-table
				v-show="playerDetail.playerID && hasWatchGameSummaryPermission"
				:page="DataTablePagingParameter.pageIndex"
				:items-per-page="DataTablePagingParameter.itemsPerPage"
				:footer-props="DataTablePagingParameter.footerProps"
				:headers="headers"
				id="playerID"
				:items="playerDetail.playerGameSummaries"
				:search="searchText"
				hide-default-footer
			>
				<template v-slot:top>
					<v-toolbar height="40px">
						<v-toolbar-title>
							<div class="f18">
								<v-icon class="mr-1"> history </v-icon>
								{{ $t('TableHeader.__gameInfo') }}

								<div style="display: inline-block; font-size: 12px; margin-left: 10px" class="orange--text text--lighten-2">
									<v-icon class="ml-3" color="orange lighten-2 "> visibility </v-icon>
									{{ $t('TableHeader.__playerViewPoint') }}
								</div>
							</div>
						</v-toolbar-title>
						<v-divider class="mx-4" inset vertical></v-divider>
						<v-spacer></v-spacer>
						<v-card-title>
							<v-text-field v-model="searchText" append-icon="search" label="Search" single-line hide-details clearable></v-text-field>
						</v-card-title>
					</v-toolbar>
				</template>
				<template v-slot:item.gameName="{ item }">
					<span>{{ item.gameName }}</span>

					<!-- 效能殺手 不再提供此 -->
					<!-- <v-tooltip top color="teal darken-1">
						<template v-slot:activator="{ on }">
							<span>
								{{ item.gameName }}
							</span>

							<a v-on="on" v-show="hasPermissionGameDetail"
								@click="openGameDetail(playerDetail.playerID , item)">
								<v-icon color="primary" small left>mdi-link</v-icon>
								{{ item.gameName }}
							</a>
						</template>
						<span>{{ $t('Button.__openLink') }}</span>
					</v-tooltip> -->
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

				<template v-slot:item.score="{ item }">
					<span :class="item.score < 0 ? 'minus-red' : ''">
						{{ item.score | amountDisplay }}
					</span>
				</template>

				<template v-slot:item.rtp="{ item }">
					<span :class="item.rtp >= 100 ? 'h-yellow' : ''">
						{{ item.rtp | amountDisplay }}
					</span>
				</template>

				<template v-slot:item.gameVendorConfigName="{ item }">
					<span v-if="item.gameVendorConfigID == 0">
						{{ $t('Message.__defaultValue') }}
					</span>

					<span v-else>
						<span v-if="hasCopyAssignIDPermission" class="pointer" @click="showCopyTextDialog(item.gameVendorAssignInfoForCopy)">
							{{ item.gameVendorConfigName }}
						</span>
						<span v-else>
							{{ item.gameVendorConfigName }}
						</span>
					</span>
				</template>

				<template v-slot:item.rtpConfigValue="{ item }">
					<div v-if="hasAdvanceUpdatePermission">
						<span v-if="!item.isSupportRTPConfig" class="grey-text">
							<!-- {{ $t('Message.__notSupport2') }} -->
						</span>

						<span v-else-if="item.rtpConfigValue == null" class="link-text-grey" @click="showEditRTPPopup(item)">
							{{ $t('Message.__defaultValue') }}
						</span>

						<span v-else class="link-text" @click="showEditRTPPopup(item)">
							{{ item.rtpConfigValue }}
						</span>
					</div>
				</template>

				<template v-slot:body.append v-if="playerDetail.playerID && playerDetail.playerGameSummaryRowTotal">
					<tr class="footer-total-row">
						<td>
							<p class="font-weight-black">#{{ $t('TableHeader.__total') }}：</p>
						</td>
						<td></td>
						<td></td>
						<td>
							<p class="font-weight-medium text-r">
								{{ playerDetail.playerGameSummaryRowTotal.totalRound }}
							</p>
						</td>

						<td>
							<p class="font-weight-medium text-r">
								{{ playerDetail.playerGameSummaryRowTotal.betAmount | amountDisplay }}
							</p>
						</td>

						<td>
							<p class="font-weight-medium text-r">
								{{ playerDetail.playerGameSummaryRowTotal.validAmount | amountDisplay }}
							</p>
						</td>

						<td>
							<p class="font-weight-medium text-r">
								{{ playerDetail.playerGameSummaryRowTotal.payoutAmount | amountDisplay }}
							</p>
						</td>

						<td>
							<p class="font-weight-medium text-r">
								<span :class="playerDetail.playerGameSummaryRowTotal.score < 0 ? 'minus-red' : ''">
									{{ playerDetail.playerGameSummaryRowTotal.score | amountDisplay }}
								</span>
							</p>
						</td>

						<td>
							<p class="font-weight-medium text-r">
								<span :class="playerDetail.playerGameSummaryRowTotal.rtp >= 100 ? 'h-yellow' : ''">
									{{ playerDetail.playerGameSummaryRowTotal.rtp }}
								</span>
							</p>
						</td>
					</tr>
				</template>
			</v-data-table>
		</v-card>

		<!-- 近日投注日報表 -->
		<v-card class="mx-auto" :min-width="playerDetail.playerID ? null : 1600" v-show="playerDetail.playerID && hasWatchGameSummaryPermission" style="margin-top: 10px; width: 100%">
			<v-data-table :headers="dailySummaryHeaders" id="playerID" hide-default-footer :items="playerDetail.playerRecentlyBetSummary">
				<template v-slot:top>
					<v-toolbar>
						<v-toolbar-title>
							<h4>
								<v-icon class="mr-1"> event </v-icon>

								{{ $t('TableHeader.__recentDailySummary') }}
							</h4>
						</v-toolbar-title>
						<v-divider class="mx-4" inset vertical></v-divider>
						<v-spacer></v-spacer>
					</v-toolbar>
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

				<template v-slot:item.winLossAmount="{ item }">
					<span :class="item.winLossAmount < 0 ? 'minus-red' : ''">
						{{ item.winLossAmount | amountDisplay }}
					</span>
				</template>

				<template v-slot:item.rtp="{ item }">
					<span :class="item.rtp >= 100 ? 'h-yellow' : ''">
						{{ item.rtp | amountDisplay }}
					</span>
				</template>
			</v-data-table>
		</v-card>

		<!-- 金幣紀錄 -->
		<!-- <v-card id="playerMoneyRecords" class="mx-auto" :min-width="playerDetail.playerID ? null : 1600"
			v-show="playerDetail.playerID && hasMoneyRecordPermission"
			style="margin-top:20px;width:100%;">

			<v-toolbar v-show="isSingleWalletMode()">
				<v-toolbar-title>
					<h4>

						<v-icon class="mr-1">
							money
						</v-icon>

						{{ $t('Menu.MembersManagementMoneyRecord') }}

						<div style="display:inline-block;font-size:12px;margin-left:10px;">
							<v-icon style="font-size:16px;" color="grey">error</v-icon>
							<span style="color:#CCC;">{{ $t('Message.__singleWalletNotSupport') }}</span>
						</div>

					</h4>
				</v-toolbar-title>
				<v-divider class="mx-4" inset vertical></v-divider>
				<v-spacer></v-spacer>

			</v-toolbar>

			<v-data-table :headers="moneyRecordHeaders" id="playerID" :items="searchMoneyRecords"
				v-show="!isSingleWalletMode()">
				<template v-slot:top>
					<v-toolbar>
						<v-toolbar-title>
							<h4>

								<v-icon class="mr-1">
									money
								</v-icon>

								{{ $t('Menu.MembersManagementMoneyRecord') }}

								<span style="font-size:14px;font-weight:400;">
									( {{ $t("Button.__top10")}} )
								</span>

								<span class='link-text' @click="walletDetail(playerDetail.playerID)"
									style="font-size:14px;font-weight:400;margin-left:10px;">
									{{ $t("Button.__showMore")}}
								</span>

							</h4>
						</v-toolbar-title>
						<v-divider class="mx-4" inset vertical></v-divider>
						<v-spacer></v-spacer>

					</v-toolbar>
				</template>

				<template v-slot:item.transactionTypeText="{ item }">

					<v-chip>
						{{ item.transactionTypeText  }}
					</v-chip>
				</template>

				<template v-slot:item.currentAmount="{ item }">
					{{ item.currentAmount | amountDisplay }}
				</template>
				<template v-slot:item.updateAmount="{ item }">

					<span :class="item.updateAmount < 0 ? 'minus-red' : ''">
						{{ item.updateAmount | amountDisplay }}
					</span>

				</template>

				<template v-slot:item.afterAmount="{ item }">
					{{ item.afterAmount | amountDisplay }}
				</template>

				<template v-slot:item.createTime="{ item }">
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</template>
			</v-data-table>

		</v-card> -->

		<!-- 手動加值 -->
		<v-dialog v-model="isShowWalletDeposit" max-width="600px">
			<v-card>
				<v-card-title class="grey darken-2">
					{{ $t('Button.__ManualDepositWallet') }}
				</v-card-title>
				<v-spacer></v-spacer>

				<v-row style="padding: 10px 50px">
					<v-col cols="12" sm="12" md="12">
						<div class="common-info-label">{{ $t('TableHeader.__playerAccount') }}</div>
						<div class="common-info-value width-adjust">
							{{ playerDetail.playerName }}
							<span style="color: #9e9e9e; font-size: 10px; margin-left: 5px"> ({{ playerDetail.playerID }}) </span>
						</div>
					</v-col>

					<v-col cols="12" sm="12" md="12">
						<div class="common-info-label">{{ $t('TableHeader.__merchant') }}</div>
						<div class="common-info-value width-adjust">
							{{ playerDetail.merchantName }}
							<span style="color: #9e9e9e; font-size: 10px; margin-left: 5px"> ({{ playerDetail.merchantID }}) </span>
						</div>
					</v-col>

					<v-col cols="12" sm="12" md="12">
						<div class="common-info-label">{{ $t('TableHeader.__walletAmount') }}</div>
						<div class="common-info-value width-adjust">
							{{ playerDetail.walletAmount | amountDisplay }}
						</div>
					</v-col>

					<v-col cols="12" sm="12" md="12" v-if="!playerDetail.isTestAccountOrMerchant">
						<div class="">
							<v-autocomplete
								:items="merchantsDropdownItems"
								v-model="manualDepositForm.confirmMerchantID"
								chips
								clearable
								:label="'(防呆确认) ' + $t('Message.__pleaseSelectMerchant') + ''"
							>
							</v-autocomplete>
						</div>
					</v-col>

					<v-col cols="12" sm="12" md="12" style="margin-top: 5px" v-if="!playerDetail.isTestAccountOrMerchant">
						<v-text-field v-model="manualDepositForm.confirmPlayerName" :label="'(防呆确认) 請輸入玩家帳號'" clearable> </v-text-field>
					</v-col>

					<v-col cols="12" sm="12" md="12" style="margin-top: 5px">
						<v-text-field ref="depositWalletAmountText" v-model="manualDepositForm.amount" :label="'加值金額'" clearable maxlength="7"> </v-text-field>
					</v-col>

					<v-col cols="12" sm="12" md="12" style="margin-top: 5px">
						<v-text-field v-model="manualDepositForm.memo" :label="'備註／原因（非必填）'" clearable maxlength="100"> </v-text-field>
					</v-col>

					<v-col cols="12" sm="12" md="12" style="margin-top: 5px">
						<v-text-field v-model="manualDepositForm.referenceNo" :label="'參考單號（非必填）'" clearable maxlength="50"> </v-text-field>
					</v-col>
				</v-row>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn text @click="closeAllDialog" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__close') }}
					</v-btn>

					<v-btn text @click="submitDepositWallet" class="btn yellow--text">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 修改備註 -->
		<v-dialog v-model="isShowUpdateMemo" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					修改備註
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text style="padding: 35px">
					<v-flex>
						<v-row>
							<v-text-field class="" v-model="playerDetail.playerName" disabled="disabled" :label="$t('TableHeader.__playerAccount')"> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="simpleUpdateForm.updatePlayerMemo.memo" :label="$t('TableHeader.__memo')" autofocus> </v-text-field>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions style="padding-bottom: 100px">
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveSimpleUpdate" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 批次修改RTP -->
		<v-dialog v-model="isShowBatchUpdateRTP" max-width="850px">
			<v-card>
				<v-card-title class="grey darken-2">
					批量修改RTP
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text style="padding: 35px">
					<v-flex>
						<v-row>
							<v-text-field class="" v-model="playerDetail.playerName" disabled="disabled" :label="$t('TableHeader.__playerAccount')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<div class="full-width">
								<v-autocomplete :items="gamesDropdownItems" v-model="batchUpdateRTPForm.gameIDs" multiple chips deletable-chips :label="$t('TableHeader.__game')"> </v-autocomplete>
							</div>

							<div class="full-width" style="text-align: right; margin-top: -5px">
								<span class="link-text-grey mr-2" @click="cancelSelectAllGame()"> 取消全選 </span>

								<span class="link-text-grey" @click="selectAllGame()"> 遊戲全選 </span>
							</div>

							<!-- <v-text-field class="" v-model="updateRTPTargetGame.gameName" disabled='disabled'
								:label="$t('TableHeader.__gameName')" autofocus>
							</v-text-field> -->
						</v-row>

						<v-row>
							<v-text-field v-show="batchUpdateRTPForm.isReset" disabled="disabled" :label="'RTP'" value="默認RTP"> </v-text-field>

							<v-text-field class="required" v-show="!batchUpdateRTPForm.isReset" v-model="batchUpdateRTPForm.RTP" :label="'RTP'" autofocus required clearable> </v-text-field>
						</v-row>

						<v-row>
							<v-checkbox v-model="batchUpdateRTPForm.isReset" :label="'使用遊戲默認RTP'"> </v-checkbox>
						</v-row>

						<v-row>
							<v-checkbox v-model="isAutoAddRTPEventForbiddenTag" :label="'標記禁止參加RTP活動標籤'"> </v-checkbox>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions style="padding-bottom: 100px">
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveBatchEditRTP" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 修改RTP -->
		<v-dialog v-model="isShowEditRTP" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					修改RTP
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="refresh">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex>
						<v-row>
							<v-text-field class="" v-model="playerDetail.playerName" disabled="disabled" :label="$t('TableHeader.__playerAccount')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="updateRTPTargetGame.gameVendorConfigID" disabled="disabled" :label="$t('TableHeader.__vendorConfig') + ' ID'" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="updateRTPTargetGame.gameVendorConfigName" disabled="disabled" :label="$t('TableHeader.__vendorConfig')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="updateRTPTargetGame.gameName" disabled="disabled" :label="$t('TableHeader.__gameName')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field v-show="updateRTPForm.isReset" disabled="disabled" :label="'RTP'" value="默認RTP"> </v-text-field>

							<v-text-field class="required" v-show="!updateRTPForm.isReset" v-model="updateRTPForm.RTP" :label="'RTP'" autofocus required clearable> </v-text-field>
						</v-row>

						<v-row>
							<v-checkbox v-model="updateRTPForm.isReset" :label="'使用遊戲默認RTP'"> </v-checkbox>
						</v-row>

						<v-row>
							<v-checkbox v-model="isAutoAddRTPEventForbiddenTag" :label="'標記禁止參加RTP活動標籤'"> </v-checkbox>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveEditRTP" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 新增Tag -->
		<v-dialog v-model="isShowCreateTagDialog" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					{{ $t('Button.__addTag') }}
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex>
						<v-row>
							<v-text-field class="" v-model="playerDetail.playerName" disabled="disabled" :label="$t('TableHeader.__playerAccount')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-autocomplete clearable :items="playerTagsOfAllowManualAddDropdownItems" class="required" chips v-model="updatePlayerTagForm.tagID" :label="$t('TableHeader.__tag')">
							</v-autocomplete>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveCreateTag" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 確認視窗 -->
		<v-dialog v-model="isShowConfirmRemoveRoomLock" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2"> {{ $t('Message.__isConfirmOperate') }}？ </v-card-title>
				<v-spacer></v-spacer>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn text @click="isShowConfirmRemoveRoomLock = false" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__close') }}
					</v-btn>

					<v-btn text @click="removeRoomLock" class="btn yellow--text">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 確認w88轉帳 -->
		<v-dialog v-model="isShowW88TransferFailCheckDialog" max-width="1100px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					{{ $t('Menu.MembersManagementTransfer') }}

					<div class="top-right-item" @click="closeAllDialog">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div class="text-c mt-2" style="min-height: 400px">
						<div v-if="playerW88TransferFailCheck.isValidateOK">
							<v-icon style="font-size: 64px" color="green">check</v-icon>

							<div>
								{{ $t('Message.__NoAbnormality') }}
							</div>
						</div>

						<div v-else class="minus-red">
							<v-icon style="font-size: 64px" color="red">close</v-icon>
							<div>
								{{ $t('Message.__pleaseContactTechTeam') }}
							</div>
						</div>

						<div class="mt-4">
							<v-divider></v-divider>

							<v-data-table
								:items="playerW88TransferFailCheck.transferRecords"
								:headers="headersOfW88Transfer"
								hide-default-footer
								:footer-props="{ 'items-per-page-options': [50, 100] }"
							>
								<template v-slot:item.createTime="{ item }">
									<div>
										{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
									</div>
								</template>

								<template v-slot:item.updateAmount="{ item }">
									<div>
										{{ item.updateAmount | amountDisplay }}
									</div>
								</template>

								<template v-slot:item.playerName="{ item }">
									<div>
										{{ item.playerName }}
									</div>
									<div class="sub-title-text">
										{{ item.playerID }}
									</div>
								</template>

								<template v-slot:item.transactionNo="{ item }">
									<div class="f13">
										{{ item.transactionNo }}
									</div>
									<div class="sub-title-text">
										{{ item.platformOrderNo }}
									</div>
								</template>
							</v-data-table>
						</div>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<v-dialog v-model="dialogLoginDetailPopup">
			<DetailLoginRecord :playerID="playerDetail.playerID" @close="dialogLoginDetailPopup = false" />
		</v-dialog>

		<v-dialog v-model="dialogDeletePlayerPopup" max-width="500px">
			<DeletePlayersPopup v-if="dialogDeletePlayerPopup" :propsData="deletePlayerIDs" :status="playerDetail.status" @update-success="updateSuccess" @close="dialogDeletePlayerPopup = false">
			</DeletePlayersPopup>
		</v-dialog>

		<GameLaunchInternalTest ref="gameLaunchTest"></GameLaunchInternalTest>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import { WalletMode } from '@/models/status/merchant';
import { PlayerTypeEnum } from '@/models/status/players';
import { GameVendorType, Status } from '@/models/status/common';
import * as Model from '@/models/interfaces/members-management/players';
import GameLaunchInternalTest from '@/components/system-setting/GameLaunchInternalTest.vue';
import PlayersApi from '@/api/members-management/players';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin, replaceToNoneIOS8601 } from '@/utilities/datetime-format';
import DeletePlayersPopup from '@/components/members-management/DeletePlayersPopup.vue';
import DetailLoginRecord from '@/components/members-management/DetailLoginRecord.vue';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DropdownApi from '@/api/commons/dropdown';
import SimpleUpdateApi from '@/api/commons/simpleUpdate';
import * as EventModel from '@/models/interfaces/system-setting/events';
import EventsApi from '@/api/system-setting/events';
import { commonMethodMixin } from '@/utilities/common-methods';
import * as GameModel from '@/models/interfaces/system-setting/gameList';

const playersModule = namespace('Players');
import * as PermissionModel from '@/models/interfaces/commons/permission';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
@Component({ components: { DeletePlayersPopup, DetailLoginRecord, GameLaunchInternalTest }, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class Detail extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;
	@playersModule.State('playerID') playerID!: number;

	isShowBatchUpdateRTP: boolean = false;
	isShowCreateTagDialog: boolean = false;
	isShowWalletDeposit: boolean = false;
	isShowConfirmRemoveRoomLock: boolean = false;
	isShowEditRTP: boolean = false;
	isShowUpdateMemo: boolean = false;
	isShowW88TransferFailCheckDialog: boolean = false;

	isAutoAddRTPEventForbiddenTag: boolean = false;
	dialogDeletePlayerPopup: boolean = false;
	dialogLoginDetailPopup: boolean = false;
	walletMode: typeof WalletMode = WalletMode;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameterSize500 = new DropdownModel.DataTablePagingParameterSize500();
	itemsPerPage: number = 100;
	extraFilterMerchantID: number | null = 0;
	playerFishWalletBalance: string = '-';
	playerType: typeof PlayerTypeEnum = PlayerTypeEnum;
	status: typeof Status = Status;
	playerDetail: Model.IPlayerDetail = {} as Model.IPlayerDetail;
	searchPlayerID: string | null = null;
	searchMoneyRecords: Model.IMoneyRecordReponse[] = [];

	playerW88TransferFailCheck: any = {};

	updateRTPTargetGame: Model.IPlayerGameSummary = {} as Model.IPlayerGameSummary;
	updateRTPForm: EventModel.IPlayerRTPUpdateRequest = {
		isReset: false,
	} as EventModel.IPlayerRTPUpdateRequest;

	batchUpdateRTPForm: EventModel.IPlayerRTPUpdateRequest = {
		isReset: false,
	} as EventModel.IPlayerRTPUpdateRequest;

	updatePlayerTagForm: Model.IPlayerTagUpdate = {} as Model.IPlayerTagUpdate;
	manualDepositForm: Model.IInternalTestAccountPlusWalletRequest = {} as Model.IInternalTestAccountPlusWalletRequest;

	playerTagsDropdownItems: DropdownModel.IDropdownItems[] = [];
	playerTagsOfAllowManualAddDropdownItems: DropdownModel.IDropdownItems[] = [];

	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];

	simpleUpdateForm: DropdownModel.ISimpleUpdateReqeust = {
		updatePlayerMemo: {},
	} as DropdownModel.ISimpleUpdateReqeust;

	get deletePlayerIDs() {
		return [this.playerDetail.playerID];
	}

	get hasPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementDetail_Lock) >= 0;
	}

	get hasPermissionGameDetail() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagementRecord) >= 0;
	}

	get hasWatchGameSummaryPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementDetail_GameSummary) >= 0;
	}

	get hasMoneyRecordPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementMoneyRecord) >= 0;
	}

	get hasAdvanceUpdatePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementDetail_AdvanceUpdate) >= 0;
	}

	get hasSuperPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get hasCopyAssignIDPermission() {
		return (
			this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0 ||
			this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_PGS_CustomerService) >= 0
		);
	}

	get hasLaunchSimulatePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementDetail_AllowSimulateGameLaunch) >= 0;
	}

	get copyTextOfPlayerInfo() {
		return this.playerDetail.copyTextForCS;
	}

	get headers() {
		return [
			{ text: this.$t('TableHeader.__gameID'), value: 'gameID' },
			{ text: this.$t('TableHeader.__gameName'), value: 'gameName' },
			{ text: this.$t('TableHeader.__vendorConfig'), value: 'gameVendorConfigName' },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', align: 'end' },
			{ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end' },
			{ text: this.$t('TableHeader.__validAmount'), value: 'validAmount', align: 'end' },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', align: 'end' },
			{ text: this.$t('TableHeader.__score'), value: 'score', align: 'end' },
			{ text: 'RTP', value: 'rtp', align: 'end' },

			{ text: 'RTP調整', value: 'rtpConfigValue', align: 'center' },
		];
	}

	get moneyRecordHeaders() {
		return [
			{ text: this.$t('TableHeader.__time'), value: 'createTime', sortable: false },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName', sortable: false },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID', sortable: false },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName', sortable: false },
			{ text: this.$t('TableHeader.__transactionType'), value: 'transactionTypeText', sortable: false },
			{ text: this.$t('TableHeader.__game'), value: 'gameName', sortable: false },
			{ text: this.$t('TableHeader.__updateAmount'), value: 'updateAmount', align: 'end', sortable: false },
			{ text: this.$t('TableHeader.__beforeAmount'), value: 'currentAmount', align: 'end', sortable: false },
			{ text: this.$t('TableHeader.__afterAmount'), value: 'afterAmount', align: 'end', sortable: false },
		];
	}

	get dailySummaryHeaders() {
		return [
			{ text: this.$t('TableHeader.__time'), value: 'dateText', sortable: false },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', align: 'end' },
			{ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end' },
			{ text: this.$t('TableHeader.__validAmount'), value: 'validAmount', align: 'end' },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', align: 'end' },
			{ text: this.$t('TableHeader.__winLossAmount'), value: 'winLossAmount', align: 'end' },
			{ text: 'RTP', value: 'rtp', align: 'end' },
		];
	}
	get playerBehaviorEventHeaders() {
		return [
			{ text: this.$t('TableHeader.__time'), value: 'createTime', sortable: false },
			{ text: this.$t('TableHeader.__memo'), value: 'memo', align: 'end' },
		];
	}

	get headersOfW88Transfer() {
		return [
			{ text: this.$t('TableHeader.__time'), value: 'createTime', sortable: false },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName', align: 'end' },
			{ text: this.$t('TableHeader.__orderNo'), value: 'transactionNo', align: 'end' },
			{ text: this.$t('TableHeader.__amount'), value: 'updateAmount', align: 'end' },
			{ text: this.$t('TableHeader.__memo'), value: 'memo', align: 'end' },
			{ text: this.$t('TableHeader.__type'), value: 'typeText', align: 'end' },
		];
	}

	get hasPlayerData() {
		return this.playerDetail.playerID && this.playerDetail.playerID > 0;
	}

	searchText: string = '';
	@Action('Players/setWalletPlayerID') private setWalletPlayerID!: any;
	@Action('Players/setGameID') private setGameID!: any;
	@Action('Players/setGamePlayerID') private setGamePlayerID!: any;

	@Watch('playerID')
	onPlayerIDChange(newVal: number) {
		this.playerDetail = {} as Model.IPlayerDetail;
		this.playerDetail.playerID = newVal;
		this.searchPlayerID = newVal.toString();

		this.search();
	}

	@Watch('defaultLangTimes')
	onDefaultLangChanged() {
		if (this.playerDetail.playerID) {
			this.search();
		}
	}

	refresh() {
		this.search();
	}

	search() {
		if (this.searchPlayerID == null || this.searchPlayerID.length <= 0) {
			this.searchPlayerID = this.playerDetail.playerID.toString();
		}

		this.searchPlayer();
	}

	showW88TransferFailCheckDialog(playerID: number) {
		this.isShowW88TransferFailCheckDialog = true;
		EventBus.$emit('show_mask', null);

		PlayersApi.playerW88TransferFailCheck(playerID)
			.then((res) => {
				this.playerW88TransferFailCheck = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	loadPlayerData(playerID: number) {
		PlayersApi.getPlayer(playerID).then((res) => {
			this.playerDetail = res.data;
			this.searchPlayerID = this.playerDetail.playerID.toString();
			this.loadExtraDataAfterSearchPlayer();
		});
	}

	searchPlayer() {
		this.playerDetail = {} as Model.IPlayerDetail;
		this.searchMoneyRecords = [];

		const myFilter = {} as Model.IPlayerDetailSearchRequest;
		myFilter.playerNameOrID = this.searchPlayerID;
		myFilter.merchantID = this.extraFilterMerchantID;

		EventBus.$emit('show_mask', null);

		PlayersApi.getPlayerDetailByFilter(myFilter)
			.then((res) => {
				this.playerDetail = res.data;
				this.loadExtraDataAfterSearchPlayer();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	loadExtraDataAfterSearchPlayer() {
		if (this.playerDetail.isFishWalletVisible) {
			this.fetchFishWallet();
		}

		this.refreshMoneyRecords();
	}

	selectAllGame() {
		this.batchUpdateRTPForm.gameIDs = [];
		this.gamesDropdownItems.forEach((item) => {
			this.batchUpdateRTPForm.gameIDs.push(Number(item.valueInt));
		});

		this.reloadBatchEditComponent();
	}

	cancelSelectAllGame() {
		this.batchUpdateRTPForm.gameIDs = [];
		this.reloadBatchEditComponent();
	}

	reloadBatchEditComponent() {
		// 畫面有時候沒刷新怪怪的
		this.isShowBatchUpdateRTP = false;
		this.isShowBatchUpdateRTP = true;
	}

	openSingleWalletLogHistoryPage() {
		EventBus.$emit('close_tab', '/system-logs/SingleWalletPlatformReqeustLog');
		const query = '?playerID=' + this.playerDetail.playerID.toString() + '&merchantID=' + this.playerDetail.merchantID.toString();
		this.$router.push({ path: '/system-logs/SingleWalletPlatformReqeustLog' + query });
	}

	refreshMoneyRecords() {
		const request: Model.IMoneyRecordRequest = {} as Model.IMoneyRecordRequest;
		request.playerID = this.playerDetail.playerID;
		request.createTimeBegin = startEndDate(daysAgo(-7));
		request.pageIndex = 1;
		request.pageSize = 10;

		if (!this.hasMoneyRecordPermission) {
			return;
		}

		// ps. 這邊只是個擴充資訊 速度有點慢 不要加遮罩

		// 2024/08 速度慢 還是別顯示這個了
		// PlayersApi.getMoneyecord(request)
		// 	.then((res) => {
		// 		this.searchMoneyRecords = res.data.items;
		// 	})
		// 	.finally(() => {});
	}

	openGameDetail(playerID: number, item: Model.IPlayerGameSummary) {
		// 程式碼的順序 會影響queryString吃不吃的到的問題
		this.$router.push({ path: '/games-management/record', query: { beginTS: item.lastBetTimeTS.toString() } });

		// 如果重複點某個用戶資料 第二次就不會有changed事件了
		this.setGameID(null);
		this.setGamePlayerID(null);

		// 沒稍微延遲下也會出問題
		setTimeout(() => {
			this.setGameID(item.gameID);
			this.setGamePlayerID(playerID);
		}, 500);
	}

	// 金幣紀錄還沒有
	walletDetail(playerID: number) {
		this.setWalletPlayerID(playerID);
		this.$router.push({ path: '/members-management/money-record' });
	}

	updateSuccess() {
		this.search();
		EventBus.$emit(`successfully-modified`);
		this.dialogDeletePlayerPopup = false;
	}

	removeRoomLock() {
		PlayersApi.removeRoomLock(Number(this.playerDetail.playerID)).then((res) => {
			EventBus.$emit(`successfully-modified`);
			this.isShowConfirmRemoveRoomLock = false;
		});
	}

	showGameLaunchDialog(item: Model.IPlayerDetail) {
		const targetRef: any = this.$refs.gameLaunchTest;

		const model: GameModel.IGameLaunchPreview = {} as GameModel.IGameLaunchPreview;
		model.gameID = 0;

		console.log(targetRef);

		targetRef.showGameLaunchDialog(model);
		targetRef.setPlayerListToOneItem(item.playerID, item.playerName);
	}

	showCreateTagDialog() {
		this.isShowCreateTagDialog = true;
		this.updatePlayerTagForm.playerID = this.playerDetail.playerID;
		this.updatePlayerTagForm.tagID = 0;
	}

	saveCreateTag() {
		this.isShowCreateTagDialog = true;
		EventBus.$emit('show_mask', null);

		PlayersApi.addTag(this.updatePlayerTagForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllDialog();
				this.search();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	deletePlayerTag(tagID: number, tagName: string) {
		const msg = this.$t('Message.__isConfirmDelete') + '? (' + tagName + ')';

		if (confirm(msg)) {
			this.updatePlayerTagForm.playerID = this.playerDetail.playerID;
			this.updatePlayerTagForm.tagID = tagID;

			EventBus.$emit('show_mask', null);

			PlayersApi.deleteTag(this.updatePlayerTagForm)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.search();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	closeAllDialog() {
		this.isShowWalletDeposit = false;
		this.isShowCreateTagDialog = false;
		this.isShowConfirmRemoveRoomLock = false;
		this.isShowEditRTP = false;
		this.isShowCreateTagDialog = false;
		this.isShowBatchUpdateRTP = false;
		this.isShowUpdateMemo = false;
		this.isShowW88TransferFailCheckDialog = false;
	}

	closeAllModal() {
		this.closeAllDialog();
	}

	isSingleWalletMode() {
		return this.playerDetail.walletMode === this.walletMode.SingleWalletMode;
	}

	showUpdateMemo() {
		this.simpleUpdateForm.updatePlayerMemo.playerID = this.playerDetail.playerID;
		this.simpleUpdateForm.updatePlayerMemo.memo = this.playerDetail.memo;

		this.isShowUpdateMemo = true;
		this.simpleUpdateForm.callback = this.afterSimpleUpdate;
	}

	afterSimpleUpdate() {
		this.closeAllDialog();
		this.search();
	}

	showCopyTextDialog(myCopyText: string) {
		const myConfig: any = {
			copyText: myCopyText,
		};

		EventBus.$emit(`globalShowCopyTextDialog`, myConfig);
	}

	showDepositWallet() {
		this.isShowWalletDeposit = true;

		this.manualDepositForm = {} as Model.IInternalTestAccountPlusWalletRequest;

		if (this.playerDetail.isTestAccountOrMerchant) {
			this.manualDepositForm.confirmMerchantID = this.playerDetail.merchantID;
			this.manualDepositForm.confirmPlayerName = this.playerDetail.playerName;
			this.manualDepositForm.amount = 5000;
		} else {
			this.manualDepositForm.confirmMerchantID = 0;
		}

		setTimeout(() => {
			const input: any = this.$refs.depositWalletAmountText;
			input.focus();
		}, 300);
	}

	showBatchUpdateRTP() {
		this.isShowBatchUpdateRTP = true;

		this.batchUpdateRTPForm.isReset = false;
		this.batchUpdateRTPForm.playerID = this.playerDetail.playerID;
		this.batchUpdateRTPForm.gameIDs = [];
		this.batchUpdateRTPForm.RTP = 95;
		this.isAutoAddRTPEventForbiddenTag = false;
	}

	showEditRTPPopup(item: Model.IPlayerGameSummary) {
		this.isShowEditRTP = true;
		this.updateRTPTargetGame = item;

		this.updateRTPForm.isReset = false;
		this.updateRTPForm.gameID = item.gameID;
		this.updateRTPForm.playerID = this.playerDetail.playerID;
		this.updateRTPForm.gameIDs = [];

		// @ts-ignore
		if (item.rtpConfigValue === null) {
			this.updateRTPForm.isReset = true;
			this.updateRTPForm.RTP = 95;
		} else {
			this.updateRTPForm.RTP = item.rtpConfigValue;
		}
	}

	saveEditRTP() {
		EventBus.$emit('show_mask', null);

		EventsApi.updatePlayerRTP(this.updateRTPForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllDialog();

				// 若有勾選自動標記Tag
				if (this.isAutoAddRTPEventForbiddenTag) {
					this.addRTPEventForbiddenTag();
				} else {
					this.search();
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	saveBatchEditRTP() {
		EventBus.$emit('show_mask', null);

		EventsApi.batchUpdatePlayerRTP(this.batchUpdateRTPForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllDialog();

				// 若有勾選自動標記Tag
				if (this.isAutoAddRTPEventForbiddenTag) {
					this.addRTPEventForbiddenTag();
				} else {
					this.search();
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showRTPChangeLog() {
		const myFilter: any = {
			querySqlID: 900071,
			playerID: this.playerDetail.playerID,
		};

		const config: any = {
			maxWidth: 1100,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, config);
	}

	showExternalLogs() {
		const myFilter: any = {
			querySqlID: 900344,
			playerID: this.playerDetail.playerID,
		};

		const config: any = {
			maxWidth: 1100,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, config);
	}

	showSingleWalletMerchantToken() {
		const myFilter: any = {
			querySqlID: 900341,
			playerID: this.playerDetail.playerID,
		};

		const config: any = {
			maxWidth: 1250,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, config);
	}

	showVendorSessionToken() {
		const myFilter: any = {
			querySqlID: 900342,
			playerID: this.playerDetail.playerID,
		};

		const config: any = {
			maxWidth: 1250,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, config);
	}

	copySuccessHint() {
		EventBus.$emit(`successfully-copy`);
	}

	showLobbyToken() {
		const myFilter: any = {
			querySqlID: 900343,
			playerID: this.playerDetail.playerID,
		};

		const config: any = {
			maxWidth: 1250,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, config);
	}

	addRTPEventForbiddenTag() {
		const payload = {} as Model.IPlayerTagUpdate;

		payload.playerID = this.playerDetail.playerID;
		payload.tagID = 4301;

		PlayersApi.addTag(payload).finally(() => {
			this.search();
		});
	}

	fetchFishWallet() {
		this.playerFishWalletBalance = '-';

		PlayersApi.fetchFishWallet(this.playerDetail.playerID)
			.then((res) => {
				this.playerFishWalletBalance = res.data.balance.toString();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	submitDepositWallet() {
		EventBus.$emit('show_mask', null);

		this.manualDepositForm.playerID = Number(this.playerDetail.playerID);

		PlayersApi.internalTestAccountDeposit(this.manualDepositForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.loadPlayerData(this.playerDetail.playerID);
				this.closeAllDialog();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	mounted() {
		const targetPlayerID = this.$route.query.playerID;

		DropdownApi.getPlayerTags().then((res) => {
			this.playerTagsDropdownItems = res;
		});

		DropdownApi.getAllowManualAddPlayerTags().then((res) => {
			this.playerTagsOfAllowManualAddDropdownItems = res;
		});

		DropdownApi.getGamesFilterGameVendor(GameVendorType.AGG).then((res) => {
			this.gamesDropdownItems = res;
		});

		DropdownApi.getMerchants().then((res) => {
			this.merchantsDropdownItems = res;
		});

		if (targetPlayerID) {
			this.searchPlayerID = String(targetPlayerID);
			this.searchPlayer();
			return;
		}

		if (!this.playerID) {
			return;
		}

		this.loadPlayerData(this.playerID);
	}
}
</script>

<style scoped>
.v-list-item__subtitle {
	padding: 3px;
	margin-bottom: 7px;
}

.width-adjust {
	width: 66%;
}

#playerMoneyRecords .v-data-footer {
	display: none;
}

.footer-total-row p {
	padding-top: 15px;
}

.full-width .v-chip__content {
	font-size: 13px;
}
.myButtonRows {
	display: inline-block;
}

@media only screen and (max-width: 480px) {
	.myButtonRows {
		display: inline-block;
		position: absolute;
		top: 75px;
	}
	.myButtonRows button {
		margin-bottom: 5px;
	}

	.my_toolbar_height {
		height: 220px !important;
	}
}
</style>
