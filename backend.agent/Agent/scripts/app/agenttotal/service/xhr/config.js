
import { projectApiUrl } from '@/agenttotal/config/api'
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
  'v-no-data': '無資料',
  'v-no-token': '沒有登入',
  'v-error-without-http-response': '不明なネットワークの問題',
  'v-timeout': '連接超時',
  'v-cancel': '操作がキャンセル',
  'v-login-error': '登入失敗',
  'v-wrong-auth-info': 'ログインIDまたはパスワードが間違っています',

  'http-403': 'もう一度ログインしてください',
  'http-404': '接続タイムアウト',
  'http-500': '接続タイムアウト'
}
