<template>
    <div v-if="data" class="grid grid-cols-8 mx-4 gap-4">
        <div class="col-span-full lg:col-span-6 lg:col-start-2">
            <div class="p-3 grid grid-cols-4 gap-4 text-center justify-center items-center">
                <div class="py-3 grid justify-center items-center gap-4">
                    <div class="py-3 flex justify-center items-center gap-3">
                        <span v-if="tableEnabled">啟用中</span>
                        <span v-if="!tableEnabled">停用中</span>
                        <UiSwitch v-model="tableEnabled" :class="{
                            'switch-success': tableEnabled,
                            'switch-danger': !tableEnabled,
                        }" />
                    </div>
                    <div class="rounded-lg h-10 px-3 py-2 leading-5 border-2 text-center align-middle" :class="{
                        'hidden': lhd.stateTxt(data.State).length == 0,
                        'animate-pulse': data.State == 1,
                    }">
                        {{ lhd.stateTxt(data.State) }}
                    </div>
                </div>
                <div class="py-3 grid justify-center items-center">
                    <div class="py-3 flex justify-center items-center">
                        <UiInput class="!p-1 !w-16 text-sm mr-2" type="number" v-model="kickCnt" />
                        <UiButtonWarn :disabled="!data.Cheatable" @click="$emit('cheat', kickCnt)">
                            {{ data.Cheating ? "Cheating" : "Cheat" }}
                        </UiButtonWarn>
                    </div>
                    <div class="py-3 flex justify-center items-center gap-5">
                        <div class="flex justify-center items-center gap-5">
                            <AdminLhdAward :award="data.NextAward" />
                            <UiButtonDanger :disabled="data.Cheating || data.SpecialRewardCount != 0" @click="assign_award_form.active = !assign_award_form.active" class="text-sm">指定開獎</UiButtonDanger>
                        </div>
                        <div v-if="assign_award_form.active" class="absolute rounded-xl p-3 grid justify-center items-center bg-neutral-200 dark:bg-neutral-700 w-3/5 h-40 z-50 top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2">
                            <span @click="assign_award_form.active = false" class="absolute top-2 right-2 cursor-pointer">
                                <svg xmlns="http://example.invalid/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-7 h-7">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M9.75 9.75l4.5 4.5m0-4.5l-4.5 4.5M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                            </span>
                            <div class="flex justify-center items-center border-b-2 pb-3 gap-5">
                                <div>指定開獎</div>
                                <AdminLhdAward :award="data.NextAward" />
                            </div>
                            <div class="flex justify-center items-center gap-4">
                                <AdminLhdAward class="cursor-pointer" :award="1" @click="$emit('assignNextAward', 1)" />
                                <AdminLhdAward class="cursor-pointer" :award="2" @click="$emit('assignNextAward', 2)" />
                                <AdminLhdAward class="cursor-pointer" :award="3" @click="$emit('assignNextAward', 3)" />
                            </div>
                        </div>
                        <div class="flex justify-center items-center gap-5">
                            <UiButtonSuccess v-if="data.JackpotInfo.Enabled" @click="jackpot_pannel.active = !jackpot_pannel.active" class="text-sm">
                                彩金控制
                            </UiButtonSuccess>
                            <UiButtonDanger v-else class="text-sm" disabled>彩金關閉</UiButtonDanger>
                        </div>
                        <div v-if="jackpot_pannel.active" class="absolute rounded-xl p-5 grid justify-center items-center gap-3 bg-neutral-200 dark:bg-neutral-700 w-1/2 z-50 top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2">
                            <span @click="jackpot_pannel.active = false" class="absolute top-2 right-2 cursor-pointer">
                                <svg xmlns="http://example.invalid/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-7 h-7">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M9.75 9.75l4.5 4.5m0-4.5l-4.5 4.5M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                            </span>
                            <div class="flex justify-center items-center border-b-2 pb-3 gap-5">
                                <div>彩金控制</div>
                            </div>
                            <div class="grid justify-center items-center gap-3">
                                <div class="grid grid-cols-4 justify-start items-center gap-3">
                                    <div>
                                        <span v-if="jackpotAwardpause">可派發彩金</span>
                                        <span v-if="!jackpotAwardpause">暫停派發彩金</span>
                                    </div>
                                    <UiSwitch v-model="jackpotAwardpause" :class="{
                                            'switch-success': jackpotAwardpause,
                                            'switch-danger': !jackpotAwardpause,
                                        }" />
                                </div>
                                <div class="grid grid-cols-4 justify-start items-center gap-3">
                                    <div>當前彩金:</div>
                                    <div v-if="!editJackpotPoolForm.show">
                                        <div class="flex items-center justify-center text-emerald-600">
                                            <CommonAmount class="mr-3" :count="data.JackpotInfo.Current" svg-class="w-5 h-5" />
                                            <span class="cursor-pointer" @click="openEditJackpotPoolForm">
                                                <svg xmlns="http://example.invalid/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-6 h-6">
                                                    <path stroke-linecap="round" stroke-linejoin="round" d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L10.582 16.07a4.5 4.5 0 01-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 011.13-1.897l8.932-8.931zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0115.75 21H5.25A2.25 2.25 0 013 18.75V8.25A2.25 2.25 0 015.25 6H10" />
                                                </svg>
                                            </span>
                                        </div>
                                    </div>
                                    <div v-else class="flex justify-center items-center">

                                        <UiInput v-model="editJackpotPoolForm.amount" class="!p-1 !w-1/2 text-sm mr-3"></UiInput>

                                        <span class="cursor-pointer" @click="closeEditJackpotPoolForm">
                                            <svg xmlns="http://example.invalid/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-6 h-6">
                                                <path stroke-linecap="round" stroke-linejoin="round" d="M4.5 12.75l6 6 9-13.5" />
                                            </svg>
                                        </span>
                                    </div>
                                    <div>出獎階段:</div> 
                                    <div>{{ data.JackpotInfo.Phase }}</div>
                                </div>
                                <div class="grid grid-cols-4 justify-start items-center gap-3">
                                    <div>基礎彩金:</div>
                                    <div><CommonAmount :count="data.JackpotInfo.BasePool" svg-class="w-5 h-5" /></div>
                                    <div>可出獎線:</div> 
                                    <div><CommonAmount :count="data.JackpotInfo.AwardLine" svg-class="w-5 h-5" /></div>
                                </div>
                                <div class="flex justify-start items-center gap-3">
                                    <div class="border rounded-full py-2 px-3 cursor-pointer" :class="{
                                        'text-sky-500 border-sky-500' : jackpot_pannel.level == 0,
                                    }" @click="jackpot_pannel.level = 0">None</div>
                                    <div class="border rounded-full py-2 px-3 cursor-pointer" :class="{
                                        'text-sky-500 border-sky-500' : jackpot_pannel.level == 1,
                                    }" @click="jackpot_pannel.level = 1">Mini</div>
                                    <div class="border rounded-full py-2 px-3 cursor-pointer" :class="{
                                        'text-sky-500 border-sky-500' : jackpot_pannel.level == 2,
                                    }" @click="jackpot_pannel.level = 2">Minor</div> 
                                    <div class="border rounded-full py-2 px-3 cursor-pointer" :class="{
                                        'text-sky-500 border-sky-500' : jackpot_pannel.level == 3,
                                    }" @click="jackpot_pannel.level = 3">Grand</div>
                                    <div class="border-0 border-l-2 h-full p-5 ml-10"></div>
                                    <div :disabled="jackpot_pannel.level == 0" class="border rounded-full py-2 px-3 text-emerald-500 border-emerald-500" :class="{
                                        'cursor-pointer' : jackpot_pannel.level != 0,
                                        'cursor-not-allowed' : jackpot_pannel.level == 0,
                                    }" @click="$emit('assignJackpotToRandomPlayer', jackpot_pannel.level)">隨機</div>
                                    <div :disabled="jackpot_pannel.level == 0" class="border rounded-full py-2 px-3 text-rose-500 border-rose-500" :class="{
                                        'cursor-pointer' : jackpot_pannel.level != 0,
                                        'cursor-not-allowed' : jackpot_pannel.level == 0,
                                    }" @click="$emit('assignJackpotToRecycel', jackpot_pannel.level)">回收</div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="py-3 flex justify-center items-center">
                    <div v-if="special_form.active" class="grid gap-3 justify-center items-center">
                        <div class="flex justify-center items-center">
                            <span class="mr-3">特殊開獎</span>
                            <span class="cursor-pointer" @click="special_form.active = false; $emit('special', special_form.cnt, special_form.rate)">
                                <svg xmlns="http://example.invalid/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-6 h-6">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M4.5 12.75l6 6 9-13.5" />
                                </svg>
                            </span>
                        </div>
                        <div>剩餘次數：<UiInput class="!p-1 !w-16 text-sm mr-2" type="number" v-model="special_form.cnt" :disabled="data.Cheating" /></div>
                        <div>發動機率：<UiInput class="!p-1 !w-16 text-sm mr-2" type="number" v-model="special_form.rate" :disabled="data.Cheating" /></div>
                    </div>
                    <div v-else class="grid gap-3 justify-center items-center">
                        <div class="flex justify-center items-center">
                            <span class="mr-3">特殊開獎</span>
                            <span v-if="!data.Cheating" class="cursor-pointer" @click="special_form.cnt = data.SpecialRewardCount; special_form.rate = data.SpecialRewardRate; special_form.active = true">
                                <svg xmlns="http://example.invalid/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-6 h-6">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L10.582 16.07a4.5 4.5 0 01-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 011.13-1.897l8.932-8.931zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0115.75 21H5.25A2.25 2.25 0 013 18.75V8.25A2.25 2.25 0 015.25 6H10" />
                                </svg>
                            </span>
                        </div>
                        <div>剩餘次數：{{ data.SpecialRewardCount }}</div>
                        <div>發動機率：{{ data.SpecialRewardRate }}%</div>
                    </div>
                </div>
                <div class="py-3 grid justify-center items-center">
                    <div>本日</div>
                    <div class="flex justify-center items-center">
                        <span class="mr-3">總押注:</span>
                        <CommonAmount svg-class="w-3 h-3" :count="data.TotalValidAmount"></CommonAmount>
                    </div>
                    <div class="flex justify-center items-center">
                        <span class="mr-3">總輸贏:</span>
                        <CommonAmount svg-class="w-3 h-3" :count="data.TotalWinLossAmount"></CommonAmount>
                    </div>
                </div>
            </div>
            <div class="relative h-28 lg:h-40 overflow-y-auto px-5 mb-5">
                <div class="sticky top-0 z-20 w-full h-2 backdrop-blur"></div>
                <div class="grid grid-cols-5 sm:grid-cols-6 md:grid-cols-7 gap-4 text-center">
                    <div class="col-span-1" v-for="(row, i) in data.History" :title="'Round-' + (i + 1)">
                        <AdminLhdAward :award="row"></AdminLhdAward>
                    </div>
                </div>
                <div class="sticky bottom-0 z-20 w-full h-2 backdrop-blur"></div>
            </div>
        </div>
        <div class="col-span-full grid grid-cols-3 gap-4">
            <AdminLhdBetArea class="col-span-full sm:col-span-1" :award="1" :player-count="data.Long.PlayerCount"
                :robot-count="data.Long.RobotCount" :player-bet-amount="data.Long.PlayerBetAmount"
                :robot-bet-amount="data.Long.RobotBetAmount" />
            <AdminLhdBetArea class="col-span-full sm:col-span-1" :award="2" :player-count="data.Tie.PlayerCount"
                :robot-count="data.Tie.RobotCount" :player-bet-amount="data.Tie.PlayerBetAmount"
                :robot-bet-amount="data.Tie.RobotBetAmount" />
            <AdminLhdBetArea class="col-span-full sm:col-span-1" :award="3" :player-count="data.Hu.PlayerCount"
                :robot-count="data.Hu.RobotCount" :player-bet-amount="data.Hu.PlayerBetAmount"
                :robot-bet-amount="data.Hu.RobotBetAmount" />
        </div>
        <div v-if="data.Players && data.Players.length != 0" class="col-span-full w-[calc(100%-2.5rem)] mx-auto">
            <div class="flex justify-center items-center p-2 mb-3">
                <input class="hidden" type="radio" id="player-all" value="all" name="playerFilter" v-model="pFilter" />
                <input class="hidden" type="radio" id="player-only" value="player" name="playerFilter" v-model="pFilter" />
                <input class="hidden" type="radio" id="player-robot-only" value="robot" name="playerFilter" v-model="pFilter" />

                <label class="label mr-3" :class="pFilter=='all' ? 'checked' : ''" for="player-all">ALL({{ data.Players.length }})</label>
                <label class="label mr-3" :class="pFilter=='player' ? 'checked' : ''" for="player-only">Player({{ pCount }})</label>
                <label class="label mr-3" :class="pFilter=='robot' ? 'checked' : ''" for="player-robot-only">Robot({{ rCount }})</label>
                <UiInput class="px-2 !py-1 w-20 text-xs" placeholder="Find User" v-model="pFilterId" />
            </div>
            <div class="
                    h-52 overflow-y-auto 
                    grid gap-4 
                    sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4">
                <AdminLhdPlayerInfo v-for="player in players" :key="player.PID" :player="player" />
            </div>
        </div>
    </div>
