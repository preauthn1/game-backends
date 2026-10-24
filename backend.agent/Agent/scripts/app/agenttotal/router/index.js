import Vue from 'vue'
import Router from 'vue-router'
import store from '@/agenttotal/store'
import auth from './helper/auth'
import Helpers from '@/agenttotal/helpers'

import login from '▶/login'
import home from '▶/home'
import transferlog from '▶/transferlog'
import management from '▶/management'
import rechargelog from '▶/rechargelog'
import rechargelog_agent from '▶/rechargelog-agent'
import backlog from '▶/backlog'
import report_day from '▶/reports/day'
import report_agent_details from '▶/reports/agent-details'
import loginlog from '▶/loginlog'

import agent_list from '▶/agent/list'
import agent_loginlog from '▶/agent/loginlog'

import errorOrderOpt from '▶/errorOrderOpt'



Vue.use(Router)

export const router = new Router({
  routes: [
    {
      path: '/',
      name: 'login',
      component: login
    },{
      path: '/home',
      name: 'home',
      component: home,
      children: [
        { path: '/transferlog', component: transferlog },
        { path: '/management', component: management },
        { path: '/rechargelog', component: rechargelog },
        { path: '/rechargelog-agent', component: rechargelog_agent },
        { path: '/loginlog', component: loginlog },
       
        { path: '/backlog', component: backlog },
        { path: '/report-day', component: report_day },
        { path: '/report-agent-details', component: report_agent_details },

        { path: '/agent/list', component: agent_list },
        { path: '/agent/loginlog', component: agent_loginlog },
        { path: '/errorOrderOpt', component: errorOrderOpt }
      ]
    },
    {
      path: '*',
      redirect: '/'
    }
  ]
})

router.beforeEach(async (to, from, next) => {
  try {
    const isRouteNameIsOneOfAuth = auth.isRouteNameIsOneOfAuth({ name: to.name })
    if (isRouteNameIsOneOfAuth) return next()

    await store.dispatch('validateToken', Helpers.other.token)

    return next()
  } catch (err) {
  } finally {
  }
})

export default router
