import service from '@/agentclub/service'
import Helpers from '@/agentclub/helpers'

const { systemService } = service
const md5 = require('md5')

const defaultState = {
  Theme: true,
  MenuItems: [
    {
      icon: 'keyboard_arrow_up',
      'icon-alt': 'keyboard_arrow_down',
      text: '系统管理',
      model: false,
      children: [
        {
          icon: 'important_devices',
          text: '系统设定',
          name: 'SystemSetting',
          url: 'system'
        }
      ]
    },
    {
      icon: 'keyboard_arrow_up',
      'icon-alt': 'keyboard_arrow_down',
      text: '代理管理',
      model: true,
      children: [
        {
          icon: 'assignment',
          text: '我的代理',
          name: 'AgentList',
          url: 'agent/list'
        },
        {
          icon: 'assignment_ind',
          text: '我的玩家',
          name: 'CustomerList',
          url: 'agent/customer'
        },
        {
          icon: 'trending_up',
          text: '阶级设定',
          name: 'LevelSetting',
          url: 'agent/level'
        }
      ]
    },
    {
      icon: 'keyboard_arrow_up',
      'icon-alt': 'keyboard_arrow_down',
      text: '紀錄查询',
      model: false,
      children: [
        {
          icon: 'list_alt',
          text: '按日税收纪录',
          name: 'Report',
          url: 'report/profitlog_day'
        },
        {
          icon: 'list_alt',
          text: '按月税收纪录',
          name: 'Report',
          url: 'report/profitLog_monthly'
        },
        {
          icon: 'list_alt',
          text: '提取纪录',
          name: 'Report',
          url: 'report/withdrawLog'
        },
        {
          icon: 'list_alt',
          text: '登入纪录',
          name: 'Report',
          url: 'report/logonLog'
        },
        {
          icon: 'list_alt',
          text: '日志纪录',
          name: 'Report',
          url: 'report/changeLog'
        }
      ]
    }
  ],
  GeneralItems: [
    {
      img: 'account_box',
      title: '我的信息',
      url: 'general/information',
      name: 'Information'
    },
    {
      img: 'share',
      title: '我的推广码(链接)',
      url: 'general/shareQRCode',
      name: 'ShareQRCode'
    },
    {
      img: 'vpn_key',
      title: '修改密码',
      url: 'general/password',
      name: 'Information'
    },
    {
      img: 'fiber_new',
      title: '添加代理',
      url: 'agent/create',
      name: 'CreateAgent'
    },
    {
      img: 'local_atm',
      title: '提取收益',
      url: 'general/withdraw',
      name: 'WithdrawApply'
    },
    {
      img: 'timelapse',
      title: '税收比例调整',
      url: 'general/rateRange',
      name: 'RateRange'
    }
  ],
  PermissionSetting: [
    {
      userType: 100, //系統管理者
      Permission: [
        {
          name: 'LiveCodeSetting',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'SystemSetting',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'AgentList',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'CustomerList',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'LevelSetting',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'Information',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'CreateAgent',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'Report',
          action: ['list', 'add', 'edit', 'delete']
        }
      ]
    },
    {
      userType: 0,
      Permission: [
        {
          name: 'AgentList',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'CustomerList',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'LevelSetting',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'Information',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'ShareQRCode',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'RateRange',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'Report',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'WithdrawApply',
          action: ['list', 'add', 'edit', 'delete']
        }
      ]
    },
    {
      userType: 1,
      Permission: [
        {
          name: 'AgentList',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'CustomerList',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'LevelSetting',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'Information',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'ShareQRCode',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'Report',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'WithdrawApply',
          action: ['list', 'add', 'edit', 'delete']
        }
      ]
    },
    {
      userType: 2,
      Permission: [
        {
          name: 'AgentList',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'CustomerList',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'LevelSetting',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'Information',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'ShareQRCode',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'Report',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'WithdrawApply',
          action: ['list', 'add', 'edit', 'delete']
        }
      ]
    },
    {
      userType: 3,
      Permission: [
        {
          name: 'AgentList',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'CustomerList',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'LevelSetting',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'Information',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'ShareQRCode',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'Report',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'WithdrawApply',
          action: ['list', 'add', 'edit', 'delete']
        }
      ]
    },
    {
      userType: 4,
      Permission: [
        {
          name: 'CustomerList',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'Information',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'ShareQRCode',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'Report',
          action: ['list', 'add', 'edit', 'delete']
        },
        {
          name: 'WithdrawApply',
          action: ['list', 'add', 'edit', 'delete']
        }
      ]
    }
  ],
  AgentClass: [
    { text: '系统管理者', value: 100 },
    { text: '渠道代理', value: 0 },
    { text: '一级代理', value: 1 },
    { text: '二级代理', value: 2 },
    { text: '三级代理', value: 3 },
    { text: '四级代理', value: 4 },
    { text: '一般玩家', value: 99 }
  ]
}

