// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************

// Comando para login de administrador
Cypress.Commands.add('loginAsAdmin', () => {
  cy.session('admin-session', () => {
    cy.request({
      method: 'POST',
      url: `${Cypress.env('apiUrl')}/api/auth/login`,
      body: {
        email: Cypress.env('adminEmail'),
        password: Cypress.env('adminPassword')
      }
    }).then((response) => {
      expect(response.status).to.eq(200)
      expect(response.body).to.have.property('token')
      expect(response.body).to.have.property('refreshToken')

      // Guardar tokens para uso posterior
      Cypress.env('accessToken', response.body.token)
      Cypress.env('refreshToken', response.body.refreshToken)

      // Configurar header de autorización para requests posteriores
      cy.window().then((win) => {
        win.localStorage.setItem('token', response.body.token)
        win.localStorage.setItem('refreshToken', response.body.refreshToken)
      })
    })
  })
})

// Comando para crear usuario de prueba
Cypress.Commands.add('createTestUser', (userData = {}) => {
  const defaultUser = {
    firstName: 'Usuario',
    lastName: 'Prueba',
    email: `test${Date.now()}@monitorimpresoras.com`,
    password: 'Test123!',
    roles: ['User']
  }

  const user = { ...defaultUser, ...userData }

  return cy.request({
    method: 'POST',
    url: `${Cypress.env('apiUrl')}/api/users`,
    headers: {
      'Authorization': `Bearer ${Cypress.env('accessToken')}`
    },
    body: user
  }).then((response) => {
    expect(response.status).to.eq(201)
    return response.body
  })
})

// Comando para crear impresora de prueba
Cypress.Commands.add('createTestPrinter', (printerData = {}) => {
  const defaultPrinter = {
    name: `Impresora Test ${Date.now()}`,
    model: 'HP LaserJet Pro M404dn',
    location: 'Oficina de Pruebas',
    ipAddress: '192.168.1.100',
    isActive: true
  }

  const printer = { ...defaultPrinter, ...printerData }

  return cy.request({
    method: 'POST',
    url: `${Cypress.env('apiUrl')}/api/printers`,
    headers: {
      'Authorization': `Bearer ${Cypress.env('accessToken')}`
    },
    body: printer
  }).then((response) => {
    expect(response.status).to.eq(201)
    return response.body
  })
})

// Comando para verificar health check
Cypress.Commands.add('checkHealth', (endpoint = 'health') => {
  return cy.request({
    method: 'GET',
    url: `${Cypress.env('apiUrl')}/api/${endpoint}`,
    failOnStatusCode: false
  }).then((response) => {
    return response
  })
})

// Comando para limpiar datos de prueba
Cypress.Commands.add('cleanupTestData', () => {
  // Este comando debería limpiar usuarios e impresoras de prueba
  // Implementar según sea necesario
  cy.log('🧹 Limpiando datos de prueba...')
})

// Comando personalizado para esperar carga de página
Cypress.Commands.add('waitForPageLoad', (url) => {
  cy.intercept('GET', '**').as('pageLoad')
  cy.visit(url)
  cy.wait('@pageLoad', { timeout: 30000 })
})

// Comando para verificar elementos críticos en página
Cypress.Commands.add('verifyPageElements', (elements) => {
  elements.forEach(element => {
    if (element.type === 'button') {
      cy.get(element.selector).should('be.visible').and('not.be.disabled')
    } else if (element.type === 'input') {
      cy.get(element.selector).should('be.visible')
    } else if (element.type === 'text') {
      cy.get(element.selector).should('contain.text', element.text)
    }
  })
})

// Comando para manejar errores de aplicación
Cypress.Commands.add('handleAppError', () => {
  cy.window().then((win) => {
    win.addEventListener('error', (event) => {
      cy.log(`🚨 Error de aplicación: ${event.message}`)
      // Puedes agregar lógica adicional aquí
    })
  })
})
