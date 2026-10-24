<template>
  <div class="text-xs-center">
    <v-menu
      v-model="menu"
      :close-on-content-click="false"
      :nudge-width="200"
      offset-x
    >
      <v-btn icon slot="activator">
        <v-badge  color="red darken-3">
          <span slot="badge">{{totalCount}}</span>
          <v-icon medium>notifications</v-icon>
        </v-badge>
      </v-btn>
      <v-card>
        <v-divider></v-divider>
        <v-list>
          <v-list-tile avatar>
            <v-list-tile-avatar>
              <v-btn icon>
                <v-badge color="red darken-3">
                  <span slot="badge">{{applyCount}}</span>
                  <v-icon>assignment_turned_in</v-icon>
                </v-badge>
              </v-btn>
            </v-list-tile-avatar>

            <v-list-tile-content>
              <v-list-tile-title>代理审核</v-list-tile-title>
              <v-list-tile-sub-title>移至代理审核</v-list-tile-sub-title>
            </v-list-tile-content>

            <v-list-tile-action>
              <v-btn icon @click="goPage('/agent/verify')">
                <v-icon>forward</v-icon>
              </v-btn>
            </v-list-tile-action>
          </v-list-tile>
        </v-list>
        <v-divider></v-divider>
      </v-card>
    </v-menu>
  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex'
export default {
  naem: 'x-notifications',

  data: () => ({
    fav: true,
    menu: false,
    message: false,
    hints: true,
    applyCount: 0,
    totalCount: 0
  }),

  computed: {
    ...mapGetters(['IsAdmin'])
  },

  async created () {
    let self = this;
    await self.getApplyCount()

    setInterval(() => {
      self.getApplyCount()
    }, 30000)
  },

  methods: {
    goPage (url) {
      this.$router.push({ path: url, replace: true })
    },
    async getApplyCount () {
      const agentApplyCount = await this.getAgentApplyCount()
      this.$set(this, 'applyCount', agentApplyCount)
      this.$set(this, 'totalCount', agentApplyCount)
    },
    ...mapActions(['getAgentApplyCount'])
  }
}
</script>

<style lang="scss" scoped>

</style>

