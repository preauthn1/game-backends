export interface User {
    name: string
    role: string
    access_token: [REDACTED]
}

export interface APIResponse {
    success: boolean
    message: string
    data: any
}

export interface RoomInfo {
    GameCode: string
    ID: number
    Name: string
    GameID: number
    GameName: string
    Flag: number
    Tables: TableInfo[]
    PlayerCnt: number
    RobotCnt: number
}

export interface TableInfo {
    ID: number
    Gaming: boolean
    Summary: string
    Players: PlayerInfo[]
    Robots: PlayerInfo[]
    SysPool: number // 系統水池
    SysPoolWinLine: number // 系統水池贏錢線
}

export interface PlayerInfo {
    ID: number
    IsRobot: boolean
}

export interface LHDPlayerInfo {
    PID: number
    BetAmount: number
    IsRobot: boolean
    Long: number
    Hu: number
    Tie: number
}

export interface LHDBetAreaInfo {
    Total: number // 所有下注金額
    PlayerCount: number // 玩家人數
    PlayerBetAmount: number // 玩家下注金額
    RobotCount: number // 機器人人數
    RobotBetAmount: number // 機器人下注金額
}

export interface LHDTableDashboard {
    RoomID: number // 房間ID
    TableID: number // 桌號
    Long: LHDBetAreaInfo // 下注區域 龍
    Hu: LHDBetAreaInfo // 下注區域 虎
    Tie: LHDBetAreaInfo // 下注區域 和
    NextAward: number // 下一場指定開獎
    History: number[] // 歷史路單
    Players: LHDPlayerInfo[] // 所有玩家
    State: number // 遊戲狀態
    Cheatable: boolean // 可否作弊
    Cheating: boolean // 已開啟作弊
    SpecialRewardCount: number // 特殊獎勵次數
    SpecialRewardRate: number // 特殊獎勵機率
    SysPool: number // 系統水池
    SysPoolWinLine: number // 系統水池贏錢線
    TotalBetAmount: number // 今日總下注
    TotalWinLossAmount: number // 今日總輸贏
    TotalValidAmount: number // 今日總流水

    TableDisabled: boolean // 是否禁用

    JackpotInfo: LHDJackpotInfo // 獎池資訊
}

export interface LHDJackpotInfo {
    Enabled: boolean
    Awardpause: boolean
    Current: number
    BasePool: number
    AwardLine: number
    Phase: number
}

export interface BetSummaryResponse {
    code: number
    message: string|null
    data: {
        serverID: number
        totalBetAmount: number
        totalWinLossAmount: number
    }
}
