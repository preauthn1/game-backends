<template>
  <div>
    <v-toolbar>
      <v-toolbar-title>我的代理</v-toolbar-title>
      <v-divider
          class="mx-2"
          inset
          vertical
      ></v-divider>
      <div class="list_title">
        {{title[index - 1].Class}}
      </div>
      <div class="list_title">
        {{title[index - 1].UserID}}
      </div>
      <v-spacer></v-spacer>
    </v-toolbar>
    <v-stepper v-model="e1"  >
      <v-stepper-items>
        <v-stepper-content
          v-for="n in steps"
          :key="`${n}-content`"
          :step="n"
        >
          <x-table
            :title.sync="title"
            :index="index"
            :pageIndex = n
            :classType.sync="classType"
            :agentID.sync="agent_id"
            :id="agent_id"
            @click="nextStep"
          ></x-table>
          <v-btn v-if="n != 1" @click="back(n)">返回上级</v-btn>
          <v-btn v-if="n != 1" @click="top()">返回最上层</v-btn>
        </v-stepper-content>
      </v-stepper-items>
    </v-stepper>    
  </div>
</template>

<script>
import xTable from '@/agentclub/components/x-table'

export default {
  name: 'AgentList',
  data() {
    return {
      title: [{
        UserID: null,
        Class: null
      }],
      agent_id: null,
      old_agent_id: null,
      classType: null,
      index: 1,
      e1: 1,
      steps: 5,
      menu1: false,
      menu2: false
    }
  },
  watch: {
    steps (val) {
      if (this.e1 > val) {
        this.e1 = val
      }
    }
  },
  methods: {
    onInput (val) {
      this.steps = parseInt(val)
    },
    nextStep (n, id) {
      this.$set(this, 'old_agent_id', id)
      if (n === this.steps) {
        this.e1 = 1
        this.index = 1
      } else {
        this.e1 = n + 1
        this.index = n + 1
      }
    },
    back (n) {
      this.$set(this, 'agent_id', this.old_agent_id)
      this.index = n - 1
      this.e1 = n - 1
    },
    top () {
      this.$set(this, 'agent_id', null)
      this.index = 1
      this.e1 = 1
    }
  },
  components: {
    xTable
  }
}
</script>
<style lang="scss" scoped>
.list_title{
  padding: 0 10px;
  font-size: 18px;
}
</style>
