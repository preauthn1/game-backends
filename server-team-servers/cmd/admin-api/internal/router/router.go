package router

import (
	"net/http"

	"example.invalid/gin-contrib/cors"
	"example.invalid/gin-gonic/gin"

	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/router/admin"
)

type Router interface {
	Mount(*gin.Engine)
}

func Mount(g *gin.Engine,
	forwardR *ForwardRouter,
	authR *AuthRouter,
	roomR *RoomRouter,
	lhdR *admin.LHDRouter,
	smlR *admin.SMLRouter,
	jzyqR *admin.JZYQRouter,
	cjlhlpR *admin.CJLHLPRouter,
) {

	{
		config := cors.DefaultConfig()
		config.AddAllowHeaders("Authorization")
		config.AllowAllOrigins = true

		g.Use(cors.New(config))
	}

	var health gin.HandlerFunc = func(ctx *gin.Context) {
		ctx.String(http.StatusOK, "Hello World")
	}

	g.GET("", health)
	g.HEAD("", health)

	authG := g.Group("/auth")
	authR.Mount(authG)

	api := g.Group("/api")
	api.Use(authR.AuthAccessToken())

	{
		forwardG := api.Group("/forward")
		forwardR.Mount(forwardG)
	}

	{
		roomG := api.Group("/room")
		roomR.Mount(roomG)
	}

	{
		adminG := api.Group("/admin")

		{
			lhdG := adminG.Group("/lhd")
			lhdR.Mount(lhdG)
		}
		{
			smlG := adminG.Group("/sml")
			smlR.Mount(smlG)
		}
		{
			jzyqG := adminG.Group("/jzyq")
			jzyqR.Mount(jzyqG)
		}
		{
			cjlhlpG := adminG.Group("/cjlhlp")
			cjlhlpR.Mount(cjlhlpG)
		}
	}

}
