<template>
    <div title="Amount" class="flex items-center justify-center group relative">
        <span class="mr-2">
            <SvgCurrency :class="svgClass" />
        </span>
        <i18n-n tag="span" :value="count ? count / 100 : 0"></i18n-n>
        <!-- <CommonToolTip>
            {{ title ? title : "Amount" }}
        </CommonToolTip> -->
    </div>
</template>

<script lang="ts" setup>

const props = defineProps({
    "title": String,
    "count": Number,
    "svgClass": String,
})

</script>