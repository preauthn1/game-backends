<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-autocomplete clearable :items="dropdownMerchants"
										v-model="searchFilter.merchantID" class="merchantDropdownItems"
										:label="$t('TableHeader.__merchant')">

										<template v-slot:item="data">
											<div
												:class="data.item.valueInt == null  ? 'merchantCategoryText': 'merchantValue'">
												<span>{{data.item.text}}</span>
											</div>
										</template>

									</v-autocomplete>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item mt-3">
									<date-picker v-model="searchBeginTime" type="date" value-type="timestamp"
										:placeholder="$t('TableHeader.__time')">
									</date-picker>
								</div>
							</v-col>

							<!-- <v-col cols="12" sm="3" md="3" class="quick-time">
								<v-chip class="mr-2"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }"
									@click="globalQuicklyPickTimeRange('today')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__today`) }}
								</v-chip>
								<v-chip class="mr-2"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }"
									@click="globalQuicklyPickTimeRange('yesterday')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__yesterday`) }}
								</v-chip>
								<v-chip class="mr-2"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }"
									@click="globalQuicklyPickTimeRange('thisWeek')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__thisWeek`) }}
								</v-chip>

								<v-chip class="mr-2"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }"
									@click="globalQuicklyPickTimeRange('lastWeek')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__lastWeek`) }}
								</v-chip>
								<v-chip class="mr-2"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }"
									@click="globalQuicklyPickTimeRange('thisMonth')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__thisMonth`) }}
								</v-chip>
								<v-chip class="mr-2"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }"
									@click="globalQuicklyPickTimeRange('lastMonth')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__lastMonth`) }}
								</v-chip>
							</v-col> -->

							<v-col cols="12" sm="3" md="3">
								<!-- <div class="item right"> -->
								<div class="item">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }}
									</v-btn>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }}
									</v-btn>

									<v-btn color="light-blue lighten-3" text class="btn search"
										@click.stop="exportFiles">
										<v-icon left>cloud_download</v-icon>
										{{ $t('Button.__downloadFile') }}
									</v-btn>&#9;

								</div>
							</v-col>

						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div class="hint-row">

			<v-icon> error </v-icon>
			{{ $t('Message.__dataDelay30Minutes') }}

			<v-icon class="ml-3"> error </v-icon>
			效能考量，僅顯示最多7筆

		</div>

		<v-row>
			<v-col cols="12">

				<v-data-table :headers="headers" :items="searchResult.items" sort :items-per-page="1000"
					hide-default-footer>

					<template v-slot:item.day1.retentionRate="{ item }">

						<span v-if="item.day1.isFutureDate" :title="$t('TableHeader.__futureTime')"
							class="grey-text">
							-
						</span>

						<span v-if="!item.day1.isFutureDate">
							{{item.day1.retentionRate}}
						</span>

					</template>

					<template v-slot:item.day3.retentionRate="{ item }">

						<span v-if="item.day3.isFutureDate" :title="$t('TableHeader.__futureTime')"
							class="grey-text">
							-
						</span>

						<span v-if="!item.day3.isFutureDate">
							{{item.day3.retentionRate}}
						</span>

					</template>

					<template v-slot:item.day7.retentionRate="{ item }">

						<span v-if="item.day7.isFutureDate" :title="$t('TableHeader.__futureTime')"
							class="grey-text">
							-
						</span>
						<span v-if="!item.day7.isFutureDate">
							{{item.day7.retentionRate}}
						</span>

					</template>

					<template v-slot:item.day14.retentionRate="{ item }">

						<span v-if="item.day14.isFutureDate" :title="$t('TableHeader.__futureTime')"
							class="grey-text">
							-
						</span>
						<span v-if="!item.day14.isFutureDate">
							{{item.day14.retentionRate}}
						</span>

					</template>

					<template v-slot:item.day30.retentionRate="{ item }">
						<span v-if="item.day30.isFutureDate" :title="$t('TableHeader.__futureTime')"
							class="grey-text">
							-
						</span>

						<span v-if="!item.day30.isFutureDate">
							{{item.day30.retentionRate}}
						</span>

					</template>

				</v-data-table>

			</v-col>
		</v-row>

	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import DropdownApi from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo } from '@/utilities/datetime-format';
