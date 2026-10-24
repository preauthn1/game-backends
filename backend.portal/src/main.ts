import Vue from 'vue';
import App from './App.vue';
import router from './router';
import store from './store';
import 'icon/materialdesignicons.min.css';
import 'font/material-icons.scss';
import '@/assets/css/_layout.scss';
import '@/assets/css/_loader.scss';
import 'vue2-datepicker/index.css';
import VueI18n from 'vue-i18n';
import CountryFlag from 'vue-country-flag';
import * as cn from '@/i18n/cn-lang';
import * as tw from '@/i18n/tw-lang';
import * as en from '@/i18n/en-lang';
import Vuetify from 'vuetify/lib';
import VueClipboard from 'vue-clipboard2';

// @ts-ignore
import VueHtml2Canvas from 'vue-html2canvas';

// @ts-ignore
import JsonExcel from 'vue-json-excel';

// @ts-ignore
import { TiptapVuetifyPlugin } from 'tiptap-vuetify';


import Notifications from 'vue-notification';
// import Sortable from 'sortablejs';


VueClipboard.config.autoSetContainer = true; // add this line
Vue.use(VueHtml2Canvas);
Vue.use(VueClipboard);
Vue.use(VueI18n);
Vue.use(Notifications);
Vue.component('country-flag', CountryFlag);
Vue.config.productionTip = false;
Vue.component('downloadExcel', JsonExcel);



// Vue.config.devtools = false;
// Vue.config.silent = true;
const locale = process.env.VUE_APP_LOCALE;
const i18n = new VueI18n({
	locale,
	messages: {
		cn,
		tw,
		en,
	},
});

Vue.use(Vuetify);




const vuetifyLang: any = {
	t: (key: string, ...params: any) => i18n.t(key, params),
};

const vuetify = new Vuetify({
	theme: {
		dark: true,
	},
	lang: vuetifyLang,
});




Vue.use(TiptapVuetifyPlugin, {
	// the next line is important! You need to provide the Vuetify Object to this place.
	vuetify, // same as "vuetify: vuetify"
	// optional, default to 'md' (default vuetify icons before v2.0.0)
	iconsGroup: 'md'
  });



new Vue({
	router,
	store,
	vuetify,
	i18n,
	render: h => h(App),
}).$mount('#app');
