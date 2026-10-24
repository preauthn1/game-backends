<template>
  <v-app>
    <div class="agenttotal__content">
      <router-view></router-view>
    </div>
  </v-app>
</template>

<script>
export default {
  name: 'app',
  data () {
    return {
    }
  }
}
</script>

<style lang="scss" scoped>
html,
body {
  margin: 0;
  height: 100%;
}

.agenttotal__content {
  height: 100%;
}
</style>
