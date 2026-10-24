<template>
    <div v-if="award" class="rounded-full leading-9 w-10 h-10 border-2 text-center align-middle"
        :class="lhd.awardCSS(award, full)">
        {{ lhd.awardTxt(award) }}
    </div>
</template>

<script lang="ts" setup>
defineProps({
    "award": Number,
    "full": Boolean,
})

const lhd = useLHD()

</script>