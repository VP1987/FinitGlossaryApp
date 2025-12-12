import axios from 'axios'

const API_BASE_URL = 'https://localhost:7098/api'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  (res) => res,
  async (err) => {
    const original = err.config

    if (err.response?.status === 401 && !original._retry) {
      original._retry = true

      const refreshToken = localStorage.getItem('refreshToken')
      if (!refreshToken) return Promise.reject(err)

      try {
        const refreshRes = await axios.post(`${API_BASE_URL}/auth/refresh`, {
          refreshToken,
        })

        const newToken = refreshRes.data.token
        localStorage.setItem('token', newToken)

        original.headers.Authorization = `Bearer ${newToken}`
        return api(original)
      } catch (e) {
        localStorage.removeItem('token')
        localStorage.removeItem('refreshToken')
        window.location.href = '/login'
        return Promise.reject(e)
      }
    }

    return Promise.reject(err)
  },
)

export default api
