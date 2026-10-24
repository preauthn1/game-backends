<template>
  <div>
    <v-toolbar flat>
      <v-toolbar-title>阶级设定</v-toolbar-title>
      <v-divider
          class="mx-2"
          inset
          vertical
      ></v-divider>
      <v-spacer></v-spacer>
    </v-toolbar>
    <v-form ref="form">
      <v-container>
        <v-layout row wrap>
          <v-flex>
            <v-text-field
              v-model="inputForm.user_id"
              label="玩家ID"
            ></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field
              v-model="inputForm.loginName"
              label="帐号名称"
            ></v-text-field>
          </v-flex>          
          <v-btn @click="submit">开始搜索</v-btn>
          <v-btn @click="clear">重置</v-btn>
        </v-layout>
      </v-container>
    </v-form>
    <v-container fluid grid-list-md>
      <v-data-iterator
        :items="items"
        :rows-per-page-items="rowsPerPageItems"
        :pagination.sync="pagination"
        rows-per-page-text="每页行数"
        no-data-text="无资料"
        content-tag="v-layout"
        row
        wrap
      >
        <v-flex
          slot="item"
          slot-scope="props"
          xs12
          sm6
          md4
          lg3
        >
          <v-card>
            <v-card-title >
              <v-layout align-center justify-space-between row fill-height>
                  <h4>{{ props.item.className }}</h4>
                  <h4>{{ props.item.playerID }}</h4>
              </v-layout>
            </v-card-title>
            <v-divider></v-divider>
            <v-list dense>
               <v-list-tile>
                <v-list-tile-content>名称</v-list-tile-content>
                <v-list-tile-content>分成</v-list-tile-content>
                <v-list-tile-content>修改</v-list-tile-content>
              </v-list-tile>
              <v-list-tile v-for="(item, index) in props.item.level" :key="index">
                <v-list-tile-content>{{item.gradeName}}</v-list-tile-content>
                <v-list-tile-content>{{ item.distributeRate }}</v-list-tile-content>
                <v-list-tile-content>
                  <v-layout align-center justify-space-between row fill-height all_width>
                    <v-icon
                      @click="editItem(item, props.item)"
                    >
                      edit
                    </v-icon>
                    <v-icon v-if="item.level != 1"
                      @click="deleteItem(item)"
                    >
                    delete
                    </v-icon>
                  </v-layout >
                </v-list-tile-content>
                
              </v-list-tile>
            </v-list>
          </v-card>
        </v-flex>
      </v-data-iterator>
    </v-container>

    <v-dialog v-model="dialog" max-width="450px">
      <v-card>
        <v-card-title>
          <span class="headline"></span>
        </v-card-title>
        <v-card-text>
          <v-form ref="form2">
          <v-container grid-list-md>
            <v-flex>
              <v-text-field type="number" v-model="editedItem.distributeRate" :rules="rateRange_rules"  label="分成"></v-text-field>
            </v-flex>
          </v-container>
          </v-form>
        </v-card-text>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="blue darken-1" flat @click.native="close">Cancel</v-btn>
          <v-btn color="blue darken-1" flat @click.native="save">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex'

export default {
  name: 'AgentLevel',
  data() {
    return {
      dialog: false,
      rowsPerPageItems: [24, 48, 96],
      pagination: {
        rowsPerPage: 24
      },
      items: [],
      inputForm: {
        user_id: null,
        loginName: null
      },
      defaultItem: {
        gradeName: null,
        level: null,
        distributeRate: null,
        upGradeLimit: null,
      },
      editedItem: {
        gradeName: null,
        level: null,
        distributeRate: null,
        upGradeLimit: null,
        RateRange: null
      },
      agentMinRate: 0,
      agentMaxRate: 0,
      rules: {
        required: value => !!value || value === 0 || '请输入'
      },
      number_rules: [
        v =>  !!v || v === 0 || '请输入',
        v => v >= 0 || '不可小于0'
      ],
      rateRange_rules: [
        v =>  !!v || v === 0 || '请输入',
        v => v >= 0 || '不可小于0',
        v =>  (this.agentMaxRate - 1) >= v || `不可大于您现在的分成 ( 目前分成为 ${ this.agentMaxRate }, 至少保留1% )`,
        v =>  (this.agentMinRate + 1) <= v || `不可小于下级代理的最大分成 ( 目前下级最大分成为 ${ this.agentMinRate }, 至少保留1% )`
      ]
    }
  },

  async mounted () {
    this.getDataFromApi()
  },
  
  computed: {
    ...mapGetters(['IsAdmin'])
  },

  methods: {
    submit () {
      this.getDataFromApi()
    },
    clear () {
      this.$refs.form.reset()
    },
    async editItem (item) {
      let max = 100
      let min = 0
      
      if(!this.IsAdmin) {
        const {MaxRate, MinRate} = await this.getAgentMaxMinRate(item.userID)
        max = MaxRate
        min = MinRate
      }
      this.$set(this, 'agentMaxRate', max)
      this.$set(this, 'agentMinRate', min)
      this.editedItem = Object.assign({}, item)
      this.dialog = true
    },
    async deleteItem (item) {
      const res = await this.deleteGrandConfig(item.grandID)
      this.restart(res)
    },
    close () {
      this.$set(this, 'dialog', false)
    },
    async save () {
      if (this.$refs.form2.validate()) {
        this.$set(this, 'dialog', false)
        const res = await this.updateGrandConfig(this.editedItem)
        this.restart(res)
      }
    },
    async getDataFromApi () {
      this.loading = true
      return new Promise((resolve, reject) => {
        this.getGrandConfig(this.inputForm).then(data => {
          this.$set(this, 'items', data)
        })
      })
    },
    restart (isSuccess) {
      if(isSuccess) this.getDataFromApi()
    },
    ...mapActions(['getGrandConfig', 'getAgentMaxMinRate', 'updateGrandConfig', 'deleteGrandConfig'])
  }
}
</script>
<style lang="scss" scoped>
</style>
