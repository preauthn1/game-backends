<template>
	<v-card>
		<v-card-title
			class="grey darken-2"
		>{{$t('Popup.__update')}}#{{editedItem.serverID}}[{{editedItem.serverName}}]{{$t('TableHeader.__server')}}{{$t('TableHeader.__merchant')}}</v-card-title>
		<v-spacer></v-spacer>
		<v-card-text>
			<v-flex>
				<v-row>
					<v-col cols="12" sm="3" md="3" v-for="(merchant, index) in availableSettings" :key="index">
						<v-checkbox
							@change="checked(merchant)"
							v-model="merchant.isEnable"
							:label="merchant.merchantName"
							:value="merchant.isEnable"
						/>
					</v-col>
				</v-row>
			</v-flex>
		</v-card-text>
		<v-card-actions>
			<v-spacer></v-spacer>
			<v-btn text @click="$emit('close-update-server-merchants')" class="btn">
				<v-icon left>close</v-icon>
				{{$t('Button.__cancel')}}
			</v-btn>
		</v-card-actions>
	</v-card>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import ServersApi from '@/api/system-setting/servers';
import * as Model from '@/models/interfaces/system-setting/servers';
@Component
export default class EditServerMerchantsPopup extends Vue {
	@Prop(Object) readonly propsData!: Model.IServer;
	editedItem: Model.IServer = {} as Model.IServer;
	availableSettings: Model.IAvailableSetting[] = [];
	created() {
		this.editedItem = { ...this.propsData };
	}

	mounted() {
		ServersApi.getAvailableMerchants(Number(this.editedItem.serverID)).then(res => {
			this.availableSettings = res.data.availableSettings;
		});
	}

	checked(merchant: Model.IAvailableSetting) {
		ServersApi.updateAvailableMerchants(<Model.IAvailableSetting>{
			serverID: this.editedItem.serverID,
			merchantID: merchant.merchantID,
			isEnable: merchant.isEnable === true ? true : false,
		}).then(() => {
			this.$emit('update-server-merchants-success');
		});
	}
}
</script>

