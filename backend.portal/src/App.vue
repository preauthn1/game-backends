<template>
	<div>
		<!-- <Layout /> -->
		<v-overlay :absolute="true" :value="true" v-if="!currentComponent">
			<!-- <v-progress-circular class="loading" :size="90" :width="9" indeterminate></v-progress-circular> -->
			<div class="loader"></div>
		</v-overlay>

		<v-overlay id="globalMask" :value="isGlobalMaskVisible">
			<v-progress-circular indeterminate size="64">
			</v-progress-circular>
		</v-overlay>

		<component :class="isLogined ? 'layout':''" :is="currentComponent" />

		<NotificationCenterDialog :max="1" v-show="isShowNotificationCenterDialog"
			:text="notificationCenterDialogText" />
		<NotificationInfo />
		<NotificationCopySuccess :text="autoHiddenErrorText" />
		<NotificationAutoHiddenError :text="autoHiddenErrorText" />
		<NotificationError v-show="showNotificationError" :title="`${errorMsg}`" :max="1"
			:confirmed-error="confirmedError" />

		<NotificationWarningV2 v-show="isShowNotificationWarning" :title="`${$t('Message.__warning')}`"
			:max="1" :text="globalWarningMsg" :confirmed-action="closeWarningDialog"
			:canceled-action="closeWarningDialog" />

		<EazyFormEditDialog ref="refEazyFormEditDialog" @parentCallbackAfterSave="eazyFormCallback">
		</EazyFormEditDialog>

		<GlobalCopyTextDialog ref="refGlobalCopyTextDialog">
		</GlobalCopyTextDialog>

		<CustomizedColumnsDisplayPopupDialog ref="refCustomizedColumnsDisplayPopupDialog">
		</CustomizedColumnsDisplayPopupDialog>

		<CustomizedColumnsDisplayPopupDialogScrollPaging
			ref="refCustomizedColumnsDisplayPopupDialogScrollPaging">
		</CustomizedColumnsDisplayPopupDialogScrollPaging>

		<ShowSelfDefineColumnPopupDialog ref="refShowSelfDefineColumnPopupDialog">
		</ShowSelfDefineColumnPopupDialog>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { Action } from 'vuex-class';
import Login from '@/views/others/Login.vue';
import MerchantComparePage from '@/views/others/MerchantComparePage.vue';
import Layout from '@/components/Layout.vue';
import PermissionApi from '@/api/commons/permission';
import Cookies from 'js-cookie';
import EventBus from '@/utilities/event-bus';
import NotificationInfo from '@/components/notification/NotificationInfo.vue';
import NotificationCenterDialog from '@/components/notification/NotificationCenterDialog.vue';
import NotificationError from '@/components/notification/NotificationError.vue';
import NotificationWarningV2 from '@/components/notification/NotificationWarningV2.vue';
import NotificationCopySuccess from '@/components/notification/NotificationCopySuccess.vue';
import NotificationAutoHiddenError from '@/components/notification/NotificationAutoHiddenError.vue';
import GlobalCopyTextDialog from '@/components/GlobalCopyTextDialog.vue';
import CustomizedColumnsDisplayPopupDialog from '@/components/CustomizedColumnsDisplayPopupDialog.vue';
import CustomizedColumnsDisplayPopupDialogScrollPaging from '@/components/CustomizedColumnsDisplayPopupDialogScrollPaging.vue';
import EazyFormEditDialog from '@/components/EazyFormEditDialog.vue';
import ShowSelfDefineColumnPopupDialog from '@/components/ShowSelfDefineColumnPopupDialog.vue';

@Component({
	components: {
		Login,
		MerchantComparePage,
		Layout,
		NotificationInfo,
		NotificationError,
		NotificationCenterDialog,
		NotificationCopySuccess,
		NotificationWarningV2,
		NotificationAutoHiddenError,
		EazyFormEditDialog,
		CustomizedColumnsDisplayPopupDialog,
		CustomizedColumnsDisplayPopupDialogScrollPaging,
		GlobalCopyTextDialog,
		ShowSelfDefineColumnPopupDialog,
	},
})
export default class App extends Vue {
	isGlobalMaskVisible: boolean = false;
	isLogined: boolean = false;
	isShowNotificationCenterDialog: boolean = false;
	isShowGlobalCommonDialog: boolean = false;
	showNotificationError: boolean = false;
	isShowNotificationWarning: boolean = false;
	errorMsg: string = '';
	autoHiddenErrorText: string = '';
	globalWarningMsg: string = '';
	notificationCenterDialogText: string = '';
	currentComponent: string = '';
	eazyFormCallback: any = this.emptyCallback;
	@Action('Auth/setUserProfile') private setUserProfile!: any;