</template>


<script lang="ts" setup>
import { LHDPlayerInfo } from '~~/types';

const props = defineProps([
    "room",
    "table",
    "data",
])

const emits = defineEmits([
    "cheat",
    "special",
    "setTableDisabled",
    "assignNextAward",
    "setJackpotAwardpause",
    "assignJackpotToRandomPlayer",
    "assignJackpotToRecycel",
    "editJackpotPool",
])

const dayjs = useDayjs()

const { auth } = useAuth()

const lhd = useLHD()

const pFilter = ref('all')

const pFilterId = ref('')

const pFilterIdRegx = computed(() => {
    return '^' + pFilterId.value + '.*$'
})

const players = computed(() => {

    let players = [] as LHDPlayerInfo[]

    if (!props.data.Players) {
        return players
    }

    props.data?.Players.forEach((element: LHDPlayerInfo) => {
        if (element.IsRobot && pFilter.value == "player") {
            return
        } else if (!element.IsRobot && pFilter.value == "robot") {
            return
        }

        const v = element.PID.toString()

        if (!v.match(pFilterIdRegx.value)) {
            return
        }

        players.push(element)
    })

    players.sort((a, b) => {
        return a.PID - b.PID
    })

    return players
})

const pCount = computed(() => {
    let n = 0
    props.data?.Players.forEach((element: LHDPlayerInfo) => {
        if (!element.IsRobot) {
            n++
        }
    });
    return n
})

