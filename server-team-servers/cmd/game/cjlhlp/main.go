package main

import (
	"fmt"
	"os"

	"example.invalid/spf13/cobra"
	"example.invalid/grpc"

	"example.invalid/ServerTeam/Servers/cmd"

	cliapisvr "example.invalid/ServerTeam/Servers/internal/cli-api-svc"
	gameadm "example.invalid/ServerTeam/Servers/internal/cli-api-svc/game-adm"
	pb "example.invalid/ServerTeam/Servers/internal/cli-api-svc/pb/cliapisvc"
	"example.invalid/ServerTeam/Servers/internal/game/roulette"

	"example.invalid/ServerTeam/Servers/pkg/app"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
)

func main() {
	if err := rootCmd.Execute(); err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}
}

var rootCmd = &cobra.Command{
	Use:   "cjlhlp",
	Short: "超級零號輪盤 Super Zero Roulette",
	RunE:  cmd.PokerCommandRunE,
}

func init() {
	cobra.MousetrapHelpText = ""

	roomserver.GameLogicCreator = roulette.GameLogicCreator

	roomserver.RobotLogicCreator = roulette.RobotLogicCreator

	roomserver.SuperToolCreator = roulette.SuperToolCreator

	rootCmd.AddCommand(cmd.VersionCmd)

	rootCmd.PersistentFlags().StringP("config", "c", "config", "config folder")

	app.Context.Provide(func() cliapisvr.RegisterServiceFunc {
		return func(s *grpc.Server) {
			pb.RegisterGameAdminCJLHLPServer(s, new(gameadm.GameAdminCJLHLPServiceServer))
		}
	})
}
