<template>
	<v-app id="inspire">
		<v-navigation-drawer v-model="drawer" hide-overlay :clipped="$vuetify.breakpoint.lgAndUp" app>
			<v-list dense expand class="menu-bar-phone">
				<template v-for="item in menus">
					<v-list-group :key="item.text" v-model="item.model" v-if="item.permissionType != 99" :prepend-icon="item.model ? icon[item.icon] : icon[item['icon-alt']]" append-icon>
						<template v-slot:activator>
							<v-list-item @click="menuClick(item)">
								<v-list-item-content class="parents">
									<v-list-item-title>
										<b>
											{{ $t(`Menu.${item.token}`) }}
										</b>
									</v-list-item-title>
								</v-list-item-content>
								<v-list-item-action v-if="item.children.length > 0">
									<v-icon>mdi-chevron-down</v-icon>
								</v-list-item-action>
							</v-list-item>
						</template>
						<!-- </router-link> -->
						<v-list-item v-for="(child, i) in getVisibleChilds(item.children)" @click="menuClick(child)" :key="i" link :class="child.active ? 'active' : ''">
							<!-- <router-link :to="child.routerPath"> -->
							<v-list-item-action v-if="icon[child.icon]">
								<v-icon>{{ child.icon }}</v-icon>
							</v-list-item-action>
							<v-list-item-content class="child">
								<v-list-item-title>
									{{ $t(`Menu.${child.token}`) }}
									<!-- {{ child.text }} -->
								</v-list-item-title>
							</v-list-item-content>
							<!-- </router-link> -->
						</v-list-item>
					</v-list-group>
				</template>
			</v-list>
		</v-navigation-drawer>

		<v-app-bar :clipped-left="$vuetify.breakpoint.lgAndUp" app :color="headerColor" dark>
			<v-app-bar-nav-icon @click.stop="drawer = !drawer" />
			<v-toolbar-title style="width: 400px" class="ml-0 pl-4">
				<img src="/img/icons/BWG_LOGO_2.png" style="height: 55px" v-if="isBWGUrl()" />

				<span class="hidden-sm-and-down" style="line-height: 60px; vertical-align: bottom">
					{{ getDisplayBrandingName() }}
				</span>

				<span
					class="hide-in-phone"
					v-show="isShowEnvLabel()"
					style="background: #c16bba; padding: 5px 20px; border-radius: 10px; margin-left: 15px; font-size: 16px; line-height: 60px; vertical-align: bottom"
				>
					{{ getEnvText() }}
				</span>
			</v-toolbar-title>
			<v-spacer />

			<div style="margin-top: 15px" class="hide-in-phone mr-2" v-if="hasPermissionSuperAdmin">
				<v-tooltip left color="orange darken-4">
					<template v-slot:activator="{ on, attrs }">
						<div v-bind="attrs" v-on="on">
							<v-switch :label="'精简下拉选单'" v-model="isEnableSimplifyDropdownOnHeader" color="orange" @change="whenSimplifyDropdownSettingValueChange()"> </v-switch>
						</div>
					</template>
					<span>若勾选此。商户下拉选单会排除所有三级代理。精简选项数量</span>
				</v-tooltip>
			</div>

			<div style="margin-top: 15px" class="hide-in-phone mr-2">
				<v-switch :label="$t('TableHeader.__FourDecimalPlaces')" v-model="isEnableDecimalFourPlaces" @change="whenDecimalSettingValueChange()"> </v-switch>
			</div>

			<div v-if="isRDAccount" class="ml-1 mr-2 pointer" @click="showExtraTools()">
				<v-chip color="pink lighten-2 hide-in-phone" v-if="isShowEnvLabel()" class="mr-2"> {{ getEnvText() }}環境 </v-chip>

				<v-icon style="font-size: 16px"> build </v-icon>
			</div>

			<div class="v-btn__content" style="margin-right: 12px; color: #bbb">
				<span class="hide-in-phone">
					{{ time }}
				</span>
				<span class="ml-1" @click="showChangeTimezoneDialog()" style="text-decoration: underline; cursor: pointer">
					(GMT{{ userProfile.timeZone > 0 ? '+' : '' }} {{ userProfile.timeZone }})
				</span>
			</div>

			<div class="user f14-in-phone" style="margin-right: 5px">
				<span class="hide-in-phone"> {{ userProfile.organizationName }}- </span>{{ userProfile.userAccount }}
			</div>

			<v-btn icon title="change password">
				<v-menu bottom offset-y>
					<template v-slot:activator="{ on }">
						<v-btn icon v-on="on" class="country">
							<v-icon>person</v-icon>
						</v-btn>
					</template>
					<v-list style="width: 250px; padding-bottom: 15px">
						<v-list-item @click="showChangePassword">
							<v-icon style="margin-right: 10px">lock</v-icon>
							<span>{{ $t('Popup.__updatePassword') }}</span>
						</v-list-item>

						<v-list-item @click="closeAllTabs">
							<v-icon style="margin-right: 10px">close</v-icon>
							<span>{{ $t('Button.__closeAllTabs') }}</span>
						</v-list-item>

						<v-list-item @click="logout">
							<v-icon style="margin-right: 10px">mdi-logout</v-icon>
							<span>{{ $t('Button.__logout') }}</span>
						</v-list-item>
					</v-list>
				</v-menu>
			</v-btn>
			<!-- 
			<v-btn icon @click="logout" title='logout'>
				<v-icon>mdi-logout</v-icon>
			</v-btn> -->

			<v-btn icon>
				<country-select :default-lang="userProfile.defaultLang"></country-select>
			</v-btn>
		</v-app-bar>
		<v-content>
			<page-header class="" ref="refPageHeader"></page-header>
			<v-container class="fill-height container" fluid>
				<keep-alive>
					<router-view />
				</keep-alive>
			</v-container>
		</v-content>

		<!-- 彈出視窗-修改密碼 -->
		<v-dialog v-model="isShowChangePassword" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					{{ $t('Popup.__updatePassword') }}
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="closeAllModal">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex>
						<v-row>
							<v-text-field class="required" v-model="updatePasswordForm.originalPassword" type="password" :label="'原密碼'" autofocus required clearable> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="required" v-model="updatePasswordForm.newPassword" type="password" :label="$t('TableHeader.__password')" autofocus required clearable> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="required" v-model="updatePasswordForm.confirmPassword" type="password" :label="$t('TableHeader.__password2')" autofocus required clearable>
							</v-text-field>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="yellow darken-1" text @click="savePassword" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 彈出視窗-額外小工具 -->
		<v-dialog v-model="isShowExtraToolDialog" max-width="600px">
			<v-card>
				<v-card-title class="grey darken-2">
					常用工具
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="closeAllModal">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text style="padding-bottom: 100px">
					<v-flex class="mt-4">
						<v-row>
							<v-col cols="12" sm="4" md="4" class="text-c">
								<div @click="removeAllCache()" class="pointer">
									<v-icon style="font-size: 36px" color="red lighten-1">delete</v-icon>
									<div>清除Cache</div>
								</div>
							</v-col>

							<v-col cols="12" sm="4" md="4" class="text-c">
								<div class="pointer" v-if="isProduction()" style="opacity: 0.3">
									<v-icon style="font-size: 36px" color="orange darken-1">visibility</v-icon>
									<div>檢查pipelineID</div>
								</div>

								<div @click="checkPipelineID()" class="pointer" v-else>
									<v-icon style="font-size: 36px" color="orange darken-1">visibility</v-icon>
									<div>檢查pipelineID</div>
								</div>
							</v-col>

							<v-col cols="12" sm="4" md="4" class="text-c">
								<div @click="copySqlDeclare()" class="pointer">
									<v-icon style="font-size: 36px" color="blue darken-1">file_copy</v-icon>
									<div>T-SQL時間參數範本</div>
								</div>
							</v-col>

							<v-col cols="12" sm="4" md="4" class="text-c">
								<div @click="copySqlOfSearchApiLog()" class="pointer">
									<v-icon style="font-size: 36px" color="blue darken-1">file_copy</v-icon>
									<div>ApiLog查詢SQL範本</div>
								</div>
							</v-col>

							<v-col cols="12" sm="4" md="4" class="text-c">
								<div @click="enableRD_SuperPermission()" class="pointer">
									<v-icon style="font-size: 36px" color="yellow darken-1">stars</v-icon>
									<div>開啟工程師特殊權限</div>
								</div>
							</v-col>
						</v-row>
						<v-divider></v-divider>

						<div style="margin-top: 20px">
							<h4>常用連結</h4>
						</div>
					</v-flex>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 彈出視窗-修改時區 -->
		<v-dialog v-model="isShowChangeTimezone" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					{{ $t('Popup.__updateTimezone') }}
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="closeAllModal">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex class="mt-4">
						<v-row>
							<v-autocomplete v-model="updateTimezoneForm.timeZoneID" :items="timezoneDropdownItems" :label="$t('TableHeader.__timeZoneArea')" dense outlined menu-props="auto">
							</v-autocomplete>

							<!-- <v-text-field class="required" v-model="updatePasswordForm.confirmPassword"
								type="password" :label="$t('TableHeader.__password2')" autofocus required clearable>
							</v-text-field> -->
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions style="padding-bottom: 150px">
					<v-spacer></v-spacer>

					<v-btn color="yellow darken-1" text @click="saveTimezone" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import AppEvents from '@/event';
