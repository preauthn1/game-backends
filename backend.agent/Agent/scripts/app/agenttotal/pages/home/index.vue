<template>
<v-app id="inspire">
    <v-navigation-drawer
      fixed
      :clipped="$vuetify.breakpoint.mdAndUp"
      app
      v-model="drawer"
    >
      <v-list dense>
        <template v-for="item in items">
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
      color="blue darken-3"
      dark
      app
      :clipped-left="$vuetify.breakpoint.mdAndUp"
      fixed
    >
      <v-toolbar-title style="width: 300px" class="ml-0 pl-3">
        <v-toolbar-side-icon @click.stop="drawer = !drawer"></v-toolbar-side-icon>
        <span class="hidden-sm-and-down">FZ</span>
      </v-toolbar-title>
      <v-spacer></v-spacer>
      <v-btn icon>
        <v-icon>apps</v-icon>
      </v-btn>
      <v-btn icon>
        <v-icon>notifications</v-icon>
      </v-btn>
      <v-btn icon large>
        <v-avatar size="32px" tile>
        </v-avatar>
      </v-btn>
    </v-toolbar>
    <v-content>
      <v-container >
          <router-view></router-view>
      </v-container>
    </v-content>
  </v-app>
</template>

<script>
export default {
  naem: 'home',
  data: () => ({
    drawer: null,
    items: [
      {
        icon: 'keyboard_arrow_up',
        'icon-alt': 'keyboard_arrow_down',
        text: '总代理入口',
        model: true,
        children: [
          { icon: 'account_balance', text: '转账日志', url: 'transferlog' },
          { icon: 'assignment_ind', text: '商人管理', url: 'management' },
          { icon: 'attach_money', text: '总代加款', url: 'rechargelog' },
          { icon: 'assignment', text: '安全日志', url: 'loginlog' },
          { icon: 'backspace', text: '玩家回调', url: 'errorOrderOpt' },
          { icon: 'add_shopping_cart', text: '商人充值', url: 'rechargelog-agent' },
          { icon: 'remove_shopping_cart', text: '商人退分', url: 'backlog' }
        ]
      },
      
      {
        icon: 'keyboard_arrow_up',
        'icon-alt': 'keyboard_arrow_down',
        text: '代理数据查询',
        model: false,
        children: [
          { icon: 'assessment', text: '代理数据表', action: 'restaurant', url: 'report-day' },
          { icon: 'assignment', text: '代理明细表', action: 'restaurant', url: 'report-agent-details' }
        ]
      },
      {
        icon: 'keyboard_arrow_up',
        'icon-alt': 'keyboard_arrow_down',
        text: '代理商管理',
        model: false,
        children: [
          { icon: 'search', text: '代理商查询', url: 'agent/list' },
          { icon: 'video_library', text: '广告列表' },
          { icon: 'list_alt', text: '代理登录日志', url: 'agent/loginlog' }
        ]
      }
    ]
  }),
  methods: {
    rewriting (url) {
      this.$router.push({ path: `/${url}`, replace: true })
    },
  }
}
</script>

<style lang="scss" scoped>
</style>


