<template>
	<div>

		<v-dialog v-model="isShowGameMerchants" max-width="700px" scrollable>

			<v-card>
				<v-card-title class="grey darken-2">

					{{$t("TableHeader.__assignMerchantTotalCount")}} ({{searchResult.gameName}})

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="close">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="padding:30px;">
					<v-flex>
						<v-row>
							<v-text-field v-model="searchResult.gameID" :label="$t('TableHeader.__gameID')"
								disabled="disabled">
							</v-text-field>
						</v-row>

						<v-row>
							<v-text-field v-model="searchResult.gameName" :label="$t('TableHeader.__displayName')"
								disabled="disabled">
							</v-text-field>
						</v-row>

						<v-row>

							<table class="myTable">

								<tr>
									<th class="text-l">
										{{$t("TableHeader.__merchant")}}
									</th>
									<th class="text-l">
										{{$t("TableHeader.__vendorConfig")}}
									</th>
									<th class="text-c">
										{{$t("TableHeader.__convertRate")}}
									</th>
								</tr>

								<tbody>
									<tr v-for="(item , index) in searchResult.merchants" :key="index">

										<td>
											<v-chip>
												{{item.merchantID}}｜ {{item.merchantName}}
											</v-chip>
										</td>

										<td :title="item.vendorConfigID">
											{{item.vendorConfigName}}
										</td>

										<td class="text-c">
											{{item.convertRate}}
										</td>

									</tr>
								</tbody>

							</table>

						</v-row>

					</v-flex>
				</v-card-text>

			</v-card>

		</v-dialog>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/gameList';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import GameListApi from '@/api/system-setting/gameList';
import DropdownApi from '@/api/commons/dropdown';
import InternalTestApi from '@/api/commons/internalTest';
import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { commonMethodMixin } from '@/utilities/common-methods';
import { State, Action, Getter, namespace } from 'vuex-class';

const authModule = namespace('Auth');
@Component({ components: { NotificationWarning }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class GameMerchantInfoDialog extends Vue {
	isShowGameMerchants = false;
	searchResult: Model.IGameMerchantInfo = {} as Model.IGameMerchantInfo;

	searchByGameID(gameID: number) {
		this.isShowGameMerchants = true;

		EventBus.$emit('show_mask', null);

		GameListApi.getGameMerchantInfo(gameID)
			.then((res) => {
				this.searchResult = res.data;
			})
			.finally(() => {
				setTimeout(() => {
					EventBus.$emit('hide_mask', null);
				}, 500);
			});
	}

	closeAllModal() {
		this.isShowGameMerchants = false;
	}

	mounted() {}
}
</script>

<style>
.myTable td,
.myTable th {
	padding: 5px;
}
</style>