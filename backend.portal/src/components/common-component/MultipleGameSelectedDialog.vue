<template>
	<div>
		<v-dialog v-model="isShowDialog" max-width="650px">
			<v-card>
				<v-card-title class="grey darken-2">
					{{ $t('Message.__useCategorySelectGames') }}

					<div class="top-right-item" @click="closeAllModal()">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="min-height: 400px">
					<v-row class="mt-4">
						<v-col cols="12" md="4" sm="6">
							<div class="text-c">
								<div class="selectBox" @click="confirmSelectedGames(1000)">
									<div>
										<v-icon>mdi-gamepad-variant</v-icon>
									</div>
									{{ $t('TableHeader.__gameType') }} ({{ $t('TableHeader.__slots') }})
								</div>
							</div>
						</v-col>

						<v-col cols="12" md="4" sm="6">
							<div class="text-c">
								<div class="selectBox" @click="confirmSelectedGames(2500)">
									<div>
										<v-icon>mdi-camera</v-icon>
									</div>
									{{ $t('TableHeader.__gameType') }} ({{ $t('TableHeader.__liveCasino') }})
								</div>
							</div>
						</v-col>

						<v-col cols="12" md="4" sm="6">
							<div class="text-c">
								<div class="selectBox" @click="confirmSelectedGames(2000)">
									<div>
										<v-icon>mdi-cards-playing-outline</v-icon>
									</div>
									{{ $t('TableHeader.__gameType') }} ({{ $t('TableHeader.__boardGame') }})
								</div>
							</div>
						</v-col>

						<!-- <v-col cols="12" md="4" sm="6">
							<div class="text-c">

								<div class="selectBox" @click="confirmSelectedGames(15777)">
									<div>
										<v-icon>mdi-folder-star</v-icon>
									</div>
									{{ $t('TableHeader.__specialCategory') }} (Promotion)
								</div>

							</div>
						</v-col> -->

						<v-col cols="12" md="4" sm="6" v-if="hasPermission_SuperAdmin">
							<div class="text-c">
								<div class="selectBox" @click="confirmSelectedGames(250000)">
									<div>
										<v-icon>check</v-icon>
									</div>
									勾選Funky遊戲
								</div>
							</div>
						</v-col>

						<v-col cols="12" md="4" sm="6" v-if="hasPermission_SuperAdmin">
							<div class="text-c">
								<div class="selectBox" @click="confirmSelectedGames(290000)">
									<div>
										<v-icon>check</v-icon>
									</div>
									勾選AGG遊戲 (IDG)
								</div>
							</div>
						</v-col>

						<v-col cols="12" md="4" sm="6" v-if="hasPermission_SuperAdmin">
							<div class="text-c">
								<div class="selectBox" @click="confirmSelectedGames(530000)">
									<div>
										<v-icon>check</v-icon>
									</div>
									勾選Tiger9遊戲
								</div>
							</div>
						</v-col>

						<v-col cols="12" md="4" sm="6" v-if="hasPermission_SuperAdmin">
							<div class="text-c">
								<div class="selectBox" @click="confirmSelectedGames(390000)">
									<div>
										<v-icon>check</v-icon>
									</div>
									勾選Jili遊戲
								</div>
							</div>
						</v-col>

						<v-col cols="12" md="4" sm="6" v-if="hasPermission_SuperAdmin">
							<div class="text-c">
								<div class="selectBox" @click="confirmSelectedGames(570000)">
									<div>
										<v-icon>check</v-icon>
									</div>
									勾選CP遊戲
								</div>
							</div>
						</v-col>

						<v-col cols="12" md="4" sm="6" v-if="hasPermission_SuperAdmin">
							<div class="text-c">
								<div class="selectBox" @click="confirmSelectedGames(590000)">
									<div>
										<v-icon>check</v-icon>
									</div>
									勾選RG遊戲
								</div>
							</div>
						</v-col>

						<v-col cols="12" md="4" sm="6" v-if="hasPermission_SuperAdmin">
							<div class="text-c">
								<div class="selectBox" @click="confirmSelectedGames(610000)">
									<div>
										<v-icon>check</v-icon>
									</div>
									勾選SM遊戲
								</div>
							</div>
						</v-col>
					</v-row>
				</v-card-text>
			</v-card>
		</v-dialog>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import EventBus from '@/utilities/event-bus';
import { Component, Prop } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import { Locale } from '@/models/status/common';
import DropdownApi from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import PlayersApi from '@/api/members-management/players';
import DatePicker from 'vue2-datepicker';
import * as ResponseModel from '@/models/interfaces/commons/response';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin, addMinutes } from '@/utilities/datetime-format';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import internalTestApi from '@/api/commons/internalTest';
import * as Permission from '@/models/status/permission';
const authModule = namespace('Auth');

// @ts-ignore
@Component({
	// @ts-ignore
	mixins: [datetimeMixin, commonDisplayFiltersMixin, formatMixin, commonMethodMixin],
	components: { DatePicker },
})
export default class MultipleGameSelectedDialog extends Vue {
	@Prop(String) readonly defaultLang!: string;

	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;

	isShowDialog: boolean = false;
	panel: number[] = [0];
	myGamesDropdowns: DropdownModel.IDropdownItems[] = [];
	resultGameIDs: number[] = [];

	get hasPermission_SuperAdmin() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get hasPermission_AMPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_AM_Permission) >= 0;
	}

	showDialog(myItems: DropdownModel.IDropdownItems[]) {
		this.isShowDialog = true;
		this.myGamesDropdowns = myItems;
		this.resultGameIDs = [];
	}

	closeAllModal() {
		this.isShowDialog = false;
	}

	confirmSelectedGames(gameType: number) {
		this.resultGameIDs = [];

		if (gameType === 1000) {
			this.setGameIDsByRange(1, 1999);
			this.setGameIDsByRange(290000, 290000 + 9999);
		} else if (gameType === 2000) {
			this.setGameIDsByRange(2000, 2999);
		} else if (gameType === 2500) {
			this.setGameIDsByRange(330000, 330000 + 9999);
			this.setGameIDsByRange(530000, 530000 + 9999);
		} else if (gameType === 15777) {
			this.setGameIDsByRange(0, 9999);
			this.setGameIDsByRange(290000, 290000 + 9999);
		} else if (gameType >= 250000 && gameType <= 990000) {
			const minID = gameType;
			const maxID = gameType + 9999;
			this.setGameIDsByRange(minID, maxID);
		}

		this.$emit('parentConfirmMultipleSelectedGames', this.resultGameIDs);
	}

	setGameIDsByRange(min: number, max: number) {
		for (const item of this.myGamesDropdowns) {
			if (item.valueIntOrZero >= min && item.valueIntOrZero <= max) {
				this.resultGameIDs.push(item.valueIntOrZero);
			}
		}
	}

	mounted() {}
}
</script>
<style lang="scss" scope>
.selectBox {
	display: inline-block;
	cursor: pointer;
	margin-bottom: 20px;
}

.selectBox:hover {
	opacity: 0.8;
}

.selectBox .v-icon {
	font-size: 36px !important;
}
</style>
