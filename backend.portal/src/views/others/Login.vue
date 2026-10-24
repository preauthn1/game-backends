<template>
	<v-app class="split-bg">
		<v-content>
			<v-container fluid fill-height>
				<v-layout align-center justify-center>
					<v-flex xs12 sm8 md4 lg4>
						<v-card class="elevation-1 pa-3">
							<v-card-text>
								<div class="layout column align-center">
									<img v-if="isBWGSite()" src="/img/icons/BWG_LOGO_2.png" alt="BWG" style="height: 60px" />
									<h1 class="flex my-4 primary--text">
										{{ extraBrandName() }}
										BO System
									</h1>
									<div style="margin-top: -10px">{{ extraBrandName() }}管理后台</div>
								</div>
								<v-form :autocomplete="isLocal ? 'off' : null">
									<v-text-field
										prepend-icon="person"
										name="login"
										:autocomplete="isLocal ? 'off' : null"
										label="Login"
										type="text"
										v-model="model.account"
										autofocus
										v-on:keyup.enter="login"
										:rules="[(value) => !!value || $t('Validation.__requiredRules')]"
										required
										clearable
									></v-text-field>

									<v-text-field
										label="Password"
										:autocomplete="isLocal ? 'new-password' : null"
										:type="show ? 'text' : 'password'"
										:append-icon="show ? 'mdi-eye' : 'mdi-eye-off'"
										prepend-icon="vpn_key"
										@click:append="show = !show"
										v-model="model.password"
										v-on:keyup.enter="login"
										:rules="[(value) => !!value || $t('Validation.__requiredRules')]"
										required
									>
									</v-text-field>
								</v-form>
							</v-card-text>
							<div class="login-btn">
								<v-spacer></v-spacer>
								<v-btn block :color="isBWGSite() ? 'red darken-4' : 'primary'" @click="login" :loading="loading">Login</v-btn>
							</div>
						</v-card>
					</v-flex>
				</v-layout>
			</v-container>
		</v-content>

		<!-- 彈出修改密碼 -->
		<v-dialog v-model="isShowResetPassword" max-width="550px">
			<v-card class="dialog-v-card-border">
				<v-card-title class="grey">
					<div>Reset Password (重设密码)</div>
				</v-card-title>

				<v-card-text>
					<div style="padding: 10px">
						<v-row class="mt-4">
							<div>
								<div class="yellow-text">
									<v-icon small color="yellow lighten-2"> info </v-icon>
									系统安全性升级。需请您更换密码
								</div>
							</div>
							<div>
								<div class="yellow-text">
									<v-icon small color="yellow lighten-2"> info </v-icon>
									For security reason, please reset your password.
								</div>
							</div>
						</v-row>

						<v-row>
							<v-text-field type="password" :label="'New password (新密码)'" v-model="passwordChangeForm.newPassword"> </v-text-field>
						</v-row>
						<v-row>
							<v-text-field type="password" :label="'Confirm password (确认密码)'" v-model="passwordChangeForm.confirmPassword"> </v-text-field>
						</v-row>

						<div></div>
					</div>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="grey lighten-1" v-if="isShowSkipButton" text @click="goToHomePage" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						略过(SKip)
					</v-btn>

					<v-btn color="yellow darken-1" text @click="saveResetPassword" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 綁定2FA -->
		<v-dialog v-model="isShowBind2FA" max-width="650px" persistent>
			<v-card class="dialog-v-card-border">
				<v-card-title class="grey darken-2"> Bind 2FA (綁定2FA) </v-card-title>

				<v-card-text style="padding: 15px 25px 40px 25px">
					<div class="text-l">
						<div>
							<h4>1. Install <span style="color: #deab1f">Google Authenticator</span> app , and scan the QRCode.</h4>
						</div>

						<div></div>

						<img :src="scan2FAQRCode" style="padding: 15px 10px" />

						<div>
							<h4>2. Enter the code from Google Authenticator. Complete binding.</h4>
							<div>
								<v-text-field placeholder="input 6 digit numbers" v-model="bind2FACode" style="width: 200px; display: inline-block; margin-right: 10px"></v-text-field>

								<v-btn color="green darken-1" @click="submitBind2FA()">Bind (送出绑定)</v-btn>
							</div>
						</div>
					</div>
				</v-card-text>

				<v-card-actions> </v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 登入輸入2FA -->
		<v-dialog v-model="isShowStep2Input2FA" max-width="700px" persistent>
			<v-card class="dialog-v-card-border">
				<v-card-title class="grey darken-2">
					Input 2FA code (输入2FA验证码)

					<div class="top-right-item" @click="isShowStep2Input2FA = false">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="padding: 15px 25px 40px 25px; min-height: 250px">
					<div class="text-l">
						<div>
							<h4>Please open your <span style="color: #deab1f">Google Authenticator</span> app.</h4>
							<h4>Your account has been bound to 2FA verification, you need to enter the verification code.</h4>
							<h4>你的帐号已绑定2FA安全验证，您需要输入验证码。</h4>
						</div>

						<div class="mt-4">
							<div>
								<v-text-field label="2FA code" ref="refInput2FA" v-model="model.auth2FACode" style="width: 250px; display: inline-block"> </v-text-field>
								<v-btn color="green darken-1" class="ml-2" @click="login()" :loading="loading"> Submit (送出) </v-btn>
							</div>
						</div>
					</div>
				</v-card-text>

				<v-card-actions> </v-card-actions>
			</v-card>
		</v-dialog>

		<v-overlay id="globalMask" :value="isLoginMaskVisible">
			<v-progress-circular indeterminate size="64"> </v-progress-circular>
		</v-overlay>

		<NotificationError v-show="showNotificationError" :title="`${errorMsg}`" :max="1" :confirmed-error="confirmedError" />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { uuid } from 'vue-uuid';
