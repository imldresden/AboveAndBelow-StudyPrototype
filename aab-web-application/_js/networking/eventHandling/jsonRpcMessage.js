import { JsonRpcCommunication } from "./jsonRpcCommunication.js";

/**
 * Defines the possible json rpc message type.
 * @type {{Answer: string, Request: string}}
 * @enum
 */
const JsonRpcMessageType = {
    "Notification": "Notification",
    "Request": "Request",
    "Answer": "Answer"
}

class JsonRpcMessage {
    //#region Properties
    /**
     * @type string
     * @private
     */
    _messageType;
    /**
     * @return {string}
     */
    get MessageType() { return this._messageType };
    /**
     * @type string
     * @private
     */
    _version = "2.0";
    /**
     * @return {string}
     */
    get Version() { return this._version };
    /**
     * @type {null|number}
     * @private
     */
    _id = null;
    /**
     * @return {null|number}
     */
    get Id() { return this._id };

    /**
     * @param {number} value
     */
    set Id(value) {
        if (this._id !== null && this._id !== undefined)
            return;

        this._id = value;
        if (this._id !== null && this._id !== undefined)
            this._messageType = JsonRpcMessageType.Request;
    }
    /**
     * @type object
     * @private
     */
    _data;
    /**
     * @return {object}
     */
    get Data() { return this._data };
    /**
     * @type string
     * @private
     */
    _method;
    /**
     * @return {string}
     */
    get Method() { return this._method };

    /**
     * @type JsonRpcCommunication
     * @private
     */
    _communication;
    /**
     * @return {JsonRpcCommunication}
     */
    get Communication() { return this._communication };

    /**
     * @type string
     * @private
     */
    _loggingMethod;
    /**
     * @return {string}
     */
    get LoggingMethod() { return this._method };

    /**
     * @param {JsonRpcCommunication} value
     */
    set Communication(value) {
        if (this._communication !== undefined)
            return;
        this._communication = value;
    }
    //#endregion

    /**
     * Creates new JsonRpcMessage.
     * @param {object} data - The data this message should transport.
     * @param {string} method - The method this message should call.
     * @param {string | null} loggingMethod - Should this message be logged on the server?
     */
    constructor({data, method, loggingMethod = null}) {
        this._data = data;
        this._method = method;
        this._loggingMethod = loggingMethod;

        this._messageType = JsonRpcMessageType.Notification;
    }

    //#region Deserialization
    /**
     * Creates a message object.
     * @param {string} message - The string message in json format.
     * @return {null|JsonRpcMessage} - The created message. Null if no message could be extracted.
     * @static
     */
    static DeserializeStringMessage(message) {
        try {
            return JsonRpcMessage.DeserializeDictMessage(JSON.parse(message));
        }
        catch (ex) {
            return null;
        }
    }

    /**
     * Creates a message object.
     * @param {object} message - The string message in json format.
     * @return {null|JsonRpcMessage} - The created message. Null if no message could be extracted.
     * @static
     */
    static DeserializeDictMessage(message) {
        try {
            if (message["data"] === null || message["method"] === null || message["_communication"] === null)
                return null;

            let jsonRpcMessage = undefined;
            if ("params" in message) {
                let loggingMethod = null;
                if ("loggingMethod" in message["params"])
                    loggingMethod = message["params"]["loggingMethod"];

                jsonRpcMessage = new JsonRpcMessage({
                    data: message["params"]["data"],
                    method: message["method"],
                    loggingMethod: loggingMethod
                });
                jsonRpcMessage.Communication = JsonRpcCommunication.FromDict(message["params"]["communication"]);
                jsonRpcMessage.Id = "id" in message ? message["id"] : null;
            } else if ("result" in message) {
                let loggingMethod = null;
                if ("loggingMethod" in message["result"])
                    loggingMethod = message["result"]["loggingMethod"];

                jsonRpcMessage = new JsonRpcMessage({
                    data: message["result"]["data"],
                    method: message["result"]["method"],
                    loggingMethod: loggingMethod
                });
                jsonRpcMessage.Communication = JsonRpcCommunication.FromDict(message["result"]["communication"]);
                jsonRpcMessage.Id = "id" in message ? message["id"] : null;
            }

            if (jsonRpcMessage === undefined)
                return null;
            else
                return jsonRpcMessage;
        }
        catch (ex) {
            return null;
        }
    }
    //#endregion

    /**
     * Creates a new answer based on this message.
     * @param {object} data - The data for the answer.
     * @return {JsonRpcMessage|null} - The new message.
     */
    CreateAnswerMessage(data = null) {
        if (this._id === null)
            return null;

        let newMessage = new JsonRpcMessage({
            data: data !== null ? data : this._data,
            method: this._method
        });
        newMessage.Communication = JsonRpcCommunication.CommunicationTo({
            name: this._communication.Name,
            ccType: this._communication.ClientConnectionType
        });
        newMessage._messageType = JsonRpcMessageType.Answer;
        newMessage.Id = this._id;

        return newMessage;
    }

    //#region Json string casting
    /**
     * Transforms this message object into an json dict.
     * @return {null|object} - The JObject representation.
     */
    ToJsonDict() {
        switch (this._messageType) {
            case JsonRpcMessageType.Notification:   return this._toJsonDict_Notification();
            case JsonRpcMessageType.Request:        return this._toJsonDict_Request();
            case JsonRpcMessageType.Answer:         return this._toJsonDict_Answer();
            default:                                return null;
        }
    }
    /**
     * Transforms this message into a notification message json dict.
     * @return {null|object} - The JObject representation.
     */
    _toJsonDict_Notification() {
        return {
            "jsonrpc": this._version,
            "method": this._method,
            "params": {
                "data": this._data,
                "communication": this._communication.ToJsonDict(),
                "loggingMethod": this._loggingMethod,
            }
        };
    }

    /**
     * Transforms this message into a request message json dict.
     * @return {null|object} - The JObject representation.
     */
    _toJsonDict_Request() {
        return {
            "jsonrpc": this._version,
            "id": this._id,
            "method": this._method,
            "params": {
                "data": this._data,
                "communication": this._communication.ToJsonDict(),
                "loggingMethod": this._loggingMethod,
            }
        };
    }

    /**
     * Transforms this message into a answer message json dict.
     * @return {null|object} - The JObject representation.
     */
    _toJsonDict_Answer() {
        return {
            "jsonrpc": this._version,
            "id": this._id,
            "result": {
                "method": this._method,
                "data": this._data,
                "communication": this._communication.ToJsonDict(),
                "loggingMethod": this._loggingMethod,
            }
        };
    }
    //#endregion
}

export {
    JsonRpcMessage,
    JsonRpcMessageType
}
