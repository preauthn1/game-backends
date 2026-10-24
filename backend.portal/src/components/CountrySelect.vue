<template>
	<v-menu bottom offset-y>
		<template v-slot:activator="{ on }">
			<!-- <v-btn icon v-on="on" class="country">
				<country-flag :country="selectCountry" v-on="on" />
			</v-btn>-->
			<v-btn icon v-on="on" class="country">
				<v-avatar color="indigo" size="36">
					<span class="white--text headline">{{selectCountry}}</span>
				</v-avatar>
			</v-btn>
		</template>
		<v-list>
			<v-list-item @click="changeLang('zh-cn')">
				<!-- <country-flag country="cn" /> -->
				<div class="lang">简体中文</div>
			</v-list-item>
			<v-list-item @click="changeLang('zh-tw')">
				<!-- <country-flag country="tw" /> -->
				<div class="lang">繁體中文</div>
			</v-list-item>
			<v-list-item @click="changeLang('en-us')">
				<!-- <country-flag country="us" /> -->
				<div class="lang">English</div>
			</v-list-item>
		</v-list>
	</v-menu>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import { Locale } from '@/models/status/common';
import DropdownApi from '@/api/commons/dropdown';
import Cookies from 'js-cookie';
@Component
export default class CountrySelect extends Vue {
	@Prop(String) readonly defaultLang!: string;
	selectCountry: string = '简';
	isMounted: boolean = false;
	@Action('Auth/setDefaultLangTime') private setDefaultLangTime!: any;
	changeLang(lang: string) {
		const temp = this.$i18n.locale;
		switch (lang) {
			case Locale.ZHCN:
				Cookies.set('locale', 'zh-cn');
				this.selectCountry = '简';
				this.$i18n.locale = 'cn';
				break;
			case Locale.ZHTW:
				Cookies.set('locale', 'zh-tw');
				this.selectCountry = '繁';
				this.$i18n.locale = 'tw';
				break;
			case Locale.ENUS:
				Cookies.set('locale', 'en-us');
				this.selectCountry = 'En';
				this.$i18n.locale = 'en';
				break;
			default:
				Cookies.set('locale', 'zh-cn');
				this.selectCountry = 'cn';
				this.$i18n.locale = 'cn';
				break;
		}
		if (this.isMounted) {
			DropdownApi.updateLanguage(lang);
			if (temp !== this.$i18n.locale) {
				this.setDefaultLangTime();
			}
		}
	}
	created() {
		this.changeLang(this.defaultLang);
	}
	mounted() {
		this.isMounted = true;
	}
}
</script>
<style lang="scss" scope>
span.white--text.headline {
	font-family: 微軟正黑體, arial !important;
}
.country {
	background-color: transparent !important;
	box-shadow: none !important;
}
.lang {
	white-space: nowrap !important;
}
</style>
