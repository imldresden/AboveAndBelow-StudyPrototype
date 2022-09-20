
class ApiInterface {
    //#region Singleton
    /**
     * @type ApiInterface
     * @private
     * @static
     */
    static _instance;
    /**
     * @return {ApiInterface}
     * @static
     */
    static get Instance() {
        if (ApiInterface._instance === undefined)
            new ApiInterface();
        return ApiInterface._instance;
    }

    constructor() {
        if (!ApiInterface._instance) {
            this._constructor();
            ApiInterface._instance = this;
        }
        return ApiInterface._instance;
    }

    _constructor() {
        this._origin = window.location.origin;
    }
    //#endregion

    /**
     * @type string
     * @private
     */
    _origin;
    /**
     * @return {string}
     * @private
     */
    get _API_PATH() { return "/api" };

    /**
     * Makes a GET request.
     * @param {string} path - The path for this GET request.
     * @param {function} callback - This will be called if the request was successfully finished.
     *                              This method should take to parameters: a string for the type, and an object with the response.
     */
    GetRequest({path, callback}) {
        let request = new XMLHttpRequest();
        request.onreadystatechange = (evt, r = request, cb = callback) => this._onReadyStateChange_GET(r, cb);
        request.open("GET", `${this._origin}${this._API_PATH}${path}`)
        request.send(null);
    }

    /**
     *
     * @param {XMLHttpRequest} request
     * @param {function} callback
     * @private
     */
    _onReadyStateChange_GET(request, callback) {
        if (request.readyState !== XMLHttpRequest.DONE)
            return;

        callback(request.responseType, request.response);
        request.onreadystatechange = null;
    }
}

export {
    ApiInterface
}