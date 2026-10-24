<template>
  <div>
    {{IsGeneralAgent}}
    {{IsAdmin}}
    <v-toolbar flat>
      <v-toolbar-title>我的信息</v-toolbar-title>
      <v-spacer></v-spacer>
      <span class="hidden-sm-and-down total_text">代理数: {{AgentTotal}}</span>
      <v-spacer></v-spacer>
      <span class="hidden-sm-and-down total_text">今日代理注册数: {{AgentToday}}</span>
      <v-spacer></v-spacer>
      <span class="hidden-sm-and-down total_text">玩家数: {{CustomerTotal}}</span>
      <v-spacer></v-spacer>
      <span class="hidden-sm-and-down total_text">今日玩家注册数: {{CustomerToday}}</span>
      <v-divider class="mx-2" inset vertical></v-divider>
      <v-spacer></v-spacer>
    </v-toolbar>
    <v-form ref="form">
      <v-container>
        <v-layout align-center justify-center row fill-height wrap>
          <v-flex v-if="IsGeneralAgent">
            <v-text-field v-model="inputForm.UserID" label="玩家ID" append-outer-icon="create"></v-text-field>
          </v-flex>
          <v-flex v-else>
            <v-text-field v-model="inputForm.UserID" label="玩家ID" readonly></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field v-model="inputForm.LoginName" label="帐号名称" readonly></v-text-field>
          </v-flex>
        </v-layout>
        <v-layout align-center justify-center row fill-height wrap>
          <v-flex>
            <v-text-field
              append-outer-icon="create"
              v-model="inputForm.Tel"
              label="电话"
              :rules="tel_rules"
            ></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field
              append-outer-icon="create"
              v-model="inputForm.Weixin"
              label="微信"
              :rules="weixin_rules"
            ></v-text-field>
          </v-flex>
        </v-layout>
        <v-layout align-center justify-center row fill-height wrap>
          <v-flex>
            <v-text-field
              append-outer-icon="create"
              v-model="inputForm.QQ"
              label="QQ"
              :rules="qq_rules"
            ></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field
              append-outer-icon="create"
              v-model="inputForm.Email"
              label="信箱"
              :rules="email_rules"
            ></v-text-field>
          </v-flex>
        </v-layout>

        <v-layout align-center justify-center row fill-height wrap>
          <v-flex>
            <v-text-field v-model="inputForm.ClassName" label="代理等级" readonly></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field v-model="inputForm.InviteCode" label="邀请码" readonly></v-text-field>
          </v-flex>
        </v-layout>

        <v-layout align-center justify-center row fill-height wrap>
          <v-flex>
            <v-text-field v-model="inputForm.PlatformName" label="平台" readonly></v-text-field>
          </v-flex>
        </v-layout>

        <v-layout
          align-center
          justify-center
          row
          fill-height
          wrap
          v-if="inputForm.Class === 1 || inputForm.Class === 2"
        >
          <v-flex>
            <v-text-field v-model="inputForm.GradeName" label="阶级" readonly></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field v-model="inputForm.DistributeRate" label="分成" readonly></v-text-field>
          </v-flex>
        </v-layout>

        <v-layout v-if="IsGeneralAgent" align-center justify-center row fill-height wrap>
          <v-flex>
            <v-select
              v-model="inputForm.User1Type"
              :items="items"
              item-text="text"
              item-value="value"
              label="客服平台"
            ></v-select>
          </v-flex>
          <v-flex>
            <v-text-field
              append-outer-icon="create"
              v-model="inputForm.OfficialUser1"
              label="代理客服"
              :rules="rules"
            ></v-text-field>
          </v-flex>
        </v-layout>

        <v-layout v-if="IsGeneralAgent" align-center justify-center row fill-height wrap>
          <v-flex>
            <v-select
              v-model="inputForm.User2Type"
              :items="items"
              item-text="text"
              item-value="value"
              label="客服平台"
            ></v-select>
          </v-flex>
          <v-flex>
            <v-text-field
              append-outer-icon="create"
              v-model="inputForm.OfficialUser2"
              label="代理客服"
              :rules="rules"
            ></v-text-field>
          </v-flex>
        </v-layout>
        <v-layout v-if="IsGeneralAgent" align-center justify-center row fill-height wrap>
          <v-flex>
            <v-select
              v-model="inputForm.User3Type"
              :items="items"
              item-text="text"
              item-value="value"
              label="客服平台"
            ></v-select>
          </v-flex>
          <v-flex>
            <v-text-field
              append-outer-icon="create"
              v-model="inputForm.OfficialUser3"
              label="代理客服"
              :rules="rules"
            ></v-text-field>
          </v-flex>
        </v-layout>

        <v-layout align-center justify-end fill-height wrap>
          <v-btn color="blue-grey darken-3" depressed @click="save">
            <v-icon left>update</v-icon>更新基本资料
          </v-btn>
        </v-layout>

        <v-layout align-center justify-space-around fill-height wrap>
          <div class="progress_content">
            <div class="progress_title">总税收</div>
            <v-progress-circular
              :rotate="-90"
              :size="180"
              :width="25"
              :value="ratio1"
              color="blue darken-2"
            >{{ totalFlows }}</v-progress-circular>
          </div>
          <div class="progress_content">
            <div class="progress_title">总收益</div>
            <v-progress-circular
              :rotate="-90"
              :size="180"
              :width="25"
              :value="ratio2"
              color="teal"
            >{{ totalIncome }}</v-progress-circular>
          </div>
          <div class="progress_content">
            <div class="progress_title">总剩余金额</div>
            <v-progress-circular
              :rotate="-90"
              :size="180"
              :width="25"
              :value="ratio3"
              color="yellow lighten-1"
            >{{ remainAmount }}</v-progress-circular>
          </div>
        </v-layout>
      </v-container>
    </v-form>
  </div>
