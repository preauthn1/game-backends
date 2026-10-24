<template>
    <NuxtLink class="relative rounded-xl p-3
        border shadow-sm
        border-neutral-400 dark:border-neutral-700 
        shadow-neutral-300 dark:shadow-neutral-700
        hover:bg-neutral-200 hover:dark:bg-neutral-700
        hover:animate-pulse
        " :to="'/admin/' + room.GameCode + '/' + room.ID + '/' + table.ID">

        <CommonGaming class="absolute top-2 right-2" :gaming="table.Gaming" svg-class="w-8 h-8" />

        <div class="text-center text-8xl font-bold">{{ table.ID }}</div>

        <div class="col-span-2 grid grid-cols-2 gap-3 text-center">
            <CommonPlayerCount :count="table.Players ? table.Players.length : 0" svg-class="w-5 h-5">
            </CommonPlayerCount>
            <CommonRobotCount :count="table.Robots ? table.Robots.length : 0" svg-class="w-5 h-5">
            </CommonRobotCount>
        </div>
    </NuxtLink>
</template>

<script lang="ts" setup>
defineProps([
    "room",
    "table",
])
</script>