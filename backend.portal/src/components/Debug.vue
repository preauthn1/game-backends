<template>
	<v-app class="app">

		<CustomizedColumnsDisplay></CustomizedColumnsDisplay>

		<!-- 
		<NotificationWarning v-show="showNotificationWarning" :title="`${$t('Message.__warning')}`"
			:max="1" :text="`${$t('Message.__isConfirmDelete')}#${deleteIndex}?`"
			:confirmed-action="confirmedAction" :canceled-action="canceledAction" /> -->

	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import MerchantsApi from '@/api/system-setting/merchants';
import DropdownApi from '@/api/commons/dropdown';

import { Locale, Currency, Status } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import CustomizedColumnsDisplay from '@/components/CustomizedColumnsDisplay.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

@Component({ components: { NotificationWarning, CustomizedColumnsDisplay }, mixins: [commonDisplayFiltersMixin, datetimeMixin] })
export default class Debug extends Vue {
	mounted() {}
}
</script>

