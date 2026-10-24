<template>

	<v-dialog v-if="show" v-model="show" max-width="1250px">
		<v-card v-show="isMounted" class="mx-auto">

			<v-card-title class="grey darken-2">

				<span>
					<b>
						{{$t("Button.__showDetail")}}
					</b>
				</span>
				<v-spacer></v-spacer>

				<div class="top-right-item">
					<div class="pointer mr-3" @click="close">
						<v-icon>close</v-icon>
					</div>
				</div>

			</v-card-title>
			<v-card-text>
				<v-flex>
					<!-- <v-row>
						<v-col cols="12" sm="11" md="11"></v-col>
						<v-spacer></v-spacer>
						<v-col>
							<v-btn icon @click="close">
								<v-icon>mdi-close</v-icon>
							</v-btn>
						</v-col>
						</v-row>-->
					<v-row v-show="$route.params.perGameLogID">

						<v-col cols="12" sm="1" md="1">
						</v-col>

						<v-col cols="12" sm="4" md="4">

							<div>
								<div class="common-info-label">
									{{$t('TableHeader.__perGameLogID')}}
								</div>
								<div class="common-info-value">
									{{ result.perGameLogID ? result.perGameLogID : '---' }}
								</div>
							</div>

							<div>
								<div class="common-info-label">
									{{$t('TableHeader.__server')}}
								</div>
								<div class="common-info-value">
									{{ result.serverName ? result.serverName : '---' }}
								</div>
							</div>

							<div>
								<div class="common-info-label">
									{{$t('TableHeader.__taxAmount')}}
								</div>
								<div class="common-info-value">
									{{result.taxAmount}}
								</div>
							</div>

						</v-col>

						<v-col cols="12" sm="1" md="1">
						</v-col>

						<v-col cols="12" sm="4" md="4">

							<div>
								<div class="common-info-label">
									{{$t('TableHeader.__serverID')}}
								</div>
								<div class="common-info-value">
									{{ result.serverID ? result.serverID : '---' }}
								</div>
							</div>

							<div>
								<div class="common-info-label">
									{{$t('TableHeader.__game')}}
								</div>
								<div class="common-info-value">
									{{ result.gameName ? result.gameName : '---' }}
								</div>
							</div>

							<div>
								<div class="common-info-label">
									{{$t('TableHeader.__time')}}
								</div>
								<div class="common-info-value">
									{{ result.beginTime | timeWithAddTimezone(userProfile.timeZone) }} - {{
											result.endTime | timeWithAddTimezone(userProfile.timeZone)
											}}

								</div>
							</div>

						</v-col>

						<v-col cols="12" sm="2" md="2">
						</v-col>

						<v-divider></v-divider>

						<div style="clear:both"></div>

						<v-col cols="12" sm="1" md="1">
						</v-col>
						<!-- </v-row>
							<v-row v-show="$route.params.perGameLogID">-->
						<v-col cols="12" sm="10" md="10">

							<div>

								<b>
									{{$t('TableHeader.__gameData')}}
								</b>
							</div>

							<div class="gameDataWrapper" style="">
								<p v-html="result.gameData" style="max-width:850px;min-width:500px;"></p>
							</div>

						</v-col>
					</v-row>

					<v-divider></v-divider>

					<!-- 游戏数据 -->
					<!-- <p v-html="result.gameData"></p> -->
					<v-data-table :items="result.playerDetails" :headers="headers" id="table">
						<template v-slot:item.betAmount="{ item }">
							{{ item.betAmount | amountDisplay }}
						</template>

						<template v-slot:item.payoutAmount="{ item }">
							{{ item.payoutAmount | amountDisplay }}
						</template>

						<template v-slot:item.updateAmount="{ item }">
							{{ item.updateAmount | amountDisplay }}
						</template>

						<template v-slot:item.taxAmount="{ item }">
							{{ item.taxAmount | amountDisplay }}
						</template>

						<template v-slot:item.gameTime="{ item }">
							{{ item.gameTime | timeWithAddTimezone(userProfile.timeZone) }}
						</template>
					</v-data-table>
				</v-flex>
			</v-card-text>
		</v-card>
		<v-overlay :absolute="true" :value="true" v-show="!isMounted">
			<!-- <v-progress-circular class="loading" :size="70" :width="7" indeterminate></v-progress-circular> -->
			<div class="loader"></div>
		</v-overlay>
	</v-dialog>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as Model from '@/models/interfaces/game-management/per-game';
import GameManagementApi from '@/api/game-management/per-game';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin] })
export default class PerGameLogDetail extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IProfileResponse;
	show: boolean = false;
	isMounted: boolean = false;
	get headers() {
		return [
			{ text: this.$t('TableHeader.__time'), value: 'gameTime' },
			{ text: this.$t('TableHeader.__betRecordNo'), value: 'transactionNo' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName' },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID' },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName' },
			{ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end' },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', align: 'end' },
			{ text: this.$t('TableHeader.__updateAmount'), value: 'updateAmount', align: 'end' },
			{ text: this.$t('TableHeader.__taxAmount'), value: 'taxAmount', align: 'end' },
		];
	}
	result: Model.IDetailResponse = {} as Model.IDetailResponse;
	@Watch('show')
	onShowChange(newValue: number) {
		if (!newValue) {
			this.close();
		}
		this.isMounted = false;
		this.search();
	}

	@Watch('$route.params.perGameLogID', { immediate: true, deep: true })
	onPerGameLogIDChange(newValue: number) {
		this.show = newValue ? true : false;
	}

	search() {
		if (!this.$route.params.perGameLogID) {
			return;
		}
		GameManagementApi.getFullInfo(Number(this.$route.params.perGameLogID)).then((res) => {
			this.result = res.data;
			this.result.playerDetails = res.data.playerDetails;
			this.isMounted = true;
		});
	}

	close() {
		this.$router.push({ path: `/games-management/record` });
	}
}
</script>
<style lang="scss" scoped>
p {
	white-space: pre;
	color: rgba(255, 255, 255, 0.7);
	font-size: 0.875rem;
}
.close {
	transition: all 300ms;
}
.close:hover {
	transform: rotate(360deg);
}

.v-list-item__subtitle {
	padding: 5px;
}

.gameDataWrapper {
	border: dashed 1px #888;
	padding: 0px 15px;
	margin-top: 10px;
	margin-bottom: 15px;
	border-radius: 5px;
}
</style>
