package router

import (
	"crypto/tls"
	"encoding/json"
	"io/ioutil"
	"net/http"
	"net/url"

	"example.invalid/gin-gonic/gin"
	"example.invalid/sirupsen/logrus"
	"example.invalid/spf13/viper"
)

type ForwardRouter struct {
	Logger *logrus.Logger
}

func NewForwardRouter(
	logger *logrus.Logger,
) *ForwardRouter {
	return &ForwardRouter{
		Logger: logger,
	}
}

func (r *ForwardRouter) Mount(g gin.IRouter) {
	g.POST("bet-summary/server", r.betSummaryServer())
}

func (r *ForwardRouter) betSummaryServer() gin.HandlerFunc {
	return func(ctx *gin.Context) {

		client := http.Client{
			Transport: &http.Transport{
				TLSClientConfig: &tls.Config{
					InsecureSkipVerify: true,
				},
			},
		}

		u := url.URL{
			Scheme: viper.GetString("api.schema"),
			Host:   viper.GetString("api.addr"),
			Path:   "/bet-summary/server",
		}

		r, err := http.NewRequest(http.MethodPost, u.String(), ctx.Request.Body)
		if err != nil {
			ctx.IndentedJSON(http.StatusInternalServerError, gin.H{
				"success": false,
				"message": err.Error(),
			})
			return
		}

		r.Header = ctx.Request.Header.Clone()
		r.Header.Add("X-Api-Certificate", viper.GetString("api.cer"))

		resp, err := client.Do(r)
		if err != nil {
			ctx.IndentedJSON(http.StatusInternalServerError, gin.H{
				"success": false,
				"message": err.Error(),
			})
			return
		}

		defer resp.Body.Close()

		b, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			ctx.IndentedJSON(http.StatusInternalServerError, gin.H{
				"success": false,
				"message": err.Error(),
			})
			return
		}

		res := gin.H{}

		if err := json.Unmarshal(b, &res); err != nil {
			ctx.IndentedJSON(http.StatusInternalServerError, gin.H{
				"success": false,
				"message": err.Error(),
			})
			return
		}

		ctx.IndentedJSON(http.StatusOK, res)
	}
}
