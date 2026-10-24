export interface SMLPlayerInfo {
    PID: number
    BetAmount: number
    IsRobot: boolean

    Goal: number
    GoalLeft: number
    GoalMiddle: number
    GoalRight: number
    Save: number
    SaveLeft: number
    SaveMiddle: number
    SaveRight: number
}

export interface SMLBetAreaInfo {
    Total: number // 所有下注金額
    PlayerCount: number // 玩家人數
    PlayerBetAmount: number // 玩家下注金額
    RobotCount: number // 機器人人數
    RobotBetAmount: number // 機器人下注金額
}

export interface SMLTableDashboard {
    RoomID: number // 房間ID
    TableID: number // 桌號
    Step: number // 遊戲狀態
    NextAward: number // 下一場指定開獎
    SysPool: number // 系統水池
    SysPoolWinLine: number // 系統水池贏錢線
    History: number[] // 歷史路單
    Players: SMLPlayerInfo[] // 所有玩家
    Cheatable: boolean // 可否作弊
    Cheating: boolean // 已開啟作弊
    SpecialRewardCount: number // 特殊獎勵次數
    SpecialRewardRate: number // 特殊獎勵機率

    Goal: SMLBetAreaInfo
    GoalLeft: SMLBetAreaInfo
    GoalMiddle: SMLBetAreaInfo
    GoalRight: SMLBetAreaInfo
    Save: SMLBetAreaInfo
    SaveLeft: SMLBetAreaInfo
    SaveMiddle: SMLBetAreaInfo
    SaveRight: SMLBetAreaInfo

    TotalBetAmount: number // 今日總下注
    TotalWinLossAmount: number // 今日總輸贏

    TableDisabled: boolean // 是否禁用
}