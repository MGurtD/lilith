// Simple reusable WebSocket client (initial minimal version)
// TODO: extend with reconnection, heartbeat, batching, subscriptions as needed.

export interface WebSocketClientOptions {
  debug?: boolean;
  autoReconnect?: boolean; // reserved for future enhancement
}

export type MessageHandler = (data: any, raw: MessageEvent) => void;
export type VoidHandler = () => void;
export type ErrorHandler = (err: Event) => void;

export class WebSocketClient {
  private ws: WebSocket | null = null;
  private readonly url: string;
  private readonly options: WebSocketClientOptions;
  private messageHandlers = new Set<MessageHandler>();
  private openHandlers = new Set<VoidHandler>();
  private closeHandlers = new Set<VoidHandler>();
  private errorHandlers = new Set<ErrorHandler>();

  constructor(url: string, options?: WebSocketClientOptions) {
    this.url = url;
    this.options = options || {};
  }

  connect() {
    if (
      this.ws &&
      (this.ws.readyState === WebSocket.OPEN ||
        this.ws.readyState === WebSocket.CONNECTING)
    ) {
      this.debug("connect() skipped – already connecting/open");
      return;
    }
    this.debug("Connecting to", this.url);
    this.ws = new WebSocket(this.url);
    this.ws.onopen = () => {
      this.debug("WebSocket open");
      this.openHandlers.forEach((h) => h());
    };
    this.ws.onmessage = (ev) => {
      let payload: any = ev.data;
      if (typeof payload === "string") {
        try {
          payload = JSON.parse(payload);
        } catch {
          // keep raw string if JSON parse fails
        }
      }
      this.messageHandlers.forEach((h) => h(payload, ev));
    };
    this.ws.onerror = (ev) => {
      this.debug("WebSocket error", ev);
      this.errorHandlers.forEach((h) => h(ev));
    };
    this.ws.onclose = () => {
      this.debug("WebSocket closed");
      this.closeHandlers.forEach((h) => h());
      this.ws = null;
    };
  }

  disconnect() {
    if (this.ws) {
      this.debug("Disconnecting WebSocket");
      this.ws.close();
      this.ws = null;
    }
  }

  send(obj: unknown) {
    if (!this.isOpen()) return false;
    try {
      const serialized = typeof obj === "string" ? obj : JSON.stringify(obj);
      this.ws!.send(serialized);
      return true;
    } catch (e) {
      this.debug("Send failed", e);
      return false;
    }
  }

  isOpen() {
    return this.ws?.readyState === WebSocket.OPEN;
  }

  onMessage(handler: MessageHandler) {
    this.messageHandlers.add(handler);
    return () => this.messageHandlers.delete(handler);
  }
  onOpen(handler: VoidHandler) {
    this.openHandlers.add(handler);
    return () => this.openHandlers.delete(handler);
  }
  onClose(handler: VoidHandler) {
    this.closeHandlers.add(handler);
    return () => this.closeHandlers.delete(handler);
  }
  onError(handler: ErrorHandler) {
    this.errorHandlers.add(handler);
    return () => this.errorHandlers.delete(handler);
  }

  private debug(...args: any[]) {
    if (this.options.debug) {
      console.log("[WS]", ...args);
    }
  }
}
