<template>
	<div class="drag_verify" ref="contain" :style="dragVerifyStyle" @mousemove="dragMoving" @mouseup="dragFinish" @touchmove="dragMoving" @touchend="dragFinish">
			<div class="dv_progress_bar" ref="progressBar" :style="progressBarStyle">
			</div>
			<div class="dv_text" :style="textStyle" ref="message">{{message}}</div>
			<div class="dv_handler" @mousedown="dragStart"  ref="handler" :style="handlerStyle" @touchstart="dragStart">
			  <v-icon large>{{iconName}}</v-icon>
			</div>
		</div>
</template>
<script>
export default {
  name: 'dragVerify',

  data() {
    return {
      width: 0,
      height: 38,
      isMoving: false,
      x: 0,
      isPassing: false,
      iconName: 'chevron_right',
      text: '向右滑动完成验证',
      successText: '验证成功'
    }
  },

  props: {
    background: {
      type: String,
      default: '#ccc'
    },
    progressBarBg: {
      type: String,
      default: '#FFFF99'
    },
    completedBg: {
      type: String,
      default: '#66cc66'
    },
    handlerBg: {
      type: String,
      default: '#fff'
    },
    isError: {
      type: Boolean,
      default: false
    },
    isPass: {
      type: Boolean,
      default: false
    }
  },

  computed: {
    handlerStyle: function() {
      return {
        left: '0px',
        width: this.height - 2 + 'px',
        height: this.height - 2 + 'px',
        background: this.handlerBg
      }
    },
    message: function() {
      return this.isPassing ? this.successText : this.text
    },
    dragVerifyStyle: function() {
      return {
        width: '100%',
        height: this.height + 'px',
        lineHeight: this.height + 'px',
        background: this.background,
      }
    },
    progressBarStyle: function() {
      return {
        background: this.progressBarBg,
        width: this.height - 2 + 'px',
        height: this.height - 2 + 'px'
      }
    },
    textStyle: function() {
      return {
        height: this.height + 'px',
        width: this.width + 'px',
        fontSize: '1em'
      }
    }
  },

  mounted () {
    this.init()
  },

  beforeUpdate() {
    this.init()
  },

  watch: {
    isPass (val, oldVal) {
      if (!val) this.reset()
    },
    isError (val, oldVal) {
      if (val) this.$refs.message.style.color = 'red'
    }
  },

  methods: {
    init () {
      const offsetWidth = this.$refs.contain.offsetWidth
      this.$set(this, 'width', offsetWidth)
    },
    dragStart: function(e) {
      if (!this.isPassing) {
        this.isMoving = true
        let handler = this.$refs.handler
        this.x =
          (e.pageX || e.touches[0].pageX) -
          parseInt(handler.style.left.replace('px', ''), 10)
      }
    },
    dragMoving: function(e) {
      if (this.isMoving && !this.isPassing) {
        let _x = (e.pageX || e.touches[0].pageX) - this.x
        let handler = this.$refs.handler
        if (_x > 0 && _x <= this.width - this.height) {
          handler.style.left = _x + 'px'
          this.$refs.progressBar.style.width = _x + this.height / 2 + 'px'
        } else if (_x > this.width - this.height) {

          handler.style.left = this.width - this.height + 'px'
          this.$refs.progressBar.style.width =
            this.width - this.height / 2 + 'px'
          this.passVerify()
        }
      }
    },
    dragFinish (e) {
      if (this.isMoving && !this.isPassing) {
        let _x = (e.pageX || e.changedTouches[0].pageX) - this.x
        if (_x < this.width - this.height) {
          this.$refs.handler.style.left = '0'
          this.$refs.progressBar.style.width = '0'
        }
        this.isMoving = false
      }
    },
    reset () {
      this.$refs.handler.style.left = '0'
      this.$refs.progressBar.style.width = '0'
      this.$refs.message.style.color = '#000'
      this.$refs.progressBar.style.background = this.handlerBg
      this.$set(this, 'iconName', 'chevron_right')
      this.isPassing = false
      this.isMoving = false
    },
    passVerify () {
      this.isPassing = true
      this.isMoving = false
      let handler = this.$refs.handler
      this.$set(this, 'iconName', 'check')
      this.$refs.progressBar.style.background = this.completedBg
      this.$refs.message.style.color = '#fff'
      this.$emit('update:isPass', true)
    }
  }
}
</script>
<style>
.drag_verify {
  position: relative;
  margin-top: 5%;
  background-color: #e8e8e8;
  text-align: center;
  border-width:1px;
  border-style:solid;
  border-color: rgba(0,0,0,.42);
}
.drag_verify .dv_handler {
  position: absolute;
  top: 0px;
  left: 0px;
  border: 1px solid #ccc;
  cursor: move;
}

.drag_verify .dv_handler i {
  color: #666;
  font-size: 1.5em;
}
.drag_verify .dv_progress_bar {
  position: absolute;
  height: 34px;
  width: 0px;
  transition: background 1s ease-in;
}
.drag_verify .dv_text {
  position: absolute;
  top: 0px;
  color: #444;
  user-select: none;
}
</style>