import PageHeader from '@/components/PageHeader.vue';
import Cookies from 'js-cookie';
import PermissionApi from '@/api/commons/permission';
import DropdownApi from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import * as Common from '@/models/status/common';
const authModule = namespace('Auth');
import EventBus from '@/utilities/event-bus';
import CountrySelect from '@/components/CountrySelect.vue';
import { timeWithTimezone } from '@/utilities/datetime-format';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import systemConfigSettingApi from '@/api/system-setting/systemConfigSetting';
import { commonMethodMixin } from '@/utilities/common-methods';
import InternalTestApi from '@/api/commons/internalTest';
import * as Permission from '@/models/status/permission';
import { Md5 } from 'ts-md5';

@Component({ components: { PageHeader, CountrySelect }, mixins: [commonDisplayFiltersMixin, commonMethodMixin] })
export default class Layout extends Vue {
	dialog: boolean = false;
	rightDrawer: boolean = false;
	drawer: any = true;
	menus: any = [];
	source: string = '';
	icon: typeof Common.MenuIcon = Common.MenuIcon;
	time: string = '';
	headerColor: string = 'deep-purple darken-2';

	isEnableSimplifyDropdownOnHeader: boolean = false;
	isEnableDecimalFourPlaces: boolean = false;
	isShowChangePassword: [REDACTED] = false;
	isShowChangeTimezone: boolean = false;
	isShowExtraToolDialog: boolean = false;

