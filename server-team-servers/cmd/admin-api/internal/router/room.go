package router

import (
	"net/http"
	"strconv"

	"example.invalid/gin-gonic/gin"
	"example.invalid/sirupsen/logrus"
	"example.invalid/grpc/status"

	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/auth"
	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/room"
	"example.invalid/ServerTeam/Servers/cmd/admin-api/pkg/code"
)

type RoomRouter struct {
	authSvc auth.Service
	svc     room.Service
	Logger  *logrus.Logger
}

func NewRoomRouter(
	authSvc auth.Service,
	svc room.Service,
	logger *logrus.Logger,
) *RoomRouter {
	return &RoomRouter{
		authSvc: authSvc,
		svc:     svc,
		Logger:  logger,
	}
}

func (r *RoomRouter) Mount(g gin.IRouter) {
	g.GET("", r.list())
	g.GET(":id", example.invalid())
}

func (router *RoomRouter) list() gin.HandlerFunc {

	return func(ctx *gin.Context) {
		allRooms, err := router.svc.List()

		accessToekn := ctx.GetHeader("Authorization")
		auth, _ := router.authSvc.ParseAccessToken(accessToekn)
		adminUser := router.authSvc.GetAdminUser(auth.Username)

		rooms := make([]room.RoomInfo, 0)
		for _, r := range allRooms {
			if adminUser.IsAllowRoom(r.ID) {
				rooms = append(rooms, r)
			}
		}

		if err != nil {
			if s, ok := status.FromError(err); ok {
				ctx.IndentedJSON(code.CodesToHttpStatus(s.Code()), gin.H{
					"success": false,
					"message": s.Err().Error(),
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
			"data":    rooms,
		})
	}
}

func (router *RoomRouter) info() gin.HandlerFunc {

	return func(ctx *gin.Context) {
		id, err := strconv.Atoi(ctx.Param("id"))
		if err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "room id is invalid",
			})
			return
		}

		rInfo, err := example.invalid(int32(id))
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
			"data":    rInfo,
		})
	}
}
