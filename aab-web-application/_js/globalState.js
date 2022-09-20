import {JsonRpcHandler} from "./networking/eventHandling/jsonRpcHandler.js";
import {WebSocketClient} from "./networking/webSocket/webClient.js";
import {WSInformationRetriever} from "./networking/utils/webSocketInformationRetriever.js";
import {JsonRpcMessage} from "./networking/eventHandling/jsonRpcMessage.js";
import {ClientConnectionType} from "./networking/webSocket/clientConnectionType.js";
import {JsonRpcEvent} from "./networking/eventHandling/jsonRpcEvent.js";

//#region Enums and Maps
const MAIN_STATES = {
    0: "StandBy",
    1: "DistanceMin",
    2: "DistanceMax",
    3: "TiltMin",
    4: "TiltMax",
    5: "Yaw",
    6: "Scale",
};

const SUB_STATES = {
    0: "StandBy",
    1: "Note",
    2: "ParameterInput",
    3: "Questionnaire",
}

const DEVICE_TYPE = {
    0: "Experimenter",
    1: "Participant",
}

const _STATE_MAP = {
    "StandBy|StandBy":              "start",
    "DistanceMin|ParameterInput":   "placementSlider",
}

//#endregion


class GlobalState extends EventEmitter{
    static get STATE_CHANGE() { return "StateChange"; }
    static get STATE_REPEATED() { return "StateRepeated"; }
    static get WS_CONNECTED() { return "WSConnected"; }

    //#region Singleton
    /**
     * @type GlobalState
     * @private
     * @static
     */
    static _instance;
    /**
     * @return {GlobalState}
     * @static
     */
    static get Instance() {
        if (GlobalState._instance === undefined)
            new GlobalState();
        return GlobalState._instance;
    }

    /***
     * @constructor
     */
    constructor() {
        super();
        if (!GlobalState._instance) {
            this._constructor();
            GlobalState._instance = this;
        }
        return GlobalState._instance;
    }

    /***
     * @constructor
     */
    _constructor() {
        this.defineEvents([
            GlobalState.STATE_CHANGE,
            GlobalState.STATE_REPEATED,
            GlobalState.WS_CONNECTED
        ]);
    }
    //#endregion

    //#region Properties and Fields
    /***
     * @type {string}
     * @private
     */
    _mainState = MAIN_STATES[0];
    /***
     * @type {string}
     * @private
     */
    _subState = SUB_STATES[0];
    /***
     * @type {number}
     * @private
     */
    _stateRepetitionCounter = 0;
    /***
     * @type {string}
     */
    DeviceType = DEVICE_TYPE[0];
    /***
     * @type {boolean}
     * @private
     */
    _webSocketConnected = false;
    //#endregion

    //#region Getter and Setter
    /***
     * @return {string}
     */
    get MainState() {
        return this._mainState;
    }

    /***
     * @param {string} value
     */
    set MainState(value) {
        let same = this._mainState === value;
        this._mainState = value;

        this._emitStateEvent(same);
    }

    /***
     * @return {string}
     */
    get SubState() {
        return this._mainState;
    }

    /***
     * @param {string} value
     */
    set SubState(value) {
        let same = this._subState === value;
        this._subState = value;

        this._emitStateEvent(same);
    }

    /***
     * @return {number}
     */
    get StateRepetitionCounter() {
        return this._stateRepetitionCounter;
    }

    /***
     * @return {boolean}
     */
    get WSConnected() {
        return this._webSocketConnected;
    }
    //#endregion

    /***
     * @param {string} mainState
     * @param {string} subState
     */
    SetCompleteState({mainState, subState}) {
        let same = this._mainState === mainState && this._subState === subState;
        this._mainState = mainState;
        this._subState = subState;

        this._emitStateEvent(same);
    }

    /***
     * @param {boolean} same
     * @private
     */
    _emitStateEvent(same) {
        if (same) {
            this._stateRepetitionCounter += 1;
            this.emitEvent(GlobalState.STATE_REPEATED, []);
        }
        else
            this.emitEvent(GlobalState.STATE_CHANGE, []);

        this._loadInnerContent();
    }

    //#region WebSocket Handling
    ConnectToWebSocket() {
        if (this._webSocketConnected)
            return;

        WSInformationRetriever.ReceiveInformation(() => this._initWebSocket());
    }

    /***
     * @private
     */
    _initWebSocket() {
        WebSocketClient.Instance.on(WebSocketClient.CONNECTION_OPENED, this._ws_Opened_Lambda);
        WebSocketClient.Instance.on(WebSocketClient.CONNECTION_CLOSED, this._ws_Closed_Lambda);

        JsonRpcHandler.Instance.ConnectToWebSocketClient();
        WebSocketClient.Instance.StartClient({
            ip: WSInformationRetriever.Ip,
            port: WSInformationRetriever.Port,
            address: "/JsonRpcService"
        });
    }

    DisconnectFromWebSocket() {
        if (!this._webSocketConnected)
            return;

        WebSocketClient.Instance.off(WebSocketClient.CONNECTION_OPENED, this._ws_Opened_Lambda);
        WebSocketClient.Instance.off(WebSocketClient.CONNECTION_CLOSED, this._ws_Closed_Lambda);

        JsonRpcHandler.Instance.DisconnectFromWebSocketClient();
        WebSocketClient.Instance.StopClient();
    }

    _webSocket_Opened() {
        JsonRpcHandler.Instance.SendRequest({
            message: new JsonRpcMessage({
                data: {
                    "name": `${this.DeviceType}Device`,
                    "clientConnectionType": ClientConnectionType.MobileDevice
                },
                method: JsonRpcEvent.DeviceRegistration
            }),
            ccType: ClientConnectionType.Server
        });

        this._webSocketConnected = true;
        this.emitEvent(GlobalState.WS_CONNECTED, []);
    }
    _ws_Opened_Lambda = () => this._webSocket_Opened();

    _webSocket_Closed() {
        this._webSocketConnected = false;
    }
    _ws_Closed_Lambda = () => this._webSocket_Closed();
    //#endregion

    _loadInnerContent() {
        let key = `${this._mainState}|${this._subState}`
        if (!(key in _STATE_MAP))
            return;

        let url = `/${this.DeviceType.toLowerCase()}/${_STATE_MAP[key]}.html`;
        console.log(`load key: ${key} from: ${url}`)

        $("body").load(url);
    }
}

export {
    MAIN_STATES,
    SUB_STATES,
    DEVICE_TYPE,
    GlobalState
};