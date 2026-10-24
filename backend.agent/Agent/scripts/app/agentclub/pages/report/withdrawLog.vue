<template>
  <div>
    <v-toolbar flat>
      <v-toolbar-title>提取纪录</v-toolbar-title>
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
          <v-flex v-if="IsAdmin">
            <v-text-field
              v-model="inputForm.user_id"
              label="玩家ID"
            ></v-text-field>
          </v-flex>
          <v-flex v-if="IsAdmin">
            <v-text-field
              v-model="inputForm.loginName"
              label="帐号名称"
            ></v-text-field>
          </v-flex>
          <v-flex v-if="IsAdmin">
            <v-select
            v-model="inputForm.agentClass"
            :items="classItems"
            item-text="text"
            item-value="value"
            label="身份"
            ></v-select>
          </v-flex>
          <v-flex>
            <v-menu
              ref="menu1"
              :close-on-content-click="false"
              v-model="menu1"
              :nudge-right="40"
              lazy
              transition="scale-transition"
              offset-y
              full-width
              max-width="290px"
              min-width="290px"
            >
              <v-text-field
                slot="activator"
                v-model="inputForm.startDate"
                label="开始时间"
                persistent-hint
                prepend-icon="event"
                readonly
              ></v-text-field>
              <v-date-picker v-model="inputForm.startDate" no-title @input="menu1 = false"></v-date-picker>
            </v-menu>
          </v-flex>
          <v-flex>
            <v-menu
              ref="menu2"
              :close-on-content-click="false"
              v-model="menu2"
              :nudge-right="40"
              lazy
              transition="scale-transition"
              offset-y
              full-width
              max-width="290px"
              min-width="290px"
            >
              <v-text-field
                slot="activator"
                v-model="inputForm.endDate"
                label="结束时间"
                persistent-hint
                prepend-icon="event"
                readonly
              ></v-text-field>
              <v-date-picker v-model="inputForm.endDate" no-title @input="menu2 = false"></v-date-picker>
            </v-menu>
          </v-flex>
          
          <v-btn @click="submit">开始搜索</v-btn>
          <v-btn @click="clear">重置</v-btn>
        </v-layout>
      </v-container>
    </v-form>
    <v-data-table
      :headers="headers"
      :items="desserts"
      :pagination.sync="pagination"
      :total-items="totalDesserts"
      :loading="loading"
      :rows-per-page-items="pageItems"
      class="elevation-1"
    >
      <template slot="items" slot-scope="props">
        <td>{{ props.item.UserID }}</td>
        <td>{{ props.item.LoginName }}</td>
        <td>{{ props.item.ClassName }}</td>
        <td>{{ props.item.PlatformName }}</td>
        <td>{{ props.item.AreaName }}</td>
        <td>{{ props.item.UpdateAmount }}</td>
        <td>{{ props.item.CurrentAmount }}</td>
        <td>{{ props.item.RemainAmount }}</td>
        <td>{{ props.item.CreateTime }}</td>
      </template>
    </v-data-table>
  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex'
import moment from 'moment'

export default {
  name: 'AgentVerify',
  data() {
    return {
      classItems: [],
      menu1: false,
      menu2: false,
      inputForm: {
        user_id: null,
        loginName: null,
        agentClass: null,
        startDate: moment().format('YYYY-MM-DD'),
        endDate: null
      },
      totalDesserts: 0,
      desserts: [],
      loading: true,
      pageItems: [
        50,
        100,
        150,
        { text: '$vuetify.dataIterator.rowsPerPageAll', value: -1 }
      ],
      pagination: {'sortBy': 'CreateTime', 'descending': true},
      headers: [
        {
          text: '玩家ID',
          align: 'left',
          value: 'UserID'
        },
        { text: '帐号名称', value: 'LoginName' },
        { text: '身份', value: 'ClassName' },
        { text: '平台', value: 'PlatformName' },
        { text: '渠道', value: 'AreaName' },
        { text: '提取金额', value: 'UpdateAmount' },
        { text: '提取前金额', value: 'CurrentAmount' },
        { text: '提取后金额', value: 'RemainAmount' },
        { text: '时间', value: 'CreateTime' }
      ]
    }
  },
  
  watch: {
    pagination: {
      handler () {
        this.getDataFromApi().then(data => {
          this.desserts = data.items
          this.totalDesserts = data.total
        })
      },
      deep: true
    }
  },

  async mounted () {
    this.$set(this, 'classItems', await this.getAgentClassItems())
  },

  computed: {
    ...mapGetters(['IsAdmin'])
  },

  methods: {
    submit () {
      this.getDataFromApi().then(data => {
        this.desserts = data.items
        this.totalDesserts = data.total
      })
    },
    clear () {
      this.$refs.form.reset()
    },

    async getDataFromApi () {
      this.loading = true
      return new Promise((resolve, reject) => {
        const { sortBy, descending, page, rowsPerPage } = this.pagination
        this.getAgentWithdrawLog(this.inputForm).then(data => {
          let items = _.cloneDeep(data)
          const total = items.length

          if (this.pagination.sortBy) {
            items = items.sort((a, b) => {
              const sortA = a[sortBy]
              const sortB = b[sortBy]

              if (descending) {
                if (sortA < sortB) return 1
                if (sortA > sortB) return -1
                return 0
              } else {
                if (sortA < sortB) return -1
                if (sortA > sortB) return 1
                return 0
              }
            })
          }

          if (rowsPerPage > 0) {
            items = items.slice((page - 1) * rowsPerPage, page * rowsPerPage)
          }
          setTimeout(() => {
            this.loading = false
            resolve({
              items,
              total
            })
          }, 1000)
        })
      })
    },
    ...mapActions(['getAgentClassItems', 'getAgentWithdrawLog'])
  }
}
</script>
<style lang="scss" scoped>
</style>
