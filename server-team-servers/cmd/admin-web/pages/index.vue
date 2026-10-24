<template>
    <div class="relative overflow-auto">
        <RoomList />
    </div>
</template>

<script lang="ts" setup>

useHead({
    title: "target Admin Dashboard",
})

definePageMeta({
    middleware: ["auth"],
})

const { auth } = useAuth()

</script>