import { Component } from 'vue-property-decorator';
import PermissionApi from '@/api/commons/permission';
import eazyFormApi from '@/api/commons/eazyFormApi';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import Cookies from 'js-cookie';
import NotificationError from '@/components/notification/NotificationError.vue';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import { Md5 } from 'ts-md5';
import error from '@/store/error';

@Component({
	components: {
		NotificationError,
	},

	mixins: [commonDisplayFiltersMixin, commonMethodMixin],
})
export default class Login extends Vue {
	isShowBind2FA: boolean = false;
	isShowStep2Input2FA: boolean = false;
	isShowResetPassword: [REDACTED] = false;
	isShowSkipButton: boolean = false;
	isLoginMaskVisible: boolean = false;

	showNotificationError: boolean = false;
	errorMsg: string = '';
	loading: boolean = false;
	show: boolean = false;
	passwordChangeForm: PermissionModel.IUpdateCurrentUserPassword = {} as PermissionModel.IUpdateCurrentUserPassword;
	saltKey: string = '921ce031968fb5f7';
	bind2FACode: string = '';
	isLock2FA: boolean = true;

	// @ts-ignore
	scan2FAQRCode: string = '';

	model: any = {
		account: '',
		password: '',
		auth2FACode: '',
		serverHash: '',
	};

	get isLocal(): boolean {
		return process.env.VUE_APP_HOST === 'local';
	}

	getDisplayBrandingName() {
		const my: any = this;
		return my.getBrandingName();
	}

	check2FACodeAndLogin() {
		if (this.model.auth2FACode.length <= 0) {
			return;
		}

		this.login();
	}

	extraBrandName() {
		if (this.isBWGSite()) {
			return 'BWG ';
		}

		if (this.isDev()) {
			return '(Dev測試環境*)';
		}

		return '';
	}

	isBWGSite() {
		const doamin = window.location.hostname;
		if (doamin.includes('target')) {
			return true;
		}

		return false;
	}

	login() {
		this.model.device = this.getDeviceID();

		if (this.model.account && this.model.account.length > 0) {
			this.model.account = this.model.account.trim();
			const beforeMD5 = this.getDeviceID() + '_' + this.model.serverHash + '_' + this.saltKey + '_' + this.model.account;
			this.model.sign = Md5.hashStr(beforeMD5);
		}

		const vm: any = this;
		this.isLoginMaskVisible = true;

		PermissionApi.login(this.model)
			.then((res) => {
				if (!res.data.isSuperAdminOrg) {
					// 重置設定
					localStorage.removeItem('isEnableSimplifyDropdown');
				}

				if (res.data.auth_2FA_QRCode != null && res.data.auth_2FA_QRCode.length > 1) {
					localStorage.setItem('temp_access_token', res.data.access_Token);
					this.scan2FAQRCode = res.data.auth_2FA_QRCode;
					this.isShowBind2FA = true;
				} else if (res.data.isResetPasswordWarning) {
					this.setTokenToCookie(res.data.access_Token);
					this.passwordChangeForm.originalPassword = [REDACTED];
					this.passwordChangeForm.newPassword = '';
					this.passwordChangeForm.confirmPassword = '';
					this.isShowResetPassword = [REDACTED];
					this.isShowSkipButton = res.data.isShowSkipResetButton;
				} else {
					this.setTokenToCookie(res.data.access_Token);
					this.goToHomePage();
				}
			})
			.catch((err: any) => {
				if (err.data.code === 403200) {
					this.isShowStep2Input2FA = true;
				}
			})
			.finally(() => {
				setTimeout(() => {
					vm.isLoginMaskVisible = false;
				}, 500);
			});
	}

