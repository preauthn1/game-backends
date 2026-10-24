<template>
  <div class="xIcon_content" @click="click">
    <div class="xIcon_button">
      <div class="xIcon_button_icon">
        <v-badge v-if="badge" color="red darken-3">
          <span slot="badge">{{badge}}</span>
          <v-icon
            dark
            large
            @click="1"
          >
          {{icon}}
          </v-icon>
        </v-badge>
        <v-icon v-else
          large
          dark
          @click="1"
        >
        {{icon}}
        </v-icon>
      </div>
      <slot></slot>
    </div>
  </div>
</template>
<script>
export default {
  naem: 'xIcon',
  props: {
    icon: [String],
    badge: [String, Number]
  },
  methods: {
    click () {
      this.$emit('click', this.value)
    }
  }
}
</script>

<style lang="scss" scoped>
.xIcon_content {
  margin: 0% 1%;
}
.xIcon_button {
  display: inline-block;
  padding: 10px 10px;
  min-width: 83px;
  box-shadow: inset 0px 1px 3px 0px #3a3b3b;
  background-color: #212121;
  border-radius: 5px;
  border: 1px solid #212121;
  color: #fff;
  font-family: Arial;
  font-size: 15px;
  font-weight: bold;
  text-decoration: none;
  text-align: center;
  cursor: pointer;

  &:hover {
    background: -webkit-gradient(
      linear,
      left top,
      left bottom,
      color-stop(0.05, #524f4f),
      color-stop(1, #4e4d4d)
    );
  }
}
.xIcon_button_icon {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}
</style>