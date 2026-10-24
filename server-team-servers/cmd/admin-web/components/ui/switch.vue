<template>
    <button class="switch" :class="{ 'active': value }" @click="value = !value">
        <span>
            <slot></slot>
        </span>
    </button>
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

<style lang="postcss">
.switch {
    /** basic **/
    & {
      @apply 
        relative rounded-full flex items-center
        disabled:opacity-50
        disabled:cursor-not-allowed
        border border-solid
      ;

      span {
        @apply 
          rounded-full flex items-center justify-center 
          transition duration-300 translate-x-1
        ;
      }
    }

    /** size **/
    & {
      & {
        @apply w-14 h-7;

        span {
          @apply w-5 h-5;

          svg {
            @apply w-3 h-3;
          }
        }

        &.active span {
          @apply translate-x-7;
        }
      }

      &.switch-xs {
        @apply w-9 h-5;

        span {
          @apply w-[14px] h-[14px];

          svg {
            @apply w-2 h-2;
          }
        }

        &.active span {
          @apply translate-x-4;
        }
      }

      &.switch-sm {
        @apply w-11 h-6;

        span {
          @apply w-4 h-4;

          svg {
            @apply w-[10px] h-[10px];
          }
        }

        &.active span {
          @apply translate-x-[22px];
        }
      }

      &.switch-lg {
        @apply w-16 h-8;

        span {
          @apply w-6 h-6;

          svg {
            @apply w-4 h-4;
          }
        }

        &.active span {
          @apply translate-x-8;
        }
      }

      &.switch-xl {
        @apply w-20 h-10;

        span {
          @apply w-8 h-8;

          svg {
            @apply w-5 h-5;
          }
        }

        &.active span {
          @apply translate-x-10;
        }
      }

      &.switch-2xl {
        @apply w-24 h-12;

        span {
          @apply w-9 h-9;

          svg {
            @apply w-6 h-6;
          }
        }

        &.active span {
          @apply translate-x-[3.25rem];
        }
      }
    }

    /** color **/
    & {
      & {
        @apply border-neutral-300 dark:border-neutral-600
        bg-neutral-100 dark:bg-neutral-700;

        &:not([disabled]):hover span {
          @apply bg-neutral-600 dark:bg-neutral-800;
        }

        span {
          @apply bg-neutral-500 text-neutral-100 dark:bg-neutral-900;
        }
      }

      &.switch-primary {
        @apply border-fuchsia-300 bg-fuchsia-100;

        &:not([disabled]):hover span {
          @apply bg-fuchsia-600;
        }

        & span {
          @apply bg-fuchsia-500;
        }
      }

      &.switch-info {
        @apply border-sky-300 bg-sky-100;

        &:not([disabled]):hover span {
          @apply bg-sky-600;
        }

        & span {
          @apply bg-sky-500;
        }
      }

      &.switch-success {
        @apply border-emerald-300 bg-emerald-100;

        &:not([disabled]):hover span {
          @apply bg-emerald-600;
        }

        & span {
          @apply bg-emerald-500;
        }
      }

      &.switch-warn {
        @apply border-orange-300 bg-orange-100;

        &:not([disabled]):hover span {
          @apply bg-orange-600;
        }

        & span {
          @apply bg-orange-500;
        }
      }

      &.switch-danger {
        @apply border-rose-300 bg-rose-100;

        &:not([disabled]):hover span {
          @apply bg-rose-600;
        }

        & span {
          @apply bg-rose-500;
        }
      }
    }
  }
</style>