<template>
    <Breadcrumb :links="links"></Breadcrumb>
    <RoomInfo :loaded="room.loaded" :room="unref(room.state)" />
    <div v-if="room.loaded" class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 p-5">
        <TableShort v-for="table of room.state.value?.Tables" :room="room.state.value" :table="table"></TableShort>
    </div>
</template>

<script lang="ts" setup>
const route = useRoute()

definePageMeta({
    middleware: ["auth"],
})

const rid = parseInt(route.params.id as string)
const room = await useRoom(rid)

const links = [
    {
        href: "/",
        text: "Home",
    },
    {
        href: "",
        text: "Room [ " + rid + " ] ",
        active: true,
    },
]

</script>