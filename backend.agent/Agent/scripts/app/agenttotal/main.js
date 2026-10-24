import Vue from 'vue'
import App from './App.vue'
import Vuetify from 'vuetify'
import router from './router'
import store from './store'
import 'vuetify/dist/vuetify.min.css'
import moment from 'moment'
Vue.prototype.moment = moment

Vue.use(Vuetify)
new Vue({
    el: '#app',
    router,
    store,
    render: h => h(App)
})
