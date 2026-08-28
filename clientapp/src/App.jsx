import React, { useEffect, useState } from 'react'
import { Routes, Route, NavLink, Outlet } from 'react-router-dom'
import { api, fmtDate, fmtDateTime } from './api'

function Badge({ text, css }) { return <span className={`badge ${css || 'secondary'}`}>{text}</span> }
function Flash({ msg }) { return msg ? <div className={`flash ${msg.ok ? 'ok' : 'err'}`}>{msg.text}</div> : null }
function Modal({ title, onClose, wide, children }) {
  return (
    <div className="modal-bg" onClick={onClose}>
      <div className="modal" style={wide ? { maxWidth: 720 } : undefined} onClick={e => e.stopPropagation()}>
        <div className="row" style={{ marginBottom: 12 }}><h2 style={{ flex: 1, margin: 0 }}>{title}</h2>
          <button className="btn gray sm" style={{ flex: 'none' }} onClick={onClose}>Đóng</button></div>
        {children}
      </div>
    </div>
  )
}
function Field({ label, children }) { return <div style={{ flex: 1 }}><label>{label}</label>{children}</div> }

function Layout() {
  return (
    <>
      <nav className="nav">
        <span className="brand">🔖 MiniStamp</span>
        <NavLink to="/" end>Tổng quan</NavLink>
        <NavLink to="/batches">Lô tem</NavLink>
        <NavLink to="/stamps">Tem</NavLink>
        <NavLink to="/products">Sản phẩm</NavLink>
        <NavLink to="/verify">Tra cứu</NavLink>
      </nav>
      <div className="wrap"><Outlet /></div>
    </>
  )
}

function Dashboard() {
  const [d, setD] = useState(null); const [cache, setCache] = useState('')
  useEffect(() => { api.dashboard().then(r => { setD(r.data); setCache(r.cache) }) }, [])
  if (!d) return <p className="muted">Đang tải…</p>
  const max = Math.max(1, ...d.byProduct.map(g => g.count))
  return (
    <>
      <h1>Tổng quan tem {cache && <span className="pill">cache: {cache}</span>}</h1>
      <div className="grid kpis" style={{ marginBottom: 18 }}>
        <div className="kpi"><div className="v">{d.products}</div><div className="l">Sản phẩm</div></div>
        <div className="kpi"><div className="v">{d.batches}</div><div className="l">Lô tem</div></div>
        <div className="kpi"><div className="v">{d.stamps.toLocaleString('vi-VN')}</div><div className="l">Tem đã phát hành</div></div>
        <div className="kpi"><div className="v" style={{ color: 'var(--success)' }}>{d.activated}</div><div className="l">Đã kích hoạt BH</div></div>
        <div className="kpi"><div className="v">{d.scans.toLocaleString('vi-VN')}</div><div className="l">Lượt quét</div></div>
      </div>
      <div className="card funnel">
        <h2>Tem theo sản phẩm</h2>
        {d.byProduct.map((g, i) => (
          <div className="bar" key={i}><div className="lbl">{g.product}</div>
            <div className="track"><div className="fill" style={{ width: `${(g.count / max) * 100}%` }} /></div><div className="n">{g.count}</div></div>
        ))}
      </div>
    </>
  )
}

function Batches() {
  const [rows, setRows] = useState([]); const [open, setOpen] = useState(null); const [show, setShow] = useState(false)
  const load = () => api.batches().then(r => setRows(r.data))
  useEffect(() => { load() }, [])
  return (
    <>
      <div className="toolbar"><h1 style={{ margin: 0, flex: 1 }}>Lô tem</h1><button className="btn sm" style={{ flex: 'none' }} onClick={() => setShow(true)}>+ Sinh lô tem</button></div>
      <div className="card" style={{ padding: 0, overflow: 'auto' }}>
        <table><thead><tr><th>Mã lô</th><th>Sản phẩm</th><th>Số lô SX</th><th>NSX</th><th className="right">SL tem</th><th>Ngày tạo</th></tr></thead>
          <tbody>{rows.map(b => (
            <tr key={b.id} style={{ cursor: 'pointer' }} onClick={() => setOpen(b.id)}>
              <td>{b.code}</td><td>{b.product}</td><td>{b.lotNo || '—'}</td><td>{fmtDate(b.mfgDate)}</td>
              <td className="right">{b.quantity.toLocaleString('vi-VN')}</td><td>{fmtDate(b.createdAt)}</td></tr>))}
            {rows.length === 0 && <tr><td colSpan={6} className="muted" style={{ padding: 20 }}>Chưa có lô tem.</td></tr>}
          </tbody></table>
      </div>
      {open && <BatchDetail id={open} onClose={() => setOpen(null)} />}
      {show && <GenerateForm onClose={() => setShow(false)} onSaved={() => { setShow(false); load() }} />}
    </>
  )
}

