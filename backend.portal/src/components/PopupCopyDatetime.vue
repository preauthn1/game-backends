<template>

	<v-dialog max-width="500px" v-model="isShowCopyDatetime">
		<v-card id="getGameLinkDialog" min-height="400px">
			<v-card-title class="grey darken-2">
				<span>
					{{$t("Button.__copyTime")}}
				</span>
				<span style="margin-left:10px;font-size:12px;color: #CCC;"
					v-if="identityText != null && identityText != ''">
					({{identityText}})
				</span>

				<div class="top-right-item" @click="closeDialog">
					<v-btn icon dark @click="closeDialog">
						<v-icon>close</v-icon>
					</v-btn>
				</div>
			</v-card-title>

			<div style="padding: 15px;">

				<div class="date-row">
					<div class="timezoneCategory">
						<v-chip>
							+0 {{$t("TableHeader.__timeZoneArea")}}
						</v-chip>
					</div>

					<span class="pointer" @click="copyText(dateTimeUTC0)">
						{{dateTimeUTC0}}
					</span>
					<v-icon class="copyIcon" color="yellow" @click="copyText(dateTimeUTC0)">
						file_copy
					</v-icon>
				</div>

				<div class="date-row">
					<div class="timezoneCategory">
						<v-chip>
							+7 {{$t("TableHeader.__timeZoneArea")}}
						</v-chip>
					</div>

					<span class="pointer" @click="copyText(dateTimeUTC7)">
						{{dateTimeUTC7}}
					</span>

					<v-icon class="copyIcon" color="yellow" @click="copyText(dateTimeUTC7)">
						file_copy
					</v-icon>
				</div>

				<div class="date-row">
					<div class="timezoneCategory">
						<v-chip>
							+8 {{$t("TableHeader.__timeZoneArea")}}
						</v-chip>
					</div>

					<span class="pointer" @click="copyText(dateTimeUTC8)">
						{{dateTimeUTC8}}
					</span>

					<v-icon class="copyIcon" color="yellow" @click="copyText(dateTimeUTC8)">
						file_copy
					</v-icon>
				</div>

				<div class="date-row">
					<div class="timezoneCategory">
						<v-chip>5分鐘內</v-chip>
					</div>

					<span class="pointer" @click="copyText(rangeTime5Minutes)">
						{{rangeTime5Minutes}}
					</span>

					<v-icon class="copyIcon" color="yellow" @click="copyText(rangeTime5Minutes)">
						file_copy
					</v-icon>
				</div>

				<div class="date-row">
					<div class="timezoneCategory">
						<v-chip>60分鐘內</v-chip>
					</div>

					<span class="pointer" @click="copyText(rangeTime60Minutes)">
						{{rangeTime60Minutes}}
					</span>

					<v-icon class="copyIcon" color="yellow" @click="copyText(rangeTime60Minutes)">
						file_copy
					</v-icon>
				</div>

				<div class="date-row">
					<div class="timezoneCategory">
						<v-chip>Copy SQL</v-chip>
					</div>

					<span class="pointer" @click="copyText(between5Minutes)">
						between ± 5 minutes
					</span>

					<v-icon class="copyIcon" color="yellow" @click="copyText(between5Minutes)">
						file_copy
					</v-icon>
				</div>

				<div class="date-row">
					<div class="timezoneCategory">
						<v-chip>Copy SQL</v-chip>
					</div>

					<span class="pointer" @click="copyText(between30Minutes)">
						between ± 30 minutes
					</span>

					<v-icon class="copyIcon" color="yellow" @click="copyText(between30Minutes)">
						file_copy
					</v-icon>
				</div>

			</div>

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
import { commonMethodMixin } from '@/utilities/common-methods';
import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { datetimeMixin, addMinutes } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
const playersModule = namespace('Players');

import * as Permission from '@/models/status/permission';

@Component({ components: {}, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class PopupCopyDatetime extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;

	gameLaunchPreview: Model.IGameLaunchPreview = {} as Model.IGameLaunchPreview;
	isShowGameLaunchDialog: boolean = false;
	dropdownInternalAccounts: any[] = [];
	isShowCopyDatetime: boolean = false;

	identityText: string = '';

	dateTimeUTC0: string = '';
	dateTimeUTC7: string = '';
	dateTimeUTC8: string = '';
	rangeTime5Minutes: string = '';
	rangeTime60Minutes: string = '';

	between5Minutes: string = '';
	between30Minutes: string = '';

	setDateTimeAndPopup(datetimeUTC0: string, myIdentityText: string = '') {
		const vm: any = this;

		this.identityText = myIdentityText;
		this.isShowCopyDatetime = true;

		this.dateTimeUTC0 = vm.toTimeWithAddTimezone(datetimeUTC0, 0);
		this.dateTimeUTC7 = vm.toTimeWithAddTimezone(datetimeUTC0, 7);
		this.dateTimeUTC8 = vm.toTimeWithAddTimezone(datetimeUTC0, 8);

		const begin5minutes = addMinutes(datetimeUTC0, -5, this.userProfile.timeZone);
		const end5minutes = addMinutes(datetimeUTC0, 5, this.userProfile.timeZone);
		const begin60minutes = addMinutes(datetimeUTC0, -60, this.userProfile.timeZone);
		const end60minutes = addMinutes(datetimeUTC0, 60, this.userProfile.timeZone);

		this.rangeTime5Minutes = begin5minutes + ' ~ ' + end5minutes;
		this.rangeTime60Minutes = begin60minutes + ' ~ ' + end60minutes;

		this.between5Minutes = ' BETWEEN  ' + this.generateSQLAddDate(datetimeUTC0, -5) + ' AND ' + this.generateSQLAddDate(datetimeUTC0, 5);
		this.between30Minutes = ' BETWEEN ' + this.generateSQLAddDate(datetimeUTC0, -30) + ' AND ' + this.generateSQLAddDate(datetimeUTC0, 30);
	}

	closeDialog() {
		this.isShowCopyDatetime = false;
	}

	/* tslint:disable */
	generateSQLAddDate(utc8601Time: string, offset: number) {
		return ' DATEADD(MINUTE , ' + offset.toString() + " , '" + addMinutes(utc8601Time, 0, 0) + "')";
	}

	copyText(text: string) {
		const vm: any = this;
		vm.copyTextToClipboard(text);
	}

	mounted() {}
}
</script>

<style>
#getGameLinkDialog .v-text-field__details {
	display: none;
}

.timezoneCategory {
	display: inline-block;
	min-width: 80px;
}

.date-row {
	margin-bottom: 10px;
	color: #ccc;
	font-size: 14px;
}

.copyIcon {
	margin-left: 10px;
}
</style>