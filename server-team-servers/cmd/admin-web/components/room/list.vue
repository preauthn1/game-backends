<template>
    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 p-5">
        <RoomShort v-for="room of rooms" :room="room"></RoomShort>
    </div>
</template>

<script lang="ts" setup>

const { data: rooms, pending, error } = await useRooms().fetchAll()

const { logout } = useAuth()

if (error.value?.statusCode == 403) {
    logout()
}

</script>
