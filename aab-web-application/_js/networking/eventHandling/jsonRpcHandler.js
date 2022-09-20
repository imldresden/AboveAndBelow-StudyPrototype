import {JsonRpcMessage, JsonRpcMessageType} from "./jsonRpcMessage.js";
import { JsonRpcEvent } from "./jsonRpcEvent.js";
import { WebSocketClient, WsConnectionStatus } from "../webSocket/webClient.js";
import { ClientConnectionType } from "../webSocket/clientConnectionType.js";
import { JsonRpcCommunication, JsonRpcMessageDirection } from "./jsonRpcCommunication.js";

/**
 *
 */
class JsonRpcHandler extends EventEmitter {
    //#region Singleton
    /**
     * @type JsonRpcHandler
     * @private
     * @static
     */
    static _instance;
    /**
     * @return {JsonRpcHandler}
     * @static
     */
    static get Instance() {
        if (JsonRpcHandler._instance === undefined)
            new JsonRpcHandler();
        return JsonRpcHandler._instance;
    }

    constructor() {
        super();

        if (!JsonRpcHandler._instance) {
            this._constructor();
            JsonRpcHandler._instance = this;
        }
        return JsonRpcHandler._instance;
    }

    _constructor() {
        for (let rpcEvent in JsonRpcEvent) {
            this._notificationDict[rpcEvent] = [];
            this._requestDict[rpcEvent] = [];
            this._answerDict[rpcEvent] = [];
            this._oneTimeNotification[rpcEvent] = [];
            this._oneTimeRequests[rpcEvent] = [];
            this._oneTimeAnswers[rpcEvent] = [];
        }
        this._messageReceivedLambda = (m) => this._webSocketClient_MessageReceived(m);
    }
    //#endregion

    //#region Events
    static MESSAGE_RECEIVED = "MessageReceived";
    static MESSAGE_SEND = "MessageSend";
    //#endregion

    //#region Private fields
    /**
     * @type {{string:function[]}}
     * @private
     */
    _notificationDict = {};
    /**
     * @type {{string:function[]}}
     * @private
     */
    _requestDict = {};
    /**
     * @type {{string:function[]}}
     * @private
     */
    _answerDict = {};
    /**
     * @type {{string:function[]}}
     * @private
     */
    _oneTimeNotification = {};
    /**
     * @type {{string:function[]}}
     * @private
     */
    _oneTimeRequests = {};
    /**
     * @type {{string:function[]}}
     * @private
     */
    _oneTimeAnswers = {};

    _messageReceivedLambda = undefined;
    //#endregion

    //#region Properties
    /**
     * @type {number}
     * @private
     */
    _currentId = 0;
    /**
     * @return {number}
     */
    get NextId() { return this._currentId++; }

    /**
     * @return {string[]}
     */
    get RegisteredRpcEvents() { return Object.keys(this._notificationDict).concat(Object.keys(this._requestDict)).concat(Object.keys(this._answerDict)); }
    /**
     * @return {string[]}
     */
    get RegisteredNotificationRpcEvents() { return Object.keys(this._notificationDict); }
    /**
     * @return {string[]}
     */
    get RegisteredRequestRpcEvents() { return Object.keys(this._requestDict); }
    /**
     * @return {string[]}
     */
    get RegisteredAnswerRpcEvents() { return Object.keys(this._answerDict); }
    //#endregion

    //#region WebSocket Connections
    ConnectToWebSocketClient() {
        WebSocketClient.Instance.on(WebSocketClient.MESSAGE_RECEIVED, this._messageReceivedLambda)
    }

    DisconnectFromWebSocketClient() {
        WebSocketClient.Instance.off(WebSocketClient.MESSAGE_RECEIVED, this._messageReceivedLambda)
    }
    //#endregion

    //#region Handle Messages
    /**
     * Called when the websocket server received a message.
     * @param {string} message - The message that the client received.
     * @private
     */
    _webSocketClient_MessageReceived(message) {
        let jsonMessage = JsonRpcMessage.DeserializeStringMessage(message);
        if (jsonMessage === null || jsonMessage === undefined)
            return;

        this.emitEvent(JsonRpcHandler.MESSAGE_RECEIVED, [jsonMessage, jsonMessage.Communication.Name]);

        if (WebSocketClient.Instance.Status !== WsConnectionStatus.Registered) {
            if (jsonMessage.MessageType !== JsonRpcMessageType.Request)
                return;
            if (jsonMessage.Method !== JsonRpcEvent.DeviceRegistration)
                return;

            WebSocketClient.Instance.HandleRegistration(jsonMessage);
            if (WebSocketClient.Instance.Status !== WsConnectionStatus.Registered)
                return;
        }

        switch (jsonMessage.MessageType) {
            case JsonRpcMessageType.Notification:   this._handleNotification(jsonMessage);  break;
            case JsonRpcMessageType.Request:        this._handleRequest(jsonMessage);       break;
            case JsonRpcMessageType.Answer:         this._handleAnswer(jsonMessage);        break;
        }
    }

