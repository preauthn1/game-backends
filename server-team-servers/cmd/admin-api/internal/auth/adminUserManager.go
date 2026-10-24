package auth

import "example.invalid/spf13/viper"

type AdminUser struct {
	Username   string
	Password   string
	AllowRooms []int32
}

func (a *AdminUser) IsAllowRoom(roomID int32) bool {
	for _, allowRoom := range a.AllowRooms {
		if allowRoom == roomID {
			return true
		}
	}
	return false
}

type AdminUserManager struct {
	AdminUsers []AdminUser
}

func NewAdminUserManager() *AdminUserManager {
	adminUserManager := &AdminUserManager{}
	err := viper.UnmarshalKey("auth.admin.users", &adminUserManager.AdminUsers)
	if err != nil {
		panic(err)
	}
	return adminUserManager
}

func (a *AdminUserManager) GetAdminUser(username string) *AdminUser {
	for _, adminUser := range a.AdminUsers {
		if adminUser.Username == username {
			return &adminUser
		}
	}
	return nil
}
