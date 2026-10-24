<template>
    <Breadcrumb :links="links"></Breadcrumb>
    <RoomInfo :loaded="room.loaded" :room="unref(room.state)" :table="table" @edit-sys-pool="(amt: number) => onSysPoolChange(amt)" />
    <AdminJzyqDashboard :room="unref(room.state)" :table="table" :data="unref(dashboard.state)"
        @cheat="(cnt: number) => dashboard.cheat(cnt)" 
        @special="(cnt: number, rate: number) => dashboard.special(cnt, rate)" 
        @setTableDisabled="(disabled: boolean) => dashboard.setTableDisabled(disabled)"
        @assignNextAward="(award: number) => dashboard.assignNextAward(award)" />
</template>

<script lang="ts" setup>
const route = useRoute()

definePageMeta({
    middleware: ["auth"],
})

const rid = parseInt(route.params.rid as string)
const tid = parseInt(route.params.tid as string)

const links = [
    {
        href: "/",
        text: "Home",
    },
    {
        href: "/room/" + rid,
        text: "Room [ " + rid + " ] ",
    },
    {
        href: "",
        text: "Table [ " + tid + " ] ",
        active: true,
    }
]

const jzyq = useJZYQ()

const room = await useRoom(rid)
const dashboard = await jzyq.dashboard(rid, tid)

const table = computed(() => {
    const r = unref(room.state)
    if (!r) {
        return null
    }
    return r.Tables[tid]
})

provide("jzyq-dashboard", dashboard)

const onSysPoolChange = (amount: number) => {
    if (!table.value) {
        return    
    }
    const diff = amount - table.value.SysPool

    if (diff != 0) {
        dashboard.incSysPool(diff)
    }
}

</script>