<template>
    <div v-if="loaded"
        class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-7 p-2 gap-2 sm:gap-4 sm:p-5 mx-4 border-b items-center">
        <div class="col-span-1 lg:col-start-2 max-h-44 pr-5">
            <img class="rounded-lg max-w-full max-h-44 mx-auto my-auto"
                :src="'/img/game-icon/' + room.GameCode + '.png'" />
        </div>
        <div class="col-span-1 sm:col-span-4 md:col-span-3 border-l pl-5 lg:col-span-4">
            <div class="text-2xl font-bold p-5">{{ room.GameName }}</div>
            <div class="grid grid-cols-3 gap-3">
                <div class="col-span-full sm:col-span-2 grid gap-1 sm:grid-cols-3">
                    <div class="sm:col-span-2 grid grid-rows-3 gap-1 text-left">
                        <div class="flex items-center group relative">
                            <span class="font-semibold mr-3">Room</span>
                        </div>
                        <div class="flex items-center group relative">
                            <span class="
                                hidden font-semibold mr-3 
                                md:inline
                                after:content-[':'] after:ml-2">ID</span>
                            <span title="Server ID">{{ room.ID }}</span>
                            <CommonToolTip>Server ID</CommonToolTip>
                        </div>
                        <div class="flex items-center group relative">
                            <span class="
                                hidden font-semibold mr-3 
                                md:inline 
                                after:content-[':'] after:ml-2">Flag</span>
                            <span title="Flag">{{ room.Flag }}</span>
                            <CommonToolTip>Flag</CommonToolTip>
                        </div>
                    </div>
                    <div class="grid gap-1 grid-cols-3 text-center sm:flex-row sm:grid-cols-none">
                        <div class="hidden sm:flex h-6"></div>
                        <CommonTableCount class="cols-span-1 sm:justify-between" :count="room.Tables ? room.Tables.length : 0"
                            svg-class="w-5 h-5" />
                        <CommonPlayerCount class="cols-span-1 sm:justify-between" :count="room.PlayerCnt" svg-class="w-5 h-5" />
                        <CommonRobotCount class="cols-span-1 sm:justify-between" :count="room.RobotCnt" svg-class="w-5 h-5" />
                    </div>
                </div>
                <div v-if="table" class="col-span-full sm:col-start-4 grid gap-1 text-center">
                    <div class="flex items-center">
                        <span class="font-semibold mr-3">Talbe</span>
                        <CommonGaming :gaming="table.Gaming" svg-class="w-5 h-5" />
                    </div>
                    <div class="grid gap-1 grid-cols-3 text-center sm:flex-row sm:grid-cols-none">
                        <div v-if="!editSysPoolForm.show">
                            <div class="flex items-center justify-center">
                                <CommonAmount class="sm:justify-between mr-10" title="System Pool" :count="table.SysPool ? table.SysPool : 0" svg-class="w-5 h-5" />
                                <span class="cursor-pointer" @click="openEditSysPoolForm">
                                    <svg xmlns="http://example.invalid/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-6 h-6">
                                        <path stroke-linecap="round" stroke-linejoin="round" d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L10.582 16.07a4.5 4.5 0 01-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 011.13-1.897l8.932-8.931zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0115.75 21H5.25A2.25 2.25 0 013 18.75V8.25A2.25 2.25 0 015.25 6H10" />
                                    </svg>
                                </span>
                            </div>
                        </div>
                        <div v-else class="flex justify-center items-center">

                            <span class="cursor-pointer mr-3" @click="editSysPoolForm.amount = (Math.round(editSysPoolForm.amount * 100) - 100000) / 100">
                                <svg xmlns="http://example.invalid/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-6 h-6">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M15 12H9m12 0a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                            </span>

                            <UiInput v-model="editSysPoolForm.amount" class="!p-1 !w-1/2 text-sm mr-3"></UiInput>

                            <span class="cursor-pointer mr-5" @click="editSysPoolForm.amount = (Math.round(editSysPoolForm.amount * 100) + 100000) / 100">
                                <svg xmlns="http://example.invalid/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-6 h-6">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v6m3-3H9m12 0a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                            </span>

                            <span class="cursor-pointer" @click="closeEditSysPoolForm">
                                <svg xmlns="http://example.invalid/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-6 h-6">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M4.5 12.75l6 6 9-13.5" />
                                </svg>
                            </span>
                        </div>
                        <div>
                            <CommonAmount class="sm:justify-between" title="Win Line" :count="table.SysPoolWinLine ? table.SysPoolWinLine : 0" svg-class="w-5 h-5" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts" setup>
const props = defineProps([
    "loaded",
    "room",
    "table",
])

const emit = defineEmits([
    'editSysPool', 
])

const editSysPoolForm = reactive({
    show: false,
    amount: Number(0),
})

const openEditSysPoolForm = () => {
    editSysPoolForm.show = true
    editSysPoolForm.amount = props.table.SysPool / 100
}

const closeEditSysPoolForm = () => {

    emit("editSysPool", editSysPoolForm.amount * 100)

    editSysPoolForm.show = false
    editSysPoolForm.amount = 0
}

</script>