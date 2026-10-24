package helper

import (
	"context"
	"fmt"
	"os"
	"strings"
	"sync"
	"time"

	"example.invalid/spf13/cobra"
	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/slot/baselogic"
)

// SlotHelperCmd 查詢房間狀態指令
type SlotHelperCmd struct {
	glm           *gamelogic.GameLogicManager
	BaseLogicFunc func(gl gamelogic.GameLogic) *baselogic.SlotLogicBase
	traceMap      map[int32]bool
	mu            sync.Mutex

	rootCmd *cobra.Command
}

// NewSlotHelperCmd Create SlotHelperCmd
func NewSlotHelperCmd(glm *gamelogic.GameLogicManager) *SlotHelperCmd {
	cmd := new(SlotHelperCmd)
	cmd.glm = glm
	cmd.traceMap = make(map[int32]bool)

	cmd.rootCmd = cmd.CobraRoot()

	cmd.rootCmd.SetIn(os.Stdin)
	cmd.rootCmd.SetOut(os.Stdout)
	cmd.rootCmd.SetErr(os.Stderr)

	cmd.rootCmd.AddCommand(cmd.infoCmd())
	cmd.rootCmd.AddCommand(cmd.protoCmd())
	cmd.rootCmd.AddCommand(cmd.traceCmd())

	return cmd
}

// Name Command name
func (h *SlotHelperCmd) Name() string {
	return "slot"
}

// Help Command Description
func (h *SlotHelperCmd) Help() string {
	return fmt.Sprintf("Slot 遊戲相關工具\t輸入『slot help』可查詢功能說明")
}

// Exec Command Logic
func (h *SlotHelperCmd) Exec(args []string) string {
	h.rootCmd.SetArgs(args)
	h.rootCmd.Execute()
	return ""
}

// CobraRoot 取得 cobra command
func (h *SlotHelperCmd) CobraRoot() *cobra.Command {
	return &cobra.Command{
		Use:   "slot",
		Short: "Slot 遊戲相關工具",
		Args:  cobra.NoArgs,
		Run: func(cmd *cobra.Command, args []string) {
			cmd.Usage()
		},
	}
}
func (h *SlotHelperCmd) infoCmd() *cobra.Command {
	infoCmd := &cobra.Command{
		Use:          "info",
		Short:        "查看遊戲設定",
		SilenceUsage: true,
		RunE: func(cmd *cobra.Command, args []string) error {

			if h.BaseLogicFunc == nil {
				return fmt.Errorf("沒有設定取得房間的程式")
			}

			// 抓一間房間來印
			l := h.glm.GetGameLogic(0)
			if l == nil {
				return fmt.Errorf("沒有設置房間")
			}

			logic := h.BaseLogicFunc(l)

			PrintLogicInfo(cmd.OutOrStdout(), logic)

			return nil
		},
	}

	return infoCmd
}

func (h *SlotHelperCmd) protoCmd() *cobra.Command {
	protoCmd := &cobra.Command{
		Use:          "proto",
		Short:        "查看遊戲機率設定",
		Hidden:       true,
		SilenceUsage: true,
		RunE: func(cmd *cobra.Command, args []string) error {

			if h.BaseLogicFunc == nil {
				return fmt.Errorf("沒有設定取得房間的程式")
			}

			// 抓一間房間來印
			l := h.glm.GetGameLogic(0)
			if l == nil {
				return fmt.Errorf("沒有設置房間")
			}

			logic := h.BaseLogicFunc(l)
			pwd, _ := cmd.PersistentFlags().GetString("password")

			if len(pwd) > 0 && pwd != conf.Password {
				return fmt.Errorf("輸入密碼錯誤")
			}

			PrintLogicProto(cmd.OutOrStdout(), logic, pwd == conf.Password)

			return nil
		},
	}
	protoCmd.PersistentFlags().StringP("table", "t", "", "指定查看類型[logic/proto]")
	protoCmd.PersistentFlags().StringP("password", "p", "", "看詳細資訊")

	return protoCmd
}

func (h *SlotHelperCmd) traceCmd() *cobra.Command {

	examples := []string{
		"slot trace -t 0 \t(追蹤第0桌1分鐘)",
		"slot trace -t 10 -d 5m \t(追蹤第10桌5分鐘)",
	}

	traceCmd := &cobra.Command{
		Use:     "trace",
		Short:   "追蹤房間封包",
		Long:    "追蹤房間封包，顯示封包內容，追蹤期間暫時無法輸入指令",
		Example: strings.Join(examples, "\n"),
		RunE: func(cmd *cobra.Command, args []string) error {

			if h.BaseLogicFunc == nil {
				return fmt.Errorf("沒有設定取得房間的程式")
			}

			tableno, err := cmd.PersistentFlags().GetInt32("table")
			if err != nil {
				return fmt.Errorf("桌號輸入格式錯誤, %v", err)
			}

			if tableno < 0 {
				return fmt.Errorf("請指定桌號")
			}

			l := h.glm.GetGameLogic(tableno)
			if l == nil {
				return fmt.Errorf("查無房間(%d)", tableno)
			}

			d, err := cmd.PersistentFlags().GetDuration("duration")
			if err != nil {
				return fmt.Errorf("監控時間輸入格式錯誤, %v", err)
			}

			logic := h.BaseLogicFunc(l)

			f := logic.SendMessgeFunc
			l.OnSendMessage(func(userid int32, m *msg.Message) {
				h.mu.Lock()
				if val, ok := h.traceMap[tableno]; ok && val {
					PrintMessage(os.Stdout, m)
				}
				h.mu.Unlock()
				f(userid, m)
			})

			ctx, cancel := context.WithTimeout(context.Background(), d)
			defer cancel()

			go func() {
				h.mu.Lock()
				defer h.mu.Unlock()
				h.traceMap[tableno] = true
				cmd.Printf("%s 開始追蹤遊戲封包, 桌號: %d, 時間: %v\n", time.Now().Format("2006-01-02 15:04:05"), tableno, d)
			}()

			select {
			case <-ctx.Done():
				h.mu.Lock()
				defer h.mu.Unlock()
				h.traceMap[tableno] = false
				cmd.Printf("%s 停止追蹤遊戲封包, 桌號: %d\n", time.Now().Format("2006-01-02 15:04:05"), tableno)
			}

			return nil
		},
	}
	traceCmd.PersistentFlags().Int32P("table", "t", 0, "桌號")
	traceCmd.PersistentFlags().DurationP("duration", "d", time.Minute, "追蹤時間")

	traceCmd.MarkPersistentFlagRequired("table")

	return traceCmd
}
