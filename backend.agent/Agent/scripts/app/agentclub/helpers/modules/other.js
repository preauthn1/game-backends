
class OTHER {
  get token () {
    const token = [REDACTED]'token')
    return token
  }

  set token (result) {
    localStorage.setItem('token', result)
  }

  get theme () {
    const theme = localStorage.getItem('theme')
    return theme
  }

  set theme (result) {
    localStorage.setItem('theme', result)
  }
}
export default new OTHER()
