<template>
    <NuxtLink v-if="room.Actived" class="relative rounded-xl p-5
        border shadow-sm grid grid-cols-3 gap-4
        border-neutral-400 dark:border-neutral-700 
        shadow-neutral-300 dark:shadow-neutral-700
        hover:bg-neutral-200 hover:dark:bg-neutral-700
        hover:animate-pulse
        justify-center items-center" :to="'/room/' + room.ID">

        <SvgPlay class="absolute top-3 right-3 w-5 h-5" />

        <div class="col-span-1">
            <img class="rounded-lg" :src="'/img/game-icon/' + room.GameCode + '.png'" />
        </div>

        <div class="col-span-2">
            <div class="font-bold">{{ room.GameName }}</div>
            <div>{{ room.ID }}</div>
            <div>Flag: {{ room.Flag }}</div>
        </div>
    </NuxtLink>
    <NuxtLink v-else class="relative rounded-xl p-5
        border shadow-sm grid grid-cols-3 gap-4
        border-neutral-400 dark:border-neutral-700 
        shadow-neutral-300 dark:shadow-neutral-700
        hover:bg-neutral-200 hover:dark:bg-neutral-700
        hover:animate-pulse
        justify-center items-center">
        <SvgPause class="absolute top-3 right-3 w-5 h-5" />

        <div class="col-span-1">
            <img class="rounded-lg" :src="'/img/game-icon/' + room.GameCode + '.png'" />
        </div>

        <div class="col-span-2">
            <div class="font-bold">{{ room.GameName }}</div>
            <div>{{ room.ID }}</div>
            <div>Flag: {{ room.Flag }}</div>
        </div>
    </NuxtLink>
</template>

<script lang="ts" setup>
defineProps([
    "room"
])
</script>
