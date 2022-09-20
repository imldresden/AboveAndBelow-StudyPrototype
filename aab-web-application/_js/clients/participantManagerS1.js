import {BaseManagerS1} from "./baseManagerS1.js";
import {JsonRpcHandler} from "../networking/eventHandling/jsonRpcHandler.js";

class ParticipantManagerS1 extends BaseManagerS1{
    //#region properties
    /**
     * @type object
     * @private
     */
    _blockDivSelection;
    //#endregion

    constructor() {
        super();

        this._lambdas = {...this._lambdas, ...{
             "BlockUserInput": (m) => this._onNotification_BlockUserInput(m),
        }};
    }

    initialize() {
        this._sliderOrientation = "horizontal";
        super.initialize();

        this._blockDivSelection = d3.select(".blocking-div");

        JsonRpcHandler.Instance.AddNotificationListener({
            rpcEvent: "BlockUserInput",
            notificationCall: this._lambdas["BlockUserInput"]
        })
    }

    /**
     * @param {JsonRpcMessage} message
     * @private
     */
    _onNotification_BlockUserInput(message) {
        this._blockDivSelection.classed("active", !this._blockDivSelection.classed("active"));
    }

    //#region Sliders
    _sliderUpdate(index, values, handle) {
        this._sliderValues = {
            "distance":         parseFloat(this._sliders["distance-slider"].get())
        }

        super._sliderUpdate(index, values, handle);
    }

    _sliderValueLabelUpdate() {
        this._sliderValueLabelSelections["distance-value"].text(this._sliders["distance-slider"].get());

        super._sliderValueLabelUpdate();
    }

    /**
     *
     * @param {{distance: number, height: number, posX: number, tilt: number, yaw: number, scale: number}} values
     * @private
     */
    _setSliders(values) {
        this._blockUpdateMessage = true;

        this._sliders["distance-slider"].set(values["distance"]);

        this._blockUpdateMessage = false;
        super._setSliders(values);
    }
    //#endregion
}

export {
    ParticipantManagerS1
}