export interface JZYQPlayerInfo {
    PID: number
    BetAmount: number
    IsRobot: boolean

    Goal: number
    GoalInside: number
    GoalOutside: number
    Lost: number
    LostInside: number
    LostOutside: number
}

export interface JZYQBetAreaInfo {
    Total: number // 所有下注金額
    PlayerCount: number // 玩家人數
    PlayerBetAmount: number // 玩家下注金額
    RobotCount: number // 機器人人數
    RobotBetAmount: number // 機器人下注金額
}

export interface JZYQTableDashboard {
    RoomID: number // 房間ID
    TableID: number // 桌號
    Step: number // 遊戲狀態
    NextAward: number // 下一場指定開獎
    SysPool: number // 系統水池
    SysPoolWinLine: number // 系統水池贏錢線
    History: number[] // 歷史路單
    Players: JZYQPlayerInfo[] // 所有玩家
    Cheatable: boolean // 可否作弊
    Cheating: boolean // 已開啟作弊
    SpecialRewardCount: number // 特殊獎勵次數
    SpecialRewardRate: number // 特殊獎勵機率

    Goal: JZYQBetAreaInfo
    GoalInside: JZYQBetAreaInfo
    GoalOutside: JZYQBetAreaInfo
    Lost: JZYQBetAreaInfo
    LostInside: JZYQBetAreaInfo
    LostOutside: JZYQBetAreaInfo

    TotalBetAmount: number // 今日總下注
    TotalWinLossAmount: number // 今日總輸贏

    TableDisabled: boolean // 是否禁用
}