    /**
     * Handles a newly gotten notification.
     * @param {JsonRpcMessage} message - The notification to handle.
     * @private
     */
    _handleNotification(message) {
        if (!(message.Method in this._notificationDict))
            return;

        for (let notificationCall of this._notificationDict[message.Method]) {
            notificationCall(message);

            if (message.Method in this._oneTimeNotification && notificationCall in this._oneTimeNotification[message.Method]){
                this._notificationDict[message.Method] = this._notificationDict[message.Method].filter(rc => rc !== notificationCall);
                this._oneTimeNotification[message.Method] = this._oneTimeNotification[message.Method].filter(rc => rc !== notificationCall);
            }
        }
    }

    /**
     * Handles a newly gotten request.
     * @param {JsonRpcMessage} message - The request to handle.
     * @private
     */
    _handleRequest(message) {
        if (!(message.Method in this._requestDict))
            return;

        for (let requestCall of this._requestDict[message.Method]) {
            requestCall(message);

            if (message.Method in this._oneTimeRequests && requestCall in this._oneTimeRequests[message.Method]){
                this._requestDict[message.Method] = this._requestDict[message.Method].filter(rc => rc !== requestCall);
                this._oneTimeRequests[message.Method] = this._oneTimeRequests[message.Method].filter(rc => rc !== requestCall);
            }
        }
    }

    /**
     * Handles a newly gotten answer.
     * @param {JsonRpcMessage} message - The answer to handle.
     * @private
     */
    _handleAnswer(message) {
        if (!(message.Method in this._answerDict))
            return;

        for (let answerCall of this._answerDict[message.Method]) {
            answerCall(message);

            if (message.Method in this._oneTimeAnswers && answerCall in this._oneTimeAnswers[message.Method]){
                this._answerDict[message.Method] = this._answerDict[message.Method].filter(ac => ac !== answerCall);
                this._oneTimeAnswers[message.Method] = this._oneTimeAnswers[message.Method].filter(ac => ac !== answerCall);
            }
        }
    }
    //#endregion

    //#region Add Listener Handling
    /**
     * Adds a new RPC Notification delegate to the given method names.
     * If the exact same method was already added, it will do nothing.
     * @param {string} rpcEvent - The name of the notification method.
     * @param {function} notificationCall - The method/delegate for the given method name.
     * @param {boolean} oneTime - Should this event only be called one time with the next event?
     */
    AddNotificationListener({rpcEvent, notificationCall, oneTime = false}) {
        this._addListener({
            methodDict: this._notificationDict,
            oneTimeDict: this._oneTimeNotification,
            rpcEvent: rpcEvent,
            method: notificationCall,
            oneTime: oneTime
        });
    }

    /**
     * Adds a new RPC Request delegate to the given method names.
     * If the exact same method was already added, it will do nothing.
     * @param {string} rpcEvent - The name of the request method.
     * @param {function} requestCall - The method/delegate for the given method name.
     * @param {boolean} oneTime - Should this event only be called one time with the next event?
     */
    AddRequestListener({rpcEvent, requestCall, oneTime = false}) {
        this._addListener({
            methodDict: this._requestDict,
            oneTimeDict: this._oneTimeRequests,
            rpcEvent: rpcEvent,
            method: requestCall,
            oneTime: oneTime
        });
    }

    /**
     * Adds a new RPC Answer delegate to the given method names.
     * If the exact same method was already added, it will do nothing.
     * @param {string} rpcEvent - The name of the answer method.
     * @param {function} answerCall - The method/delegate for the given method name.
     * @param {boolean} oneTime - Should this event only be called one time with the next event?
     */
    AddAnswerListener({rpcEvent, answerCall, oneTime = false}) {
        this._addListener({
            methodDict: this._answerDict,
            oneTimeDict: this._oneTimeAnswers,
            rpcEvent: rpcEvent,
            method: answerCall,
            oneTime: oneTime
        });
    }