	mounted() {
		// 未登入情況下的錯誤 需要特別攔截
		EventBus.$on('login-error', (err: any) => {
			this.errorMsg = err.response.data.message;
			this.showNotificationError = true;
			this.$notify({
				group: 'error',
				title: this.$t('Message.__error').toString(), // '错误',
				type: 'error',
				duration: -1,
				speed: 10,
			});
		});

		const currentUrl = window.location.pathname;

		if (currentUrl === '/view/XXXXXXXXXX') {
			this.currentComponent = 'XXXXXXXXXXXXX';
		} else if (currentUrl === '/system-setting/merchants/compare') {
			this.currentComponent = 'MerchantComparePage';
		} else {
			PermissionApi.getProfile()
				.then((res) => {
					this.setUserProfile(res.data);
					this.isLogined = true;
					EventBus.$on('api-error', (err: any) => {
						const statusCode = err.response.status;

						// 400為標準API可攔截到的錯誤
						if (statusCode === 400) {
							if (err.response.data.code >= 401001 && err.response.data.code <= 401099) {
								Cookies.remove('token');
								Cookies.remove('locale');
								history.go(0);
							}
							this.errorMsg = err.response.data.message;
							this.showNotificationError = true;
							this.$notify({
								group: 'error',
								title: this.$t('Message.__error').toString(), // '错误',
								type: 'error',
								duration: -1,
								speed: 10,
							});
						} else {
							if (statusCode === 504) {
								this.errorMsg =
									'Request timedout. The possible reasons are that system is busy or the your operating is too long. 请求超时、可能原因为系统忙碌中或此操作作业时间非常久。' +
									` (StatusCode: ${statusCode.toString()})`;
							} else {
								this.errorMsg =
									'(⚠️undefined) Send request failed. Possible reasons are that system is busy or timedout. 请求失败、可能原因为系统忙碌中或逾时' +
									` (StatusCode: ${statusCode.toString()})`;
							}

							this.showNotificationError = true;
							this.$notify({
								group: 'error',
								title: this.$t('Message.__error').toString(), // '错误',
								type: 'error',
								duration: -1,
								speed: 10,
							});
						}
					});
					EventBus.$on('event-error', (err: string) => {
						this.errorMsg = err;
						this.showNotificationError = true;
						this.$notify({
							group: 'error',
							title: this.$t('Message.__error').toString(), // '错误',
							type: 'error',
							duration: -1,
							speed: 10,
						});
					});

					EventBus.$on('event-success-dialog', (msg: string) => {
						this.notificationCenterDialogText = msg;
						this.isShowNotificationCenterDialog = true;

						this.$notify({
							group: 'success-dailog',
							title: 'XXXXXXXXX',
							text: 'XXXXXXXXX',
							type: 'success',
							duration: 1000 * 3,
						});
					});

					EventBus.$on('event-warning', (err: string) => {
						this.globalWarningMsg = err;
						this.isShowNotificationWarning = true;

						this.$notify({
							group: 'warningV2',
							title: this.$t('Message.__warning').toString(), // '错误',
							type: 'warningV2',
							duration: -1,
							speed: 10,
						});
					});

					EventBus.$on('globalShowEazyFormEditPopupDialog', (myFilter: any, config: any) => {
						this.$nextTick(() => {
							if (!myFilter || myFilter == null) {
								alert('請傳入參數 myFilter');
							}

							if (config != null) {
								if (config.eazyFormCallback != null) {
									this.eazyFormCallback = config.eazyFormCallback;
								} else {
									this.eazyFormCallback = this.emptyCallback;
								}
							}

							const myRef: any = this.$refs.refEazyFormEditDialog;
							myRef.getFormColumns(myFilter);
						});
					});

					EventBus.$on('globalShowCopyTextDialog', (config: any) => {
						this.$nextTick(() => {
							const myRef: any = this.$refs.refGlobalCopyTextDialog;

							console.log(myRef);

							myRef.showDialog(config);
						});
					});

					EventBus.$on('globalShowCustomizedColumnsDisplayPopupDialog', (myFilter: any, config: any) => {
						this.$nextTick(() => {
							if (!myFilter || myFilter == null) {
								alert('請傳入參數 myFilter');
							}
							const myRef: any = this.$refs.refCustomizedColumnsDisplayPopupDialog;
							myRef.searchAndShowDialog(myFilter, config);
						});
					});

					EventBus.$on('globalShowCustomizedColumnsDisplayPopupDialogScrollPaging', (myFilter: any, config: any) => {
						this.$nextTick(() => {
							if (!myFilter || myFilter == null) {
								alert('請傳入參數 myFilter');
							}
							const myRef: any = this.$refs.refCustomizedColumnsDisplayPopupDialogScrollPaging;
							myRef.searchAndShowDialog(myFilter, config);
						});
					});

					EventBus.$on('globalShowSelfDefineColumnsDisplayPopupDialog', (myObject: any) => {
						this.$nextTick(() => {
							if (!myObject || myObject == null) {
								alert('請傳入參數 myObject');
							}
							const myRef: any = this.$refs.refShowSelfDefineColumnPopupDialog;
							myRef.displayColumnsByObject(myObject);
						});
					});

					EventBus.$on('globalPrintTextPopupDialog', (text: string) => {
						this.$nextTick(() => {
							const myResult: any = {};
							myResult.title = 'Result (查询结果)';
							myResult.items = [];
							myResult.items.push({ displayName: '', value: text, format: '' });

							const myRef: any = this.$refs.refShowSelfDefineColumnPopupDialog;
							myRef.displayColumnsByObject(myResult);
						});
					});

					EventBus.$on('successfully-modified', (info: any) => {
						this.$notify({
							group: 'updated',
							title: this.$t('Message.__successfully').toString(), // '系统提示：修改成功',
							type: 'success',
							duration: 2500,
						});
					});

					EventBus.$on('successfully-copy', (info: any) => {
						this.$notify({
							group: 'copy-success',
							title: this.$t('Message.__copySuccessful').toString(), // '系统提示：修改成功',
							type: 'copy-success',
							duration: 2500,
						});
					});

					EventBus.$on('auto-hidden-error', (msg: string) => {
						this.autoHiddenErrorText = msg;
						this.$notify({
							group: 'auto-hidden-error',
							text: msg, // '系统提示：修改成功',
							type: 'auto-hidden-error ',
							duration: 2500,
						});
					});

					EventBus.$on('show_mask', (info: any) => {
						this.isGlobalMaskVisible = true;
					});

					EventBus.$on('hide_mask', (info: any) => {
						this.isGlobalMaskVisible = false;
					});

					(() => {
						const d: any = {};
						document.addEventListener('mousedown', (e: any) => {
							if (!e.target) {
								return;
							}
							const closestDialog = e.target.closest('.v-dialog.v-dialog--active');
							if (e.button === 0 && closestDialog != null && e.target.classList.contains('v-card__title')) {
								// element which can be used to move element
								d.el = closestDialog; // element which should be moved
								d.mouseStartX = e.clientX;
								d.mouseStartY = e.clientY;
								d.elStartX = d.el.getBoundingClientRect().left;
								d.elStartY = d.el.getBoundingClientRect().top;
								d.el.style.position = 'fixed';
								d.el.style.margin = 0;
								d.oldTransition = d.el.style.transition;
								d.el.style.transition = 'none';
							}
						});
						document.addEventListener('mousemove', (e) => {
							if (!d.el) {
								return;
							}
							d.el.style.left = Math.min(Math.max(d.elStartX + e.clientX - d.mouseStartX, 0), window.innerWidth - d.el.getBoundingClientRect().width) + 'px';
							example.invalid = Math.min(Math.max(d.elStartY + e.clientY - d.mouseStartY, 0), window.innerHeight - d.el.getBoundingClientRect().height) + 'px';
						});
						document.addEventListener('mouseup', () => {
							if (!d.el) {
								return;
							}
							d.el.style.transition = d.oldTransition;
							d.el = undefined;
						});
						setInterval(() => {
							// prevent out of bounds
							const dialog = document.querySelector('.v-dialog.v-dialog--active') as HTMLElement;
							if (!dialog) {
								return;
							}
							dialog.style.left = `${Math.min(parseInt(dialog.style.left || '', 10), window.innerWidth - dialog.getBoundingClientRect().width)}'px'`;
							example.invalid = `${Math.min(parseInt(example.invalid || '', 10), window.innerHeight - dialog.getBoundingClientRect().height)}'px'`;
						}, 100);
					})();
				})
				.catch(() => {
					Cookies.remove('token');
					Cookies.remove('locale');
				})
				.finally(() => {
					this.currentComponent = this.isLogined ? 'Layout' : 'Login';
				});
		}
	}

	emptyCallback() {}

	confirmedError() {
		this.showNotificationError = false;
	}

	closeWarningDialog() {
		this.isShowNotificationWarning = false;
	}
}
</script>

<style lang="scss">
.layout {
	overflow: hidden;
}
</style>
