const { defineConfig } = require('cypress')

module.exports = defineConfig({
  e2e: {
    baseUrl: 'https://staging.monitorimpresoras.com',
    viewportWidth: 1280,
    viewportHeight: 720,
    video: true,
    screenshotOnRunFailure: true,
    defaultCommandTimeout: 10000,
    requestTimeout: 15000,
    responseTimeout: 15000,
    retries: {
      runMode: 2,
      openMode: 0
    },
    env: {
      apiUrl: 'https://staging-api.monitorimpresoras.com',
      adminEmail: 'admin@monitorimpresoras.com',
      adminPassword: 'Admin123!',
      testUserEmail: 'test@monitorimpresoras.com',
      testUserPassword: 'Test123!'
    },
    setupNodeEvents(on, config) {
      // Configuración adicional de eventos
      on('task', {
        log(message) {
          console.log(message)
          return null
        }
      })
    }
  }
})
