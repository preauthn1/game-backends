<template>
	<v-dialog max-width="500px" v-model="isShowDialog">
		<v-card id="getGameLinkDialog" min-height="500px">
			<v-card-title class="grey darken-2">
				<span> 編輯 遊戲投注結果圖片 </span>
				<div class="top-right-item" @click="closeGameLaunchDialog">
					<v-btn icon dark @click="closeGameLaunchDialog">
						<v-icon>close</v-icon>
					</v-btn>
				</div>
			</v-card-title>

			<v-card-text>
				<v-flex>
					<v-row>
						<v-col cols="12" sm="12" md="12">
							<v-select class="required" attach chips :disabled="isDisabledColumn" :label="$t('TableHeader.__game')" v-model="editForm.gameID" :items="dropdownGames"> </v-select>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="12" md="12">
							<v-text-field v-model="editForm.code" :disabled="isDisabledColumn" :label="$t('TableHeader.__code')"> </v-text-field>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="12" md="12">
							<v-text-field v-model="editForm.memo" :label="$t('TableHeader.__memo')"> </v-text-field>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="12" md="12">
							<div class="text-c">
								<div>
									<img :src="editForm.imageUrl" style="height: 150px" />
								</div>

								<input type="file" class="fileUpload" ref="fileupload" @change="uploadFile" />

								<div style="padding-left: 10%; padding-right: 10%; padding-top: 15px; color: #aaa">Tips: 建議使用正方形的圖片，最佳大小為 256*256，且不建議圖片周圍有去背空間佔據</div>
							</div>
						</v-col>
					</v-row>
				</v-flex>
			</v-card-text>
			<v-card-actions>
				<v-spacer></v-spacer>
				<v-btn text @click="closeGameLaunchDialog" class="btn">
					<v-icon left>mdi-cancel</v-icon>
					{{ $t('Button.__cancel') }}
				</v-btn>

				<v-btn color="yellow darken-1" text @click="save" class="btn">
					<v-icon left>mdi-check-circle</v-icon>
					{{ $t('Button.__submit') }}
				</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
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
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import axios, { AxiosResponse, AxiosRequestConfig } from 'axios';
import eazyFormApi from '@/api/commons/eazyFormApi';

@Component({ components: {}, mixins: [commonDisplayFiltersMixin, datetimeMixin] })
export default class GameLaunchInternalTest extends Vue {
	editForm: Model.IGamePayTableImageEditRequest = {} as Model.IGamePayTableImageEditRequest;
	tempEditForm: Model.IGamePayTableImageEditRequest = {} as Model.IGamePayTableImageEditRequest;

	isDisabledColumn: boolean = false;
	isShowDialog: boolean = false;
	dropdownGames: any[] = [];
	uploadFileItem: any = null;

	showDialog() {
		this.isShowDialog = true;
	}

	showCreateDefaultDialog(gameID: number, code: string, imageUrl: string) {
		this.isShowDialog = true;

		this.editForm = {} as Model.IGamePayTableImageEditRequest;
		this.editForm.gameID = gameID;
		this.editForm.code = code;
		this.editForm.imageUrl = imageUrl;
		this.isDisabledColumn = true;
		this.resetFileUpload();
	}

	showCreateEmptyValueDialog() {
		this.isShowDialog = true;
		this.isDisabledColumn = false;
		this.editForm = {} as Model.IGamePayTableImageEditRequest;
		this.resetFileUpload();
	}

	showEditDialog(model: Model.IGamePayTableImageEditRequest) {
		this.isDisabledColumn = false;
		this.isShowDialog = true;
		this.editForm = model;
		this.resetFileUpload();
	}

	resetFileUpload() {
		const target: any = this.$refs.fileupload;

		if (target) {
			target.value = null;
		}
	}

	closeGameLaunchDialog() {
		this.isShowDialog = false;
	}

	save() {
		GameListApi.createOrUpdatePayTableImage(this.editForm).then((res) => {
			EventBus.$emit(`successfully-modified`);
			this.$emit('updateSuccess', '');
			this.isShowDialog = false;
		});
	}

	async uploadFile(e: any) {
		const hashResult = await eazyFormApi.getHashKeyByServerTime();

		const formData = new FormData();
		formData.append('file', e.target.files[0]);

		const apiUrl = 'upload/images/v2?signHash=' + hashResult.data.hash;

		EventBus.$emit('show_mask', null);

		axios
			.post(process.env.VUE_APP_FILE_API + apiUrl, formData, {
				headers: {
					'Content-Type': 'multipart/form-data',
				},
			})
			.then((res) => {
				this.tempEditForm = this.editForm;

				if (res.data.code === 0) {
					this.editForm = {} as Model.IGamePayTableImageEditRequest;
					this.editForm.gameID = this.tempEditForm.gameID;
					this.editForm.memo = this.tempEditForm.memo;
					this.editForm.code = this.tempEditForm.code;
					this.editForm.imageUrl = res.data.file_path;
				} else {
					alert(res.data.message);
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	mounted() {
		DropdownApi.getGames().then((res) => {
			this.dropdownGames = res;
		});
	}
}
</script>

<style>
#getGameLinkDialog .v-text-field__details {
	display: none;
}

.fileUpload {
	color: #888;
}
</style>
