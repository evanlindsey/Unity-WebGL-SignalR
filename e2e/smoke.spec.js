// WebGL SignalR smoke test
// Verifies the jslib bridge works end-to-end in a real browser:
//   1. Unity WebGL instance loads without errors
//   2. SignalR connection to hub succeeds (Debug.Log -> console.log)
//   3. No JavaScript errors during connection, invocation, or handler callbacks

const { test, expect } = require('@playwright/test');

const BASE_URL = process.env.BASE_URL || 'http://localhost:5000';
const TIMEOUT = 120_000; // Unity WebGL can take a while to load

function log(msg) {
  console.log(`[smoke] ${msg}`);
}

test('WebGL SignalR connection and round-trip', async ({ page }) => {
  test.setTimeout(TIMEOUT);

  const errors = [];
  const logs = [];

  page.on('console', msg => {
    const text = msg.text();
    logs.push(text);
    log(`[browser ${msg.type()}] ${text}`);
    if (msg.type() === 'error') {
      errors.push(text);
    }
  });

  page.on('pageerror', err => {
    errors.push(err.message);
    log(`[pageerror] ${err.message}`);
  });

  log(`Navigating to ${BASE_URL}`);
  await page.goto(BASE_URL, { waitUntil: 'domcontentloaded' });
  log('Page loaded (domcontentloaded)');

  // Wait for Unity to finish loading (progress bar disappears)
  log('Waiting for Unity loading bar to disappear...');
  await page.waitForFunction(() => {
    const bar = document.querySelector('#unity-loading-bar');
    return bar && bar.style.display === 'none';
  }, { timeout: TIMEOUT });
  log('Unity loaded');

  // Wait for the SignalR "Connected:" log from TestScript.cs
  // (Debug.Log in WebGL outputs to browser console)
  log('Waiting for SignalR "Connected:" log...');
  await expect.poll(() =>
    logs.some(l => l.includes('Connected:')),
    { timeout: 30_000, message: 'Expected "Connected:" in console logs' }
  ).toBeTruthy();
  log('SignalR connected');

  // Wait for round-trip completion: TestScript invokes SendPayloadCaller,
  // and the handler logs "ReceivePayloadCaller:" when the response arrives.
  // This confirms InvokeJs, OnJs, and invokeCallback all work end-to-end.
  log('Waiting for round-trip "ReceivePayloadCaller:" log...');
  await expect.poll(() =>
    logs.some(l => l.includes('ReceivePayloadCaller:')),
    { timeout: 30_000, message: 'Expected "ReceivePayloadCaller:" in console logs' }
  ).toBeTruthy();
  log('Round-trip complete');

  // Filter out expected browser noise from error check
  const realErrors = errors.filter(e =>
    !e.includes('favicon.ico') &&
    !e.includes('The above error')
  );

  log(`Done — ${logs.length} console messages, ${realErrors.length} errors`);
  expect(realErrors).toEqual([]);
});
