<template>
    <Breadcrumb :links="links"></Breadcrumb>
    <RoomInfo :loaded="room.loaded" :room="unref(room.state)" :table="table" @edit-sys-pool="(amt: number) => onSysPoolChange(amt)" />
    <AdminLhdDashboard :room="unref(room.state)" :table="table" :data="unref(dashboard.state)"
        @cheat="(cnt: number) => dashboard.cheat(cnt)"
        @special="(cnt: number, rate: number) => dashboard.special(cnt, rate)" />
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

const lhd = useLHD()

const room = await useRoom(rid)
const dashboard = await lhd.dashboard(rid, tid)

const table = computed(() => {
    const r = unref(room.state)
    if (!r) {
        return null
    }
    return r.Tables[tid]
})

provide("lhd-dashboard", dashboard)

const onSysPoolChange = (amount: number) => {
    if (!table.value) {
        return    
    }
    const diff = amount - table.value.SysPool

    if (diff != 0) {
        dashboard.incSysPool(diff)
    }
}

const abc = (s: string) => {
    console.log("pages", s)
}

</script>