<template>
  <div>
    <v-toolbar flat>
      <v-toolbar-title>系统设定</v-toolbar-title>
      <v-divider class="mx-2" inset vertical></v-divider>
      <v-spacer></v-spacer>
    </v-toolbar>
    <v-form ref="form">
      <v-container>
        <v-layout align-center justify-center row fill-height wrap>
          <v-flex>
            <v-text-field v-model="inputForm.OfficialSite" label="全代理游戏下载位置" :rules="url_rules"></v-text-field>
          </v-flex>
        </v-layout>
        <v-btn color="blue-grey darken-3" depressed @click="save">
          <v-icon left>update</v-icon>更新系统设定
        </v-btn>
      </v-container>
    </v-form>
  </div>
</template>
<script>
import { mapActions } from "vuex";
export default {
  name: "information",
  data() {
    return {
      items: [{ text: "微信", value: 1 }, { text: "QQ", value: 2 }],
      inputForm: {
        Class1ChargeFee: null,
        Class2ChargeFee: null,
        Class3ChargeFee: null,
        Class4ChargeFee: null,
        OfficialUser1: null,
        OfficialUser2: null,
        OfficialUser3: null,
        User1Type: 1,
        User2Type: 1,
        User3Type: 1,
        OfficialSite: null
      },
      rules: [v => !!v || "请输入"],
      url_rules: [
        v => {
          if (v === null || v.length === 0) return true;
          const urlPattern = /(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&:/~\+#]*[\w\-\@?^=%&/~\+#])?/;
          return urlPattern.test(v) || "请输入正确游戏下载位置";
        }
      ]
    };
  },

  async mounted() {
    const item = await this.getSystemConfig();
    this.inputForm = Object.assign({}, item);
  },

  methods: {
    async save() {
      if (this.$refs.form.validate()) {
        const res = await this.updateSystemConfig(this.inputForm);
        if (res) {
          const item = await this.getSystemConfig();
          this.inputForm = Object.assign({}, item);
        }
      }
    },
    ...mapActions(["getSystemConfig", "updateSystemConfig"])
  }
};
</script>

<style lang="scss" scoped>
</style>
