import { xhr } from './xhr/'
import { xhrErrors } from './xhr/config'

class REPORT {
  async GetReportDay (data) {
    try {
      const res = await xhr({ url: '/Report/GetReportDay', data: data })

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }

      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentDetails (data) {
    try {
      const res = await xhr({ url: '/Report/GetAgentDetails', data: data })

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }

      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }
}

export default new REPORT()