    /**
     * Adds the given method delegate to the given dicts.
     * If the exact same method was already added, it will do nothing.
     * @param {{string:function}} methodDict - The dict where all registered delegates are stored.
     * @param {{string:function}} oneTimeDict - The dict that defines if a delegate should only be fired one time.
     * @param {string} rpcEvent - The name of the answer method.
     * @param {function} method - The method/delegate for the given method name.
     * @param {boolean} oneTime - Should this event only be called one time with the next event?
     */
    _addListener({methodDict, oneTimeDict, rpcEvent, method, oneTime = false}) {
        if (!(rpcEvent in methodDict)) {
            methodDict[rpcEvent] = [];
            oneTimeDict[rpcEvent] = [];
        }

        if (method in methodDict[rpcEvent])
            return;

        if (oneTime)
            oneTimeDict[rpcEvent].push(method);
        methodDict[rpcEvent].push(method);
    }
    //#endregion

    //#region Remove Listener Handling
    /**
     * Removes the given notification method from the given event.
     * @param {string} rpcEvent - The event the method should be removed from.
     * @param {function} notificationCall - The method to remove.
     */
    RemoveNotificationListener({rpcEvent, requestCall: notificationCall}) {
        this._removeListener({
            methodDict: this._notificationDict,
            oneTimeDict: this._oneTimeNotification,
            rpcEvent: rpcEvent,
            method: notificationCall
        });
    }

    /**
     * Removes the given request method from the given event.
     * @param {string} rpcEvent - The event the method should be removed from.
     * @param {function} requestCall - The method to remove.
     */
    RemoveRequestListener({rpcEvent, requestCall}) {
        this._removeListener({
            methodDict: this._requestDict,
            oneTimeDict: this._oneTimeRequests,
            rpcEvent: rpcEvent,
            method: requestCall
        });
    }

    /**
     * Removes the given answer method from the given event.
     * @param {string} rpcEvent - The event the method should be removed from.
     * @param {function} answerCall - The method to remove.
     */
    RemoveAnswerListener({rpcEvent, answerCall}) {
        this._removeListener({
            methodDict: this._answerDict,
            oneTimeDict: this._oneTimeAnswers,
            rpcEvent: rpcEvent,
            method: answerCall
        });
    }

    /**
     * Removes the given method delegate from the given dicts.
     * @param {{string:function}} methodDict - The dict where all registered delegates are stored.
     * @param {{string:function}} oneTimeDict - The dict that defines if a delegate should only be fired one time.
     * @param {string} rpcEvent - The name of the answer method.
     * @param {function} method - The method/delegate for the given method name.
     */
    _removeListener({methodDict, oneTimeDict, rpcEvent, method}) {
        if (!(rpcEvent in methodDict))
            return;
        if (!(method in methodDict[rpcEvent]))
            return;

        if (method in oneTimeDict[rpcEvent])
            oneTimeDict[rpcEvent] = oneTimeDict[rpcEvent].filter(ac => ac !== method);
        methodDict[rpcEvent] = methodDict[rpcEvent].filter(ac => ac !== method);
    }
    //#endregion

    //#region Send Methods
    /**
     * Sends a new notification message over the websocket.
     * @param {JsonRpcMessage} message - The json rpc message to send.
     * @param {ClientConnectionType|null} ccType - The connection type to send this message to.
     * @param {string|null} connectionName - The connection name to send this message to.
     */
    SendNotification({message, ccType = null, connectionName = null}) {
        if(message === undefined || message === null)
            return;

        message.Communication = JsonRpcCommunication.CommunicationTo({
            name: connectionName,
            ccType: ccType
        });

        WebSocketClient.Instance.Send(message);
        this.emitEvent(JsonRpcHandler.MESSAGE_SEND, [message, connectionName]);
    }

    /**
     * Sends a new request message over the websocket.
     * @param {JsonRpcMessage} message - The json rpc message to send.
     * @param {ClientConnectionType|null} ccType - The connection type to send this message to.
     * @param {string|null} connectionName - The connection name to send this message to.
     */
    SendRequest({message, ccType = null, connectionName = null}) {
        if(message === undefined || message === null)
            return;

        message.Communication = JsonRpcCommunication.CommunicationTo({
            name: connectionName,
            ccType: ccType
        })
        message.Id = this.NextId;

        WebSocketClient.Instance.Send(message);
        this.emitEvent(JsonRpcHandler.MESSAGE_SEND, [message, connectionName]);
    }

    /**
     * Sends a new answer message over the websocket client.
     * @param {JsonRpcMessage} message - The json rpc message to send.
     */
    SendAnswer(message) {
        if(message === undefined || message === null)
            return;

        let newMessage = message.CreateAnswerMessage();
        WebSocketClient.Instance.Send(newMessage);
        this.emitEvent(JsonRpcHandler.MESSAGE_SEND, [message, connectionName]);
    }
    //#endregion
}

export {
    JsonRpcHandler
}
