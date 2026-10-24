import { EventSourcePolyfill } from 'event-source-polyfill';

import { LHDBetAreaInfo, LHDPlayerInfo, LHDTableDashboard, LHDJackpotInfo } from "~~/types"

export const dashboard = async (rid: number, tid: number) => {
    
    const conf = useRuntimeConfig()
    const { apiBase } = conf.public

    const { auth, logout } = useAuth()

    const dayjs = useDayjs()

    const notify = useNotification()

    const empty = () => {
        return merge({} as LHDTableDashboard)
    }

    const uri = computed(() => {
        return "/api/admin/lhd/"+rid+"/"+tid
    })

    const mergeBetAreaInfo = (data: LHDBetAreaInfo): LHDBetAreaInfo => {
        const obj = {} as LHDBetAreaInfo
        if (data == null) {
            data = {} as LHDBetAreaInfo
        }
        obj.Total = data.Total ? data.Total : 0
        obj.PlayerCount = data.PlayerCount ? data.PlayerCount : 0
        obj.PlayerBetAmount = data.PlayerBetAmount ? data.PlayerBetAmount : 0
        obj.RobotBetAmount = data.RobotBetAmount ? data.RobotBetAmount : 0
        obj.RobotCount = data.RobotCount ? data.RobotCount : 0
        return obj
    }

    const mergePlayerInfo = (data: LHDPlayerInfo): LHDPlayerInfo => {
        const obj = {} as LHDPlayerInfo
        obj.PID = data.PID ? data.PID : 0
        obj.BetAmount = data.BetAmount ? data.BetAmount : 0
        obj.IsRobot = data.IsRobot ? data.IsRobot : false
        obj.Long = data.Long ? data.Long : 0
        obj.Hu = data.Hu ? data.Hu : 0
        obj.Tie = data.Tie ? data.Tie : 0
        return obj
    }

    const mergeJackpotInfo = (data: LHDJackpotInfo): LHDJackpotInfo => {
        const obj = {} as LHDJackpotInfo
        obj.Enabled = data.Enabled ? data.Enabled : false
        obj.Awardpause = data.Awardpause ? data.Awardpause : false
        obj.Current = data.Current ? data.Current : 0
        obj.BasePool = data.BasePool ? data.BasePool : 0
        obj.AwardLine = data.AwardLine ? data.AwardLine : 0
        obj.Phase = data.Phase ? data.Phase : 0
        return obj
    }

    const merge = (data: LHDTableDashboard): LHDTableDashboard => {
        const obj = {} as LHDTableDashboard
        if (data == null) {
            data = {} as LHDTableDashboard
        }
        obj.RoomID = data.RoomID ? data.RoomID : 0
        obj.TableID = data.TableID ? data.TableID : 0
        obj.Long = data.Long ? mergeBetAreaInfo(data.Long) : {} as LHDBetAreaInfo
        obj.Hu = data.Hu ? mergeBetAreaInfo(data.Hu) : {} as LHDBetAreaInfo
        obj.Tie = data.Tie ? mergeBetAreaInfo(data.Tie) : {} as LHDBetAreaInfo
        obj.NextAward = data.NextAward ? data.NextAward : 0
        obj.History = data.History ? data.History.reverse() : []
        obj.Players = data.Players ? data.Players : [] as LHDPlayerInfo[]
        obj.State = data.State ? data.State : 0
        obj.Cheatable = data.Cheatable ? data.Cheatable : false
        obj.Cheating = data.Cheating ? data.Cheating : false
        obj.SpecialRewardCount = data.SpecialRewardCount ? data.SpecialRewardCount : 0
        obj.SpecialRewardRate = data.SpecialRewardRate ? data.SpecialRewardRate : 0
        obj.SysPool = data.SysPool ? data.SysPool : 0
        obj.SysPoolWinLine = data.SysPoolWinLine ? data.SysPoolWinLine : 0
        obj.TotalBetAmount = data.TotalBetAmount ? data.TotalBetAmount : 0
        obj.TotalValidAmount = data.TotalValidAmount ? data.TotalValidAmount : 0
        obj.TotalWinLossAmount = data.TotalWinLossAmount ? data.TotalWinLossAmount : 0
        obj.TableDisabled = data.TableDisabled ? data.TableDisabled : false
        obj.JackpotInfo = data.JackpotInfo ? mergeJackpotInfo(data.JackpotInfo) : {} as LHDJackpotInfo
        obj.Players.forEach((p)=>{
            p = mergePlayerInfo(p)
        })
        // for (let i = 0; i < 100; i++) {
        //     obj.Players.push(mergePlayerInfo({
        //         PID: i + 1000000,
        //         BetAmount: Math.random() * 1000,
        //         IsRobot: Math.round(Math.random()) == 0  ? true : false,
        //         Long: Math.random() * 1000,
        //         Hu: Math.random() * 1000,
        //         Tie: Math.random() * 1000,
        //     } as LHDPlayerInfo))
        // }
        return obj
    }

    const state = useState<LHDTableDashboard>("lhd-"+rid+"-"+tid, () => empty() )
    
    const fetch = async () => {

        const promise = $fetch<LHDTableDashboard>(unref(uri), {
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
        $fetch<LHDTableDashboard>(unref(uri)+"/cheat", {
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

        console.log("lhd", cnt, rate)
        $fetch<LHDTableDashboard>(unref(uri)+"/special_reward", {
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

    const setJackpotAwardpause = (enabled: boolean) => {
        let s = "false"
        if (!enabled) {
            s = "true"
        }

        $fetch(unref(uri)+"/set_jackpot_enabled/"+s, {
            method: "PUT",
            onRequest({ options }) {
                options.headers = {
                    "Authorization": auth.value?.access_token,
                    ...options.headers
                } as HeadersInit
            }
        })
    } 

    const assignJackpotToRandomPlayer = (jplevel: number) => {
        if (!jplevel) {
            return
        }

        if (!state || !state.value || (state.value.State != 0 && state.value.State != 1)) {
            notify.notify({
                title: "指定彩金給隨機玩家錯誤",
                text: "只有在下注時才能指定彩金給隨機玩家",
                color: "error",
            })
            return
        }

        $fetch(unref(uri)+"/assign_jackpot_to_random_player/"+jplevel, {
            method: "PUT",
            onRequest({ options }) {
                options.headers = {
                    "Authorization": auth.value?.access_token,
                    ...options.headers
                } as HeadersInit
            },
            onResponseError({ response }) {
                if (response._data && response._data.data && response._data.data.message) {

                    let msg = response._data.data.message

                    if (msg == "gRPC AssignJackpotToRandomPlayer error, rpc error: code = Aborted desc = jackpot not awardable") {
                        msg = "目前彩金不可派發"
                    }

                    notify.notify({
                        title: "指定彩金給隨機玩家錯誤",
                        text: "錯誤訊息: "+msg,
                        color: "error",
                    })
                }
            },
            onResponse({ response }) {
                if (response.ok) {
                    notify.notify({
                        title: "指定彩金給隨機玩家成功",
                        color: "success",
                    })
                }
            }
        })
    } 

    const assignJackpotToRecycel = (jplevel: number) => {
        if (!jplevel) {
            return
        }

        if (!state || !state.value || (state.value.State != 0 && state.value.State != 1)) {
            notify.notify({
                title: "回收彩金錯誤",
                text: "只有在下注時才能回收彩金",
                color: "error",
            })
            return
        }

        $fetch(unref(uri)+"/assign_jackpot_to_recycel/"+jplevel, {
            method: "PUT",
            onRequest({ options }) {
                options.headers = {
                    "Authorization": auth.value?.access_token,
                    ...options.headers
                } as HeadersInit
            },
            onResponseError({ response }) {
                if (response._data && response._data.data && response._data.data.message) {

                    let msg = response._data.data.message

                    if (msg == "gRPC AssignJackpotToRecycle error, rpc error: code = Aborted desc = jackpot not awardable") {
                        msg = "目前彩金不可回收"
                    }

                    notify.notify({
                        title: "回收彩金錯誤",
                        text: "錯誤訊息: "+msg,
                        color: "error",
                    })
                }
            },
            onResponse({ response }) {
                if (response.ok) {
                    notify.notify({
                        title: "回收彩金成功",
                        color: "success",
                    })
                }
            }
        })
    } 

    const editJackpotPool = (amount: number) => {
        $fetch(unref(uri)+"/edit_jackpot_pool/"+amount, {
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
        setJackpotAwardpause,
        assignJackpotToRandomPlayer,
        assignJackpotToRecycel,
        editJackpotPool,
    }
}