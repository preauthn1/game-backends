<template>
  <div>
    <v-toolbar flat>
      <v-toolbar-title>税收比例调整</v-toolbar-title>
      <v-divider
          class="mx-2"
          inset
          vertical
      ></v-divider>
      <v-spacer></v-spacer>
    </v-toolbar>
    <v-form ref="form">
      <v-container>
        <v-layout justify-center column fill-height wrap >
          <v-flex>
            <v-card flat color="transparent">
              <v-slider
                v-model="inputForm.DistributeRate"
                label="渠道代理"
                step="1"
                thumb-label="always"
                ticks
                :max="rateMax"
              ></v-slider>
            </v-card>
          </v-flex>
          <v-btn @click="submit">确认修改</v-btn>
          <v-btn @click="clear">重置</v-btn>
        </v-layout>
      </v-container>
    </v-form>
  </div>
</template>
<script>
  import { mapActions } from 'vuex'
  export default {
    name: 'information',
    data() {
      return {
        rateMax: 100,
        inputForm: {
          ID: null,
          DistributeRate: 0,
          Class1RateRange: 0,
          Class2RateRange: 0,
          Class3RateRange: 0,
          Class4RateRange: 0
        },

        defaultForm: {
          ID: null,
          DistributeRate: 0,
          Class1RateRange: 0,
          Class2RateRange: 0,
          Class3RateRange: 0,
          Class4RateRange: 0
        }
      }
    },

    async mounted () {
      const res = await this.getRateRange()
      this.inputForm = Object.assign({}, res)
      this.defaultForm = Object.assign({}, res)
    },

    methods: {
      async submit () {
        if (this.$refs.form.validate()) {
          const res = await this.updateAgentUser(this.inputForm)
        }
      },
      clear () {
        this.inputForm = Object.assign({}, this.defaultForm)
      },
      ...mapActions(['getRateRange', 'updateAgentUser'])
    }
  }
</script>

<style lang="scss" scoped>
</style>
