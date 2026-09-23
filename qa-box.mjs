export default async function run(page, ui) {
  const base = process.env.QA_BASE
  await page.goto(base + '/Stamp')
  const codes = await page.evaluate(() =>
    Array.from(document.querySelectorAll('table tbody tr'))
      .map(r => r.querySelector('td') ? r.querySelector('td').innerText.trim() : '')
      .filter(Boolean).slice(0, 2))

  await page.goto(base + '/Box/Create')
  await page.selectOption('select[name=productId]', { index: 1 })
  await page.fill('input[name=boxNo]', 'BOX-QA-001')
  await page.fill('textarea[name=codes]', codes.join('\n'))
  await Promise.all([page.waitForNavigation(), page.locator('form').evaluate(f => f.submit())])
  await page.waitForLoadState('networkidle')

  const afterUrl = page.url()
  const afterTitle = await page.title()
  const body = await page.evaluate(() => document.querySelector('.card-body').innerText.replace(/\s+/g, ' ').slice(0, 160))
  const body = await page.evaluate(() => document.querySelector('.card-body').innerText.replace(/\s+/g, ').slice(0, 160))
  return { codes, afterUrl, afterTitle, body }
}
