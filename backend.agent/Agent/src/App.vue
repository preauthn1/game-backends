<template>
  <div id="app">
    <img src="./assets/logo.png">
    <h1>{{ msg }}</h1>
    <h2>Essential Links</h2>
    <ul>
      <li><a href="https://example.invalid" target="_blank">Core Docs</a></li>
      <li><a href="https://example.invalid" target="_blank">Forum</a></li>
      <li><a href="https://example.invalid" target="_blank">Community Chat</a></li>
      <li><a href="https://example.invalid/vuejs" target="_blank">Twitter</a></li>
    </ul>
    <h2>Ecosystem</h2>
    <ul>
      <li><a href="http://example.invalid/" target="_blank">vue-router</a></li>
      <li><a href="http://example.invalid/" target="_blank">vuex</a></li>
      <li><a href="http://example.invalid/" target="_blank">vue-loader</a></li>
      <li><a href="https://example.invalid/vuejs/awesome-vue" target="_blank">awesome-vue</a></li>
    </ul>
  </div>
</template>

<script>
export default {
  name: 'app',
  data () {
    return {
      msg: 'Welcome to Your Vue.js App'
    }
  }
}
</script>

<style>
#app {
  font-family: 'Avenir', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}

h1, h2 {
  font-weight: normal;
}

ul {
  list-style-type: none;
  padding: 0;
}

li {
  display: inline-block;
  margin: 0 10px;
}

a {
  color: #42b983;
}
</style>
