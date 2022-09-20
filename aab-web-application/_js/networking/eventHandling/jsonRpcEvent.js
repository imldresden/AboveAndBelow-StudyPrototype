
/**
 * Defines predefined json rpc event types.
 * @type {{OpenConnections: string, OptiTrackMessageRequest: string, DeviceRegistration: string, OptiTrackMessage: string, EmulatedOptiTrackMessage: string}}
 */
const JsonRpcEvent = {
    "EmulatedOptiTrackMessage" : "EmulatedOptiTrackMessage",
    "DeviceRegistration" : "DeviceRegistration",
    "OptiTrackMessageRequest" : "OptiTrackMessageRequest",
    "OptiTrackMessage" : "OptiTrackMessage",
    "OpenConnections" : "OpenConnections"
}

export {
    JsonRpcEvent
}