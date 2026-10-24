package auth

import (
	"reflect"
	"testing"

	"example.invalid/dgrijalva/jwt-go"
	"example.invalid/spf13/viper"
)

func TestAuth_AuthAccessToken(t *testing.T) {
	str := "[JWT_REDACTED]"

	auth := new(Auth)

	token, err := jwt.ParseWithClaims(str, auth, func(t *jwt.Token) (interface{}, error) {
		return []byte("z8@*$h96Z*284H(^"), nil
	})

	t.Log(token, err)
}

func initViperTest() (err error) {
	viper.SetConfigName("config.example")
	viper.SetConfigType("yaml")
	viper.AddConfigPath(".")
	viper.AddConfigPath("../../")

	if err := viper.ReadInConfig(); err != nil && reflect.TypeOf(err) != reflect.TypeOf(viper.ConfigFileNotFoundError{}) {
		return err
	}

	return nil
}
