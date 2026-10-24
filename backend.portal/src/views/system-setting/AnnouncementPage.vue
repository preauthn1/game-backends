<template>
	<div>
		<v-flex>
			<Announcement />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import Announcement from '@/components/system-setting/Announcement.vue';

@Component({ components: { Announcement } })
export default class AnnouncementPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
