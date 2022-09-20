import {ClientConnectionType} from "../webSocket/clientConnectionType.js";

/**
 * Defines the direction in which a message is send.
 * @type {{from: string, to: string}}
 * @enum
 */
const JsonRpcMessageDirection = {
    "to": "to",
    "from": "from"
}

/**
 * Represnts the json rpcs communication information.
 */
class JsonRpcCommunication {
    //#region Properties
    /**
     * @type string
     * @private
     */
    _direction = undefined;
    /**
     * @return {string}
     */
    get Direction() { return this._direction; }
    /**
     * @type string
     * @private
     */
    _name = undefined;
    /**
     * @return {string}
     */
    get Name() { return this._name; }
    /**
     * @type string
     * @private
     */
    _clientConnectionType = ClientConnectionType.NotDefined;
    /**
     * @return {string}
     */
    get ClientConnectionType() { return this._clientConnectionType; }
    //#endregion

    /**
     * Creates a json rpc communication object based on a given jObject.
     * @param {{direction:string, name:string, clientConnectionType:string}} jsonDict - The json object representation of the data needed for such an object.
     * @return {null|JsonRpcCommunication} - The deserialized object, or null.
     * @static
     */
    static FromDict(jsonDict) {
        if (!(jsonDict.direction in JsonRpcMessageDirection))
            return null;

        let communication = new JsonRpcCommunication();

        communication._direction = jsonDict.direction;
        communication._name = jsonDict.name;
        communication._clientConnectionType = jsonDict.clientConnectionType;

        return communication;
    }

    /**
     * Creates a communication object that has as the direction "to".
     * @param {string} name - The name of the connection to communicate to.
     * @param {string} ccType - The type of the connection to communicate to.
     * @return {JsonRpcCommunication} - A new communication.
     * @static
     */
    static CommunicationTo({name, ccType}) {
        let communication = new JsonRpcCommunication();

        communication._direction = JsonRpcMessageDirection.to;
        communication._name = name;
        communication._clientConnectionType = ccType;

        return communication;
    }

    /**
     * Creates a communication object that has as the direction "from".
     * @param {string} name - The name of the connection this communication comes from..
     * @param {string} ccType - The type of the connection this communication comes from.
     * @return {JsonRpcCommunication} - A new communication.
     * @static
     */
    static CommunicationFrom({name, ccType}) {
        let communication = new JsonRpcCommunication();

        communication._direction = JsonRpcMessageDirection.from;
        communication._name = name;
        communication._clientConnectionType = ccType;

        return communication;
    }

    /**
     * Transforms this communication object into an json dict.
     * @return {{clientConnectionType: string, name: string, direction: string}} - The json dict representation.
     */
    ToJsonDict() {
        return {
            "direction": this._direction,
            "name": this._name,
            "clientConnectionType": this._clientConnectionType
        }
    }
}

export {
    JsonRpcCommunication,
    JsonRpcMessageDirection
}