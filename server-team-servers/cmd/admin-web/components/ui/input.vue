<template>
    <input type="text" class="
        rounded-2xl border-2
        text-lg text-center w-full
        px-10 py-4 outline-none
        bg-neutral-100 dark:bg-neutral-800
        border-neutral-200 dark:border-neutral-700" v-model="value" />
</template>

<script lang="ts" setup>

const props = defineProps([
    "modelValue",
])

const emit = defineEmits([
    "update:modelValue"
])

const value = computed({
    get() {
        return props.modelValue
    },
    set(value) {
        emit("update:modelValue", value)
    },
})

</script>