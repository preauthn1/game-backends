import { xhr } from './xhr/'
import { xhrErrors } from './xhr/config'
const md5 = require('md5')

class AGENT {
  async GetAgentWithdraw (data) {
    try {
      const res = await xhr({ url: '/Agent/GetAgentWithdraw', data: data  })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async AgentWithdraw (data) {
    try {
      const res = await xhr({ url: '/Agent/AgentWithdraw', data: data  })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async UpdateWithdrawApply (data) {
    try {
      const res = await xhr({ url: '/Agent/UpdateWithdrawApply', data: data  })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentApply (data) {
    try {
      const res = await xhr({ url: '/Agent/GetAgentApply', data: data })

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }
      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentUser (data) {
    try {
      const res = await xhr({ url: '/Agent/GetAgentUser', data: data })

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }
      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async UpdateAgentUser (data) {
    try {
      const res = await xhr({ url: '/Agent/UpdateAgentUser', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async UpdateInformation (data) {
    try {
      const res = await xhr({ url: '/Agent/UpdateInformation', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentCustomer (data) {
    try {
      const res = await xhr({ url: '/Agent/GetAgentCustomer', data: data })

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }
      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetRateRange () {
    try {
      const res = await xhr({ url: '/Agent/GetRateRange' })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetGrandConfig (data) {
    try {
      const res = await xhr({ url: '/Agent/GetGrandConfig', data: data })

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }
      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentMaxMinRate (data) {
    try {
      const res = await xhr({ url: '/Agent/GetAgentMaxMinRate', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async UpdateGrandConfig (data) {
    try {
      const res = await xhr({ url: '/Agent/UpdateGrandConfig', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async DeleteGrandConfig (data) {
    try {
      const res = await xhr({ url: '/Agent/DeleteGrandConfig', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async AgentPassed (data) {
    try {
      const res = await xhr({ url: '/Agent/AgentPassed', data: data })

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }
      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async UpdatePassword (data) {
    try {
      _.set(data, 'oldPassword', md5(data.oldPassword))
      const res = await xhr({ url: '/Agent/UpdatePassword', data: data })
      
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async CreateAgent (data) {
    try {
      const res = await xhr({ url: '/Agent/CreateAgent', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async CustomerUpgrade (data) {
    try {
      const res = await xhr({ url: '/Agent/CustomerUpgrade', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentRecursiveDown (data) {
    try {
      const res = await xhr({ url: '/Agent/GetAgentRecursiveDown', method: 'GET',  params: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentApplyCount () {
    try {
      const res = await xhr({ url: '/Agent/GetAgentApplyCount', method: 'GET' })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetGrandConfigItems (data) {
    try {
      const res = await xhr({ url: '/Agent/GetGrandConfigItems', method: 'GET', params: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }
}

export default new AGENT()