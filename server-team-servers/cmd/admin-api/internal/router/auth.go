package router

import (
	"net/http"

	"example.invalid/gin-gonic/gin"
	"example.invalid/sirupsen/logrus"
	"example.invalid/grpc/status"

	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/auth"
	"example.invalid/ServerTeam/Servers/cmd/admin-api/pkg/code"
)

type AuthRouter struct {
	svc    auth.Service
	Logger *logrus.Logger
}

func NewAuthRouter(
	svc auth.Service,
	logger *logrus.Logger,
) *AuthRouter {
	return &AuthRouter{
		svc:    svc,
		Logger: logger,
	}
}

func (router *AuthRouter) Mount(g gin.IRouter) {
	g.POST("", router.auth())
}

func (router *AuthRouter) auth() gin.HandlerFunc {

	type Request struct {
		Username string `json:"username"`
		Password string `json:"password"`
	}

	return func(ctx *gin.Context) {

		req := Request{}

		if err := ctx.ShouldBindJSON(&req); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "params is invalid",
			})
			return
		}

		a, token, err := router.svc.AuthUser(req.Username, req.Password)
		if err != nil {
			if s, ok := status.FromError(err); ok {
				ctx.IndentedJSON(code.CodesToHttpStatus(s.Code()), gin.H{
					"success": false,
					"message": s.Message(),
				})
				return
			}

			ctx.IndentedJSON(http.StatusInternalServerError, gin.H{
				"success": false,
				"message": err.Error(),
			})
			return
		}

		ctx.IndentedJSON(http.StatusOK, gin.H{
			"success": true,
			"message": "Success",
			"data": gin.H{
				"auth":         a,
				"access_token": token,
			},
		})
	}
}

func (router *AuthRouter) AuthAccessToken() gin.HandlerFunc {
	return func(ctx *gin.Context) {

		accessToekn := ctx.GetHeader("Authorization")

		if err := router.svc.AuthAccessToken(accessToekn); err != nil {
			ctx.IndentedJSON(http.StatusForbidden, gin.H{
				"success": false,
				"message": "permission denied",
			})
			ctx.AbortWithStatus(http.StatusForbidden)
		}
	}
}
