// WebGL SignalR smoke test
// Verifies the jslib bridge works end-to-end in a real browser:
//   1. Unity WebGL instance loads without errors
//   2. SignalR connection to hub succeeds (Debug.Log -> console.log)
//   3. No JavaScript errors during connection, invocation, or handler callbacks

const { test, expect } = require('@playwright/test');

const BASE_URL = process.env.BASE_URL || 'http://localhost:5000';
const TIMEOUT = 120_000; // Unity WebGL can take a while to load

test('WebGL SignalR connection and round-trip', async ({ page }) => {
  test.setTimeout(TIMEOUT);

  const errors = [];
  const logs = [];

  page.on('console', msg => {
    const text = msg.text();
    logs.push(text);
    if (msg.type() === 'error') {
      errors.push(text);
    }
  });

  page.on('pageerror', err => {
    errors.push(err.message);
  });

  await page.goto(BASE_URL, { waitUntil: 'domcontentloaded' });

  // Wait for Unity to finish loading (progress bar disappears)
  await page.waitForFunction(() => {
    const bar = document.querySelector('#unity-loading-bar');
    return bar && bar.style.display === 'none';
  }, { timeout: TIMEOUT });

  // Wait for the SignalR "Connected:" log from TestScript.cs
  // (Debug.Log in WebGL outputs to browser console)
  await expect.poll(() =>
    logs.some(l => l.includes('Connected:')),
    { timeout: 30_000, message: 'Expected "Connected:" in console logs' }
  ).toBeTruthy();

  // Give time for hub invocations (SendPayloadAll, SendPayloadCaller)
  // and handler callbacks to complete — any errors here indicate
  // jslib issues with InvokeJs, OnJs, or invokeCallback
  await page.waitForTimeout(3000);

  // Filter out expected browser noise from error check
  const realErrors = errors.filter(e =>
    !e.includes('favicon.ico') &&
    !e.includes('The above error')
  );

  expect(realErrors).toEqual([]);
});
