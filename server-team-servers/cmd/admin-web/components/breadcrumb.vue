<template>

    <div class="hidden md:flex py-1 lg:px-8 mx-4">
        <NuxtLink class="mr-3 
                font-normal hover:font-bold
                after:ml-3 after:content-['/'] after:last:hidden" v-for="link of links" :to="link.href"
            :class="link.active ? 'text-neutral-700 dark:text-neutral-100' : 'text-neutral-400 dark:text-neutral-400'">
            {{ link.text }}
        </NuxtLink>
    </div>

</template>

<script lang="ts" setup>
defineProps([
    "links"
])
</script>