<template>
  <v-app id="inspire" :dark="Theme">
    <v-navigation-drawer
      fixed
      :clipped="$vuetify.breakpoint.mdAndUp"
      app
      v-model="drawer"
    >
      <v-list dense>
        <template v-for="item in menu">
          <v-layout
            row
            v-if="item.heading"
            align-center
            :key="item.heading"
          >
            <v-flex xs6>
              <v-subheader v-if="item.heading">
                {{ item.heading }}
              </v-subheader>
            </v-flex>
            <v-flex xs6 class="text-xs-center">
              <a href="#!" class="body-2 black--text">EDIT</a>
            </v-flex>
          </v-layout>
          <v-list-group
            v-else-if="item.children"
            v-model="item.model"
            :key="item.text"
            :prepend-icon="item.model ? item.icon : item['icon-alt']"
            append-icon=""
          >
            <v-list-tile slot="activator" >
              <v-list-tile-content>
                <v-list-tile-title>
                  {{ item.text }}
                </v-list-tile-title>
              </v-list-tile-content>
            </v-list-tile>
            <v-list-tile
              v-for="(child, i) in item.children"
              :key="i"
              @click="rewriting(child.url)"
            >
              <v-list-tile-action v-if="child.icon">
                <v-icon>{{ child.icon }}</v-icon>
              </v-list-tile-action>
              <v-list-tile-content>
                <v-list-tile-title>
                  {{ child.text }}
                </v-list-tile-title>
              </v-list-tile-content>
            </v-list-tile>
          </v-list-group>
          <v-list-tile v-else :key="item.text">
            <v-list-tile-action>
              <v-icon>{{ item.icon }}</v-icon>
            </v-list-tile-action>
            <v-list-tile-content>
              <v-list-tile-title>
                {{ item.text }}
              </v-list-tile-title>
            </v-list-tile-content>
          </v-list-tile>
        </template>
      </v-list>
    </v-navigation-drawer>
    <v-toolbar
      color="light-blue darken-4"
      dark
      app
      :clipped-left="$vuetify.breakpoint.mdAndUp"
      fixed
    >
      <v-toolbar-title style="width: 300px" class="ml-0 pl-3">
        <v-toolbar-side-icon @click.stop="drawer = !drawer"></v-toolbar-side-icon>
        <span class="hidden-sm-and-down">代理中心</span>
      </v-toolbar-title>
      <v-spacer></v-spacer>
      <span class="hidden-sm-and-down home_loginname">{{UserLoginName}}</span>
      <x-notifications></x-notifications>
      <v-btn icon>
        <v-icon @click="sheet = true">apps</v-icon>
      </v-btn>
      <x-avatar></x-avatar>
    </v-toolbar>
    <v-content>
      <v-container>
        <router-view></router-view>
      </v-container>
    </v-content>
    <v-bottom-sheet v-model="sheet">
      <v-list>
        <v-subheader>Open in</v-subheader>
        <v-list-tile
          v-for="tile in tiles"
          :key="tile.title"
          :disabled="tile.url===''"
          @click="rewriting(tile.url, tile.fun)"
        >
          <v-list-tile-avatar>
            <v-avatar size="32px" tile>
              <v-icon
                large
              >
                {{tile.img}}
              </v-icon>
            </v-avatar>
          </v-list-tile-avatar>
          <v-list-tile-title>{{ tile.title }}</v-list-tile-title>
        </v-list-tile>
      </v-list>
    </v-bottom-sheet>
  </v-app>
</template>

<script>
import { mapActions, mapGetters } from 'vuex'
import xAvatar from '@/agentclub/components/x-avatar'
import xNotifications from '@/agentclub/components/x-notifications'

export default {
  naem: 'home',
  data: () => ({
    tools: false,
    theme: false,
    drawer: null,
    menu: null,
    sheet: false,
    tiles: null
  }),

  computed: {
    ...mapGetters(['UserLoginName', 'Theme'])
  },

  async created () {
    this.$set(this, 'theme',!this.Theme)
    const menu =  await this.getMenuItems()
    const tiles =  await this.getGeneralItems()

    this.$set(this, 'menu', menu)
    this.$set(this, 'tiles', tiles)
  },

  methods: {
    rewriting (url, fun) {
      if(url){
        this.$set(this, 'sheet', false)
      }else {
        this.$store.dispatch(fun)
      }
      this.$router.push({ path: `/${url}`, replace: true })
    },
    ...mapActions(['getMenuItems', 'getGeneralItems'])
  },
  
  components: {
    xAvatar,
    xNotifications
  }
}
</script>

<style lang="scss" scoped>
  .home_loginname{
    font-size: 1.4em;
  }
  .v-list--disabled {
    color: rgb(228, 93, 93) !important ;
  }
</style>


