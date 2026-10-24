<template>
    <div v-if="award"
        class="relative rounded-xl border-2 p-3 mt-5 ml-5 grid grid-rows-2 gap-4 h-52 w-[calc(100%-2.5rem)]"
        :class="cjlhlp.awardCSS(award, true)">
        <AdminCjlhlpAward :award="award" class="absolute -top-5 -left-5 z-20" :full="true"></AdminCjlhlpAward>
        <div class="py-3 border-b border-slate-100 dark:border-slate-700 grid grid-rows-2 gap-3">
            <CommonPlayerCount class="justify-between" :count="playerCount" svg-class="w-5 h-5" />
            <CommonAmount title="Player Bet" class="justify-between" :count="playerBetAmount" svg-class="w-5 h-5" />
        </div>
        <div class="py-3 grid grid-rows-2 gap-3">
            <CommonRobotCount class="justify-between" :count="robotCount" svg-class="w-5 h-5" />
            <CommonAmount title="Robot Bet" class="justify-between" :count="robotBetAmount" svg-class="w-5 h-5" />
        </div>
    </div>
</template>

<script lang="ts" setup>
const props = defineProps({
    "award": Number,
    "playerCount": Number,
    "playerBetAmount": Number,
    "robotCount": Number,
    "robotBetAmount": Number,
})

const cjlhlp = useCJLHLP()

</script>