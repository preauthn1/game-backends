<template>
    <span v-if="gaming" title="Gaming" class="font-bold">
        <SvgPlay :class="svgClass" />
    </span>
    <span v-else title="Wait" class="font-bold">
        <SvgPause :class="svgClass" />
    </span>
</template>

<script lang="ts" setup>
const props = defineProps({
    gaming: Boolean,
    svgClass: String,
})
</script>