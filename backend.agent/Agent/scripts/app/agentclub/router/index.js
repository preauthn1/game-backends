import Vue from 'vue'
import Router from 'vue-router'
import auth from './helper/auth'
import store from '@/agentclub/store'
import Helpers from '@/agentclub/helpers'
import login from '@/agentclub/pages/login'
import home from '@/agentclub/pages/home'

import system from '@/agentclub/pages/system'
import verify from '@/agentclub/pages/agent/verify'
import withdraw from '@/agentclub/pages/agent/withdraw'
import agent_list from '@/agentclub/pages/agent/list'
import customer from '@/agentclub/pages/agent/customer'
import level from '@/agentclub/pages/agent/level'

import profitLog_day from '@/agentclub/pages/report/profitLog_day'
import profitLog_monthly from '@/agentclub/pages/report/profitLog_monthly'
import changeLog from '@/agentclub/pages/report/changeLog'
import logonLog from '@/agentclub/pages/report/logonLog'
import withdrawLog from '@/agentclub/pages/report/withdrawLog'

import information from '@/agentclub/pages/general/information'
import shareQRCode from '@/agentclub/pages/general/shareQRCode'
import password from '@/agentclub/pages/general/password'
import create_agent from '@/agentclub/pages/general/create_agent'
import create_withdraw from '@/agentclub/pages/general/withdraw'
import rate_range from '@/agentclub/pages/general/rateRange'
import pageNotFound from '@/agentclub/pages/pageNotFound'

const md5 = require('md5')

Vue.use(Router)

export const router = new Router({
  routes: [
    {
      path: '/',
      name: 'login',
      component: login
    },
    {
      path: '/home',
      component: home,
      children: [
        { path: '', component: agent_list },
        { path: '/system', component: system },

        { path: '/agent/withdraw', component: withdraw },
        { path: '/agent/verify', component: verify },
        { path: '/agent/list', component: agent_list },
        { path: '/agent/customer', component: customer },
        { path: '/agent/level', component: level },
        { path: '/agent/create', component: create_agent },

        { path: '/report/profitlog_day', component: profitLog_day },
        { path: '/report/profitLog_monthly', component: profitLog_monthly },
        { path: '/report/changeLog', component: changeLog },
        { path: '/report/logonLog', component: logonLog },
        { path: '/report/withdrawLog', component: withdrawLog },

        { path: '/general/information', component: information },
        { path: '/general/shareQRCode', component: shareQRCode },
        { path: '/general/password', component: password },
        { path: '/general/withdraw', component: create_withdraw },
        { path: '/general/rateRange', component: rate_range }
      ]
    },
    { path: "*", component: pageNotFound }
  ]
})

router.beforeEach(async (to, from, next) => {
  try {
    const isRouteNameIsOneOfAuth = auth.isRouteNameIsOneOfAuth({
      name: to.name
    })
    if (isRouteNameIsOneOfAuth) return next()
    if(!Helpers.other.token) return next('/')

    await store.dispatch('validateToken', Helpers.other.token).then(res => {
      if (res == false) return next('/')
    })

    const userPermission = store.getters.UserPermission
    const userAllowWithdraw = store.getters.AllowWithdraw

    if (to.fullPath === '/general/withdraw' && userAllowWithdraw === 0) return next('/home')
    const permission = md5(4)
    
    if(userPermission === permission){
      if(to.fullPath === '/home') return next('/agent/customer')
    }
    
    await checkMapAuthPermission(to).then(res => {
      if (!res) return next(from)
    })
    return next()
  } catch (err) {
  } finally {
  }
})

async function checkMapAuthPermission(to) {
  try {
    const permissionName = to.name
    let isPermission = false
    if (_.isUndefined(permissionName) || _.isNull(permissionName)) return Promise.resolve(true)

    await store.dispatch('checkPermission', permissionName).then(res => {
      isPermission = res
    })
    return isPermission
  } catch (err) {
    return Promise.reject(`[checkMapAuthPermission] -> ${err}`)
  }
}
export default router
