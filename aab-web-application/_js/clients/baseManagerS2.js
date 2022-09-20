import { Config } from "../config.js";
import {JsonRpcHandler} from "../networking/eventHandling/jsonRpcHandler.js";
import {JsonRpcMessage} from "../networking/eventHandling/jsonRpcMessage.js";

/**
 * @abstract
 */
class BaseManagerS2 {
    //#region properties
    /**
     * @type string
     * @protected
     */
    _clientType;

    /**
     * @type number
     * @protected
     */
    _participantNumber;

    /**
     * @type object(string, selection)
     * @protected
     */
    _sliderSelections = {};

    /**
     * @type object(string, object)
     * @protected
     */
    _sliders = {};

    /**
     * @type {string}
     * @protected
     */
    _sliderOrientation = "vertical";

    /**
     * @type object(string, object)
     * @protected
     */
    _sliderValueLabelSelections = {}

    /**
     * @type {{distance: number, tilt: number, size: number}}
     * @protected
     */
    _sliderValues = {};

    /**
     * @type {boolean}
     * @protected
     */
    _blockUpdateMessage = true;

    /**
     * @type {{string: function}}
     * @protected
     */
    _lambdas = {};
    //#endregion

    //#region Lifecycle methods
    constructor() {
        this._lambdas = {
            "UpdateParameters": (m) => this._onNotification_UpdateParameters(m)
        }
    }

    /***
     *
     */
    initialize() {
        document.body.addEventListener("unload", () => this._unload());

        this._initSliders();

        JsonRpcHandler.Instance.AddNotificationListener({
            rpcEvent: "UpdateParameters",
            notificationCall: this._lambdas["UpdateParameters"]
        });

        this._blockUpdateMessage = false;
    }

    /***
     * @private
     */
    _unload() {
        JsonRpcHandler.Instance.RemoveNotificationListener({
            rpcEvent: "UpdateParameters",
            notificationCall: this._lambdas["UpdateParameters"]
        });
    }
    //#endregion

    //#region Websocket Events
    /**
     * @param {JsonRpcMessage} message
     * @private
     */
    _onNotification_UpdateParameters(message) {
        this._blockUpdateMessage = true;
        this._setSliders(message.Data);
        this._blockUpdateMessage = false;
    }
    //#endregion

    //#region Sliders
    /**
     * @protected
     */
    _initSliders() {
        this._blockUpdateMessage = true;
        let that = this;

        d3.selectAll(".slider-value-div")
            .each(function(d, i) {
                let selection = d3.select(this);
                let index = selection.attr("id");

                that._sliderValueLabelSelections[index] = selection;
            });

        this._sliderSelections = {};
        this._sliders = {};
        d3.selectAll(".ar-placement-slider")
            .each(function(d, i) {
                let selection = d3.select(this);
                let index = selection.attr("id");
                let config = Object.assign({}, Config.PlacementSliderConfigs[index]);
                config["orientation"] = that._sliderOrientation;

                that._sliderSelections[index] = selection;
                that._sliders[index] = noUiSlider.create(
                    selection.node(),
                    config
                );
            });

        for (let ind in this._sliderSelections)
            this._sliders[ind].on("update", (v, h, u, t, p, s, i = ind) => this._sliderUpdate(i, v, h));

        this._blockUpdateMessage = false;
    }

    /**
     * @param index
     * @param values
     * @param handle
     * @abstract
     * @protected
     */
    _sliderUpdate(index, values, handle) {
        if (this._blockUpdateMessage)
            return;

        JsonRpcHandler.Instance.SendNotification({
            message: new JsonRpcMessage({
                data: {
                    ...this._sliderValues
                },
                method: "UpdateParameters",
                loggingMethod: `Logging_Input-${this._clientType}-${this._participantNumber}`
            }),
        });
    }

    /**
     * @param {{distance: number, tilt: number, size: number}} values
     * @abstract
     * @protected
     */
    _setSliders(values) {
        this._sliderValues = values;
    }
    //#endregion
}

export {
    BaseManagerS2
}