function BatchDetail({ id, onClose }) {
  const [b, setB] = useState(null)
  useEffect(() => { api.batch(id).then(r => setB(r.data)) }, [id])
  if (!b) return <Modal title="…" onClose={onClose}><p className="muted">Đang tải…</p></Modal>
  return (
    <Modal title={`Lô ${b.code}`} onClose={onClose} wide>
      <dl className="dl"><dt>Sản phẩm</dt><dd>{b.product}</dd><dt>Số lô SX</dt><dd>{b.lotNo || '—'}</dd>
        <dt>NSX</dt><dd>{fmtDate(b.mfgDate)}</dd><dt>SL tem</dt><dd>{b.quantity} (đã kích hoạt {b.activated})</dd></dl>
      <div className="section-t">Tem (tối đa 200)</div>
      <div style={{ maxHeight: 320, overflow: 'auto' }}>
        <table><thead><tr><th>QR ID</th><th>PIN</th><th className="right">Quét</th><th>Trạng thái</th></tr></thead>
          <tbody>{b.stamps.map(s => (<tr key={s.id}><td style={{ fontFamily: 'monospace' }}>{s.qrId}</td><td>{s.pin}</td>
            <td className="right">{s.scanCount}</td><td><Badge text={s.statusText} css={s.status === 1 ? 'success' : s.status === 2 ? 'danger' : 'secondary'} /></td></tr>))}</tbody></table>
      </div>
    </Modal>
  )
}

function GenerateForm({ onClose, onSaved }) {
  const [prods, setProds] = useState([]); const [f, setF] = useState({ productId: '', lotNo: '', quantity: 100 }); const [err, setErr] = useState('')
  useEffect(() => { api.products().then(r => { setProds(r.data); if (r.data[0]) setF(s => ({ ...s, productId: r.data[0].id })) }) }, [])
  const up = (k, v) => setF({ ...f, [k]: v })
  const save = async () => { try { if (!f.productId) { setErr('Chọn sản phẩm'); return } await api.generate({ productId: Number(f.productId), lotNo: f.lotNo, quantity: Number(f.quantity) }); onSaved() } catch (e) { setErr(e.message) } }
  return (
    <Modal title="Sinh lô tem QR" onClose={onClose}>
      {err && <Flash msg={{ ok: false, text: err }} />}
      <Field label="Sản phẩm"><select value={f.productId} onChange={e => up('productId', e.target.value)}>{prods.map(p => <option key={p.id} value={p.id}>{p.code} · {p.name}</option>)}</select></Field>
      <div className="row"><Field label="Số lô SX"><input value={f.lotNo} onChange={e => up('lotNo', e.target.value)} /></Field>
        <Field label="Số lượng tem"><input type="number" value={f.quantity} onChange={e => up('quantity', e.target.value)} /></Field></div>
      <div style={{ marginTop: 16 }}><button className="btn" onClick={save}>Sinh tem</button></div>
    </Modal>
  )
}

function Stamps() {
  const [rows, setRows] = useState([]); const [q, setQ] = useState('')
  const load = () => api.stamps(q).then(r => setRows(r.data))
  useEffect(() => { load() }, [])
  return (
    <>
      <div className="toolbar"><h1 style={{ margin: 0, flex: 'none' }}>Tem</h1><div className="sp" />
        <input style={{ maxWidth: 240 }} placeholder="Tìm QR ID / PIN…" value={q} onChange={e => setQ(e.target.value)} onKeyDown={e => e.key === 'Enter' && load()} />
        <button className="btn ghost sm" style={{ flex: 'none' }} onClick={load}>Tìm</button></div>
      <div className="card" style={{ padding: 0, overflow: 'auto' }}>
        <table><thead><tr><th>QR ID</th><th>Sản phẩm</th><th>Lô</th><th className="right">Quét</th><th>SĐT kích hoạt</th><th>Trạng thái</th></tr></thead>
          <tbody>{rows.map(s => (<tr key={s.id}><td style={{ fontFamily: 'monospace' }}>{s.qrId}</td><td>{s.product}</td><td>{s.batch}</td>
            <td className="right">{s.scanCount}</td><td>{s.activatedPhone || '—'}</td>
            <td><Badge text={s.statusText} css={s.status === 1 ? 'success' : s.status === 2 ? 'danger' : 'secondary'} /></td></tr>))}
            {rows.length === 0 && <tr><td colSpan={6} className="muted" style={{ padding: 20 }}>Không có tem.</td></tr>}</tbody></table>
      </div>
    </>
  )
}

function Products() {
  const [rows, setRows] = useState([]); const [show, setShow] = useState(false)
  const load = () => api.products().then(r => setRows(r.data))
  useEffect(() => { load() }, [])
  return (
    <>
      <div className="toolbar"><h1 style={{ margin: 0, flex: 1 }}>Sản phẩm</h1><button className="btn sm" style={{ flex: 'none' }} onClick={() => setShow(true)}>+ Thêm</button></div>
      <div className="card" style={{ padding: 0, overflow: 'auto' }}>
        <table><thead><tr><th>Mã</th><th>Tên</th><th>NSX</th><th className="right">Bảo hành (tháng)</th></tr></thead>
          <tbody>{rows.map(p => (<tr key={p.id}><td>{p.code}</td><td>{p.name}</td><td>{p.manufacturer || '—'}</td><td className="right">{p.warrantyMonths}</td></tr>))}</tbody></table>
      </div>
      {show && <ProductForm onClose={() => setShow(false)} onSaved={() => { setShow(false); load() }} />}
    </>
  )
}

