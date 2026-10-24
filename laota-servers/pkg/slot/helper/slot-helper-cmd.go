package helper

import (
	"fmt"
	"os"
	"strconv"
	"sync"

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
}

// NewSlotHelperCmd Create SlotHelperCmd
func NewSlotHelperCmd(glm *gamelogic.GameLogicManager) *SlotHelperCmd {
	cmd := new(SlotHelperCmd)
	cmd.glm = glm
	cmd.traceMap = make(map[int32]bool)
	// cmd.mu = sync.Mutex{}
	return cmd
}

// Name Command name
func (h *SlotHelperCmd) Name() string {
	return "slot"
}

// Help Command Description
func (h *SlotHelperCmd) Help() string {
	return fmt.Sprintf("Slot 相關工具")
}

// Exec Command Logic
func (h *SlotHelperCmd) Exec(args []string) string {
	if len(args) == 0 {
		return "請輸入指令"
	}

	var password string = "ㄎㄢˋㄙㄢㄒㄧㄠˇ"
	cmdname := args[0]

	switch cmdname {
	case "info":

		args = append([]string(nil), args[1:]...)

		if h.BaseLogicFunc == nil {
			return "沒有設定取得房間的程式"
		}

		// 抓一間房間來印
		l := h.glm.GetGameLogic(0)
		if l == nil {
			return "沒有設置房間"
		}

		logic := h.BaseLogicFunc(l)

		if len(args) == 0 {
			PrintLogicInfo(os.Stdout, logic)
			PrintLogicProto(os.Stdout, logic, false)
			return ""
		}

		switch args[0] {
		case "proto":
			args = append([]string(nil), args[1:]...)

			if len(args) == 0 {
				PrintLogicProto(os.Stdout, logic, false)
				return ""
			}

			var deep bool
			if args[0] == password {
				deep = true
			}

			PrintLogicProto(os.Stdout, logic, deep)

			return ""
		case "logic":
			PrintLogicInfo(os.Stdout, logic)
			return ""
		case password:
			[REDACTED], logic)
			PrintLogicProto(os.Stdout, logic, true)
			return ""
		}

		return ""
	case "proto":

		if h.BaseLogicFunc == nil {
			return "沒有設定取得房間的程式"
		}

		// 抓一間房間來印
		l := h.glm.GetGameLogic(0)
		if l == nil {
			return "沒有設置房間"
		}

		logic := h.BaseLogicFunc(l)
		PrintLogicInfo(os.Stdout, logic)

		return ""

	case "trace":

		args = append([]string(nil), args[1:]...)

		if len(args) == 0 {
			return "command error, please enter: slot trace {tableno} on/off"
		}

		var tableno int32
		if f, err := strconv.ParseFloat(args[0], 32); err == nil {
			tableno = int32(f)
		} else {
			return "command error, {tableno} must be a number"
		}

		if h.BaseLogicFunc == nil {
			return "沒有設定取得房間的程式"
		}

		// 抓一間房間來印
		l := h.glm.GetGameLogic(tableno)
		if l == nil {
			return fmt.Sprintf("查無房間(%d)", tableno)
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

		if args[1] == "on" {
			h.mu.Lock()
			defer h.mu.Unlock()
			h.traceMap[tableno] = true
			return fmt.Sprintf("Tracing table message, table %d", tableno)
		} else if args[1] == "off" {
			h.mu.Lock()
			defer h.mu.Unlock()
			h.traceMap[tableno] = false
			return fmt.Sprintf("Untracing table message, table %d", tableno)
		} else {
			return "command error, please enter: slot trace {tableno} on/off"
		}
	}

	return "fail"
}
