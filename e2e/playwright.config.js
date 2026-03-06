const { defineConfig } = require('@playwright/test');

module.exports = defineConfig({
  testDir: '.',
  timeout: 120_000,
  retries: 1,
  use: {
    headless: true,
    viewport: { width: 1024, height: 768 },
    launchOptions: {
      args: ['--use-gl=angle'],
    },
  },
});
