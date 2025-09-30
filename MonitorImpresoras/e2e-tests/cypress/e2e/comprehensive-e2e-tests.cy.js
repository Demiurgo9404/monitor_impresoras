describe('Flujos End-to-End - Monitor de Impresoras', () => {
  beforeEach(() => {
    // Configuración antes de cada test
    cy.handleAppError()
    cy.loginAsAdmin()
  })

  afterEach(() => {
    // Limpieza después de cada test
    cy.cleanupTestData()
  })

  describe('🔐 Autenticación y Autorización', () => {
    it('Debe permitir login exitoso con credenciales válidas', () => {
      cy.visit('/login')

      cy.get('[data-cy="email-input"]').type(Cypress.env('adminEmail'))
      cy.get('[data-cy="password-input"]').type(Cypress.env('adminPassword'))
      cy.get('[data-cy="login-button"]').click()

      // Verificar redirección al dashboard
      cy.url().should('include', '/dashboard')
      cy.get('[data-cy="dashboard-title"]').should('contain.text', 'Dashboard')
    })

    it('Debe rechazar login con credenciales inválidas', () => {
      cy.visit('/login')

      cy.get('[data-cy="email-input"]').type('invalid@monitorimpresoras.com')
      cy.get('[data-cy="password-input"]').type('wrongpassword')
      cy.get('[data-cy="login-button"]').click()

      // Verificar que permanece en login
      cy.url().should('include', '/login')
      cy.get('[data-cy="error-message"]').should('be.visible')
    })

    it('Debe manejar refresh token correctamente', () => {
      // Simular expiración de token y refresh automático
      cy.window().then((win) => {
        win.localStorage.setItem('token', 'expired_token')
        win.localStorage.setItem('refreshToken', Cypress.env('refreshToken'))
      })

      cy.visit('/dashboard')

      // Debería redirigir a login si refresh falla
      cy.url().should('include', '/login')
    })
  })

  describe('📊 Dashboard y Navegación', () => {
    beforeEach(() => {
      cy.visit('/dashboard')
    })

    it('Debe mostrar métricas básicas en dashboard', () => {
      // Verificar elementos críticos del dashboard
      cy.verifyPageElements([
        { type: 'text', selector: '[data-cy="total-printers"]', text: /\d+/ },
        { type: 'text', selector: '[data-cy="online-printers"]', text: /\d+/ },
        { type: 'text', selector: '[data-cy="offline-printers"]', text: /\d+/ },
        { type: 'button', selector: '[data-cy="refresh-dashboard"]' }
      ])

      // Verificar gráficos de métricas
      cy.get('[data-cy="metrics-chart"]').should('be.visible')
      cy.get('[data-cy="status-chart"]').should('be.visible')
    })

    it('Debe permitir navegación entre módulos', () => {
      // Navegar a gestión de impresoras
      cy.get('[data-cy="nav-printers"]').click()
      cy.url().should('include', '/printers')
      cy.get('[data-cy="printers-title"]').should('contain.text', 'Gestión de Impresoras')

      // Navegar a gestión de usuarios
      cy.get('[data-cy="nav-users"]').click()
      cy.url().should('include', '/users')
      cy.get('[data-cy="users-title"]').should('contain.text', 'Gestión de Usuarios')

      // Navegar a políticas
      cy.get('[data-cy="nav-policies"]').click()
      cy.url().should('include', '/policies')
      cy.get('[data-cy="policies-title"]').should('contain.text', 'Políticas de Impresión')
    })
  })

  describe('🖨️ Gestión de Impresoras (CRUD)', () => {
    beforeEach(() => {
      cy.visit('/printers')
    })

    it('Debe permitir crear nueva impresora', () => {
      cy.get('[data-cy="new-printer-button"]').click()

      // Llenar formulario
      cy.get('[data-cy="printer-name"]').type(`Impresora E2E ${Date.now()}`)
      cy.get('[data-cy="printer-model"]').type('HP LaserJet Pro M404dn')
      cy.get('[data-cy="printer-location"]').type('Oficina de Pruebas E2E')
      cy.get('[data-cy="printer-ip"]').type('192.168.1.200')
      cy.get('[data-cy="printer-active"]').check()

      cy.get('[data-cy="save-printer-button"]').click()

      // Verificar creación
      cy.get('[data-cy="success-message"]').should('contain.text', 'Impresora creada')
      cy.get('[data-cy="printer-list"]').should('contain.text', 'Impresora E2E')
    })

    it('Debe permitir editar impresora existente', () => {
      // Crear impresora primero
      cy.createTestPrinter({
        name: `Editar Test ${Date.now()}`,
        location: 'Oficina Original'
      }).then((createdPrinter) => {
        // Editar impresora
        cy.get(`[data-cy="edit-printer-${createdPrinter.id}"]`).click()

        cy.get('[data-cy="printer-name"]').clear().type('Impresora Editada E2E')
        cy.get('[data-cy="printer-location"]').clear().type('Oficina Editada')

        cy.get('[data-cy="save-printer-button"]').click()

        // Verificar cambios
        cy.get('[data-cy="success-message"]').should('contain.text', 'Impresora actualizada')
        cy.get('[data-cy="printer-list"]').should('contain.text', 'Impresora Editada E2E')
      })
    })

    it('Debe permitir eliminar impresora', () => {
      // Crear impresora primero
      cy.createTestPrinter({
        name: `Eliminar Test ${Date.now()}`
      }).then((createdPrinter) => {
        // Eliminar impresora
        cy.get(`[data-cy="delete-printer-${createdPrinter.id}"]`).click()

        // Confirmar eliminación
        cy.get('[data-cy="confirm-delete-button"]').click()

        // Verificar eliminación
        cy.get('[data-cy="success-message"]').should('contain.text', 'Impresora eliminada')
        cy.get('[data-cy="printer-list"]').should('not.contain.text', createdPrinter.name)
      })
    })

    it('Debe mostrar detalles de impresora', () => {
      // Crear impresora primero
      cy.createTestPrinter({
        name: `Detalle Test ${Date.now()}`
      }).then((createdPrinter) => {
        // Ver detalles
        cy.get(`[data-cy="view-printer-${createdPrinter.id}"]`).click()

        cy.url().should('include', `/printers/${createdPrinter.id}`)

        // Verificar información mostrada
        cy.verifyPageElements([
          { type: 'text', selector: '[data-cy="printer-detail-name"]', text: createdPrinter.name },
          { type: 'text', selector: '[data-cy="printer-detail-model"]', text: createdPrinter.model },
          { type: 'text', selector: '[data-cy="printer-detail-location"]', text: createdPrinter.location }
        ])

        // Verificar métricas históricas
        cy.get('[data-cy="printer-metrics-chart"]').should('be.visible')
        cy.get('[data-cy="printer-history-table"]').should('be.visible')
      })
    })
  })

  describe('👥 Gestión de Usuarios', () => {
    beforeEach(() => {
      cy.visit('/users')
    })

    it('Debe permitir crear nuevo usuario', () => {
      cy.get('[data-cy="new-user-button"]').click()

      // Llenar formulario
      cy.get('[data-cy="user-firstname"]').type('Usuario')
      cy.get('[data-cy="user-lastname"]').type('Prueba E2E')
      cy.get('[data-cy="user-email"]').type(`usertest${Date.now()}@monitorimpresoras.com`)
      cy.get('[data-cy="user-password"]').type('Test123!')
      cy.get('[data-cy="user-confirmpassword"]').type('Test123!')

      // Seleccionar roles
      cy.get('[data-cy="user-role-user"]').check()

      cy.get('[data-cy="save-user-button"]').click()

      // Verificar creación
      cy.get('[data-cy="success-message"]').should('contain.text', 'Usuario creado')
      cy.get('[data-cy="user-list"]').should('contain.text', 'Usuario Prueba E2E')
    })

    it('Debe permitir editar usuario existente', () => {
      // Crear usuario primero
      cy.createTestUser({
        firstName: 'Editar',
        lastName: 'Usuario E2E',
        email: `edituser${Date.now()}@monitorimpresoras.com`
      }).then((createdUser) => {
        // Editar usuario
        cy.get(`[data-cy="edit-user-${createdUser.id}"]`).click()

        cy.get('[data-cy="user-firstname"]').clear().type('Editado')
        cy.get('[data-cy="user-lastname"]').clear().type('Usuario Editado')

        cy.get('[data-cy="save-user-button"]').click()

        // Verificar cambios
        cy.get('[data-cy="success-message"]').should('contain.text', 'Usuario actualizado')
        cy.get('[data-cy="user-list"]').should('contain.text', 'Editado Usuario Editado')
      })
    })
  })

  describe('📋 Políticas de Impresión', () => {
    beforeEach(() => {
      cy.visit('/policies')
    })

    it('Debe permitir crear nueva política', () => {
      cy.get('[data-cy="new-policy-button"]').click()

      // Llenar formulario
      cy.get('[data-cy="policy-name"]').type(`Política E2E ${Date.now()}`)
      cy.get('[data-cy="policy-description"]').type('Política creada para pruebas E2E')
      cy.get('[data-cy="policy-type"]').select('MonthlyLimit')
      cy.get('[data-cy="policy-monthly-limit"]').type('1000')
      cy.get('[data-cy="policy-cost-per-page"]').type('0.05')

      cy.get('[data-cy="save-policy-button"]').click()

      // Verificar creación
      cy.get('[data-cy="success-message"]').should('contain.text', 'Política creada')
      cy.get('[data-cy="policy-list"]').should('contain.text', 'Política E2E')
    })
  })

  describe('📈 Reportes y Descargas', () => {
    beforeEach(() => {
      cy.visit('/dashboard')
    })

    it('Debe generar reporte básico correctamente', () => {
      // Crear algunos datos de prueba primero
      cy.createTestPrinter({
        name: `Reporte Test ${Date.now()}`
      })

      // Navegar a sección de reportes (si existe)
      cy.get('[data-cy="reports-button"]').click()

      // Generar reporte
      cy.get('[data-cy="generate-report-button"]').click()
      cy.get('[data-cy="report-format-json"]').check()
      cy.get('[data-cy="generate-button"]').click()

      // Verificar generación
      cy.get('[data-cy="report-download-link"]').should('be.visible')
      cy.get('[data-cy="report-status"]').should('contain.text', 'Generado')
    })

    it('Debe permitir descarga de archivos CSV/PDF', () => {
      // Crear datos de prueba
      cy.createTestPrinter({
        name: `Export Test ${Date.now()}`
      })

      // Solicitar exportación
      cy.get('[data-cy="export-button"]').click()
      cy.get('[data-cy="export-format-csv"]').check()
      cy.get('[data-cy="export-generate-button"]').click()

      // Verificar descarga (simulada)
      cy.get('[data-cy="export-status"]').should('contain.text', 'Procesando')
    })
  })

  describe('🔍 Verificaciones de Health Checks', () => {
    it('Debe responder correctamente a health checks básicos', () => {
      cy.checkHealth('health').then((response) => {
        expect(response.status).to.eq(200)
        expect(response.body.status).to.eq('healthy')
      })
    })

    it('Debe proporcionar información detallada en health extendido', () => {
      cy.checkHealth('health/extended').then((response) => {
        expect(response.status).to.eq(200)
        expect(response.body).to.have.property('components')
        expect(response.body.components).to.be.an('array')
        expect(response.body).to.have.property('timestamp')
      })
    })

    it('Debe verificar conectividad de base de datos', () => {
      cy.checkHealth('health/database').then((response) => {
        expect(response.status).to.eq(200)
        expect(response.body.component).to.eq('Database')
      })
    })
  })

  describe('🚨 Manejo de Errores', () => {
    it('Debe mostrar página 404 para rutas inexistentes', () => {
      cy.visit('/ruta-inexistente', { failOnStatusCode: false })
      cy.get('[data-cy="error-404"]').should('be.visible')
      cy.get('[data-cy="error-message"]').should('contain.text', 'Página No Encontrada')
    })

    it('Debe manejar errores de red correctamente', () => {
      // Simular error de red
      cy.intercept('GET', '/api/printers', { forceNetworkError: true })

      cy.visit('/printers')
      cy.get('[data-cy="network-error"]').should('be.visible')
      cy.get('[data-cy="retry-button"]').should('be.visible')
    })
  })
})
