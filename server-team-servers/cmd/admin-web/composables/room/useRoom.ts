import { BetSummaryResponse, PlayerInfo, RoomInfo, TableInfo } from "~~/types"

export const useRoom = async (id: number) => {

    const {auth, logout} = useAuth()

    const uri = computed(()=>{
        return "/api/room/"+id
    })

    const empty = () => {
        return {
            GameCode: "",
            ID: 0,
            Name: "",
            GameID: 0,
            GameName: "",
            Flag: 0,
            Tables: [],
            PlayerCnt: 0,
            RobotCnt: 0,
        } as RoomInfo
    }

    const state = useState<RoomInfo>("room-"+id, () => empty() )

    const mergePlayerInfo = (data: PlayerInfo): PlayerInfo => {
        data.ID = data.ID ? data.ID : 0
        data.IsRobot = data.IsRobot ? data.IsRobot : false
        return data
    }

    const mergeTableInfo = (data: TableInfo): TableInfo => {
        data.ID = data.ID ? data.ID : 0
        data.Gaming = data.Gaming ? data.Gaming : false
        data.Summary = data.Summary ? data.Summary : ""
        data.Players = data.Players ? data.Players : []
        data.Robots = data.Robots ? data.Robots : []
        data.SysPool = data.SysPool ? data.SysPool : 0
        data.SysPoolWinLine = data.SysPoolWinLine ? data.SysPoolWinLine : 0
        data.Players.forEach((p)=>{
            p = mergePlayerInfo(p)
        })
        data.Robots.forEach((p)=>{
            p = mergePlayerInfo(p)
        })
        return data
    }

    const merge = (data: RoomInfo): RoomInfo => {
        data.GameCode = data.GameCode ? data.GameCode : ""
        data.ID = data.ID ? data.ID : 0
        data.Name = data.Name ? data.Name : ""
        data.GameID = data.GameID ? data.GameID : 0
        data.GameName = data.GameName ? data.GameName : ""
        data.Flag = data.Flag ? data.Flag : 0
        data.Tables = data.Tables ? data.Tables : []
        data.PlayerCnt = data.PlayerCnt ? data.PlayerCnt : 0
        data.RobotCnt = data.RobotCnt ? data.RobotCnt : 0
        data.Tables.forEach((t)=>{
            t = mergeTableInfo(t)
        })
        return data
    }

    const fetch = async () => {

        const promise = $fetch<RoomInfo>(unref(uri), {
            method: "GET",
            onRequest({ options }) {
                options.headers = {
                    "Authorization": auth.value?.access_token,
                    ...options.headers
                } as HeadersInit
            },
            onResponse({response}) {
                response._data = response._data.data
            }
        })

        promise.then((e) => {
            state.value = merge(e) 
        })

        promise.catch((e) => {
            if (e.statusCode == 403) {
                logout()
            }
        })
    }

    const loaded = computed(()=>{
        const data = unref(state)
        return data.ID > 0
    })

    if (!unref(loaded)) {
        await fetch()
    }

    const reCount = () => {
        state.value.PlayerCnt = 0
        state.value.RobotCnt = 0

        for (let i = 0; i < state.value.Tables.length; i++) {
            const t = state.value.Tables[i]
            state.value.PlayerCnt += t.Players.length
            state.value.RobotCnt += t.Robots.length
        }
    }

    const betSummary = async (rid: number, begin: string, end: string) => {

        const key = "bet-summary-"+rid+"-"+begin+"-"+end

        const cookie = useCookie<BetSummaryResponse>(key, {
            maxAge: 5,
        })

        if (cookie.value) {
            return cookie.value
        }

        const promise = await $fetch<BetSummaryResponse>("/api/forward/bet-summary/server", {
            method: "POST",
            body: {
                ServerID: rid,
                Begin: begin,
                End: end,
            },
            onRequest({ options }) {
                options.headers = {
                    "Authorization": auth.value?.access_token,
                    ...options.headers
                } as HeadersInit
            },
            onResponse({response}) {
                response._data = response._data
            }
        })

        cookie.value = promise

        return promise
    }

    return {
        loaded,
        state,
        fetch,
        reCount,
        betSummary,
    }
}