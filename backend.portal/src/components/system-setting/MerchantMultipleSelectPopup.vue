<template>

	<v-card>

		<v-card-title>
			<div>
				請於下方貼上商戶
			</div>

			<div class="top-right-item" @click="closeDialog">
				<v-btn icon dark>
					<v-icon>close</v-icon>
				</v-btn>
			</div>

		</v-card-title>

		<v-card-actions>

			<div style="min-height:400px;width: 100%;">

				<v-row>

					<v-col cols="12">
						<div style="">
							<v-textarea v-model="rawText" :label="'請貼上商戶ID (支援幾種格式)'" class='myTextfield'>
							</v-textarea>
						</div>

						<div>

							<v-btn color="pink lighten-3" text class="btn search"
								@click.stop="parseMerchantIDAndPassToParentComponent()">
								<v-icon left>clear</v-icon>
								送出
							</v-btn>&#9;

						</div>

					</v-col>

				</v-row>

			</div>
		</v-card-actions>

	</v-card>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/gameList';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import GameListApi from '@/api/system-setting/gameList';
import DropdownApi from '@/api/commons/dropdown';
import MerchantsApi from '@/api/system-setting/merchants';
import EventsApi from '@/api/system-setting/events';
import * as ResponseModel from '@/models/interfaces/commons/response';
import InternalTestApi from '@/api/commons/internalTest';
import { commonMethodMixin } from '@/utilities/common-methods';
import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import * as MerchantCreditModel from '@/models/interfaces/agent/merchantCredit';
import { State, Action, Getter, namespace } from 'vuex-class';

const authModule = namespace('Auth');
const playersModule = namespace('Players');

import * as Permission from '@/models/status/permission';
@Component({ components: {}, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class MerchantMultipleSelectPopup extends Vue {
	isShowGameLaunchDialog: boolean = false;
	rawText: string = '';

	@Action('Players/setPlayerID') private statePlayerID!: any;

	parseMerchantIDAndPassToParentComponent() {
		EventBus.$emit('show_mask', null);
		MerchantsApi.parseMerchantIDsByRawText(this.rawText)
			.then((res) => {
				this.$emit('setParentMerchantIDs', res.data.merchantIDs);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeDialog() {
		this.$emit('parentCloseMultipleMerchantSelect');
	}

	mounted() {}
}
</script>

<style >
.myTextfield {
	background: #333;
	padding: 2px;
}
</style>