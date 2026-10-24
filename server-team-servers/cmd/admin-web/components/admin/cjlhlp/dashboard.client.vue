<template>
    <div v-if="data" class="grid grid-cols-8 mx-4 gap-4">
        <div class="col-span-full lg:col-span-6 lg:col-start-2">
            <div class="p-3 grid grid-cols-3 gap-4 text-center justify-center items-center">
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
                        'hidden': cjlhlp.stateTxt(data.Step).length == 0,
                        'animate-pulse': data.Step == 2,
                    }">
                        {{ cjlhlp.stateTxt(data.Step) }}
                    </div>
                </div>
                <div class="py-3 grid justify-center items-center">
                    <div class="py-3 grid justify-center items-center">
                        <div class="flex justify-center items-center gap-5">
                            <AdminCjlhlpAward v-if="data.NextAwardList.length > 0" :award="data.NextAwardList[0]" />
                            <UiButtonDanger :disabled="data.Cheating || data.SpecialRewardCount != 0" @click="assign_award_form.active = !assign_award_form.active" class="text-sm">指定開獎</UiButtonDanger>
                        </div>
                        <div v-if="assign_award_form.active" class="absolute rounded-xl p-3 grid justify-center items-center bg-neutral-200 dark:bg-neutral-700 w-3/5 z-50 top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2">
                            <span @click="assign_award_form.active = false" class="absolute top-2 right-2 cursor-pointer">
                                <svg xmlns="http://example.invalid/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-7 h-7">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M9.75 9.75l4.5 4.5m0-4.5l-4.5 4.5M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                            </span>
                            <div class="flex justify-center items-center border-b-2 pb-3 gap-5 mb-5">
                                <div>指定開獎</div>
                                <AdminCjlhlpAward v-if="data.NextAwardList.length > 0" :award="data.NextAwardList[0]" />
                            </div>
                            <div class="grid grid-cols-6 justify-center items-center gap-4">
                                <AdminCjlhlpAward v-for="i in 37" class="cursor-pointer" :award="i-1" @click="$emit('assignNextAward', i-1)" />
                            </div>
                        </div>
                    </div>
                </div>
                <div class="py-3 grid justify-center items-center">
                    <div>本日</div>
                    <div class="flex justify-center items-center">
                        <span class="mr-3">總押注:</span>
                        <CommonAmount svg-class="w-3 h-3" :count="data.TotalBetAmount"></CommonAmount>
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
                        <AdminCjlhlpAward :award="row" />
                    </div>
                </div>
                <div class="sticky bottom-0 z-20 w-full h-2 backdrop-blur"></div>
            </div>
        </div>
    </div>
</template>


<script lang="ts" setup>
import { CJLHLPPlayerInfo } from '~/@types/cjlhlp';

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
])

const dayjs = useDayjs()

const { auth } = useAuth()

const cjlhlp = useCJLHLP()

const pFilter = ref('all')

const pFilterId = ref('')

const pFilterIdRegx = computed(() => {
    return '^' + pFilterId.value + '.*$'
})

const players = computed(() => {

    let players = [] as CJLHLPPlayerInfo[]

    if (!props.data.Players) {
        return players
    }

    props.data?.Players.forEach((element: CJLHLPPlayerInfo) => {
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
    props.data?.Players.forEach((element: CJLHLPPlayerInfo) => {
        if (!element.IsRobot) {
            n++
        }
    });
    return n
})

const rCount = computed(() => {
    let n = 0
    props.data?.Players.forEach((element: CJLHLPPlayerInfo) => {
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

const tableEnabled = computed({
    get() {
        return !props.data.TableDisabled
    },
    set(value) {
        emits("setTableDisabled", !value)
    },
})

const dashboard = await cjlhlp.dashboard(rid, tid)
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