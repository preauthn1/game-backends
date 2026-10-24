<template>
	<div>
		<v-flex>
			<IMChannelAnnouncement />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import IMChannelAnnouncement from '@/components/system-setting/IMChannelAnnouncement.vue';

@Component({ components: { IMChannelAnnouncement } })
export default class TGAnnouncementPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
