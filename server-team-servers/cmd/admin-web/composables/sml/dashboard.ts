import { EventSourcePolyfill } from 'event-source-polyfill';

import { SMLBetAreaInfo, SMLPlayerInfo, SMLTableDashboard } from "~/@types/sml"

export const dashboard = async (rid: number, tid: number) => {
    
    const conf = useRuntimeConfig()
    const { apiBase } = conf.public

    const { auth, logout } = useAuth()

    const dayjs = useDayjs()

    const notify = useNotification()

    const empty = () => {
        return merge({} as SMLTableDashboard)
    }

    const uri = computed(() => {
        return "/api/admin/sml/"+rid+"/"+tid
    })

    const mergeBetAreaInfo = (data: SMLBetAreaInfo): SMLBetAreaInfo => {
        const obj = {} as SMLBetAreaInfo
        if (data == null) {
            data = {} as SMLBetAreaInfo
        }
        obj.Total = data.Total ? data.Total : 0
        obj.PlayerCount = data.PlayerCount ? data.PlayerCount : 0
        obj.PlayerBetAmount = data.PlayerBetAmount ? data.PlayerBetAmount : 0
        obj.RobotBetAmount = data.RobotBetAmount ? data.RobotBetAmount : 0
        obj.RobotCount = data.RobotCount ? data.RobotCount : 0
        return obj
    }

    const mergePlayerInfo = (data: SMLPlayerInfo): SMLPlayerInfo => {
        const obj = {} as SMLPlayerInfo
        obj.PID = data.PID ? data.PID : 0
        obj.BetAmount = data.BetAmount ? data.BetAmount : 0
        obj.IsRobot = data.IsRobot ? data.IsRobot : false
        obj.Goal = data.Goal ? data.Goal : 0
        obj.GoalLeft = data.GoalLeft ? data.GoalLeft : 0
        obj.GoalMiddle = data.GoalMiddle ? data.GoalMiddle : 0
        obj.GoalRight = data.GoalRight ? data.GoalRight : 0
        obj.Save = data.Save ? data.Save : 0
        obj.SaveLeft = data.SaveLeft ? data.SaveLeft : 0
        obj.SaveMiddle = data.SaveMiddle ? data.SaveMiddle : 0
        obj.SaveRight = data.SaveRight ? data.SaveRight : 0
        return obj
    }

    const merge = (data: SMLTableDashboard): SMLTableDashboard => {
        const obj = {} as SMLTableDashboard
        if (data == null) {
            data = {} as SMLTableDashboard
        }
        obj.RoomID = data.RoomID ? data.RoomID : 0
        obj.TableID = data.TableID ? data.TableID : 0
        obj.NextAward = data.NextAward ? data.NextAward : 0
        obj.History = data.History ? data.History.reverse() : []
        obj.Players = data.Players ? data.Players : [] as SMLPlayerInfo[]
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
        obj.Goal = data.Goal ? mergeBetAreaInfo(data.Goal) : {} as SMLBetAreaInfo
        obj.GoalLeft = data.GoalLeft ? mergeBetAreaInfo(data.GoalLeft) : {} as SMLBetAreaInfo
        obj.GoalMiddle = data.GoalMiddle ? mergeBetAreaInfo(data.GoalMiddle) : {} as SMLBetAreaInfo
        obj.GoalRight = data.GoalRight ? mergeBetAreaInfo(data.GoalRight) : {} as SMLBetAreaInfo
        obj.Save = data.Save ? mergeBetAreaInfo(data.Save) : {} as SMLBetAreaInfo
        obj.SaveLeft = data.SaveLeft ? mergeBetAreaInfo(data.SaveLeft) : {} as SMLBetAreaInfo
        obj.SaveMiddle = data.SaveMiddle ? mergeBetAreaInfo(data.SaveMiddle) : {} as SMLBetAreaInfo
        obj.SaveRight = data.SaveRight ? mergeBetAreaInfo(data.SaveRight) : {} as SMLBetAreaInfo

        // for (let i = 0; i < 100; i++) {
        //     obj.Players.push(mergePlayerInfo({
        //         PID: i + 1000000,
        //         BetAmount: Math.random() * 1000,
        //         IsRobot: Math.round(Math.random()) == 0  ? true : false,
        //         Goal: Math.random() * 1000,
        //         GoalLeft: Math.random() * 1000,
        //         GoalMiddle: Math.random() * 1000,
        //         GoalRight: Math.random() * 1000,
        //         Save: Math.random() * 1000,
        //         SaveLeft: Math.random() * 1000,
        //         SaveMiddle: Math.random() * 1000,
        //         SaveRight: Math.random() * 1000,
        //     } as SMLPlayerInfo))
        // }

        // obj.History = [17, 18, 19, 33, 34, 35]
        return obj
    }

    const state = useState<SMLTableDashboard>("sml-"+rid+"-"+tid, () => empty() )
    
    const fetch = async () => {

        const promise = $fetch<SMLTableDashboard>(unref(uri), {
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
        $fetch<SMLTableDashboard>(unref(uri)+"/cheat", {
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

        console.log("sml", cnt, rate)
        $fetch<SMLTableDashboard>(unref(uri)+"/special_reward", {
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