</template>
<script>
import { mapActions, mapGetters } from "vuex";
export default {
  name: "information",
  data() {
    return {
      items: [{ text: "微信", value: 1 }, { text: "QQ", value: 2 }],
      AgentTotal: 0,
      AgentToday: 0,
      CustomerTotal: 0,
      CustomerToday: 0,
      interval: {},
      ratio1: 0,
      ratio2: 0,
      ratio3: 0,
      totalFlows: 0,
      totalIncome: 0,
      remainAmount: 0,
      rules: [v => !!v || "请输入"],
      tel_rules: [
        v => !!v || "请输入",
        v => {
          const mPattern = /^1[345789]\d{9}$/;
          return mPattern.test(v) || "请输入正确的手机号";
        }
      ],
      email_rules: [
        v => {
          if (v === null || v.length === 0) return true;
          const ePattern = /^([A-Za-z0-9_\-\.])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]{2,4})$/;
          return ePattern.test(v) || "请输入正确的信箱";
        }
      ],
      weixin_rules: [v => !!v || "请输入"],
      qq_rules: [
        v => {
          if (v === null || v.length === 0) return true;
          const qqPattern = /^[1-9][0-9]{4,10}$/;
          return qqPattern.test(v) || "请输入正确的QQ";
        }
      ],
      url_rules: [
        v => {
          if (v === null || v === void 0 || v.length === 0) return true;
          const urlPattern = /(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&:/~\+#]*[\w\-\@?^=%&/~\+#])?/;
          return urlPattern.test(v) || "请输入正确游戏下载位置";
        }
      ],
      inputForm: {
        UserID: null,
        LoginName: null,
        Tel: null,
        Weixin: null,
        QQ: null,
        Email: null,
        Class: null,
        ClassName: null,
        InviteCode: null,
        PlatformName: null,
        GradeName: null,
        DistributeRate: null,
        TotalFlows: null,
        TotalIncome: null,
        RemainAmount: null,
        OfficialUser1: null,
        OfficialUser2: null,
        OfficialUser3: null,
        User1Type: null,
        User2Type: null,
        User3Type: null
      }
    };
  },

  computed: {
    ...mapGetters(["IsAdmin", "IsGeneralAgent"])
  },

  async mounted() {
    const item = await this.getInformation();
    const registeredCount = item.RegisteredCount;
    this.$set(this, "AgentTotal", registeredCount.AgentTotal);
    this.$set(this, "AgentToday", registeredCount.AgentToday);
    this.$set(this, "CustomerTotal", registeredCount.CustomerTotal);
    this.$set(this, "CustomerToday", registeredCount.CustomerToday);
    this.inputForm = Object.assign({}, item);
    const { TotalFlows, TotalIncome, RemainAmount } = item;

    this.totalFlows = TotalFlows;
    this.totalIncome = TotalIncome;
    this.remainAmount = RemainAmount;
    this.ratio1 = 100;
    this.ratio2 = 100;
    this.ratio3 = 100;
  },

  methods: {
    async save() {
      if (this.$refs.form.validate()) {
        const res = await this.updateInformation(this.inputForm);
      }
    },

    copyInput() {
      document.getElementById("copyInput").select();
      document.execCommand("copy");
    },
    ...mapActions(["getInformation", "updateInformation"])
  }
};
</script>

<style lang="scss" scoped>
.progress_content {
  font-size: 18px;
  text-align: center;
}
.progress_title {
  padding: 8%;
}

.total_text {
  font-size: 1.5em;
}
</style>
