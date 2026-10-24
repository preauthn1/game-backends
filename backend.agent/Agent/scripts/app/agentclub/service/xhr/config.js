
import { projectApiUrl } from '@/agentclub/config/api'
export const XHR_TIMEOUT = 600000
export const API_URL = projectApiUrl
export const LOCAL_TOKEN_CHECK_DURATION = 3000

export const xhrErrors = key => {
  try {
    if (!_.has(XHR_ERRORS, key)) throw new Error('error')
    return XHR_ERRORS[key].toString()
  } catch (error) {
    return `不明問題 (${key})`
  }
}

export const XHR_ERRORS = {
  'v-success': '操作成功',
  'v-error': '操作失败',
  'v-no-data': '無資料',
  'v-no-token': '沒有登入',
  'v-error-without-http-response': '未知的网络问题',
  'v-timeout': '連接超時',
  'v-cancel': '操作取消',
  'v-login-error': '帐号密码错误',
  'v-old-password-error': '旧密码不一致，请重新输入',

  'http-403': '请再次登录',
  'http-404': '连接超时',
  'http-500': '连接超时'
}

export const ERRORS_CODE = {
  '10001': 'v-login-error',
  '10008': 'v-old-password-error',
  '10009': 'v-no-data',
  '10010': 'v-error',
  '20001': 'v-success'
}