	updatePasswordForm: PermissionModel.IUpdateCurrentUserPassword = {} as PermissionModel.IUpdateCurrentUserPassword;
	updateTimezoneForm: PermissionModel.IUpdateCurrentUserTimezone = {} as PermissionModel.IUpdateCurrentUserTimezone;

	timezoneDropdownItems: DropdownModel.IDropdownItems[] = [];

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('apiPaths') apiPaths!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('nestedPermissions') nestedPermissions!: PermissionModel.INestedPermission[];

	get isRDAccount() {
		const userAccountHash = Md5.hashStr(this.userProfile.userAccount || '');
		return userAccountHash === '701fb90716e10ecc7a43852e0eae27f1' || userAccountHash === '60444958e806c630f60482e6f8aa3071';
	}

	get hasPermissionSuperAdmin() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	@Action('Auth/setApiPath') private setApiPath!: any;
	@Action('Auth/setAllPermissions') private setAllPermissions!: any;

	openThemeSettings() {
		this.$vuetify.goTo(0);
		this.rightDrawer = !this.rightDrawer;
	}

	menuClick(data: any) {
		if (data.children && data.children.length > 0) {
			return;
		}
		this.$router.push({ path: data.routerPath });
	}

	showChangePassword() {
		this.updatePasswordForm = {} as PermissionModel.IUpdateCurrentUserPassword;
		this.isShowChangePassword = [REDACTED];
	}

	getVisibleChilds(childs: any[]) {
		// 編號99 請參考 PermissionTypeEnum
		return childs.filter((x) => x.permissionType !== 99);
	}

	showExtraTools() {
		this.isShowExtraToolDialog = true;
	}

	closeAllModal() {
		this.isShowChangePassword = [REDACTED];
		this.isShowChangeTimezone = false;
		this.isShowExtraToolDialog = false;
	}

