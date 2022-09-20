import {BaseManagerS2} from "./baseManagerS2.js";
import {JsonRpcHandler} from "../networking/eventHandling/jsonRpcHandler.js";
import {JsonRpcMessage} from "../networking/eventHandling/jsonRpcMessage.js";
import {ClientConnectionType} from "../networking/webSocket/clientConnectionType.js";

class ExperimenterManagerS2 extends BaseManagerS2 {
    //#region properties
    /**
     * @type object
     * @private
     */
    _blockDivSelection;

    /**
     * @type object(string, selection)
     * @protected
     */
    _participantIDSelection;

    /**
     * @type object(string, selection)
     * @protected
     */
    _labelSelections = {};

    /**
     * @type object(string, selection)
     * @protected
     */
    _inputSelection = {};

    /**
     * @type object(string, selection)
     * @protected
     */
    _btnSelections = {};
    //#endregion

    initialize() {
        let that = this;

        d3.selectAll(".values-display")
            .each(function(d, i) {
                let selection = d3.select(this).select("input");
                let index = selection.attr("id");

                that._labelSelections[index] = selection;
            });

        d3.selectAll(".values-input")
            .each(function(d, i) {
                let selection = d3.select(this).select("input");
                let index = selection.attr("id");

                that._inputSelection[index] = selection;
            });

        this._sliderOrientation = "vertical";
        this._clientType = "E";
        super.initialize();

        this._participantIDSelection = d3.select("#participant-id");
        this._blockDivSelection = d3.select(".blocking-div");

        d3.selectAll(".ar-placement-btn")
            .each(function(d, i) {
                let selection = d3.select(this);
                let index = selection.attr("id");

                that._btnSelections[index] = selection;
                selection.on("click", (btnId=index) => that._onBtnClick(btnId));
            });

        JsonRpcHandler.Instance.AddNotificationListener({
           rpcEvent: "NextTrial",
            notificationCall: (m) => this._onNotification_NextTrial(m)
        });
        JsonRpcHandler.Instance.AddNotificationListener({
            rpcEvent: "StateChange",
            notificationCall: (m) => this._onNotification_StateChange(m)
        });
    }

    /**
     * @param {JsonRpcMessage} message
     * @private
     */
    _onNotification_NextTrial(message) {
        this._labelSelections["label-part"].node().value = message.Data["part"];
        this._labelSelections["label-trial"].node().value = message.Data["trial"];
        this._labelSelections["label-area"].node().value = message.Data["area"];
        this._labelSelections["label-content"].node().value = message.Data["content"];
        this._setSliders(message.Data);
    }

    /**
     * @param {JsonRpcMessage} message
     * @private
     */
    _onNotification_StateChange(message) {
        this._labelSelections["label-state"].node().value = message.Data["newState"];
    }

    //#region Sliders
    _sliderUpdate(index, values, handle) {
        this._sliderValues = {
            "distance":         parseFloat(this._sliders["distance-slider"].get()),
            "tilt":             parseFloat(this._sliders["tilt-slider"].get()),
            "size":             parseFloat(this._sliders["scale-slider"].get()),
        }

        super._sliderUpdate(index, values, handle);
        this._sliderValueLabelUpdate();
    }

    _sliderValueLabelUpdate() {
        this._labelSelections["label-distance"].node().value = this._sliders["distance-slider"].get();
        this._labelSelections["label-tilt"].node().value = this._sliders["tilt-slider"].get();
        this._labelSelections["label-size"].node().value = this._sliders["scale-slider"].get();
    }

    /**
     *
     * @param {{distance: number, tilt: number, size: number}} values
     * @private
     */
    _setSliders(values) {
        this._blockUpdateMessage = true;

        this._sliders["distance-slider"].set(values["distance"]);
        this._sliders["tilt-slider"].set(values["tilt"]);
        this._sliders["scale-slider"].set(values["size"]);

        this._blockUpdateMessage = false;
        super._setSliders(values);
    }
    //#endregion

    _onBtnClick(btnId) {
        // Start Button
        if (btnId === "btn-start") {
            this._participantNumber = this._participantIDSelection.node().value;
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    data: {
                        "subjectId": this._participantNumber
                    },
                    method: "StartExperiment",
                })
            });
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    data: {
                        "subjectId": this._participantIDSelection.node().value
                    },
                    method: "StartExperiment",
                }),
                ccType: ClientConnectionType.Server
            });
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    data: {
                        log: `The experiment has started for: ${this._participantIDSelection.node().value}`,
                    },
                    method: `Logging_Input-${this._clientType}-${this._participantNumber}`,
                }),
                ccType: ClientConnectionType.Server
            });

            this._btnSelections["btn-start"].classed("disabled", true);
            this._participantIDSelection.attr("disabled", true);
        }
        else if (btnId === "btn-locking") {
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    data: {},
                    method: "ToggleQRLock",
                }),
                ccType: ClientConnectionType.HoloLens
            });
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    data: {
                        log: `The Lock of the QR Code Tracking was toggled.`,
                    },
                    method: `Logging_Input-${this._clientType}-${this._participantNumber}`,
                }),
                ccType: ClientConnectionType.Server
            });
        }
        else if (btnId === "btn-blocking") {
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    data: {},
                    method: "ToggleBlockUserInput"
                }),
                connectionName: "ParticipantDevice"
            });
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    data: {
                        log: `The UI for the participant was blocked: ${this._btnSelections[btnId].classed("toggled")}`,
                    },
                    method: `Logging_Input-${this._clientType}-${this._participantNumber}`,
                }),
                ccType: ClientConnectionType.Server
            });
        }
        else if (btnId === "btn-nextTrial") {
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    method: "FinishTrial",
                    data: {},
                })
            });
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    data: {
                        log: `The experimenter forwarded the trials.`,
                    },
                    method: `Logging_Input-${this._clientType}-${this._participantNumber}`,
                }),
                ccType: ClientConnectionType.Server
            });
        }
        else if (btnId === "btn-nextState") {
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    method: "NextState",
                    data: {},
                }),
                ccType: ClientConnectionType.HoloLens
            });
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    data: {
                        log: `The experimenter forwarded to the next state.`,
                    },
                    method: `Logging_Input-${this._clientType}-${this._participantNumber}`,
                }),
                ccType: ClientConnectionType.Server
            });
        }
        else if (btnId === "btn-setCoordinateSystem") {
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    method: "SetCoordinateSystem",
                    data: {
                        "ceilingHeight": this._inputSelection["value-ceiling"].node().value,
                        "floorHeight": this._inputSelection["value-floor"].node().value,
                        "translationX": this._inputSelection["value-translationX"].node().value,
                        "translationY": this._inputSelection["value-translationY"].node().value,
                        "translationZ": this._inputSelection["value-translationZ"].node().value,
                        "rotationX": 0,
                        "rotationY": this._inputSelection["value-rotationY"].node().value,
                        "rotationZ": 0,
                    }
                }),
                ccType: ClientConnectionType.HoloLens
            });
        }
    }
}

export {
    ExperimenterManagerS2
}