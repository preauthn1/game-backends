<template>
  <div>
    <v-toolbar flat>
      <v-toolbar-title>操作纪录</v-toolbar-title>
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
      rows-per-page-text="每页行数"
      no-data-text="无资料"
    >
      <template slot="headers" slot-scope="props">
        <tr>
          <th
            v-for="header in props.headers"
            :key="header.text"
            :class="['column sortable', pagination.descending ? 'desc' : 'asc', header.value === pagination.sortBy ? 'active' : '']"
            @click="changeSort(header.value)"
          >
            <v-icon small>arrow_upward</v-icon>
            {{ header.text }}
          </th>
        </tr>
      </template>
      <template slot="items" slot-scope="props">
        <td>{{ props.item.OperateName }}</td>
        <td>{{ props.item.UserID }}</td>
        <td>{{ props.item.TargetName }}</td>
        <td>{{ props.item.OldData }}</td>
        <td>{{ props.item.NewData }}</td>
        <td>{{ props.item.ChangeType }}</td>
        <td>{{ props.item.Memo }}</td>
        <td>{{ props.item.CreateTime }}</td>
      </template>
    </v-data-table>
  </div>
</template>

<script>
import { mapActions } from 'vuex'
import moment from 'moment'

export default {
  name: 'AgentVerify',
  data() {
    return {
      menu1: false,
      menu2: false,
      readonly: true,
      classItems: [],
      inputForm: {
        user_id: null,
        loginName: null,
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
        { text: '$vuetify.dataIterator.rowsPerPageAll', value: 999999 }
      ],
      pagination: {'sortBy': 'CreateTime', 'descending': true},
      headers: [
        { text: '操作者', value: 'OperateName' },
        { text: '玩家ID', value: 'UserID' },
        { text: '用户名', value: 'TargetName' },
        { text: '旧数据', value: 'OldData' },
        { text: '新数据', value: 'NewData' },
        { text: '类型', value: 'ChangeType' },
        { text: '备注', value: 'Memo' },
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

  methods: {
    submit () {
      this.pagination.page = 1
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
        _.set(this.inputForm, 'sys_Order', sortBy)
        _.set(this.inputForm, 'sortDirection', descending ? 'd' : 'a')
        _.set(this.inputForm, 'PageIndex', page)
        _.set(this.inputForm, 'PageSize', rowsPerPage)

        this.getChangeLog(this.inputForm).then(res => {
          const data = _.get(res, 'data')
          const items = _.cloneDeep(data)
          const total = _.get(res, 'pageCount')
          
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
    changeSort (column) {
      if (this.pagination.sortBy === column) {
        this.pagination.descending = !this.pagination.descending
      } else {
        this.pagination.sortBy = column
        this.pagination.descending = false
      }
    },
    ...mapActions(['getChangeLog'])
  }
}
</script>
<style lang="scss" scoped>
</style>
