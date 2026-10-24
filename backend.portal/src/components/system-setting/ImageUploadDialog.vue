<template>
	<v-dialog max-width="500px" v-model="isShowDialog">
		<v-card id="getGameLinkDialog" min-height="500px">
			<v-card-title class="grey darken-2">
				<span> 上傳圖片 </span>
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
							<div class="text-c">
								<div>
									<a :href="imageUrl" target="_blank" class="link-text-grey">
										<img :src="imageUrl" style="height: 150px" />
									</a>
								</div>

								<div v-if="imageUrl != null && imageUrl.length > 0" class="mt-2">
									<div class="mb-2 f18" style="color: #ff923c; font-weight: 700">上傳成功! 請自行複製下方網址</div>

									<div style="display: inline-block; vertical-align: top" class="mr-2 pointer" @click="copyTextToClipboard(imageUrl)">
										<v-icon color="yellow"> file_copy </v-icon>

										<span
											style="
												text-decoration: underline;
												color: #f7e691;
												vertical-align: middle;
												width: 300px;
												white-space: nowrap;
												text-overflow: ellipsis;
												overflow: hidden;
												display: inline-block;
											"
											class="ml-2"
										>
											{{ imageUrl }}
										</span>
									</div>

									<!-- 
									<div
										style="width:300px;display: inline-block;display: inline-block;overflow: hidden;text-overflow: ellipsis;white-space: nowrap;">

										<a :href="imageUrl" target="_blank" class="link-text-grey">
											{{imageUrl}}
										</a>

									</div> -->

									<span> </span>
								</div>

								<div style="margin-top: 30px">
									<input type="file" class="fileUpload" ref="fileupload" @change="resetImageUrl" />
								</div>
							</div>
						</v-col>
					</v-row>
				</v-flex>
			</v-card-text>
			<v-card-actions>
				<v-spacer></v-spacer>

				<!-- <v-btn color="blue lighten-1" text @click="uploadFile" class="btn">
					<v-icon left>file_upload</v-icon>
					上傳圖片並取得路徑
				</v-btn> -->
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
import { commonMethodMixin } from '@/utilities/common-methods';
import eazyFormApi from '@/api/commons/eazyFormApi';

@Component({ components: {}, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class ImageUploadDialog extends Vue {
	imageUrl: string = '';
	isShowDialog: boolean = false;
	isCloseDialogAfterUpload: boolean = false;
	dropdownGames: any[] = [];
	uploadFileInput: any = null;

	showDialog() {
		this.isShowDialog = true;
	}

	showDialogAndEmptyValue(isCloseDialogAfterUpload: boolean = false) {
		this.isShowDialog = true;
		this.isCloseDialogAfterUpload = isCloseDialogAfterUpload;
		this.imageUrl = '';

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

	resetImageUrl(e: any) {
		this.imageUrl = '';
		this.uploadFileInput = e.target;

		this.uploadFile();
	}

	async uploadFile() {
		const hashResult = await eazyFormApi.getHashKeyByServerTime();

		const formData = new FormData();
		formData.append('file', this.uploadFileInput.files[0]);

		EventBus.$emit('show_mask', null);

		const apiUrl = 'upload/images/v2?signHash=' + hashResult.data.hash;

		const vm: any = this;

		axios
			.post(process.env.VUE_APP_FILE_API + apiUrl, formData, {
				headers: {
					'Content-Type': 'multipart/form-data',
				},
			})
			.then((res) => {
				if (res.data.code === 0) {
					this.imageUrl = res.data.file_path;
					vm.copyTextToClipboard(this.imageUrl);
					this.parentSetImageUrl();
				} else {
					alert(res.data.message);
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	parentSetImageUrl() {
		if (this.isCloseDialogAfterUpload) {
			this.isShowDialog = false;
			this.$emit('parentSetImageUrlAndClose', this.imageUrl);
		}
	}

	mounted() {}
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