	savePassword() {
		EventBus.$emit('show_mask', null);
		PermissionApi.updatePassword(this.updatePasswordForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	copyJsText() {
		// 2021/10/05 已無需再使用
		// const vm: any = this;
		// // 取得sessionStorage的值 然後複製到剪貼簿
		// const jsCode =
		// 	'const textarea = document.createElement("textarea");' +
		// 	'document.body.appendChild(textarea);' +
		// 	'textarea.value = sessionStorage.getItem("AuthToken");' +
		// 	'textarea.select();' +
		// 	'textarea.focus();' +
		// 	'textarea.setSelectionRange(0, 99999);' +
		// 	'console.log("", document.execCommand("copy"));';
		// vm.copyTextToClipboard(jsCode);
	}

	copySqlDeclare() {
		const vm: any = this;

		const changeLine = '\n';

		// @ts-ignore
		const jsCode =
			"DECLARE @beginTime datetime = '2023-01-01'" +
			changeLine +
			'DECLARE @endTime datetime = dateadd(day , 14 , @beginTime)' +
			changeLine +
			changeLine +
			changeLine +
			"DECLARE @begin datetime = dateadd(hour , -8 , '2023-01-01')" +
			changeLine +
			'DECLARE @end datetime = dateadd(day , 1 , @begin)' +
			changeLine;

		vm.copyTextToClipboard(jsCode);
	}

	enableRD_SuperPermission() {
		if (confirm('是否開通工程師特殊權限?')) {
			EventBus.$emit('show_mask', null);

			const backendButtonForm: any = {};
			backendButtonForm.backendButtonActionID = 9901;

			InternalTestApi.executeBackendButtonAction(backendButtonForm)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	copySqlOfSearchApiLog() {
		const vm: any = this;

		const changeLine = '\n';

		// @ts-ignore
		const jsCode =
			'SELECT top 55 * FROM [PGS_SystemLog].[dbo].[ApiRequestLogExternalVendorAPI] WITH (NOLOCK)' +
			changeLine +
			' WHERE CreateTime >= DATEADD(hour , -4 , getutcdate()) ' +
			changeLine +
			'ORDER BY CreateTime desc' +
			changeLine +
			changeLine +
			'' +
			'SELECT top 55 * FROM [PGS_SystemLog].[dbo].[ApiRequestLogExternalVendorAPIError] WITH (NOLOCK)' +
			changeLine +
			' WHERE CreateTime >= DATEADD(hour , -4 , getutcdate()) ' +
			changeLine +
			'ORDER BY CreateTime desc' +
			changeLine +
			changeLine +
			'' +
			'SELECT top 55 * FROM [PGS_SystemLog].[dbo].[ApiRequestLogVendorAPIError] WITH (NOLOCK)' +
			changeLine +
			' WHERE CreateTime >= DATEADD(hour , -4 , getutcdate()) ' +
			changeLine +
			'ORDER BY CreateTime desc' +
			changeLine +
			changeLine +
			'' +
			changeLine +
			'SELECT top 55 * FROM [PGS_SystemLog].[dbo].[ApiRequestLogVendorAPI] WITH (NOLOCK)' +
			changeLine +
			"WHERE CreateTime >= DATEADD(hour , -4 , getutcdate()) AND  IdentityKey = '175031'" +
			changeLine +
			'ORDER BY CreateTime desc' +
			changeLine +
			changeLine +
			'SELECT top 55 * FROM [PGS_SystemLog].[dbo].[ApiRequestLogVendorAPIError] WITH (NOLOCK)' +
			changeLine +
			"WHERE CreateTime >= DATEADD(hour , -8 , '2022-10-07 01:30:00') AND CreateTime < DATEADD(hour , -8 , '2022-10-07 02:00:00') AND relativeUrl = '/v1/player' AND DebugInfo = '500029'" +
			changeLine +
			'ORDER BY CreateTime desc' +
			changeLine +
			'' +
			changeLine +
			'SELECT distinct RelativeUrl FROM [PGS_SystemLog].[dbo].[ApiRequestLogVendorAPI] WITH (NOLOCK)' +
			changeLine +
			'WHERE CreateTime >= DATEADD(hour , -4 , getutcdate())' +
			changeLine +
			changeLine;

		vm.copyTextToClipboard(jsCode);
	}

	removeAllCache() {
		EventBus.$emit('show_mask', null);
		systemConfigSettingApi
			.removeAllCache()
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	setFavicon(targetPath = '/favicon42.ico') {
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

	setDecimalPlacesByLocalStorage() {
		const value = localStorage.getItem('isEnableDecimalFourPlaces');
		if (value && value != null && value === '1') {
			this.isEnableDecimalFourPlaces = true;
		}
	}

	setSimplifyDropdownByLocalStorage() {
		const value = localStorage.getItem('isEnableSimplifyDropdown');
		if (value && value != null && value === '1') {
			this.isEnableSimplifyDropdownOnHeader = true;
		}
	}

	whenDecimalSettingValueChange() {
		localStorage.setItem('isEnableDecimalFourPlaces', this.isEnableDecimalFourPlaces ? '1' : '0');
		location.reload();
	}

	whenSimplifyDropdownSettingValueChange() {
		localStorage.setItem('isEnableSimplifyDropdown', this.isEnableSimplifyDropdownOnHeader ? '1' : '0');
		location.reload();
	}

	@Watch('$route.path', { immediate: true, deep: true })
	onRouteChange() {
		this.changeMenuClicked();
	}

	@Watch('nestedPermissions', { immediate: true, deep: true })
	onPermissionsChanged(newVal: PermissionModel.INestedPermission[]) {
		if (!this.nestedPermissions || this.nestedPermissions.length < 1) {
			return;
		}
		const menuTree = this.nestedPermissions.map((parent: PermissionModel.INestedPermission) => {
			return {
				icon: parent.token,
				activator: parent.token,
				'icon-alt': parent.token,
				text: parent.displayName,
				routerPath: parent.pageUrl,
				permissionType: parent.permissionType,
				token: [REDACTED],
				id: parent.permissionID,
				children: parent.childPermissions.map((child: PermissionModel.INestedPermission) => {
					return {
						icon: child.token,
						activator: child.token,
						'icon-alt': child.token,
						text: child.displayName,
						permissionType: child.permissionType,
						routerPath: child.pageUrl,
						token: [REDACTED],
					};
				}),
			};
		});
		this.menus = menuTree;

		console.log('----menu----');
		console.log(this.menus);

		this.changeMenuClicked();
	}

	getDisplayBrandingName() {
		const my: any = this;
		return my.getBrandingName();
	}

	isProduction() {
		const env = process.env.VUE_APP_HOST;
		if (env === 'development') {
			return false;
		} else if (env === 'local') {
			return false;
		} else if (env === 'qa') {
			return false;
		} else {
			return true;
		}
	}

	// 根據環境調整顏色
	setDocumentTitleByEnv() {
		const env = process.env.VUE_APP_HOST;

		if (env && !(env === undefined)) {
			if (env === 'development') {
				document.title = '(Dev)' + this.getDisplayBrandingName();
				this.headerColor = 'teal darken-2';
				this.setFavicon();
			} else if (env === 'local') {
				document.title = '(Local)' + this.getDisplayBrandingName();
				this.headerColor = 'blue-grey darken-3';
				this.setFavicon();
			} else if (env === 'qa') {
				document.title = '(QA)' + this.getDisplayBrandingName();
				this.headerColor = 'light-blue darken-4';
				this.setFavicon();
			} else {
				document.title = this.getDisplayBrandingName();
			}
		} else {
			document.title = this.getDisplayBrandingName();
		}

		if (this.isBWGUrl()) {
			this.headerColor = 'brown darken-3';
			this.setFavicon('/favicon_b3.ico');
		}
	}

	isBWGUrl() {
		const vm: any = this;
		return vm.isBWGSiteUrl();
	}

	isShowEnvLabel() {
		const env = process.env.VUE_APP_HOST;

		if (env && !(env === undefined)) {
			if (env === 'development') {
				return true;
			}
			if (env === 'local') {
				return true;
			}
			if (env === 'qa') {
				return true;
			}
		}
		return false;
	}

	getEnvText() {
		const env = process.env.VUE_APP_HOST;

		if (env && !(env === undefined)) {
			if (env === 'development') {
				return 'DEV';
			}
			return env.toUpperCase();
		}

		return '';
	}

	getTime() {
		const date = new Date();
		const seperator1 = '-';
		const seperator2 = ':';
		let month: string | number = date.getMonth() + 1;
		let strDate: string | number = date.getDate();
		if (month >= 1 && month <= 9) {
			month = '0' + month;
		}
		if (strDate >= 0 && strDate <= 9) {
			strDate = '0' + strDate;
		}
		const currentdate = date.getFullYear() + seperator1 + month + seperator1 + strDate + ' ' + date.getHours() + seperator2 + date.getMinutes() + seperator2 + date.getSeconds();
		this.time = timeWithTimezone(currentdate, this.userProfile.timeZone);
	}

	changeMenuClicked() {
		this.menus.find((s: any) => {
			s.model = false;
			s.children.forEach((child: any) => {
				child.active = false;
			});
		});
		const splitPath: string[] = this.$router.currentRoute.path.split('/');

		const currentUrl = this.$router.currentRoute.path;
		const matchParentItem = this.allPermissions.find((x: any) => {
			if (x.pageUrl === currentUrl) {
				return x;
			}
		});

		let parent: any = null;

		if (matchParentItem && matchParentItem !== undefined) {
			parent = this.menus.find((item: any) => {
				if (item.id === matchParentItem.parentPermissionID) {
					return item;
				}
			});
		}

		if (!parent) {
			// 舊版寫法 (保留寫法補救使用)
			parent = this.menus.find((s: any) => {
				if (splitPath.length === 2) {
					return s.routerPath === '/';
				} else if (s.routerPath) {
					return s.routerPath.includes(`/${splitPath[1]}`);
				}
			});
		}

		if (!parent) {
			return;
		}

		parent.model = true;
		if (splitPath && splitPath.length > 2) {
			const child = parent.children.find((s: any) => {
				if (s.routerPath) {
					return s.routerPath.includes(`/${splitPath[2]}`);
				}
			});
			if (child) {
				child.active = true;
			}
		}
	}

	logout() {
		Cookies.remove('token');
		Cookies.remove('locale');
		history.go(0);
	}

	showChangeTimezoneDialog() {
		this.updateTimezoneForm.timeZoneID = this.userProfile.timeZoneAreaID;
		this.isShowChangeTimezone = true;
	}

	saveTimezone() {
		EventBus.$emit('show_mask', null);
		PermissionApi.updateTimezone(this.updateTimezoneForm)
			.then((res) => {
				// 強制重新整理頁面
				EventBus.$emit(`successfully-modified`);
				location.reload();
			})
			.finally(() => {
				location.reload();
			});
	}

	checkPipelineID() {
		const myUrl = (process.env.VUE_APP_API_RAW + '/internal-test/pipeline-id-check').replace(/([^:]\/)\/+/g, '$1');
		window.open(myUrl);
	}

	closeAllTabs() {
		const ref: any = this.$refs.refPageHeader;
		ref.closeAllTabs();
	}

	MD5Hash(myValue: string) {
		return Md5.hashStr(myValue);
	}

	getCurrentTokenHash() {
		const myCookieValue = Cookies.get('token');

		if (myCookieValue != null) {
			const tokenMD5 = this.MD5Hash(myCookieValue);
			return tokenMD5;
		} else {
			return '';
		}
	}

	regularCheckTokenChanged() {
		// 定期檢查當前分頁 首次render拿到token 跟 當前的token是否一致
		const lastTokenHash = sessionStorage.getItem('last_tokenHash');

		if (lastTokenHash == null || lastTokenHash === '') {
			return;
		}
		const myCookieTokenValue = this.getCurrentTokenHash();

		if (myCookieTokenValue == null || myCookieTokenValue === '') {
			location.href = '/';
		} else if (myCookieTokenValue !== lastTokenHash) {
			// 若檢查到token不一致，強制重整頁面
			location.reload();
		}
	}

	mounted() {
		// 手機版預設不要開啟menu
		if (window.screen && window.screen.width <= 480) {
			this.drawer = false;
		}
		const myCookieHash = this.getCurrentTokenHash();

		if (myCookieHash != null && myCookieHash !== '') {
			sessionStorage.setItem('last_tokenHash', myCookieHash);
		}

		DropdownApi.getTimezoneAreas().then((res) => {
			this.timezoneDropdownItems = res;
		});

		this.setDocumentTitleByEnv();

		setInterval(() => {
			this.regularCheckTokenChanged();
		}, 1000 * 5);

		setInterval(() => {
			this.getTime();
		}, 350);

		this.setDecimalPlacesByLocalStorage();
		this.setSimplifyDropdownByLocalStorage();
	}
}
</script>

<style scoped>
.container {
	display: inline-block;
}

.parents {
	height: 50px;
}

.child {
	height: 40px;
	text-indent: 90px;
}

.active {
	background-color: #5c5f63;
}

.user {
	white-space: nowrap;
}
.time {
	color: rgb(245, 102, 162);
	white-space: nowrap;
}
header {
	z-index: 35 !important;
}
.v-btn__content {
	flex: none;
}
</style>
