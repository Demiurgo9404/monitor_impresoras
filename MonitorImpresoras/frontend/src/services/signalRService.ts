import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

export interface PrinterStatus {
  id: number
  name: string
  isOnline: boolean
  status: string
  lastSeen: string
}

export interface Alert {
  id: number
  printerId: number
  printerName: string
  type: string
  message: string
  severity: 'Low' | 'Medium' | 'High' | 'Critical'
  status: 'Active' | 'Acknowledged' | 'Resolved'
  createdAt: string
}

export interface ConsumableStatus {
  printerId: number
  printerName: string
  consumableType: string
  currentLevel: number
  maxLevel: number
  status: 'OK' | 'Low' | 'Critical'
  lastUpdated: string
}

class SignalRService {
  private connection: HubConnection | null = null
  private isConnecting = false

  private eventHandlers: {
    [eventName: string]: ((...args: any[]) => void)[]
  } = {}

  async startConnection(): Promise<void> {
    if (this.connection?.state === 'Connected') {
      return
    }

    if (this.isConnecting) {
      return
    }

    this.isConnecting = true

    try {
      this.connection = new HubConnectionBuilder()
        .withUrl('/hubs/printer', {
          accessTokenFactory: () => localStorage.getItem('token') || ''
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build()

      this.setupEventHandlers()

      await this.connection.start()
      console.log('SignalR connection started successfully')
    } catch (error) {
      console.error('Error starting SignalR connection:', error)
      throw error
    } finally {
      this.isConnecting = false
    }
  }

  private setupEventHandlers(): void {
    if (!this.connection) return

    this.connection.onclose(() => {
      console.log('SignalR connection closed')
    })

    this.connection.onreconnecting(() => {
      console.log('SignalR reconnecting...')
    })

    this.connection.onreconnected(() => {
      console.log('SignalR reconnected')
    })

    // Event handlers for different message types
    this.connection.on('PrinterStatusUpdate', (printer: PrinterStatus) => {
      this.emit('printerStatusUpdate', printer)
    })

    this.connection.on('NewAlert', (alert: Alert) => {
      this.emit('newAlert', alert)
    })

    this.connection.on('AlertAcknowledged', (alertId: number) => {
      this.emit('alertAcknowledged', alertId)
    })

    this.connection.on('AlertResolved', (alertId: number) => {
      this.emit('alertResolved', alertId)
    })

    this.connection.on('ConsumableStatusUpdate', (consumable: ConsumableStatus) => {
      this.emit('consumableStatusUpdate', consumable)
    })

    this.connection.on('TestNotification', (notification: any) => {
      this.emit('testNotification', notification)
    })

    this.connection.on('Error', (error: string) => {
      this.emit('error', error)
    })
  }

  async stopConnection(): Promise<void> {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
    }
  }

  // Hub methods
  async subscribeToPrinter(printerId: number): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('SubscribeToPrinter', printerId)
    }
  }

  async unsubscribeFromPrinter(printerId: number): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('UnsubscribeFromPrinter', printerId)
    }
  }

  async subscribeToAllPrinters(): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('SubscribeToAllPrinters')
    }
  }

  async unsubscribeFromAllPrinters(): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('UnsubscribeFromAllPrinters')
    }
  }

  async subscribeToAlerts(): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('SubscribeToAlerts')
    }
  }

  async unsubscribeFromAlerts(): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('UnsubscribeFromAlerts')
    }
  }

  async subscribeToConsumables(): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('SubscribeToConsumables')
    }
  }

  async unsubscribeFromConsumables(): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('UnsubscribeFromConsumables')
    }
  }

  async requestPrinterSummary(): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('RequestPrinterSummary')
    }
  }

  async requestAlertsSummary(): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('RequestAlertsSummary')
    }
  }

  async requestConsumablesSummary(): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('RequestConsumablesSummary')
    }
  }

  async sendTestNotification(): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('SendTestNotification')
    }
  }

  // Event subscription methods
  on(eventName: string, handler: (...args: any[]) => void): void {
    if (!this.eventHandlers[eventName]) {
      this.eventHandlers[eventName] = []
    }
    this.eventHandlers[eventName].push(handler)
  }

  off(eventName: string, handler: (...args: any[]) => void): void {
    if (this.eventHandlers[eventName]) {
      this.eventHandlers[eventName] = this.eventHandlers[eventName].filter(
        h => h !== handler
      )
    }
  }

  private emit(eventName: string, ...args: any[]): void {
    if (this.eventHandlers[eventName]) {
      this.eventHandlers[eventName].forEach(handler => {
        handler(...args)
      })
    }
  }

  get connectionState(): string {
    return this.connection?.state || 'Disconnected'
  }
}

export const signalRService = new SignalRService()