	unlockInput() {
		if (this.isLock2FA === false) {
			return;
		}

		this.isLock2FA = false;
		const refInput: any = this.$refs.refInput2FA;

		setTimeout(() => {
			refInput.focus();
		}, 100);
	}

	setTokenToCookie(accessToken: [REDACTED] {
		Cookies.set('token', accessToken);
	}

	submitBind2FA() {
		const myModel = {
			auth2FACode: this.bind2FACode,
		};

		this.isLoginMaskVisible = true;

		let myAcessToken: [REDACTED] = '';
		const localAccessToken: [REDACTED] | null = localStorage.getItem('temp_access_token');

		if (localAccessToken != null) {
			myAcessToken = [REDACTED];
		}

		PermissionApi.bind2FA(myModel, myAcessToken)
			.then((res) => {
				if (res.code === 0) {
					this.setTokenToCookie(myAcessToken);
					this.goToHomePage();
				}
			})
			.catch((err) => {
				alert(err.data.message);
				console.log('error (catch)');
			})
			.finally(() => {
				this.isLoginMaskVisible = false;
			});
	}

	goToHomePage() {
		location.href = '/';
	}

	setBrowserDefaultLanguage() {
		try {
			const myNavigator: any = navigator;
			const browserLang = myNavigator.language || myNavigator.userLanguage;

			const cookieLang = Cookies.get('locale');

			if (cookieLang && cookieLang.length > 0) {
			} else {
				if (browserLang === 'zh-TW' || browserLang === 'zh-CN' || browserLang === 'zh-HK' || browserLang === 'zh-SG') {
					Cookies.set('locale', browserLang);
				} else {
					Cookies.set('locale', 'en-us');
				}
			}
			return;
		} catch (e) {}

		return;
	}

	getHashByServerTime() {
		eazyFormApi.getHashKeyByServerTime().then((res) => {
			this.model.serverHash = res.data.hash;
		});
	}

	getDeviceID() {
		const deviceID = localStorage.getItem('deviceID');
		return deviceID;
	}

	generateDeviceID_WhenNotExist() {
		const deviceID = this.getDeviceID();

		if (!deviceID) {
			const randomUUID = uuid.v1();
			localStorage.setItem('deviceID', randomUUID);
		}
	}

	saveResetPassword() {
		PermissionApi.updatePassword(this.passwordChangeForm)
			.then((res) => {
				this.goToHomePage();
			})
			.catch((err) => {
				console.log(err);
				alert('Error： ' + err.data.message);
			})
			.finally(() => {});
	}

	setDocumentTitleByEnv() {
		const env = process.env.VUE_APP_HOST;

		if (env && !(env === undefined)) {
			if (env === 'development') {
				document.title = '(Dev)' + this.getDisplayBrandingName();
				this.setFavicon();
			} else if (env === 'local') {
				document.title = '(Local)' + this.getDisplayBrandingName();
				this.setFavicon();
			} else if (env === 'qa') {
				document.title = '(QA)' + this.getDisplayBrandingName();
				this.setFavicon();
			} else {
				document.title = this.getDisplayBrandingName();
			}
		} else {
			document.title = this.getDisplayBrandingName();
		}

		if (this.isBWGSite()) {
			document.title = 'BWG ' + document.title;
			this.setFavicon('/favicon_b3.ico');
		}
	}

	isDev() {
		const env = process.env.VUE_APP_HOST;

		if (env && !(env === undefined)) {
			if (env === 'development') {
				return true;
			}
		}
		return false;
	}

	setFavicon(targetPath: string = '/favicon42.ico') {
		const favicon1: any = document.getElementById('icon_favicon');
		const favicon2: any = document.getElementById('shortlink_favicon');
		const favicon3: any = document.getElementById('shortcut_favicon');

		if (favicon1) {
			favicon1.href = targetPath;
		}
		if (favicon2) {
			favicon2.href = targetPath;
		}
		if (favicon3) {
			favicon3.href = targetPath;
		}
	}

	mounted() {
		const vm: any = this;

		this.generateDeviceID_WhenNotExist();

		this.setDocumentTitleByEnv();

		this.getHashByServerTime();

		localStorage.setItem('routerHistories', JSON.stringify([]));

		if (process.env.NODE_ENV === 'development' && process.env.VUE_APP_HOST === 'local') {
			this.model.account = process.env.VUE_APP_DEFAULT_ACCOUNT;
			this.model.password = [REDACTED];
		}

		this.setBrowserDefaultLanguage();
	}
	confirmedError() {
		this.showNotificationError = false;
	}
}
</script>

<style scoped lang="css">
.split-bg {
	height: 50%;
	width: 100%;
	position: absolute;
	top: 0;
	left: 0;
	content: '';
	z-index: 0;
}
</style>
