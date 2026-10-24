<template>
    <div v-if="award != null" class="rounded-full leading-9 w-10 h-10 border-2 text-center align-middle"
        :class="cjlhlp.awardCSS(award, full)">
        {{ award }}
    </div>
</template>

<script lang="ts" setup>
defineProps({
    "award": Number,
    "full": Boolean,
})

const cjlhlp = useCJLHLP()

</script>