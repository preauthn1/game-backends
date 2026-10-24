<template>
	<v-app v-if="renderComponent" class="app">

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-header>
					<template v-slot:actions>
						<v-icon color="primary">filter_list</v-icon>
					</template>
					{{$t(`Filter.__filter`)}}
				</v-expansion-panel-header>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-autocomplete clearable :items="dropdownGames" chips v-model="filterGame"
										:label="$t('TableHeader.__game')">
									</v-autocomplete>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<!-- <div class="item right"> -->
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="searchTable">
										<v-icon left>search</v-icon>
										{{$t('Button.__search')}}
									</v-btn>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{$t('Button.__reset')}}
									</v-btn>&#9;
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table :headers="headers" id="table" :items="ListData" :search="search"
			:page.sync="pageIndex" :items-per-page="itemsPerPage" :footer-props="footerProps" multi-sort>
			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title>
						<h4>{{ $t('TableHeader.__game') }}</h4>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>

					<v-card-title>
						<v-text-field v-model="search" append-icon="search"
							:label="`${$t('TableHeader.__gameID')} / ${$t('TableHeader.__gameName')}`" single-line
							hide-details clearable></v-text-field>
					</v-card-title>

					<v-card-title>
						<v-btn icon dark @click="addItem">
							<v-icon>add</v-icon>
						</v-btn>
					</v-card-title>
				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">

				<div class="text-c">
					<v-icon small class="mr-2" @click="editItem(item)">
						edit
					</v-icon>

				</div>

			</template>

			<template v-slot:item.image="{ item }">

				<img :src="item.imageUrl" style="height:60px;">
			</template>

			<template v-slot:item.gameName="{ item }">
				{{item.gameName}}
			</template>

			<template v-slot:item.gameCode="{ item }">

				{{item.gameCode}}

				<span v-if="item.isThirdPartyGame">
					｜{{ item.thirdPartyGameCode }}
				</span>
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.gameType="{ item }">
				<v-chip class="ma-2">{{$t(`GameType.${gameTypeDic[item.gameType]}`)}}</v-chip>
			</template>

			<template v-slot:item.gameVendorTypeText="{ item }">

				<v-chip class="ma-2" color="">
					{{item.gameVendorTypeText}}
				</v-chip>
			</template>

			<template v-slot:item.isSkipInsertZeroBet="{ item }">

				<div class="statusChips">

					<span v-if="!item.isSkipInsertZeroBet">
						<v-chip color="grey darken-1" class="ma-2">{{$t(`Status.Disable`)}}</v-chip>
					</span>

					<span v-if="item.isSkipInsertZeroBet">
						<v-chip color="green darken-2" class="ma-2">{{$t(`Status.Enable`)}}</v-chip>
					</span>

				</div>

			</template>

			<template v-slot:item.status="{ item }">

				<div class="statusChips">
					<v-chip v-show="status.PermanentDelete === item.status" class="ma-2">
						{{$t(`Status.${status[item.status]}`)}}
					</v-chip>
					<v-chip v-show="status.Disable === item.status" class="ma-2" color="grey darken-1">
						{{$t(`Status.${status[item.status]}`)}}
					</v-chip>
					<v-chip v-show="status.Enable === item.status" class="ma-2" color="green darken-2">
						{{$t(`Status.${status[item.status]}`)}}
					</v-chip>
				</div>

			</template>
		</v-data-table>

		<GamePayTableImageCreate ref="gamePayTableImageCreate" @updateSuccess="updateSuccessReloadData">
		</GamePayTableImageCreate>

	</v-app>

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
import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
// import Validation from '@/utilities/validation';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import GamePayTableImageCreate from '@/components/system-setting/GamePayTableImageCreate.vue';

@Component({ components: { NotificationWarning, GamePayTableImageCreate }, mixins: [commonDisplayFiltersMixin, datetimeMixin] })
export default class GamePayTableImage extends Vue {
	locale: typeof Locale = Locale;
	currency: typeof Currency = Currency;
	status: typeof Status = Status;
	gameTypeDic: typeof GameType = GameType;
	WalletMode: typeof WalletMode = WalletMode;
	CRUDActionEnums: typeof CRUDAction = CRUDAction;
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;
	isShowEditDialog: boolean = false;

	search: string = '';
	ListData: Model.IGamePayTableImageResponse[] = [];

	editedIndex: number = -1;
	editedItem: Model.IGameListRequest = {} as Model.IGameListRequest;
	editAction: CRUDAction = CRUDAction.Create;
	isEdit: boolean = false;
	isThirdPartyGameLockEdit: boolean = false;

	panel: number[] = [0];

	pageIndex: number = 1;
	itemsPerPage: number = 10;
	footerProps: any = { 'items-per-page-options': [10, 20, 100, -1] };

	filterGame: number | null = null;

	dropdownGames: any[] = [];

	get headers() {
		const data = [
			{ text: '圖片', value: 'image', sortable: false, align: 'start', width: '5%' },
			{ text: this.$t('TableHeader.__gameID'), value: 'gameID', sortable: false, align: 'start', width: '5%' },
			{ text: this.$t('TableHeader.__displayName'), value: 'gameName', sortable: false, width: '10%' },

			{ text: '遊戲結果代碼', value: 'code', sortable: false, width: '7%' },
			{ text: '網址', value: 'imageUrl', sortable: false, width: '15%' },
			{ text: this.$t('TableHeader.__memo'), value: 'memo', sortable: false, width: '10%' },
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime', sortable: false, width: '10%' },
		];

		if (true) {
			data.splice(0, 0, { text: this.$t('TableHeader.__action'), value: 'action', align: 'center', sortable: false, width: '8%' });
		}
		return data;
	}

	showGameLaunchDialog(item: Model.IGameList) {
		const targetRef: any = this.$refs.gameLaunchTest;

		const model: Model.IGameLaunchPreview = {} as Model.IGameLaunchPreview;
		model.gameID = item.gameID;
		model.gameName = item.gameName;
		model.vendorName = item.gameVendorTypeText;

		targetRef.showGameLaunchDialog(model);
	}

	showCreateImageDialog(gameID: number, code: string, imageUrl: string) {}

	searchTable() {
		this.pageIndex = 1;
		this.refresh();
	}

	updateSuccessReloadData() {
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		GameListApi.getPayTableImages(<Model.IGamePayTableImageRequest>{
			gameID: this.filterGame,
		}).then((res) => {
			this.ListData = res.data.items;
		});

		this.editedItem.status = Status.Enable;
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	addItem() {
		const targetRef: any = this.$refs.gamePayTableImageCreate;
		targetRef.showCreateEmptyValueDialog();
	}

	editItem(item: Model.IGamePayTableImageResponse) {
		const targetRef: any = this.$refs.gamePayTableImageCreate;
		targetRef.showEditDialog(<Model.IGamePayTableImageEditRequest>{
			gameID: item.gameID,
			memo: item.memo,
			code: item.code,
			imageUrl: item.imageUrl,
		});
	}

	clear() {
		this.filterGame = null;
	}

	mounted() {
		DropdownApi.getGames().then((res) => {
			this.dropdownGames = res;
		});

		this.refresh();
	}
}
</script>

<style>
#getGameLinkDialog .v-text-field__details {
	display: none;
}

.statusChips .v-chip {
	min-width: 70px;
	text-align: center;
}

.statusChips .v-chip .v-chip__content {
	margin: auto;
}
</style>