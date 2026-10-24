package main

import (
	"fmt"
	"os"
	"strings"

	"example.invalid/gin-gonic/gin"
	"example.invalid/spf13/cobra"
	"example.invalid/spf13/viper"

	"example.invalid/ServerTeam/DeployServer/internal/deploy"
)

// InitViper setup viper config
func InitViper(cmd *cobra.Command, args []string) (err error) {
	var appName string
	if cmd.Parent() != nil {
		appName += cmd.Parent().Use
		appName += "."
	}

	appName += cmd.Use

	var cfg string

	if f := cmd.Flags().Lookup("config"); f != nil {
		cfg = f.Value.String()
	}

	if cfg != "" {
		viper.SetConfigFile(cfg)
	} else {
		viper.SetConfigName(appName)
		viper.SetConfigType("yaml")
		viper.AddConfigPath(".")
		viper.AddConfigPath("./configs")
		viper.AddConfigPath("./config")
		viper.AddConfigPath("$HOME/." + appName)
		viper.AddConfigPath("$HOME")
		viper.AddConfigPath("/")
	}

	if err := viper.BindPFlags(cmd.Flags()); err != nil {
		return err
	}

	viper.SetEnvPrefix(appName)
	viper.AutomaticEnv()
	viper.SetEnvKeyReplacer(strings.NewReplacer(".", "_", "-", "_"))

	if err := viper.ReadInConfig(); err != nil {
		return err
	}

	cmd.Println("Using config file:", viper.ConfigFileUsed())

	return nil
}

func init() {
	rootCmd.Flags().StringP("config", "c", "", "Config file location")
	rootCmd.Flags().StringP("server.port", "p", "", "Listen port for http")
}

// rootCmd for delopy
var rootCmd = &cobra.Command{
	Use:               "deploy",
	Short:             "Run Deploy Server",
	PersistentPreRunE: InitViper,
	RunE: func(cmd *cobra.Command, args []string) error {

		cfg := deploy.Config{}
		if err := viper.Unmarshal(&cfg); err != nil {
			return err
		}

		deploy := deploy.New(cfg)

		r := gin.Default()

		// Ping test
		r.GET("/ping", func(c *gin.Context) {
			c.String(200, "pong")
		})

		// Mount Action Deploy
		r.GET("/deploy/:build_name", func(c *gin.Context) {
			buildNum := c.Params.ByName("build_name")
			h := deploy.ActionDeploy(buildNum)
			c.JSON(200, h)
		})

		return r.Run(":" + cfg.Server.Port)
	},
}

func main() {
	if err := rootCmd.Execute(); err != nil {
		fmt.Println(err)
		os.Exit(1)
	}
}
