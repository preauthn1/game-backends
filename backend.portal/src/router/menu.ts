import PermissionApi from '@/api/commons/permission';
import * as PermissionModel from '@/models/interfaces/commons/permission';
const getMenuText = (nestedPermissions: PermissionModel.INestedPermission[], path: string, apiPaths: any, callback: any) => {
	if (apiPaths.length > 0) {
		computeMenuTree(apiPaths, path, apiPaths, callback);
	} else {
		if (!nestedPermissions || nestedPermissions.length < 1) {
			PermissionApi.getMyPermissions().then(res => {
				nestedPermissions = res.data.nestedPermissions;
				computeMenuTree(nestedPermissions, path, apiPaths, callback);
			});
		} else {
			computeMenuTree(nestedPermissions, path, apiPaths, callback);
		}
	}
};

const computeMenuTree = (nestedPermissions: PermissionModel.INestedPermission[], path: string, apiPaths: any, callback: any) => {
	const menuTree = nestedPermissions.map((parent: PermissionModel.INestedPermission) => {
		return {
			icon: parent.token,
			activator: parent.token,
			'icon-alt': parent.token,
			text: parent.displayName,
			routerPath: parent.pageUrl,
			children: parent.childPermissions.map((child: PermissionModel.INestedPermission) => {
				return {
					icon: child.token,
					activator: child.token,
					'icon-alt': child.token,
					active: false,
					permissionID: child.permissionID,
					text: child.displayName,
					routerPath: child.pageUrl,
				};
			}),
		};
	});
	// PermissionApi.getMyPermissions().then(res => {
	menuTree.forEach(item => {
		if (item.routerPath === path) {
			callback(item);
		}
		const children = item.children;
		children.forEach(child => {
			if (child.routerPath === path) {
				callback(child);
			}
		});
	});
	// });
};

export { getMenuText };