const state = () => {
  return _.cloneDeep(defaultState)
}

const mutations = {
  RESET(s) {
    const initial = state()
    Object.keys(initial).forEach(key => {
      s[key] = initial[key]
    })
  },
  THEME(state, payload) {
    state.Theme = payload
  }
}
const actions = {
  async changeTheme({ commit }, light) {
    Helpers.other.theme = !light
    commit('THEME', !light)
  },

  async getMenuItems({ state, getters }) {
    let menuItems = _.cloneDeep(state.MenuItems)
    let permissionSetting = _.cloneDeep(state.PermissionSetting)
    const user_permission = getters.UserPermission
    const setting = _.filter(permissionSetting, function(o) {
      const userType = md5(o.userType)
      return userType == user_permission
    })

    if (setting.length === 0) return []

    const permission = setting[0].Permission
    const menu = menuItems
      .map(function(item) {
        const children = item.children.filter(function(data) {
          const filter = _.filter(permission, function(o) {
            return o.name === data.name
          })
          return filter.length > 0
        })
        item.children = children
        return item.children.length > 0 ? item : {}
      })
      .filter(x => x.children !== void 0)

    return Promise.resolve(menu)
  },

  async getGeneralItems({ state, getters }) {
    try {
      let generalItems = _.cloneDeep(state.GeneralItems)
      let permissionSetting = _.cloneDeep(state.PermissionSetting)
      const user_permission = getters.UserPermission
      const setting = _.filter(permissionSetting, function(o) {
        const userType = md5(o.userType)
        return userType == user_permission
      })
      if (setting.length === 0) return []
      const permission = setting[0].Permission
      const menu = generalItems.filter(function(item) {

        if( item.name === 'WithdrawApply') {
          const userAllowWithdraw = getters.AllowWithdraw
          console.log(userAllowWithdraw)
          if(userAllowWithdraw == 0){
            item.title = '提取收益(禁止)'
            item.url = ''
          }
        }
        const filter = _.filter(permission, function(o) {
          return o.name === item.name
        })
        return filter.length !== 0
      })
      return Promise.resolve(menu)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAgentClassItems({ state, getters }) {
    let agentClass = _.cloneDeep(state.AgentClass)
    const user_permission = getters.UserPermission
    let indexClass = null
    agentClass.forEach(function(item, index) {
      const value = md5(item.value)
      if (value == user_permission) indexClass = index
    })
    if (indexClass != null) {
      return agentClass.splice(indexClass + 1, agentClass.length)
    }
    return []
  },

  async getPlatformItems({ getters, commit, dispatch }) {
    try {
      const res = await systemService.GetPlatform()
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAreaItems({ getters, commit, dispatch }, id) {
    try {
      const res = await systemService.GetArea({ platformID: id })
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getSystemConfig({ getters, commit, dispatch }) {
    try {
      const res = await systemService.GetSystemConfig()
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getDomainConfig({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)

      const res = await systemService.GetDomainConfig(deep)
      const data = _.get(res, 'result')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async updateSystemConfig({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      const res = await systemService.UpdateSystemConfig(deep)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async updateDomainConfig({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      const res = await systemService.UpdateDomainConfig(deep)
      const data = _.get(res, 'result')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  }
}

const getters = {
  PermissionSetting: ({ PermissionSetting }) => {
    return PermissionSetting
  },
  Theme: ({ Theme }) => {
    const theme = Helpers.other.theme
    if (theme === null) return Theme
    return theme === 'true'
  }
}

export default { state, mutations, actions, getters }
