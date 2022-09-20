import {BaseManagerS1} from "./baseManagerS1.js";
import {JsonRpcHandler} from "../networking/eventHandling/jsonRpcHandler.js";
import {JsonRpcMessage} from "../networking/eventHandling/jsonRpcMessage.js";
import {ClientConnectionType} from "../networking/webSocket/clientConnectionType.js";

class ExperimenterManagerS1 extends BaseManagerS1 {
    //#region Sliders
    _sliderUpdate(index, values, handle) {
        this._sliderValues = {
            "distance":         parseFloat(this._sliders["distance-slider"].get()),
            "heightAddition":   parseFloat(this._sliders["height-slider"].get()),
            "posX":             parseFloat(this._sliders["posX-slider"].get()),
            "egoRotation":      parseFloat(this._sliders["egoRotation-slider"].get()),
            "tilt":             parseFloat(this._sliders["tilt-slider"].get()),
            "yaw":              parseFloat(this._sliders["yaw-slider"].get()),
            "size":            parseFloat(this._sliders["scale-slider"].get()),
        }

        super._sliderUpdate(index, values, handle);
    }

    _sliderValueLabelUpdate() {
        this._sliderValueLabelSelections["distance-value"].text(this._sliders["distance-slider"].get());
        this._sliderValueLabelSelections["height-value"].text(this._sliders["height-slider"].get());
        this._sliderValueLabelSelections["posX-value"].text(this._sliders["posX-slider"].get());
        this._sliderValueLabelSelections["egoRotation-value"].text(this._sliders["egoRotation-slider"].get());
        this._sliderValueLabelSelections["tilt-value"].text(this._sliders["tilt-slider"].get());
        this._sliderValueLabelSelections["yaw-value"].text(this._sliders["yaw-slider"].get());
        this._sliderValueLabelSelections["scale-value"].text(this._sliders["scale-slider"].get());

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
        this._sliders["height-slider"].set(values["heightAddition"]);
        this._sliders["posX-slider"].set(values["posX"]);
        this._sliders["egoRotation-slider"].set(values["egoRotation"]);
        this._sliders["tilt-slider"].set(values["tilt"]);
        this._sliders["yaw-slider"].set(values["rotation"]);
        this._sliders["scale-slider"].set(values["scale"]);

        this._blockUpdateMessage = false;
        super._setSliders(values);
    }
    //#endregion

    //#region Buttons
    _fillBtnGroups() {
        super._fillBtnGroups();
        this._btnToGroups = {...this._btnToGroups, ...{
            "btn-exocentric": "referenceFrame",
            "btn-egocentric": "referenceFrame",
            "btn-scene1": "scene",
            "btn-scene2": "scene",
            "btn-scene3": "scene",
        }};

        this._btnGroupsToBtn = {...this._btnGroupsToBtn, ...{
            "referenceFrame": [
                "btn-exocentric",
                "btn-egocentric",
            ],
            "scene": [
                "btn-scene1",
                "btn-scene2",
                "btn-scene3",
            ]
        }};
    }

    _onOtherBtnClick(btnId) {
        super._onOtherBtnClick(btnId);

        if (btnId in this._btnToGroups)
            return;

        this._btnSelections[btnId].classed("toggled", !this._btnSelections[btnId].classed("toggled"))

        if (btnId === "btn-locking") {
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    data: {},
                    method: "LockCoordinateSystem",
                }),
                ccType: ClientConnectionType.HoloLens
            });
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    data: {
                        log: `The Coordinate System on the HoloLens was locked: ${this._btnSelections[btnId].classed("toggled")}`,
                    },
                    method: "LoggingGeneral",
                }),
                ccType: ClientConnectionType.Server
            });
        }
        else if (btnId === "btn-blocking") {
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    data: {},
                    method: "BlockUserInput"
                }),
                ccType: ClientConnectionType.MobileDevice
            });
            JsonRpcHandler.Instance.SendNotification({
                message: new JsonRpcMessage({
                    data: {
                        log: `The UI for the participant was blocked: ${this._btnSelections[btnId].classed("toggled")}`,
                    },
                    method: "LoggingGeneral",
                }),
                ccType: ClientConnectionType.Server
            });
        }
    }

    //#endregion
}

export {
    ExperimenterManagerS1
}