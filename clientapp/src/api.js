const base = '/api/v1'
async function req(path, opts = {}) {
  const res = await fetch(base + path, {
    headers: { 'Content-Type': 'application/json' }, credentials: 'same-origin',
    ...opts, body: opts.body ? JSON.stringify(opts.body) : undefined
  })
  const text = await res.text(); const data = text ? JSON.parse(text) : null
  if (!res.ok) throw new Error(data?.error || `Lỗi ${res.status}`)
  return { data, cache: res.headers.get('X-Cache') }
}
export const api = {
  dashboard: () => req('/dashboard'),
  products: () => req('/products'),
  createProduct: (b) => req('/products', { method: 'POST', body: b }),
  importPim: () => req('/products/import-pim', { method: 'POST' }),
  batches: () => req('/batches'),
  batch: (id) => req(`/batches/${id}`),
  generate: (b) => req('/batches', { method: 'POST', body: b }),
  stamps: (q, batchId) => req(`/stamps?${q ? `q=${encodeURIComponent(q)}&` : ''}${batchId ? `batchId=${batchId}` : ''}`),
  verify: (code) => req(`/verify/${encodeURIComponent(code)}`),
  activate: (code, phone) => req(`/verify/${encodeURIComponent(code)}/activate`, { method: 'POST', body: { phone } }),
  spin: (code) => req(`/verify/${encodeURIComponent(code)}/spin`, { method: 'POST' })
}
export const fmtDate = (s) => s ? new Date(s).toLocaleDateString('vi-VN') : '—'
export const fmtDateTime = (s) => s ? new Date(s).toLocaleString('vi-VN') : '—'
