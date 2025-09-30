// ***********************************************************
// This example support/e2e.js is processed and
// loaded automatically before your test files.
//
// This is a great place to put global configuration and
// behavior that modifies Cypress.
//
// You can change the location of this file or turn off
// automatically serving support files with the
// 'supportFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/configuration
// ***********************************************************

// Import commands.js using ES2015 syntax:
import './commands'

// Alternatively you can use CommonJS syntax:
// require('./commands')

// Configuración global para pruebas E2E
Cypress.on('uncaught:exception', (err, runnable) => {
  // Ignorar errores de red en staging (pueden ser esperados)
  if (err.message.includes('Network Error') ||
      err.message.includes('Failed to fetch') ||
      err.message.includes('Script error')) {
    return false
  }
  return true
})

// Configuración de timeouts específicos para staging
Cypress.config('defaultCommandTimeout', 15000)
Cypress.config('requestTimeout', 20000)
Cypress.config('responseTimeout', 20000)
