import {BaseManagerS2} from "./baseManagerS2.js";
import {JsonRpcHandler} from "../networking/eventHandling/jsonRpcHandler.js";
import {JsonRpcMessage} from "../networking/eventHandling/jsonRpcMessage.js";

class ParticipantManagerS2 extends BaseManagerS2 {
    //#region properties
    /**
     * @type object
     * @private
     */
    _blockDivSelection;

    /**
     * @type object
     * @private
     */
    _controlBtnSelection;

    /**
     * @type object
     * @private
     */
    _leftSliderRow;

    /**
     * @type object
     * @private
     */
    _leftSliderLabel;

    /**
     * @type object
     * @private
     */
    _rightSliderRow;

    /**
     * @type object
     * @private
     */
    _rightSliderLabel;
    //#endregion

    constructor() {
        super();

        this._lambdas = {...this._lambdas, ...{
                "ToggleBlockUserInput": (m) => this._onNotification_ToggleBlockUserInput(m),
                "StateChange": (m) => this._onNotification_StateChange(m),
                "NextTrial": (m) => this._onNotification_NextTrial(m),
                "StartExperiment": (m) => this._onNotification_StartExperiment(m),
            }};
    }

    initialize() {
        this._sliderOrientation = "vertical";
        this._clientType = "P";
        super.initialize();

        this._blockDivSelection = d3.select(".blocking-div");
        this._controlBtnSelection = d3.select("#btn-control");
        this._leftSliderRow = d3.select("#left-slider-row");
        this._rightSliderRow = d3.select("#right-slider-row");
        this._leftSliderLabel = d3.select("#left-slider-label");
        this._rightSliderLabel = d3.select("#right-slider-label");

        this._controlBtnSelection.on("click", () => this._onControlBtnClick());

        JsonRpcHandler.Instance.AddNotificationListener({
            rpcEvent: "ToggleBlockUserInput",
            notificationCall: this._lambdas["ToggleBlockUserInput"]
        });
        JsonRpcHandler.Instance.AddNotificationListener({
            rpcEvent: "StateChange",
            notificationCall: this._lambdas["StateChange"]
        });
        JsonRpcHandler.Instance.AddNotificationListener({
            rpcEvent: "NextTrial",
            notificationCall: this._lambdas["NextTrial"]
        });
        JsonRpcHandler.Instance.AddNotificationListener({
            rpcEvent: "StartExperiment",
            notificationCall: this._lambdas["StartExperiment"]
        });
    }

    /**
     * @param {JsonRpcMessage} message
     * @private
     */
    _onNotification_StartExperiment(message) {
        this._participantNumber = message.Data["subjectId"];
    }

    /**
     * @param {JsonRpcMessage} message
     * @private
     */
    _onNotification_ToggleBlockUserInput(message) {
        this._blockDivSelection.classed("active", !this._blockDivSelection.classed("active"));
    }

    /**
     * @param {JsonRpcMessage} message
     * @private
     */
    _onNotification_StateChange(message) {
        this._leftSliderLabel.text("");
        this._leftSliderRow.select("div").remove();
        this._rightSliderLabel.text("");
        this._rightSliderRow.select("div").remove();

        if (message.Data["newState"] === "Part1" || message.Data["newState"] === "Training1") {
            this._leftSliderLabel.text("Distance");
            this._leftSliderRow.append("div")
                .classed("ar-placement-slider", true)
                .attr("id", "distance-slider");
            this._rightSliderLabel.text("Tilt");
            this._rightSliderRow.append("div")
                .classed("ar-placement-slider", true)
                .classed("mirrored-slider", true)
                .attr("id", "tilt-slider");
        }
        else if (message.Data["newState"] === "Part2" || message.Data["newState"] === "Training2") {
            this._leftSliderLabel.text("Size");
            this._leftSliderRow.append("div")
                .classed("ar-placement-slider", true)
                .attr("id", "scale-slider");
            this._rightSliderLabel.text("Distance");
            this._rightSliderRow.append("div")
                .classed("ar-placement-slider", true)
                .classed("mirrored-slider", true)
                .attr("id", "distance-slider");
        }
        else if (message.Data["newState"] === "Part3") {
            this._leftSliderLabel.text("Tilt");
            this._leftSliderRow.append("div")
                .classed("ar-placement-slider", true)
                .attr("id", "tilt-slider");
            this._rightSliderLabel.text("Size");
            this._rightSliderRow.append("div")
                .classed("ar-placement-slider", true)
                .classed("mirrored-slider", true)
                .attr("id", "scale-slider");
        }

        this._initSliders();
    }

    /**
     * @param {JsonRpcMessage} message
     * @private
     */
    _onNotification_NextTrial(message) {
        this._sliderValues = {
            "distance": message.Data["distance"],
            "tilt": message.Data["tilt"],
            "size": message.Data["size"]
        };

        this._setSliders({...this._sliderValues});
    }

    //#region Sliders
    _sliderUpdate(index, values, handle) {
        if ("distance-slider" in this._sliders)
            this._sliderValues["distance"] = parseFloat(this._sliders["distance-slider"].get());
        if ("tilt-slider" in this._sliders)
            this._sliderValues["tilt"] = parseFloat(this._sliders["tilt-slider"].get());
        if ("scale-slider" in this._sliders)
            this._sliderValues["size"] = parseFloat(this._sliders["scale-slider"].get());

        super._sliderUpdate(index, values, handle);
    }

    /**
     *
     * @param {{distance: number, tilt: number, size: number}} values
     * @private
     */
    _setSliders(values) {
        this._blockUpdateMessage = true;

        if ("distance-slider" in this._sliders)
            this._sliders["distance-slider"].set(values["distance"]);
        if ("tilt-slider" in this._sliders)
            this._sliders["tilt-slider"].set(values["tilt"]);
        if ("scale-slider" in this._sliders)
            this._sliders["scale-slider"].set(values["size"]);

        this._blockUpdateMessage = false;
        super._setSliders(values);
    }
    //#endregion

    _onControlBtnClick() {
        JsonRpcHandler.Instance.SendNotification({
            message: new JsonRpcMessage({
                method: "FinishTrial",
                data: {
                    "info": "Trial finished!"
                },
                loggingMethod: `Logging_Input-${this._clientType}-${this._participantNumber}`
            })
        });
    }
}

export {
    ParticipantManagerS2
}