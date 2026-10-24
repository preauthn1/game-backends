<template>
  <div>  
    <v-menu
      v-model="tools"
      :close-on-content-click="false"
      :nudge-width="200"
      offset-x
    >
      <v-btn icon
        slot="activator">
        <v-icon x-large>face</v-icon>
      </v-btn>
      <v-card>
        <v-list>
          <v-list-tile avatar>
            <v-list-tile-avatar>
              <v-icon x-large>face</v-icon>
            </v-list-tile-avatar>
            <v-list-tile-content>
              <v-list-tile-title>{{UserLoginName}}</v-list-tile-title>
              <v-list-tile-sub-title>代理中心</v-list-tile-sub-title>
            </v-list-tile-content>
            <v-list-tile-action>
              <v-btn icon>
                <v-icon>computer</v-icon>
              </v-btn>
            </v-list-tile-action>
          </v-list-tile>
        </v-list>
        <v-divider></v-divider>
        <v-list>
          <v-list-tile>
            <v-list-tile-action>
              <v-switch v-model="theme"></v-switch>
            </v-list-tile-action>
            <v-list-tile-title>切换主题</v-list-tile-title>
          </v-list-tile>
        </v-list>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn flat @click="logout">登出</v-btn>
        </v-card-actions>
      </v-card>
    </v-menu>
  </div>
</template>
<script>
import { mapActions, mapGetters } from 'vuex'

export default {
  naem: 'x-avatar',
  data: () => ({
    tools: false,
    theme: false
  }),

  watch: {
    theme (val) {
      this.changeTheme(val)
    }
  },

  computed: {
    ...mapGetters(['UserLoginName', 'Theme'])
  },

  methods: {
    ...mapActions(['logout', 'changeTheme'])
  }
}
</script>

<style lang="scss" scoped>
</style>


