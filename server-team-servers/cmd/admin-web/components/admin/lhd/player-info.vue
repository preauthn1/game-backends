<template>
    <div class="
        rounded-3xl border overflow-hidden border-neutral-400 dark:border-neutral-600
        h-16
        grid grid-rows-2 gap-2">
        <div class="flex justify-start items-center p-5">
            <div class="flex">
                <SvgRobot v-if="player.IsRobot" class="w-5 h-5 mr-5" />
                <SvgPlayer v-else class="w-5 h-5 mr-5" />
                {{ player.PID }}
            </div>
            <i18n-n class="flex ml-auto" tag="div"
                :value="player.BetAmount ? player.BetAmount / 100 : 0"></i18n-n>
        </div>
        <div class="flex">
            <i18n-n class="border w-1/3 
                flex justify-center items-center" :class="lhd.awardCSS(1, true)" tag="div"
                :value="player.Long ? player.Long / 100 : 0"></i18n-n>
            <i18n-n class="border w-1/3 
                flex justify-center items-center" :class="lhd.awardCSS(2, true)" tag="div"
                :value="player.Tie ? player.Tie / 100 : 0"></i18n-n>
            <i18n-n class="border w-1/3 
                flex justify-center items-center" :class="lhd.awardCSS(3, true)" tag="div"
                :value="player.Hu ? player.Hu / 100 : 0"></i18n-n>
        </div>
    </div>
</template>

<script lang="ts" setup>
const props = defineProps([
    "player",
])

const lhd = useLHD()

</script>