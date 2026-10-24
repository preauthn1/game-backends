<template>
  <div>
    <v-toolbar flat>
      <v-toolbar-title>活码管理</v-toolbar-title>
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
              v-model="inputForm.Domain"
              label="域名"
            ></v-text-field>
          </v-flex>
           <v-btn @click="submit">开始搜索</v-btn>
            <v-btn @click="clear">重置</v-btn>
        </v-layout>
      </v-container>
    </v-form>

    <v-toolbar flat>
      <v-spacer></v-spacer>
      <v-dialog v-model="dialog" max-width="500px">
        <v-card>
          <v-card-text>
            <v-form ref="form2">
              <v-container grid-list-md>
                <v-layout wrap>
                  <v-flex >
                    <v-text-field 
                    v-model="editedItem.Domain" 
                    :rules="url_rules"
                    label="域名"></v-text-field>
                  </v-flex>
                </v-layout>
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
    </v-toolbar>
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
        <td>{{ props.item.ID }}</td>
        <td>{{ props.item.Domain }}</td>
        <td >
          <v-icon
            v-if="props.item.Flag === 0"
            small
            class="mr-2"
            @click="editItem(props.item)"
          >
            edit
          </v-icon>
          <v-btn v-else
            color="brown darken-1"
            class="white--text"
          >
            更新中
            <v-icon right dark>update</v-icon>
          </v-btn>
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
      dialog: false,
      inputForm: {
        Domain: null
      },
      url_rules: [
        v => {
          if (v === null || v === void 0 || v.length === 0) return true
          const urlPattern = /(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&:/~\+#]*[\w\-\@?^=%&/~\+#])?/;
          return urlPattern.test(v)|| '请输入正确活码位置'
        }
      ],
      defaultItem: {
        ID: null,
        Domain: null
      },
      editedItem: {
        ID: null,
        Domain: null
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
          text: 'ID',
          align: 'left',
          value: 'ID'
        },
        { text: '域名', value: 'Domain' },
        { text: '操作', value: 'name', sortable: false }
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
    },
    dialog (val) {
      val || this.close()
    }
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
    
    async editItem (item) {
      this.editedItem = Object.assign({}, item)
      this.dialog = true
    },

    async save () {
      if (this.$refs.form2.validate()) {
        const res = await this.updateDomainConfig(this.editedItem)
         this.getDataFromApi().then(data => {
          this.desserts = data.items
          this.totalDesserts = data.total
        })
        this.$set(this, 'dialog', false)
      }
    },

    close () {
      this.editedItem = Object.assign({}, this.defaultItem)
      this.$set(this, 'dialog', false)
    },

    async getDataFromApi () {
      this.loading = true
      return new Promise((resolve, reject) => {
        const { sortBy, descending, page, rowsPerPage } = this.pagination
        this.getDomainConfig(this.inputForm).then(data => {
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
    ...mapActions(['getDomainConfig', 'updateDomainConfig'])
  }
}
</script>
<style lang="scss" scoped>
</style>
