export function getUserIdFromToken(token) {
  if (!token) return null

  try {
    const payload = token.split('.')[1]
    const decoded = JSON.parse(atob(payload))
    return decoded.id || null
  } catch (e) {
    console.error('Invalid JWT token:', e)
    return null
  }
}
export function getUsernameFromToken(token) {
  if (!token) return null

  try {
    const payload = token.split('.')[1]
    const decoded = JSON.parse(atob(payload))
    return decoded.username ?? null
  } catch {
    return null
  }
}
