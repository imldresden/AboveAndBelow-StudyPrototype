import {JsonRpcHandler} from "../networking/eventHandling/jsonRpcHandler.js";
import {JsonRpcMessage} from "../networking/eventHandling/jsonRpcMessage.js";
import {ClientConnectionType} from "../networking/webSocket/clientConnectionType.js";

class DebugManager {

    initialize() {
        this._initButtons();
    }

    _initButtons() {
        let that = this;

        d3.selectAll(".debug-btn")
            .each(function(d, i) {
                let selection = d3.select(this);
                let index = selection.attr("id");

                selection.on("click", (btnId=index) => that._onBtnClick(btnId));
            });
    }

    _onBtnClick(btnIndex) {
        let id = btnIndex.split("-")[1];

        JsonRpcHandler.Instance.SendNotification({
            message: new JsonRpcMessage({
                data: {"id": id},
                method: "changeScene"
            }),
            ccType: ClientConnectionType.HoloLens
        });
    }
}

export {
    DebugManager
}