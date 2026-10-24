<template>
	<v-card>
		<v-card-title class="grey darken-2">
			{{propsData.serverID === -1? `${$t('Popup.__create')} ${$t('TableHeader.__server')}`:`${$t('Popup.__update')} ${$t('TableHeader.__server')} # ${propsData.serverID}`}}
		</v-card-title>
		<v-spacer></v-spacer>
		<v-card-text>
			<v-flex>
				<v-row>
					<v-autocomplete attach chips class="required" :label="$t('TableHeader.__game')"
						v-model="editedItem.gameID" :items="games"></v-autocomplete>
				</v-row>

				<v-row>
					<v-text-field class="required" v-model="editedItem.serverName"
						:label="$t('TableHeader.__serverName')"
						:rules="[value => !!value || $t('Validation.__requiredRules')]" required clearable>
					</v-text-field>
				</v-row>

				<v-row>
					<v-text-field type="number" :disabled="propsData.serverID !== -1" v-model="serverID"
						:label="$t('TableHeader.__serverID')"
						:rules="[value => (!!value && Number(value) > 0) || $t('Validation.__numberRules')]"
						required class="number-text-field required" prepend-inner-icon="mdi-minus"
						append-icon="mdi-plus" @click:prepend-inner="serverIDMinusOrPlus(false)"
						@click:append="serverIDMinusOrPlus(true)"></v-text-field>
				</v-row>
				<v-row>
					<v-text-field type="number" v-model="flagLevel" :label="$t('TableHeader.__flagLevel')"
						:rules="[value => (!!value && Number(value) > 0) || $t('Validation.__numberRules')]"
						required class="number-text-field required" prepend-inner-icon="mdi-minus"
						append-icon="mdi-plus" @click:prepend-inner="flagLevelMinusOrPlus(false)"
						@click:append="flagLevelMinusOrPlus(true)"></v-text-field>
				</v-row>

				<v-row>
					<v-select class="required" attach chips :label="$t('TableHeader.__roomFlagLevel')"
						v-model="editedItem.roomFlagLevel" :items="dropdownRommLevels"></v-select>
				</v-row>

				<v-row>
					<v-select class="required" attach chips :label="$t('TableHeader.__currency')"
						v-model="editedItem.currency" :items="currencySelectItems">

						<template v-slot:item="data">

							<div class="letter-size-equal">
								<span>{{data.item.value}}</span>｜<span>{{data.item.text}}</span>
							</div>

						</template>

					</v-select>
				</v-row>
				<v-row>
					<v-text-field type="number" v-model="minAvailableMoney"
						:label="$t('TableHeader.__minAvailableMoney')" class="number-text-field required"
						prepend-inner-icon="mdi-minus" append-icon="mdi-plus"
						@click:prepend-inner="minAvailableMoneyMinusOrPlus(false)"
						@click:append="minAvailableMoneyMinusOrPlus(true)"></v-text-field>
				</v-row>

				<v-row>
					<v-radio-group row v-model="editedItem.status">
						<v-radio :label="$t('ServerStatus.Enable')" :value="serverStatusEnum.Enable"></v-radio>
						<v-radio :label="$t('ServerStatus.Disable')" :value="serverStatusEnum.Disable">
						</v-radio>
						<v-radio :label="$t('ServerStatus.ServerDown')" :value="serverStatusEnum.ServerDown">
						</v-radio>
					</v-radio-group>
				</v-row>

				<v-row>
					<v-text-field v-model="editedItem.clientAddr" :label="$t('TableHeader.__clientAddr')"
						clearable></v-text-field>
				</v-row>
				<v-row>
					<v-text-field v-model="editedItem.notifyAddr" :label="$t('TableHeader.__notifyAddr')"
						clearable></v-text-field>
				</v-row>
				<v-row>
					<v-text-field v-model="editedItem.httpAddr" :label="$t('TableHeader.__httpAddr')"
						clearable></v-text-field>
				</v-row>
			</v-flex>
		</v-card-text>
		<v-card-actions>
			<v-spacer></v-spacer>
			<v-btn text @click="$emit('close-update-server')" class="btn">
				<v-icon left>mdi-cancel</v-icon>
				{{$t('Button.__cancel')}}
			</v-btn>
			<v-btn color="yellow darken-1" text @click="save" class="btn">
				<v-icon left>mdi-check-circle</v-icon>
				{{$t('Button.__submit')}}
			</v-btn>
		</v-card-actions>
	</v-card>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DropdownApi from '@/api/commons/dropdown';
import ServersApi from '@/api/system-setting/servers';
import { ServerStatusEnum } from '@/models/status/system';
import * as Model from '@/models/interfaces/system-setting/servers';
// import Validation from '@/utilities/validation';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import { Currency } from '@/models/status/common';
@Component({ mixins: [commonDisplayFiltersMixin] })
export default class EditServerPopup extends Vue {
	@Prop(Object) readonly propsData!: Model.IServer;
	serverStatusEnum: typeof ServerStatusEnum = ServerStatusEnum;
	editedItem: Model.IServer = {} as Model.IServer;
	games: any[] = [];

	dropdownRommLevels: any[] = [];
	currencySelectItems: any[] = [];

	serverID: number | null = null;
	flagLevel: number | null = null;

	minAvailableMoney: number | null = null;
	serverIDMinusOrPlus(isPlus: boolean) {
		if (!this.serverID) {
			this.serverID = 0;
		}
		this.serverID = isPlus ? Number(this.serverID) + 1 : Number(this.serverID) - 1;
	}
	flagLevelMinusOrPlus(isPlus: boolean) {
		if (!this.flagLevel) {
			this.flagLevel = 0;
		}
		this.flagLevel = isPlus ? Number(this.flagLevel) + 1 : Number(this.flagLevel) - 1;
	}
	minAvailableMoneyMinusOrPlus(isPlus: boolean) {
		if (!this.minAvailableMoney) {
			this.minAvailableMoney = 0;
		}
		this.minAvailableMoney = isPlus ? Number(this.minAvailableMoney) + 1 : Number(this.minAvailableMoney) - 1;
	}

	save() {
		this.editedItem.serverID = this.serverID;
		this.editedItem.flagLevel = this.flagLevel;
		this.editedItem.minAvailableMoney = this.minAvailableMoney;
		if (this.propsData.serverID === -1) {
			ServersApi.addServer(<Model.IServer>this.editedItem).then(() => {
				this.$emit('update-server-success');
			});
		} else {
			ServersApi.updateServer(<Model.IServer>this.editedItem).then(() => {
				this.$emit('update-server-success');
			});
		}
	}

	created() {
		DropdownApi.getGames().then((res) => {
			this.games = res.map((s) => {
				return { value: s.value, text: s.text };
			});
		});

		DropdownApi.getRoomLevels().then((res) => {
			this.dropdownRommLevels = res.map((s) => {
				return { value: s.value, text: s.text };
			});
		});
		DropdownApi.getCurrency().then((res) => {
			this.currencySelectItems = res;
		});

		if (this.propsData.serverID === -1) {
			this.editedItem = {} as Model.IServer;
			this.minAvailableMoney = 0;
			this.editedItem.status = ServerStatusEnum.Enable;
			this.editedItem.roomFlagLevel = '1';
		} else {
			this.editedItem = { ...this.propsData };
			this.serverID = this.editedItem.serverID;
			this.flagLevel = this.editedItem.flagLevel;
			this.minAvailableMoney = this.editedItem.minAvailableMoney;
			this.editedItem.roomFlagLevel = this.propsData.roomFlagLevel.toString();
			this.editedItem.gameID = this.propsData.gameID.toString();
		}
	}
}
</script>