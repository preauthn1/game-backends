import Vue from 'vue';
export default class FormValidationMixin extends Vue {
	// requiredRules: any = (value: string) => !!value || '必填项目.';
	// accountRules: any = (value: string) => (value && value.length >= 4 && value.length <= 30) || '帐号必须为4-30位的字元';
	// passwordRules: any = (value: string) => (value && /^(?=.*[a-zA-Z]+)(?=.*[0-9]+)[a-zA-Z0-9]+$/.test(value) && value.length >= 6 && value.length <= 30) || '密码必须为6-30位的数字和字母的组合';
	// numberRules: any = (value: number) => (!!value && Number(value) > 0) || '必须为正值数字.';
}
