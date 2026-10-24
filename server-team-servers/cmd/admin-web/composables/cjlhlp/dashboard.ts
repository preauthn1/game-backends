import { EventSourcePolyfill } from 'event-source-polyfill';

import { CJLHLPBetAreaInfo, CJLHLPPlayerInfo, CJLHLPTableDashboard } from "~/@types/cjlhlp"

export const dashboard = async (rid: number, tid: number) => {
    
    const conf = useRuntimeConfig()
    const { apiBase } = conf.public

    const { auth, logout } = useAuth()

    const dayjs = useDayjs()

    const notify = useNotification()

    const empty = () => {
        return merge({} as CJLHLPTableDashboard)
    }

    const uri = computed(() => {
        return "/api/admin/cjlhlp/"+rid+"/"+tid
    })

    const mergeBetAreaInfo = (data: CJLHLPBetAreaInfo): CJLHLPBetAreaInfo => {
        const obj = {} as CJLHLPBetAreaInfo
        if (data == null) {
            data = {} as CJLHLPBetAreaInfo
        }
        obj.Total = data.Total ? data.Total : 0
        obj.PlayerCount = data.PlayerCount ? data.PlayerCount : 0
        obj.PlayerBetAmount = data.PlayerBetAmount ? data.PlayerBetAmount : 0
        obj.RobotBetAmount = data.RobotBetAmount ? data.RobotBetAmount : 0
        obj.RobotCount = data.RobotCount ? data.RobotCount : 0
        return obj
    }

    const mergePlayerInfo = (data: CJLHLPPlayerInfo): CJLHLPPlayerInfo => {
        const obj = {} as CJLHLPPlayerInfo
        obj.PID = data.PID ? data.PID : 0
        obj.BetAmount = data.BetAmount ? data.BetAmount : 0
        obj.IsRobot = data.IsRobot ? data.IsRobot : false
        return obj
    }

    const merge = (data: CJLHLPTableDashboard): CJLHLPTableDashboard => {
        const obj = {} as CJLHLPTableDashboard
        if (data == null) {
            data = {} as CJLHLPTableDashboard
        }
        obj.RoomID = data.RoomID ? data.RoomID : 0
        obj.TableID = data.TableID ? data.TableID : 0
        obj.NextAward = data.NextAward ? data.NextAward : (data.NextAward == 0 ? 0 : undefined)
        obj.NextAwardList = data.NextAwardList ? data.NextAwardList : []
        obj.History = data.History ? data.History.reverse() : []
        obj.Players = data.Players ? data.Players : [] as CJLHLPPlayerInfo[]
        obj.Step = data.Step ? data.Step : 0
        obj.Cheatable = data.Cheatable ? data.Cheatable : false
        obj.Cheating = data.Cheating ? data.Cheating : false
        obj.SpecialRewardCount = data.SpecialRewardCount ? data.SpecialRewardCount : 0
        obj.SpecialRewardRate = data.SpecialRewardRate ? data.SpecialRewardRate : 0
        obj.SysPool = data.SysPool ? data.SysPool : 0
        obj.SysPoolWinLine = data.SysPoolWinLine ? data.SysPoolWinLine : 0
        obj.TotalBetAmount = data.TotalBetAmount ? data.TotalBetAmount : 0
        obj.TotalWinLossAmount = data.TotalWinLossAmount ? data.TotalWinLossAmount : 0
        obj.TableDisabled = data.TableDisabled ? data.TableDisabled : false
        obj.Players.forEach((p)=>{
            p = mergePlayerInfo(p)
        })
        obj.BetSummary = data.BetSummary ? mergeBetAreaInfo(data.BetSummary) : {} as CJLHLPBetAreaInfo

        // for (let i = 0; i < 100; i++) {
        //     obj.Players.push(mergePlayerInfo({
        //         PID: i + 1000000,
        //         BetAmount: Math.random() * 1000,
        //         IsRobot: Math.round(Math.random()) == 0  ? true : false,
        //     } as CJLHLPPlayerInfo))
        // }

        // obj.History = [17, 18, 19, 33, 34, 35]
        return obj
    }

    const state = useState<CJLHLPTableDashboard>("CJLHLP-"+rid+"-"+tid, () => empty() )
    
    const fetch = async () => {

        const promise = $fetch<CJLHLPTableDashboard>(unref(uri), {
            method: "GET",
            onRequest({ options }) {
                options.headers = {
                    "Authorization": auth.value?.access_token,
                    ...options.headers
                } as HeadersInit
            },
            onResponse({response}){
                response._data = response._data.data
            }
        })

        const room = await useRoom(rid)
        
        promise.then(async (e)=>{
            state.value = merge(e)
            state.value.RoomID = rid 

            if (!room.state.value || !room.state.value.Tables[tid]) {
                return
            }

            room.state.value.Tables[tid].SysPool = state.value.SysPool
            room.state.value.Tables[tid].SysPoolWinLine = state.value.SysPoolWinLine

            room.state.value.Tables[tid].Players = []
            room.state.value.Tables[tid].Robots = []
            
            for (let i = 0; i < state.value.Players.length; i++) {
                const p = state.value.Players[i]
                let _p = {
                    ID: p.PID,
                    IsRobot: p.IsRobot,
                }
                if (!p.IsRobot) {
                    room.state.value.Tables[tid].Players.push(_p)
                } else {
                    room.state.value.Tables[tid].Robots.push(_p)
                }
            }

            room.reCount()

        })

        promise.catch((e) => {
            if (e.statusCode == 403) {
                logout()
            }
        })
    }

    const loaded = computed(()=>{
        const data = unref(state)
        return data.RoomID
    })

    if (!unref(loaded)) {
        await fetch()
    }

    const stream = async () => {
        let stream = new EventSourcePolyfill(apiBase+unref(uri)+"/stream", {
            headers: {
                "Authorization": auth.value ? auth.value.access_token : "",
            },
        })

        const room = await useRoom(rid)

        stream.addEventListener("message", async (e) => {
            const TotalBetAmount = state.value.TotalBetAmount
            const TotalWinLossAmount = state.value.TotalWinLossAmount

            state.value = merge(JSON.parse(e.data))
            state.value.RoomID = rid

            if (!room.state.value || !room.state.value.Tables[tid]) {
                return
            }

            room.state.value.Tables[tid].SysPool = state.value.SysPool
            room.state.value.Tables[tid].SysPoolWinLine = state.value.SysPoolWinLine

            room.state.value.Tables[tid].Players = []
            room.state.value.Tables[tid].Robots = []
            
            for (let i = 0; i < state.value.Players.length; i++) {
                const p = state.value.Players[i]
                let _p = {
                    ID: p.PID,
                    IsRobot: p.IsRobot,
                }
                if (!p.IsRobot) {
                    room.state.value.Tables[tid].Players.push(_p)
                } else {
                    room.state.value.Tables[tid].Robots.push(_p)
                }
            }

            room.reCount()

        })

        return stream
    }

    const cheat = (cnt: number) => {
        $fetch<CJLHLPTableDashboard>(unref(uri)+"/cheat", {
            method: "POST",
            body: {
                "kick_count": cnt,
            },
            onRequest({ options }) {
                options.headers = {
                    "Authorization": auth.value?.access_token,
                    ...options.headers
                } as HeadersInit
            },
        })
    }

    const special = (cnt: number, rate: number) => {

        if (cnt < 0) {
            notify.notify({
                title: "特殊開獎設定錯誤",
                text: "剩餘次數不能小於 0",
                color: "error",
            })
            return
        }
        if (cnt > 1000) {
            notify.notify({
                title: "特殊開獎設定錯誤",
                text: "剩餘次數不能大於 1000",
                color: "error",
            })
            return
        }

        if (rate < 0) {
            notify.notify({
                title: "特殊開獎設定錯誤",
                text: "發動機率不能小於 0%",
                color: "error",
            })
            return
        }
        if (rate > 100) {
            notify.notify({
                title: "特殊開獎設定錯誤",
                text: "發動機率不能大於 100%",
                color: "error",
            })
            return
        }

        console.log("CJLHLP", cnt, rate)
        $fetch<CJLHLPTableDashboard>(unref(uri)+"/special_reward", {
            method: "POST",
            body: {
                "count": cnt,
                "rate": rate,
            },
            onRequest({ options }) {
                options.headers = {
                    "Authorization": auth.value?.access_token,
                    ...options.headers
                } as HeadersInit
            },
        })
    }

    const incSysPool = (amount: number) => {
        $fetch(unref(uri)+"/increase_sys_pool/"+amount, {
            method: "PUT",
            onRequest({ options }) {
                options.headers = {
                    "Authorization": auth.value?.access_token,
                    ...options.headers
                } as HeadersInit
            }
        })
    }

    const assignNextAward = (award: number) => {
        $fetch(unref(uri)+"/assign_next_award/"+award, {
            method: "PUT",
            onRequest({ options }) {
                options.headers = {
                    "Authorization": auth.value?.access_token,
                    ...options.headers
                } as HeadersInit
            }
        })
    }

    const setTableDisabled = (disabled: boolean) => {
        let s = "false"
        if (disabled) {
            s = "true"
        }

        $fetch(unref(uri)+"/set_table_disabled/"+s, {
            method: "PUT",
            onRequest({ options }) {
                options.headers = {
                    "Authorization": auth.value?.access_token,
                    ...options.headers
                } as HeadersInit
            }
        })
    } 

    return {
        loaded,
        state,
        fetch,
        stream,
        cheat,
        special,
        incSysPool,
        assignNextAward,
        setTableDisabled,
    }
}