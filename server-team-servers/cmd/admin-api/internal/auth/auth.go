package auth

import (
	"crypto/md5"
	"fmt"
	"time"

	"example.invalid/dgrijalva/jwt-go"
	"example.invalid/sirupsen/logrus"
	"example.invalid/spf13/viper"
	"example.invalid/grpc/codes"
	"example.invalid/grpc/status"
)

type Auth struct {
	Username string
	Role     string
	jwt.StandardClaims
}

type Service interface {
	AuthAccessToken(accessToken string) error
	AuthUser(username string, password string) (*Auth, string, error)
	ParseAccessToken(accessToken string) (*Auth, error)
	GetAdminUser(username string) *AdminUser
}

func NewService(logger *logrus.Logger) Service {
	return &service{
		Logger:           logger,
		AdminUserManager: NewAdminUserManager(),
	}
}

type service struct {
	Logger           *logrus.Logger
	AdminUserManager *AdminUserManager
}

func (svc *service) AuthAccessToken(accessToken string) error {

	auth := new(Auth)

	_, err := jwt.ParseWithClaims(accessToken, auth, func(t *jwt.Token) (interface{}, error) {
		return []byte(viper.GetString("auth.token.key")), nil
	})
	if err != nil {
		return status.Errorf(codes.PermissionDenied, "parse access token error, %v", err)
	}

	return nil
}

func (svc *service) AuthUser(username string, password string) (*Auth, string, error) {

	adminUser := svc.AdminUserManager.GetAdminUser(username)

	if adminUser == nil {
		return nil, "", status.Error(codes.PermissionDenied, "username or password is invalid 1")
	}
	passwd := fmt.Sprintf("%x", md5.Sum([]byte(adminUser.Password)))
	if passwd != password {
		return nil, "", status.Error(codes.PermissionDenied, "username or password is invalid 2")
	}

	auth := Auth{
		Username: username,
		Role:     "admin",
	}

	token, err := svc.createAccessToken(auth)
	if err != nil {
		return nil, "", status.Errorf(codes.Internal, "create access_token error, %v", err)
	}

	return &auth, token, nil
}

func (svc *service) createAccessToken(src Auth) (string, error) {
	now := time.Now()
	expire := now.Add(viper.GetDuration("auth.token.expire"))

	src.StandardClaims = jwt.StandardClaims{
		ExpiresAt: expire.Unix(),
		IssuedAt:  now.Unix(),
	}

	t := jwt.NewWithClaims(jwt.SigningMethodHS256, src)

	return t.SignedString([]byte(viper.GetString("auth.token.key")))
}

func (svc *service) GetAdminUser(username string) *AdminUser {
	return svc.AdminUserManager.GetAdminUser(username)
}

func (svc *service) ParseAccessToken(accessToken string) (*Auth, error) {

	auth := new(Auth)

	_, err := jwt.ParseWithClaims(accessToken, auth, func(t *jwt.Token) (interface{}, error) {
		return []byte(viper.GetString("auth.token.key")), nil
	})
	if err != nil {
		return nil, status.Errorf(codes.PermissionDenied, "parse access token error, %v", err)
	}

	return auth, nil
}
