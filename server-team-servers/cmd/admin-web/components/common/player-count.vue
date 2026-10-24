<template>
    <div title="Player Count" class="flex items-center justify-center group relative">
        <span class="mr-2">
            <SvgPlayer :class="svgClass" />
        </span>
        <i18n-n tag="span" :value="count ? count : 0"></i18n-n>
        <CommonToolTip>
            {{ title ? title : "Player Count" }}
        </CommonToolTip>
    </div>
</template>

<script lang="ts" setup>
const props = defineProps({
    "title": String,
    "count": Number,
    "svgClass": String,
})
</script>