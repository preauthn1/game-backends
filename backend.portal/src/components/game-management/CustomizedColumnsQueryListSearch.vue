<template>
	<v-app class="app">
		<CustomizedColumnsDisplay></CustomizedColumnsDisplay>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import CustomizedColumnsDisplay from '@/components/CustomizedColumnsDisplay.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');

@Component({ components: { NotificationWarning, CustomizedColumnsDisplay }, mixins: [commonDisplayFiltersMixin, datetimeMixin] })
export default class CustomizedColumnsQueryListSearch extends Vue {
	mounted() {}
}
</script>

