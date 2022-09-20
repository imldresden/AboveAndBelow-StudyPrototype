import { Config } from "../config.js";
import {JsonRpcHandler} from "../networking/eventHandling/jsonRpcHandler.js";
import {JsonRpcMessage} from "../networking/eventHandling/jsonRpcMessage.js";

/**
 * @abstract
 */
class BaseManagerS1 {
    //#region properties
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
     * @type {{distance: number, height: number, posX: number, angle: number, tilt: number, yaw: number, scale: number}}
     * @protected
     */
    _sliderValues = {};

    /**
     * @type object(string, string)
     * @protected
     */
    _btnToGroups = {};

    /**
     * @type object(string, [string])
     * @protected
     */
    _btnGroupsToBtn = {}

    /**
     * @type object(string, selection)
     * @protected
     */
    _btnSelections = {};

    /**
     * @type {{placementArea: string, referenceFrame: string, contentType: string}}
     * @protected
     */
    _sceneValues = {};

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
            "PropertyUpdateEvent": (m) => this._onNotification_PropertyUpdateEvent(m),
            "SceneUpdateEvent": (m) => this._onNotification_SceneUpdateEvent(m),
        }
    }

    /***
     *
     */
    initialize() {
        document.body.addEventListener("unload", () => this._unload());

        this._initSliders();
        this._initButtons();

        JsonRpcHandler.Instance.AddNotificationListener({
            rpcEvent: "PropertyUpdateEvent",
            notificationCall: this._lambdas["PropertyUpdateEvent"]
        });
        JsonRpcHandler.Instance.AddNotificationListener({
            rpcEvent: "SceneUpdateEvent",
            notificationCall: this._lambdas["SceneUpdateEvent"]
        });

        this._blockUpdateMessage = false;
    }

    /***
     * @private
     */
    _unload() {
        JsonRpcHandler.Instance.RemoveNotificationListener({
            rpcEvent: "PropertyUpdateEvent",
            notificationCall: this._lambdas["PropertyUpdateEvent"]
        });
        JsonRpcHandler.Instance.RemoveNotificationListener({
            rpcEvent: "SceneUpdateEvent",
            notificationCall: this._lambdas["SceneUpdateEvent"]
        });
    }
    //#endregion

    //#region Websocket Events
    /**
     * @param {JsonRpcMessage} message
     * @private
     */
    _onNotification_PropertyUpdateEvent(message) {
        this._setSliders(message.Data);
    }

    /**
     * @param {JsonRpcMessage} message
     * @private
     */
    _onNotification_SceneUpdateEvent(message) {
        this._setBtns(message.Data);
    }
    //#endregion

    //#region Sliders
    /**
     * @protected
     */
    _initSliders() {
        let that = this;

        d3.selectAll(".slider-value-div")
            .each(function(d, i) {
                let selection = d3.select(this);
                let index = selection.attr("id");

                that._sliderValueLabelSelections[index] = selection;
            });

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

        for (let index in this._sliderSelections)
            this._sliders[index].on("update", (v, h, i = index) => this._sliderUpdate(i, v, h));

        this._sliderValueLabelUpdate();
    }

    /**
     * @param index
     * @param values
     * @param handle
     * @abstract
     * @protected
     */
    _sliderUpdate(index, values, handle) {
        this._sliderValueLabelUpdate();

        if (this._blockUpdateMessage)
            return;

        JsonRpcHandler.Instance.SendNotification({
            message: new JsonRpcMessage({
                data: {
                    "id": this._sceneValues.contentType.slice(-1),
                    ...this._sliderValues
                },
                method: "UpdateParametersS1",
                loggingMethod: "LoggingGeneral"
            }),
            // ccType: ClientConnectionType.HoloLens
        });
    }

    /**
     * @abstract
     * @protected
     */
    _sliderValueLabelUpdate() {
    }

    /**
     * @param {{distance: number, height: number, posX: number, tilt: number, yaw: number, scale: number}} values
     * @abstract
     * @protected
     */
    _setSliders(values) {
        this._sliderValues = values;

        this._sliderValueLabelUpdate();
    }
    //#endregion

    //#region Buttons
    /**
     * @protected
     */
    _initButtons() {
        let that = this;

        that._fillBtnGroups();

        d3.selectAll(".ar-placement-btn")
            .each(function(d, i) {
                let selection = d3.select(this);
                let index = selection.attr("id");

                that._btnSelections[index] = selection;

                if (index in that._btnToGroups)
                    selection.on("click", (btnId=index) => that._onNormalBtnClick(btnId));
                else
                    selection.on("click", (btnId=index) => that._onOtherBtnClick(btnId));

                if (selection.classed("active"))
                    that._sceneValues[that._btnToGroups[index]] = index.split("-")[1];
            });
    }

    /**
     * @protected
     */
    _fillBtnGroups() {
        this._btnToGroups = {
            "btn-ceiling": "placementArea",
            "btn-floor": "placementArea",
            "btn-content1": "contentType",
            "btn-content2": "contentType",
            "btn-content3": "contentType",
            "btn-content4": "contentType",
            "btn-content5": "contentType",
            "btn-content6": "contentType",
        };

        this._btnGroupsToBtn = {
            "placementArea": [
                "btn-ceiling",
                "btn-floor",
            ],
            "contentType": [
                "btn-content1",
                "btn-content2",
                "btn-content3",
                "btn-content4",
                "btn-content5",
                "btn-content6",
            ]
        }
    }

    /**
     * @param {string} btnId
     * @private
     */
    _onNormalBtnClick(btnId) {
        if (!(btnId in this._btnToGroups))
            return;

        let btnGroup = this._btnToGroups[btnId];
        for (let otherBtnId of this._btnGroupsToBtn[btnGroup])
            this._btnSelections[otherBtnId].classed("active", btnId === otherBtnId);

        this._sceneValues[btnGroup] = btnId.split("-")[1];

        if (this._blockUpdateMessage)
            return;

        JsonRpcHandler.Instance.SendNotification({
            message: new JsonRpcMessage({
                data: this._sceneValues,
                method: "SceneUpdateEvent",
                loggingMethod: "LoggingGeneral"
            })
        });
    }

    /**
     * @param {string} btnId
     * @protected
     */
    _onOtherBtnClick(btnId) {
    }

    /**
     * @param {{distance: number, height: number, posX: number, angle: number, tilt: number, yaw: number, scale: number}} values
     * @protected
     */
    _setBtns(values) {
       this._blockUpdateMessage = true;

       for (let index in values)
           this._onNormalBtnClick(`btn-${values[index]}`);

       this._blockUpdateMessage = false;
    }
    //#endregion
}

export {
    BaseManagerS1
}