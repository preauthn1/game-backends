package admin

import (
	"encoding/json"
	"io"
	"net/http"

	"example.invalid/gin-gonic/gin"
	"example.invalid/sirupsen/logrus"
	"example.invalid/grpc/status"

	pb "example.invalid/ServerTeam/Servers/internal/cli-api-svc/pb/cliapisvc"

	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/lhd"
	"example.invalid/ServerTeam/Servers/cmd/admin-api/pkg/code"
)

type LHDRouter struct {
	svc    lhd.Service
	Logger *logrus.Logger
}

func NewLHDRouter(
	svc lhd.Service,
	logger *logrus.Logger,
) *LHDRouter {
	return &LHDRouter{
		svc:    svc,
		Logger: logger,
	}
}

func (router *LHDRouter) Mount(g gin.IRouter) {
	g.GET(":rid/:tid", router.dashboard())
	g.GET(":rid/:tid/stream", router.stream())
	g.POST(":rid/:tid/cheat", router.cheat())
	g.POST(":rid/:tid/special_reward", router.specialReward())
	g.PUT(":rid/:tid/increase_sys_pool/:amount", router.increaseSysPool())
	g.PUT(":rid/:tid/assign_next_award/:award", router.assignNextAward())
	g.PUT(":rid/:tid/set_table_disabled/:disabled", router.setTableDisabled())
	g.PUT(":rid/:tid/set_jackpot_enabled/:enabled", router.setJackpotEnabled())
	g.PUT(":rid/:tid/assign_jackpot_to_random_player/:level", router.assignJackpotToRandomPlayer())
	g.PUT(":rid/:tid/assign_jackpot_to_recycel/:level", router.assignJackpotToRecycle())
	g.PUT(":rid/:tid/edit_jackpot_pool/:amount", router.editJackpotPool())
}

func (router *LHDRouter) dashboard() gin.HandlerFunc {

	type Params struct {
		RoomID  int32 `uri:"rid"`
		TableID int32 `uri:"tid"`
	}

	return func(ctx *gin.Context) {
		params := Params{}
		if err := ctx.ShouldBindUri(&params); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "params is invalid",
			})
			return
		}

		data, err := router.svc.Dashboard(params.RoomID, params.TableID)
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
			"data":    data,
		})
	}
}

func (router *LHDRouter) stream() gin.HandlerFunc {
	type Params struct {
		RoomID  int32 `uri:"rid"`
		TableID int32 `uri:"tid"`
	}

	return func(ctx *gin.Context) {
		params := Params{}
		if err := ctx.ShouldBindUri(&params); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "params is invalid",
			})
			return
		}

		ch := make(chan *pb.LHDTableDashboard, 10)

		go router.svc.Stream(params.RoomID, params.TableID, ctx.Request.Context(), ch)

		ctx.Stream(func(w io.Writer) bool {
			select {
			case <-ctx.Request.Context().Done():
				return false
			case m, ok := <-ch:
				if !ok {
					return false
				}

				b, err := json.Marshal(m)
				if err != nil {
					return true
				}

				ctx.SSEvent("message", string(b))
				return true
			}
		})
	}
}

func (router *LHDRouter) cheat() gin.HandlerFunc {

	type Params struct {
		RoomID    int32 `uri:"rid"`
		TableID   int32 `uri:"tid"`
		KickCount int32 `json:"kick_count"`
	}

	return func(ctx *gin.Context) {
		params := Params{}
		if err := ctx.ShouldBindUri(&params); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "params is invalid",
			})
			return
		}

		if err := ctx.ShouldBindJSON(&params); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "params is invalid",
			})
			return
		}

		if err := router.svc.Cheat(params.RoomID, params.TableID, params.KickCount); err != nil {
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
		})
	}
}

func (router *LHDRouter) specialReward() gin.HandlerFunc {
	type Params struct {
		RoomID  int32 `uri:"rid"`
		TableID int32 `uri:"tid"`
		Count   int32 `json:"count"`
		Rate    int32 `json:"rate"`
	}

	return func(ctx *gin.Context) {

		params := Params{}
		if err := ctx.ShouldBindUri(&params); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "params RoomID / TableID is invalid",
			})
			return
		}

		if err := ctx.ShouldBindJSON(&params); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "json body `count` / `rate` is invalid",
			})
			return
		}

		if err := router.svc.SetSpecialRewardCount(params.RoomID, params.TableID, params.Count, params.Rate); err != nil {
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
		})
	}
}

