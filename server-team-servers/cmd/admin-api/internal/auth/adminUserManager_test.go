package auth

import "testing"

func TestAdminUser(t *testing.T) {
	// Test Viper config parser
	initViperTest()
	adminUserManager := NewAdminUserManager()
	if adminUser := adminUserManager.GetAdminUser("admin"); adminUser == nil {
		t.Error("Get admin user error")
	} // Get admin user
}
