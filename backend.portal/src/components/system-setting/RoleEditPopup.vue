<template>
	<v-card>
		<v-card-title class="grey darken-2">
			{{editedItem.roleID === -1? `${$t('Popup.__create')} ${$t('TableHeader.__role')}`:`${$t('Popup.__update')} ${$t('TableHeader.__role')} #${editedItem.roleID}`}}
		</v-card-title>
		<v-spacer></v-spacer>
		<v-card-text>
			<v-flex>
				<v-row>

					<v-col cols="12" md="12" sm="12">
						<v-text-field class="required" v-model="editedItem.roleName"
							:label="$t('TableHeader.__roleName')"
							:rules="[value => !!value || $t('Validation.__requiredRules')]" required clearable>
						</v-text-field>
					</v-col>

					<v-col cols="12" md="12" sm="12" class="minus-mt-4">
						<v-text-field class="required" v-model="editedItem.memo"
							:label="$t('TableHeader.__memo')">
						</v-text-field>
					</v-col>

					<v-col cols="12" md="12" sm="12" class="minus-mt-4">
						<v-text-field class="required" v-model="editedItem.sort"
							:label="$t('TableHeader.__sort')">
						</v-text-field>
					</v-col>

				</v-row>

			</v-flex>
		</v-card-text>
		<v-card-actions>
			<v-spacer></v-spacer>
			<v-btn text @click="$emit('close-update-role')" class="btn">
				<v-icon left>mdi-cancel</v-icon>
				{{$t('Button.__cancel')}}
			</v-btn>
			<v-btn color="yellow darken-1" text @click="save" class="btn">
				<v-icon left>mdi-check-circle</v-icon>
				{{$t('Button.__submit')}}
			</v-btn>
		</v-card-actions>
	</v-card>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import RolesApi from '@/api/system-setting/roles';
import * as Model from '@/models/interfaces/system-setting/roles';
// import Validation from '@/utilities/validation';
@Component
export default class EditRolePopup extends Vue {
	@Prop(Object) readonly propsData!: Model.IRole;
	editedItem: Model.IUpdateRoleRequest = {} as Model.IUpdateRoleRequest;
	rolesDropdownItems: DropdownModel.IDropdownItems[] = [];
	selectedRole: DropdownModel.IDropdownItems = {} as DropdownModel.IDropdownItems;

	save() {
		if (this.propsData.roleID === -1) {
			const form = {} as Model.ICreateRoleRequest;
			form.roleName = this.editedItem.roleName;
			form.sort = this.editedItem.sort;
			form.memo = this.editedItem.memo;

			RolesApi.createRole(form).then(() => {
				this.$emit('update-role-success');
			});
		} else {
			RolesApi.updateRole(this.editedItem).then(() => {
				this.$emit('update-role-success');
			});
		}
	}

	created() {
		this.editedItem = this.propsData;
	}
}
</script>

