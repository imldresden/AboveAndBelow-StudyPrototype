import {ApiInterface} from "./apiInterface.js";

class WebSocketInformationRetriever {
    ReceiveInformation(callback) {
        if (this._state === "ready") {
            callback();
            return;
        }

        this._callbacks.push(callback);
        if (this._state !== "standby")
            return;

        this._state = "retrieving";

        ApiInterface.Instance.GetRequest({
            path: "/wsIp",
            callback: (t, r) => this._onGetRequest_Ip(t, r)
        });
        ApiInterface.Instance.GetRequest({
            path: "/wsPort",
            callback: (t, r) => this._onGetRequest_Port(t, r)
        });
    }

    /***
     * @param type
     * @param response
     * @private
     */
    _onGetRequest_Ip(type, response)
    {
        this._ip = String(response).replaceAll("\"", "");
        console.log(`IP  : ${this._ip}`);

        if (this._ip !== undefined && this._port !== undefined)
            this._answerCallbacks();
    }

    /***
     * @param type
     * @param response
     * @private
     */
    _onGetRequest_Port(type, response)
    {
        this._port = response;
        console.log(`PORT: ${this._port}`);

        if (this._ip !== undefined && this._port !== undefined)
            this._answerCallbacks();
    }

    /***
     * @private
     */
    _answerCallbacks() {
        this._state = "ready";

        for (let callback of this._callbacks)
            callback();

        this._callbacks = [];
    }

    get State() {
        return this._state;
    }

    get Ip() {
        return this._ip;
    }

    get Port() {
        return this._port;
    }

    /***
     * @type {function[]}
     * @private
     */
    _callbacks = [];
    /***
     * @type {string}
     * @private
     */
    _state = "standby";
    /***
     * @type {string}
     * @private
     */
    _ip = undefined;
    /***
     * @type {string}
     * @private
     */
    _port = undefined;
}

let WSInformationRetriever = new WebSocketInformationRetriever();

export {
    WSInformationRetriever
};