<template>
	<v-app v-if="renderComponent">
		<v-row>
			<v-col cols="12">
				<v-row justify="center">
					<v-col>
						<v-autocomplete
							v-model="selectMerchants"
							:items="merchantsDropdownItems"
							dense
							chips
							small-chips
							:label="$t('TableHeader.__merchant')"
							solo
							outlined
						></v-autocomplete>
					</v-col>
					<v-col>
						<v-btn color="yellow darken-1" text class="btn" @click.stop="search">
							<v-icon left>search</v-icon>
							{{$t('Button.__search')}}
						</v-btn>
					</v-col>
				</v-row>
				<v-row>
					<v-col>
						<v-data-table :headers="headers" id="table" :items="result" :search="searchText" multi-sort>
							<template v-slot:top>
								<v-toolbar>
									<!-- <v-divider class="mx-4" inset vertical></v-divider> -->
									<v-spacer></v-spacer>
									<v-card-title>
										<v-text-field
											v-model="searchText"
											append-icon="search"
											:label="$t('Button.__search')"
											single-line
											hide-details
											clearable
										></v-text-field>
									</v-card-title>
								</v-toolbar>
							</template>
							<template v-slot:item.status="{ item }">
								<v-chip
									v-show="serverStatusEnum.ServerDown === item.status"
									class="ma-2"
								>{{$t(`ServerStatusEnum.${serverStatusEnum[item.status]}`)}}</v-chip>
								<v-chip
									v-show="serverStatusEnum.Disable === item.status"
									class="ma-2"
									color="red"
								>{{$t(`ServerStatusEnum.${serverStatusEnum[item.status]}`)}}</v-chip>
								<v-chip
									v-show="serverStatusEnum.Enable === item.status"
									class="ma-2"
									color="green"
								>{{$t(`ServerStatusEnum.${serverStatusEnum[item.status]}`)}}</v-chip>
							</template>
							<template v-slot:body.append v-if="summary">
								<tr>
									<td>
										<p class="font-weight-black">#{{ $t('TableHeader.__total') }}：</p>
									</td>
									<td>
										<p class="font-weight-medium"></p>
									</td>
									<td>
										<p class="font-weight-medium"></p>
									</td>

									<td>
										<p class="font-weight-black">{{summary.onlinePlayer}}</p>
									</td>
									<td>
										<p class="font-weight-black">{{summary.profitAmount}}</p>
									</td>
									<td>
										<p class="font-weight-black">{{summary.flowAmount}}</p>
									</td>
								</tr>
							</template>
						</v-data-table>
					</v-col>
				</v-row>
			</v-col>
		</v-row>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import { Status } from '@/models/status/common';
import DropdownApi from '@/api/commons/dropdown';
import ServersApi from '@/api/system-setting/servers';
import * as Model from '@/models/interfaces/system-setting/servers';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';

import { ServerStatusEnum } from '@/models/status/system';
@Component
export default class ServersHealthSummary extends Vue {
	serverStatusEnum: typeof ServerStatusEnum = ServerStatusEnum;
	renderComponent: boolean = true;
	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	selectMerchants: number | null = null;
	result: Model.IServersHealthSummaryItem[] = [];
	summary: Model.IServersHealthSummaryItem = {} as Model.IServersHealthSummaryItem;
	searchText: string = '';
	get headers() {
		return [
			{ text: this.$t('TableHeader.__server'), value: 'serverName' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName' },
			{ text: this.$t('TableHeader.__serverStatus'), value: 'status' },
			{ text: this.$t('TableHeader.__onlinePlayer'), value: 'onlinePlayer' },
			{ text: this.$t('TableHeader.__profitAmount'), value: 'profitAmount' },
			{ text: this.$t('TableHeader.__flowAmount'), value: 'flowAmount' },
		];
	}

	mounted() {
		this.registerReloadComponent();
		this.init();
	}
	init() {
		DropdownApi.getMerchants().then((res) => {
			this.merchantsDropdownItems = res;
		});
	}
	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.selectMerchants = null;
				this.searchText = '';
				this.result = [];
				this.summary = {} as Model.IServersHealthSummaryItem;
				this.init();
				this.renderComponent = true;
			});
		});
	}
	search() {
		if (this.selectMerchants == null || this.selectMerchants === 0) {
			EventBus.$emit('auto-hidden-error', this.$t('Message.__pleaseSelectMerchant'));
			return;
		}

		ServersApi.getServersHealthSummary(this.selectMerchants).then((res) => {
			this.result = res.data.items;
			this.summary = res.data.rowTotal;
		});
	}
}
</script>