const rCount = computed(() => {
    let n = 0
    props.data?.Players.forEach((element: LHDPlayerInfo) => {
        if (element.IsRobot) {
            n++
        }
    });
    return n
})

const kickCnt = ref(2)

const route = useRoute()
const rid = parseInt(route.params.rid as string)
const tid = parseInt(route.params.tid as string)

const special_form = reactive({
    active: false,
    cnt: 0,
    rate: 70,
})

const assign_award_form = reactive({
    active: false,
    award: 0,
})

const jackpot_pannel = reactive({
    active: false,
    level: 0,
})

const editJackpotPoolForm = reactive({
    show: false,
    amount: Number(0),
})

const openEditJackpotPoolForm = () => {
    editJackpotPoolForm.show = true
    editJackpotPoolForm.amount = props.data.JackpotInfo.Current / 100
}

const closeEditJackpotPoolForm = () => {

    if (editJackpotPoolForm.amount < props.data.JackpotInfo.BasePool / 100) {
        alert("彩金不可低於基礎彩金")
        return
    }

    emits("editJackpotPool", editJackpotPoolForm.amount * 100)

    editJackpotPoolForm.show = false
    editJackpotPoolForm.amount = 0
}

const tableEnabled = computed({
    get() {
        return !props.data.TableDisabled
    },
    set(value) {
        emits("setTableDisabled", !value)
    },
})

const jackpotAwardpause = computed({
    get() {
        return !props.data.JackpotInfo.Awardpause
    },
    set(value) {
        emits("setJackpotAwardpause", !value)
    },
})

const dashboard = await lhd.dashboard(rid, tid)
const stream = await dashboard.stream()

onUnmounted(() => {
    stream?.close()
})

</script>

<style lang="postcss" scoped>
.label {
    @apply text-xs py-1.5 px-3 rounded-xl cursor-pointer
    bg-sky-100 dark:bg-sky-300
    hover:bg-sky-200 hover:dark:bg-sky-400

    before:content-['#']
    before:mr-2
}

.label.checked {
    @apply bg-sky-300 dark:bg-sky-500
}

</style>