import { xhr } from './xhr/'
import { xhrErrors } from './xhr/config'

class REPORT {

  async GetChangeLog (data) {
    try {
      const res = await xhr({ url: '/Report/GetChangeLog', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetLogonLog (data) {
    try {
      const res = await xhr({ url: '/Report/GetLogonLog', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentProfitLog (data) {
    try {
      const res = await xhr({ url: '/Report/GetAgentProfitLog', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentProfitMonthlyLog (data) {
    try {
      const res = await xhr({ url: '/Report/GetAgentProfitMonthlyLog', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentWithdrawLog (data) {
    try {
      const res = await xhr({ url: '/Report/GetAgentWithdrawLog', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }
}

export default new REPORT()