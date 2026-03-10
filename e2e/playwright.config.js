const { defineConfig } = require('@playwright/test');

module.exports = defineConfig({
  testDir: '.',
  timeout: 120_000,
  retries: 1,
  outputDir: 'test-results',
  reporter: process.env.CI
    ? [['html', { open: 'never' }], ['github']]
    : [['list', { printSteps: true }]],
  use: {
    headless: true,
    viewport: { width: 1024, height: 768 },
    trace: 'retain-on-failure',
    launchOptions: {
      args: ['--use-gl=angle'],
    },
  },
});
