package main

import (
	"reflect"
	"strings"

	"example.invalid/gin-gonic/gin"
	"example.invalid/spf13/cobra"
	"example.invalid/spf13/viper"
	"example.invalid/dig"

	"example.invalid/ServerTeam/Servers/cmd/admin-api/di"
	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/auth"
	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/cjlhlp"
	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/jzyq"
	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/lhd"
	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/room"
	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/router"
	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/router/admin"
	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/sml"
)

func main() {
	rootCmd.Execute()
}

var rootCmd = &cobra.Command{
	Use:     "admin-api",
	Short:   "",
	PreRunE: initViper,
	RunE: func(cmd *cobra.Command, args []string) error {

		appCtx := dig.New()

		{ // Logger
			if err := appCtx.Provide(di.NewLoguraseLogger); err != nil {
				return err
			}
		}

		{ // Configs
			appCtx.Provide(func() (room.ConfList, room.ConfMap, error) {

				var confL []room.Conf
				var confM = make(map[int32]room.Conf)

				err := viper.UnmarshalKey("grpc.rooms", &confL)
				if err != nil {
					return nil, nil, err
				}

				for _, conf := range confL {
					confM[conf.ServerID] = conf
				}

				return confL, confM, nil
			})
		}

		// { // Auth
		// 	appCtx.Provide(auth.ParseConfigFromViper)
		// 	appCtx.Provide(auth.New)
		// }

		{ // Services
			appCtx.Provide(auth.NewService)
			appCtx.Provide(room.NewService)
			appCtx.Provide(lhd.NewService)
			appCtx.Provide(sml.NewService)
			appCtx.Provide(jzyq.NewService)
			appCtx.Provide(cjlhlp.NewService)
		}

		{ // Routers
			appCtx.Provide(router.NewAuthRouter)
			appCtx.Provide(router.NewForwardRouter)
			appCtx.Provide(router.NewRoomRouter)
			appCtx.Provide(admin.NewLHDRouter)
			appCtx.Provide(admin.NewSMLRouter)
			appCtx.Provide(admin.NewJZYQRouter)
			appCtx.Provide(admin.NewCJLHLPRouter)
		}

		{ // Server
			appCtx.Provide(gin.Default)
		}

		if err := appCtx.Invoke(router.Mount); err != nil {
			return err
		}

		return appCtx.Invoke(func(r *gin.Engine) error {
			return r.Run(viper.GetString("server.addr"))
		})
	},
}

func initViper(cmd *cobra.Command, args []string) (err error) {
	viper.SetConfigName("config")
	viper.SetConfigType("yaml")
	viper.AddConfigPath(".")

	if err := viper.ReadInConfig(); err != nil && reflect.TypeOf(err) != reflect.TypeOf(viper.ConfigFileNotFoundError{}) {
		return err
	}

	cmd.Printf("Using config file: %s\n", viper.GetViper().ConfigFileUsed())

	viper.AutomaticEnv()
	viper.SetEnvPrefix("ADMINAPI")
	viper.SetEnvKeyReplacer(strings.NewReplacer(".", "_"))

	if err := viper.BindPFlags(cmd.Flags()); err != nil {
		return err
	}

	return nil
}
