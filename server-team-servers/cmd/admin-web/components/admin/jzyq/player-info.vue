<template>
    <div class="
        rounded-3xl border overflow-hidden border-neutral-400 dark:border-neutral-600
        h-28
        grid grid-rows-3 gap-2">
        <div class="row-span-1 flex justify-start items-center p-5">
            <div class="flex">
                <SvgRobot v-if="player.IsRobot" class="w-5 h-5 mr-5" />
                <SvgPlayer v-else class="w-5 h-5 mr-5" />
                {{ player.PID }}
            </div>
            <i18n-n class="flex ml-auto" tag="div"
                :value="player.BetAmount ? player.BetAmount / 100 : 0"></i18n-n>
        </div>
        <div class="row-span-2 grid grid-cols-2">
            <div class="grid grid-rows-2">
                <div class="flex">
                    <i18n-n class="border w-1/2
                        flex justify-center items-center" :class="jzyq.awardCSS(17, true)" tag="div"
                        :value="player.GoalInside ? player.GoalInside / 100 : 0"></i18n-n>
                    <i18n-n class="border w-1/2
                        flex justify-center items-center" :class="jzyq.awardCSS(18, true)" tag="div"
                        :value="player.GoalOutside ? player.GoalOutside / 100 : 0"></i18n-n>
                </div>
                <div class="flex">
                    <i18n-n class="border w-full
                        flex justify-center items-center" :class="jzyq.awardCSS(16, true)" tag="div"
                        :value="player.Goal ? player.Goal / 100 : 0"></i18n-n>
                </div>
            </div>
            <div class="grid grid-rows-2">
                <div class="flex">
                    <i18n-n class="border w-1/2
                        flex justify-center items-center" :class="jzyq.awardCSS(33, true)" tag="div"
                        :value="player.LostInside ? player.LostInside / 100 : 0"></i18n-n>
                    <i18n-n class="border w-1/2 
                        flex justify-center items-center" :class="jzyq.awardCSS(34, true)" tag="div"
                        :value="player.LostOutside ? player.LostOutside / 100 : 0"></i18n-n>
                </div>
                <div class="flex">
                    <i18n-n class="border w-full
                        flex justify-center items-center" :class="jzyq.awardCSS(32, true)" tag="div"
                        :value="player.Lost ? player.Lost / 100 : 0"></i18n-n>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts" setup>
const props = defineProps([
    "player",
])

const jzyq = useJZYQ()

</script>