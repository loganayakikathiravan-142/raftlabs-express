import * as signalR from '@microsoft/signalr';
import { OrderDto } from '../types';

const HUB_URL = import.meta.env.VITE_HUB_URL || '/hubs/orders';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private isConnecting = false;

  private async ensureConnection(): Promise<signalR.HubConnection> {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      return this.connection;
    }

    if (!this.connection) {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL, {
          skipNegotiation: false,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();
    }

    if (this.connection.state === signalR.HubConnectionState.Disconnected && !this.isConnecting) {
      this.isConnecting = true;
      try {
        await this.connection.start();
      } catch (err) {
        console.warn('SignalR connection failed, falling back to polling.', err);
      } finally {
        this.isConnecting = false;
      }
    }

    return this.connection;
  }

  async joinOrderTracking(orderNumber: string, onUpdate: (order: OrderDto) => void): Promise<() => void> {
    try {
      const conn = await this.ensureConnection();
      if (conn.state === signalR.HubConnectionState.Connected) {
        await conn.invoke('JoinOrderGroup', orderNumber);
      }

      const handler = (order: OrderDto) => {
        if (order.orderNumber === orderNumber) {
          onUpdate(order);
        }
      };

      conn.on('OrderStatusChanged', handler);

      return () => {
        conn.off('OrderStatusChanged', handler);
        if (conn.state === signalR.HubConnectionState.Connected) {
          conn.invoke('LeaveOrderGroup', orderNumber).catch(() => {});
        }
      };
    } catch {
      return () => {};
    }
  }

  async subscribeAdmin(onUpdate: (order: OrderDto) => void, onCreated?: (order: OrderDto) => void): Promise<() => void> {
    try {
      const conn = await this.ensureConnection();
      if (conn.state === signalR.HubConnectionState.Connected) {
        await conn.invoke('JoinAdminGroup');
      }

      const updateHandler = (order: OrderDto) => onUpdate(order);
      conn.on('OrderStatusChanged', updateHandler);

      let createdHandler: ((order: OrderDto) => void) | undefined;
      if (onCreated) {
        createdHandler = (order: OrderDto) => onCreated(order);
        conn.on('OrderCreated', createdHandler);
      }

      return () => {
        conn.off('OrderStatusChanged', updateHandler);
        if (createdHandler) {
          conn.off('OrderCreated', createdHandler);
        }
      };
    } catch {
      return () => {};
    }
  }
}

export const signalrService = new SignalRService();