import * as Model from '@/models/interfaces/statistics/game';
import * as ChartModel from '@/models/interfaces/statistics/chart';
import ChartApi from '@/api/statistics/chart';
import DatePicker from 'vue2-datepicker';
import LineChartPopup from '@/components/statistics/LineChartPopup.vue';
import { ChartDataTypeEnum } from '@/models/status/statistics';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import * as StatModel from '@/models/interfaces/statistics/merchant-summary';
import MerchantSummaryApi from '@/api/statistics/merchant-summary';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, commonMethodMixin], components: { DatePicker, LineChartPopup } })
export default class RetentionReport extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;

	panel: number[] = [0];
	renderComponent: boolean = true;

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	searchBeginTime: number = 0;

	searchFilter: StatModel.IRetentionReportRequest = {} as StatModel.IRetentionReportRequest;
	searchResult: StatModel.IRetentionReportResponse = {} as StatModel.IRetentionReportResponse;

	dropdownMerchants: any[] = [];

	get headers() {
		return [
			{ text: '日期', value: 'dateText', align: 'start', sortable: false, width: '10%' },
			{ text: '新玩家數', value: 'newPlayerCount', align: 'end', sortable: false },
			{ text: '1日留存率', value: 'day1.retentionRate', align: 'end', sortable: false },
			{ text: '3日留存率', value: 'day3.retentionRate', align: 'end', sortable: false },
			{ text: '7日留存率', value: 'day7.retentionRate', align: 'end', sortable: false },
			{ text: '14日留存率', value: 'day14.retentionRate', align: 'end', sortable: false },
			{ text: '30日留存率', value: 'day30.retentionRate', align: 'end', sortable: false },
		];
	}

	reloadComponent() {
		setTimeout(() => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		}, 250);
	}

	search() {
		EventBus.$emit('show_mask', null);

		this.searchFilter.timezone = this.userProfile.timeZone;
		this.searchFilter.begin = startEndDate(this.searchBeginTime);

		MerchantSummaryApi.getRetentionReport(this.searchFilter)
			.then((res) => {
				this.searchResult = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	clear() {
		this.searchFilter = {} as StatModel.IRetentionReportRequest;
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	async exportFiles() {
		const excelHeaders: any = ['日期', '新玩家數', '1日', '3日', '7日', '14日', '30日'];
		const excelData: any = [];

		this.searchFilter.timezone = this.userProfile.timeZone;
		this.searchFilter.begin = startEndDate(this.searchBeginTime);

		EventBus.$emit('show_mask', null);

		const result = await MerchantSummaryApi.getRetentionReport(this.searchFilter).finally(() => {
			EventBus.$emit('hide_mask', null);
		});

		result.data.items.forEach((value) => {
			const eachRow: any = [];

			eachRow.push(value.dateText);
			eachRow.push(value.newPlayerCount);
			eachRow.push(value.day1.retentionRate);
			eachRow.push(value.day3.retentionRate);
			eachRow.push(value.day7.retentionRate);
			eachRow.push(value.day14.retentionRate);
			eachRow.push(value.day30.retentionRate);
		});
		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_RetentionReport.xlsx',
		};
		// @ts-ignore
		this.downloadExcel(config);
	}

	init() {
		this.searchBeginTime = daysAgo(-7);
	}

	mounted() {
		DropdownApi.getMerchants(true).then((res) => {
			this.dropdownMerchants = res;
		});

		this.reloadComponent();
		this.init();
	}
}
</script>
<style lang="scss" scoped>
</style>
