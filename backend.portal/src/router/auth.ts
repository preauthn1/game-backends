/**
 * @description router.beforeEach 檢查
 */
const checkAuth = (to: any, roleStrings: string[]) => {
	return true;
};

export { checkAuth };
