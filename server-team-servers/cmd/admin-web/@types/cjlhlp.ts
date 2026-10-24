export interface CJLHLPPlayerInfo {
    PID: number
    BetAmount: number
    IsRobot: boolean
}

export interface CJLHLPBetAreaInfo {
    Total: number // 所有下注金額
    PlayerCount: number // 玩家人數
    PlayerBetAmount: number // 玩家下注金額
    RobotCount: number // 機器人人數
    RobotBetAmount: number // 機器人下注金額
}

export interface CJLHLPTableDashboard {
    RoomID: number // 房間ID
    TableID: number // 桌號
    Step: number // 遊戲狀態
    NextAward: number|undefined // 下一場指定開獎
    SysPool: number // 系統水池
    SysPoolWinLine: number // 系統水池贏錢線
    History: number[] // 歷史路單
    Players: CJLHLPPlayerInfo[] // 所有玩家
    Cheatable: boolean // 可否作弊
    Cheating: boolean // 已開啟作弊
    SpecialRewardCount: number // 特殊獎勵次數
    SpecialRewardRate: number // 特殊獎勵機率

    NextAwardList: number[] // 下一場指定開獎列表

    BetSummary: CJLHLPBetAreaInfo

    TotalBetAmount: number // 今日總下注
    TotalWinLossAmount: number // 今日總輸贏

    TableDisabled: boolean // 是否禁用
}