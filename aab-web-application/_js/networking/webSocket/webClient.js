import { ClientConnectionType } from "./clientConnectionType.js";
import {JsonRpcMessage} from "../eventHandling/jsonRpcMessage.js";

/**
 * Defines the possible connection types of the websocket client.
 * @type {{Connected: number, Disconnected: number, Connecting: number, Registered: number}}
 * @enum
 */
const WsConnectionStatus = {
    "Disconnected": 0,
    "Connected": 1,
    "Registered": 2
};

/**
 * A Wrapper for the overall websocket client implementation.
 */
class WebSocketClient extends EventEmitter {
    //#region Event Names
    static get STARTED() { return "Started"; }
    static get STOPPED() { return "Stopped"; }
    static get CONNECTION_OPENED() { return "ConnectionOpened"; }
    static get CONNECTION_CLOSED() { return "ConnectionClosed"; }
    static get MESSAGE_RECEIVED() { return "MessageReceived"; }
    static get MESSAGE_SEND() { return "MessageSend"; }
    static get REGISTERED() { return "Registered"; }
    //#endregion

    //#region Singleton
    /**
     * @type WebSocketClient
     * @private
     * @static
     */
    static _instance;
    /**
     * @return {WebSocketClient}
     * @static
     */
    static get Instance() {
        if (WebSocketClient._instance === undefined)
            new WebSocketClient();
        return WebSocketClient._instance;
    }

    constructor() {
        if (!WebSocketClient._instance) {
            super();
            this._constructor();
            WebSocketClient._instance = this;
        }
        return WebSocketClient._instance;
    }

    _constructor() {
        this.defineEvents( [
            WebSocketClient.STARTED,
            WebSocketClient.STOPPED,
            WebSocketClient.CONNECTION_OPENED,
            WebSocketClient.CONNECTION_CLOSED,
            WebSocketClient.MESSAGE_RECEIVED,
            WebSocketClient.MESSAGE_SEND,
            WebSocketClient.REGISTERED
        ]);
    }
    //#endregion

    //#region Properties - Address
    /**
     * @type string
     * @private
     */
    _ip = undefined;
    /**
     * @return {string}
     */
    get Ip() { return this._ip; }
    /**
     * @type number
     * @private
     */
    _port = undefined;
    /**
     * @return {number}
     */
    get Port() { return this._port; }
    /**
     * @type string
     * @private
     */
    _address = "";
    /**
     * @return {string}
     */
    get Address() { return this._address; }

    /**
     * @return {string}
     */
    get CompleteWsAddress() { return `ws://${this._ip}:${this._port}${this._address}`; }
    //#endregion

    //#region Properties - State and Name
    /**
     * @type number
     * @private
     */
    _status = WsConnectionStatus.Disconnected;
    /**
     * @return {number}
     */
    get Status() { return this._status; }
    /**
     * @type string
     * @private
     */
    _clientConnectionType = ClientConnectionType.NotDefined;
    /**
     * @return {string}
     */
    get ClientConnectionType() { return this._clientConnectionType; }
    /**
     * @type string
     * @private
     */
    _connectionName = undefined;
    /**
     * @return {string}
     */
    get ConnectionName() { return this._connectionName; }

    /**
     * @return {boolean}
     */
    get Usable() { return this._status >= 1 && this._clientConnectionType !== ClientConnectionType.NotDefined}
    //#endregion

    /**
     * @type WebSocket
     * @private
     */
    _webSocket;

    //#region Lifecycle Methods
    /***
     * Starts the websocket client.
     * @param {!string} ip - The ip to connect to.
     * @param {!number} port - The port to open to.
     * @param {string} address - The address of the websocket connection.
     */
    StartClient({ip, port, address = ""}) {
        this._ip = ip;
        this._port = port;
        this._address = address

        this._webSocket = new WebSocket(this.CompleteWsAddress);
        this._webSocket.onopen = evt => this._onOpen(evt);
        this._webSocket.onclose = evt => this._onClose(evt);
        this._webSocket.onmessage = evt => this._onMessage(evt);

        this.emitEvent(WebSocketClient.STARTED, []);
    }

    /**
     * Stops this websocket client.
     */
    StopClient() {
        if (this._status === WsConnectionStatus.Connected)
            this._webSocket.close();

        this._webSocket.onopen = undefined;
        this._webSocket.onclose = undefined;
        this._webSocket.onmessage = undefined;
        this._webSocket = undefined;

        this._ip = undefined;
        this._port = undefined;
        this._address = "";

        this._status = WsConnectionStatus.Disconnected;
        this._clientConnectionType = ClientConnectionType.NotDefined;
        this.emitEvent(WebSocketClient.STOPPED, []);
    }
    //#endregion

    //#region Websocket Events
    /**
     * Called when the websocket client has successfully opened a connection to the server.
     * @param evt
     * @private
     */
    _onOpen(evt) {
        this._status = WsConnectionStatus.Connected;
        this.emitEvent(WebSocketClient.CONNECTION_OPENED, []);
    }

    /**
     * Called when the websocket client has successfully closed the open connection to the server.
     * @param evt
     * @private
     */
    _onClose(evt) {
        this._status = WsConnectionStatus.Disconnected;
        this._clientConnectionType = ClientConnectionType.NotDefined;
        this.emitEvent(WebSocketClient.CONNECTION_CLOSED, []);
    }
    //#endregion

    /**
     * Called when a new message was received from the server.
     * @param evt
     * @private
     */
    _onMessage(evt) {
        this.emitEvent(WebSocketClient.MESSAGE_RECEIVED, [evt.data]);
    }

    /**
     * Handles only one specific message type: "DeviceRegistration".
     * @param {JsonRpcMessage} message - The message send over the connection.
     */
    HandleRegistration(message) {
        let ccType = message.Data["clientConnectionType"];
        if (ccType === undefined || ccType === null || ccType === ClientConnectionType.NotDefined)
            return;

        this._clientConnectionType = ccType;
        this._connectionName = message.Data["name"];
        this._status = WsConnectionStatus.Registered;

        this.emitEvent(WebSocketClient.REGISTERED, []);
    }

    /**
     * Sends a message to the server.
     * @param {JsonRpcMessage} message - The message to send.
     */
    Send(message) {
        if (this._webSocket === undefined)
            return;

        let stringMessage = JSON.stringify(message.ToJsonDict());
        this._webSocket.send(stringMessage);

        this.emitEvent(WebSocketClient.MESSAGE_SEND, [stringMessage]);
    }
}

export {
    WebSocketClient,
    WsConnectionStatus
}