func (router *LHDRouter) increaseSysPool() gin.HandlerFunc {
	type Params struct {
		RoomID  int32 `uri:"rid"`
		TableID int32 `uri:"tid"`
		Amount  int64 `uri:"amount"`
	}

	return func(ctx *gin.Context) {
		params := Params{}
		if err := ctx.ShouldBindUri(&params); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "params is invalid",
			})
			return
		}

		if err := router.svc.IncreaseSysPool(params.RoomID, params.TableID, params.Amount); err != nil {
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
		})
	}
}

func (router *LHDRouter) assignNextAward() gin.HandlerFunc {
	type Params struct {
		RoomID  int32 `uri:"rid"`
		TableID int32 `uri:"tid"`
		Award   int32 `uri:"award"`
	}

	return func(ctx *gin.Context) {
		params := Params{}
		if err := ctx.ShouldBindUri(&params); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "params is invalid",
			})
			return
		}

		if err := router.svc.AssignNextAward(params.RoomID, params.TableID, params.Award); err != nil {
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
		})
	}
}

func (router *LHDRouter) setTableDisabled() gin.HandlerFunc {
	type Params struct {
		RoomID   int32 `uri:"rid"`
		TableID  int32 `uri:"tid"`
		Disabled bool  `uri:"disabled"`
	}

	return func(ctx *gin.Context) {
		params := Params{}
		if err := ctx.ShouldBindUri(&params); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "params is invalid",
			})
			return
		}

		if err := router.svc.SetTableDisabled(params.RoomID, params.TableID, params.Disabled); err != nil {
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
		})
	}
}

func (router *LHDRouter) setJackpotEnabled() gin.HandlerFunc {
	type Params struct {
		RoomID  int32 `uri:"rid"`
		TableID int32 `uri:"tid"`
		Enabled bool  `uri:"enabled"`
	}

	return func(ctx *gin.Context) {
		params := Params{}
		if err := ctx.ShouldBindUri(&params); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "params is invalid",
			})
			return
		}

		if err := router.svc.SetJackpotEnabled(params.RoomID, params.TableID, params.Enabled); err != nil {
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
		})
	}
}

func (router *LHDRouter) assignJackpotToRandomPlayer() gin.HandlerFunc {
	type Params struct {
		RoomID  int32 `uri:"rid"`
		TableID int32 `uri:"tid"`
		Level   int32 `uri:"level"`
	}

	return func(ctx *gin.Context) {
		params := Params{}
		if err := ctx.ShouldBindUri(&params); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "params is invalid",
			})
			return
		}

		if err := router.svc.AssignJackpotToRandomPlayer(params.RoomID, params.TableID, params.Level); err != nil {
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
		})
	}
}

func (router *LHDRouter) assignJackpotToRecycle() gin.HandlerFunc {
	type Params struct {
		RoomID  int32 `uri:"rid"`
		TableID int32 `uri:"tid"`
		Level   int32 `uri:"level"`
	}

	return func(ctx *gin.Context) {
		params := Params{}
		if err := ctx.ShouldBindUri(&params); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "params is invalid",
			})
			return
		}

		if err := router.svc.AssignJackpotToRecycle(params.RoomID, params.TableID, params.Level); err != nil {
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
		})
	}
}

func (router *LHDRouter) editJackpotPool() gin.HandlerFunc {
	type Params struct {
		RoomID  int32 `uri:"rid"`
		TableID int32 `uri:"tid"`
		Amount  int64 `uri:"amount"`
	}

	return func(ctx *gin.Context) {
		params := Params{}
		if err := ctx.ShouldBindUri(&params); err != nil {
			ctx.IndentedJSON(http.StatusBadRequest, gin.H{
				"success": false,
				"message": "params is invalid",
			})
			return
		}

		if err := router.svc.EditJackpotPool(params.RoomID, params.TableID, params.Amount); err != nil {
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
		})
	}
}
