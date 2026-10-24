<template>
  <div>
    <v-toolbar flat>
      <v-toolbar-title>邀请链接</v-toolbar-title>
      <v-divider
          class="mx-2"
          inset
          vertical
      ></v-divider>
      <v-spacer></v-spacer>
    </v-toolbar>
    <v-layout align-center justify-center column class="shareQRCode_contain" >
      <img :src="ShareQRCodeURL" class="shareQRCode_btn">
      <v-btn round large @click="downloadQRCode" class="shareQRCode_btn">
        下载二维码
        <v-icon right large>cloud_download</v-icon>
      </v-btn>
      <v-btn round large @click="copy(DownloadURL)" class="shareQRCode_btn">
        复制链接
        <v-icon right large>file_copy</v-icon>
      </v-btn>
    </v-layout>
  </div>
</template>
<script>
  import { mapActions } from 'vuex'
  export default {
    name: 'shareQRCode',
    data() {
      return {
        InviteCode: null,
        ShareQRCodeURL: null,
        DownloadURL: null
      }
    },
    async mounted () {
      const res = await this.getInformation()

      this.$set(this, 'InviteCode', res.InviteCode)
      this.$set(this, 'ShareQRCodeURL', `/agentclub/admin/GetLiveQRCode?url=${res.QRUrl}`)
      this.$set(this, 'DownloadURL', res.QRUrl)
    },

    methods: {
    copy(s) {
      let clip_area = document.createElement('textarea');
      clip_area.textContent = s;
      document.body.appendChild(clip_area);
      clip_area.select();
      document.execCommand('copy');
      clip_area.remove();
    },
    downloadQRCode () {
      let link = document.createElement('a');
      link.href = this.ShareQRCodeURL;
      link.download = `${this.InviteCode}.jpg`;
      document.body.appendChild(link);
      link.click();
    },
      ...mapActions(['getInformation'])
    }
  }
</script>

<style lang="scss" scoped>
.shareQRCode_contain {
  padding: 8%;
}

.shareQRCode_btn {
  width: 18em;
  height: auto;
}
</style>
