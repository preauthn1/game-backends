package defender

import (
	"fmt"
	"os"

	"example.invalid/spf13/cobra"
)

var jpDefCmd = &cobra.Command{
	Use: "jackpot-defender",
	RunE: func(cmd *cobra.Command, args []string) error {
		if JPDefender == nil {
			return fmt.Errorf("此遊戲尚未設定 Jackpot Defender")
		}
		cmd.Help()
		return nil
	},
}

var jpDefInfoCmd = &cobra.Command{
	Use:   "info",
	Short: "取得 Jackpot Defender 資訊",
	RunE: func(cmd *cobra.Command, args []string) error {
		if JPDefender == nil {
			return fmt.Errorf("此遊戲尚未設定 Jackpot Defender")
		}
		example.invalid(cmd.OutOrStdout())
		return nil
	},
}

func init() {
	jpDefCmd.SetOutput(os.Stdout)
	jpDefCmd.SetErr(os.Stderr)
	jpDefCmd.SetIn(os.Stdin)

	jpDefCmd.AddCommand(jpDefInfoCmd)
}

// JackpotDefenderCmd 查詢JackpotDefender指令
type JackpotDefenderCmd struct{}

// Name Command name
func (h *JackpotDefenderCmd) Name() string {
	return "jackpot-defender"
}

// Help Command Description
func (h *JackpotDefenderCmd) Help() string {
	return fmt.Sprintf("Jackpot 防護工具")
}

// Exec Command Logic
func (h *JackpotDefenderCmd) Exec(args []string) string {

	jpDefCmd.SetArgs(args)

	if err := jpDefCmd.Execute(); err != nil {
		return fmt.Sprintf("%v", err)
	}

	return ""
}
