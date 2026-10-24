import { xhr } from './xhr/'

class SYSTEM {

  async GetSystemConfig () {
    try {
      const res = await xhr({ url: '/Admin/GetSystemConfig', method: 'GET' })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async UpdateSystemConfig (data) {
    try {
      const res = await xhr({ url: '/Admin/UpdateSystemConfig', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetDomainConfig (data) {
    try {
      const res = await xhr({ url: '/Admin/GetDomainConfig', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async UpdateDomainConfig (data) {
    try {
      const res = await xhr({ url: '/Admin/UpdateDomainConfig', data: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetPlatform () {
    try {
      const res = await xhr({ url: '/Admin/GetPlatform', method: 'GET' })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetArea (data) {
    try {
      const res = await xhr({ url: '/Admin/GetArea', method: 'GET', params: data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }
}

export default new SYSTEM()