function ProductForm({ onClose, onSaved }) {
  const [f, setF] = useState({ name: '', code: '', manufacturer: '', warrantyMonths: 12 }); const [err, setErr] = useState('')
  const up = (k, v) => setF({ ...f, [k]: v })
  const save = async () => { try { await api.createProduct({ ...f, warrantyMonths: Number(f.warrantyMonths) }); onSaved() } catch (e) { setErr(e.message) } }
  return (
    <Modal title="Thêm sản phẩm" onClose={onClose}>
      {err && <Flash msg={{ ok: false, text: err }} />}
      <div className="row"><Field label="Tên *"><input value={f.name} onChange={e => up('name', e.target.value)} /></Field>
        <Field label="Mã"><input value={f.code} onChange={e => up('code', e.target.value)} /></Field></div>
      <div className="row"><Field label="Nhà sản xuất"><input value={f.manufacturer} onChange={e => up('manufacturer', e.target.value)} /></Field>
        <Field label="Bảo hành (tháng)"><input type="number" value={f.warrantyMonths} onChange={e => up('warrantyMonths', e.target.value)} /></Field></div>
      <div style={{ marginTop: 16 }}><button className="btn" onClick={save}>Lưu</button></div>
    </Modal>
  )
}

// Tra cứu tem (mô phỏng người tiêu dùng quét QR) — dùng chung API verify công khai.
function Verify() {
  const [code, setCode] = useState(''); const [res, setRes] = useState(null); const [phone, setPhone] = useState(''); const [msg, setMsg] = useState(null)
  const doVerify = async () => { try { const r = await api.verify(code.trim()); setRes(r.data); setMsg(null) } catch (e) { setMsg({ ok: false, text: e.message }) } }
  const activate = async () => { try { const r = await api.activate(code.trim(), phone); setMsg({ ok: true, text: r.data.msg }); doVerify() } catch (e) { setMsg({ ok: false, text: e.message }) } }
  const spin = async () => { try { const r = await api.spin(code.trim()); setMsg({ ok: true, text: '🎁 ' + r.data.prize }); doVerify() } catch (e) { setMsg({ ok: false, text: e.message }) } }
  return (
    <>
      <h1>Tra cứu tem (QR)</h1>
      <div className="card"><div className="row">
        <Field label="Mã QR trên tem"><input value={code} onChange={e => setCode(e.target.value)} placeholder="VD: A1B2C3D4E5F6" onKeyDown={e => e.key === 'Enter' && doVerify()} /></Field>
        <div style={{ flex: 'none', alignSelf: 'flex-end' }}><button className="btn" onClick={doVerify}>Kiểm tra</button></div>
      </div></div>
      <Flash msg={msg} />
      {res && (
        <div className="card" style={{ borderLeft: `5px solid ${res.genuine ? 'var(--success)' : 'var(--danger)'}` }}>
          <h2 style={{ color: res.genuine ? 'var(--success)' : 'var(--danger)' }}>{res.title}</h2>
          <p>{res.message}</p>
          {res.warnings?.length > 0 && <ul>{res.warnings.map((w, i) => <li key={i} className="muted">{w}</li>)}</ul>}
          {res.product && <dl className="dl"><dt>Sản phẩm</dt><dd>{res.product.name}</dd><dt>NSX</dt><dd>{res.product.manufacturer || '—'}</dd>
            {res.batch && <><dt>Lô SX</dt><dd>{res.batch.lotNo} · {fmtDate(res.batch.mfgDate)}</dd></>}
            <dt>Số lần quét</dt><dd>{res.stamp?.scanCount}</dd>
            {res.stamp?.warrantyEnd && <><dt>Bảo hành đến</dt><dd>{fmtDate(res.stamp.warrantyEnd)}</dd></>}</dl>}
          {res.found && res.genuine && (
            <div style={{ marginTop: 12 }}>
              {!res.stamp?.activated && <div className="row" style={{ marginBottom: 8 }}>
                <input placeholder="SĐT kích hoạt bảo hành" value={phone} onChange={e => setPhone(e.target.value)} />
                <div style={{ flex: 'none' }}><button className="btn sm" onClick={activate}>Kích hoạt BH</button></div></div>}
              {!res.stamp?.hasSpun ? <button className="btn ghost sm" onClick={spin}>🎡 Quay thưởng</button> : <span className="pill">Đã quay: {res.stamp.prize}</span>}
            </div>
          )}
        </div>
      )}
    </>
  )
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<Dashboard />} />
        <Route path="batches" element={<Batches />} />
        <Route path="stamps" element={<Stamps />} />
        <Route path="products" element={<Products />} />
        <Route path="verify" element={<Verify />} />
      </Route>
    </Routes>
  )
}
