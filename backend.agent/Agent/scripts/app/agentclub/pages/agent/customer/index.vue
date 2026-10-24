<template>
  <div>
    <v-toolbar flat>
      <v-toolbar-title>我的玩家</v-toolbar-title>
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
        <td>{{ props.item.Class }}</td>
        <td>{{ props.item.Tel }}</td>
        <td>{{ props.item.Weixin }}</td>
        <td>{{ props.item.QQ }}</td>
        <td>{{ props.item.Email }}</td>
        <td>{{ props.item.UserTotalFlows }}</td>
        <td>{{ props.item.CreateTime }}</td>
        <td>
          <div v-if="!IsSecondAgent">
            <v-icon
              class="mr-4"
              @click="customerUp(props.item.UserID)"
            >
              group_add
            </v-icon>
          </div>
        </td>
      </template>
    </v-data-table>
  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex'

export default {
  name: 'AgentVerify',
  data() {
    return {
      menu1: false,
      menu2: false,
      inputForm: {
        user_id: null,
        loginName: null,
        agentClass: null,
        startDate: null,
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
        { text: '身份', value: 'Class' },
        { text: '电话', value: 'Tel' },
        { text: '微信', value: 'Weixin' },
        { text: 'QQ', value: 'QQ' },
        { text: '信箱', value: 'Email' },
        { text: '顾客总税收', value: 'UserTotalFlows' },
        { text: '时间', value: 'CreateTime' },
        { text: '升级成为代理', value: 'name', sortable: false }
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

  computed: {
    ...mapGetters(['IsSecondAgent'])
  },

  methods: {
    submit () {
      this.getDataFromApi().then(data => {
        this.desserts = data.items
        this.totalDesserts = data.total
      })
    },
    async customerUp (userID) {
      await this.customerUpgrade(userID)
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
        this.getAgentCustomer(this.inputForm).then(data => {
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
    ...mapActions(['getAgentCustomer', 'customerUpgrade'])
  }
}
</script>
<style lang="scss" scoped>
</style>
