export function useApi() {
  const config = useRuntimeConfig()
  const baseUrl = config.public.apiUrl as string

  async function get<T>(path: string, params?: Record<string, any>): Promise<T> {
    const query = params ? '?' + new URLSearchParams(
      Object.entries(params).filter(([, v]) => v != null && v !== '').map(([k, v]) => [k, String(v)])
    ).toString() : ''
    return await $fetch<T>(`${baseUrl}${path}${query}`)
  }

  async function post<T>(path: string, body?: any): Promise<T> {
    return await $fetch<T>(`${baseUrl}${path}`, { method: 'POST', body })
  }

  return { get, post, baseUrl }
}
