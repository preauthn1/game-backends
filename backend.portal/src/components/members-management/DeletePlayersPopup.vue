<template>
	<v-card>
		<v-card-title
			class="grey darken-2"
		>{{propsData.length > 1 ? `${$t('Popup.__batchLock')}` : `${propsData[0]} ${ status === playerStatus.Enable ? `${$t('Popup.__lock')}` :`${$t('Popup.__unlock')}`}` }}</v-card-title>
		<v-spacer></v-spacer>
		<v-card-text>
			<v-flex>
				<v-row>
					<v-textarea class="required" v-model="memo" :label="`${$t('Popup.__reason')}`"></v-textarea>
				</v-row>
			</v-flex>
		</v-card-text>
		<v-card-actions>
			<v-spacer></v-spacer>
			<v-btn text @click="$emit('close')" class="btn">
				<v-icon left>mdi-cancel</v-icon>
				{{ $t('Button.__cancel') }}
			</v-btn>
			<v-btn color="yellow darken-1" text @click="save" class="btn">
				<v-icon left>mdi-check-circle</v-icon>
				{{ $t('Button.__submit') }}
			</v-btn>
		</v-card-actions>
	</v-card>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import * as Model from '@/models/interfaces/members-management/players';
import { Status } from '@/models/status/common';
import PlayersApi from '@/api/members-management/players';
@Component
export default class DeletePlayersPopup extends Vue {
	playerStatus: typeof Status = Status;
	@Prop(Number) readonly status!: number;
	@Prop(Array) readonly propsData!: number[];
	memo: string = '';
	save() {
		if (this.propsData.length === 1) {
			PlayersApi.playerLock(<Model.ILockRequest>{
				playerID: this.propsData[0],
				memo: this.memo,
				status: this.status === Status.Enable ? Status.Disable : Status.Enable,
			}).then(() => {
				this.$emit('update-success');
			});
		} else {
			PlayersApi.playerBatchLock(<Model.IBatchLockRequest>{
				playerIDs: this.propsData,
				memo: this.memo,
				status: this.status === Status.Enable ? Status.Disable : Status.Enable,
			}).then(() => {
				this.$emit('update-success');
			});
		}
	}
}
</script>
