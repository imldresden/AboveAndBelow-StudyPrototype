
/**
 * Defines the possible client connection types.
 * @type {{NotDefined: string, MobileDevice: string, Server: string, Desktop: string, EmulationDevice: string, HoloLens: string, Wearable: string}}
 */
const ClientConnectionType = {
    "NotDefined": "NotDefined",
    "Server": "Server",
    "EmulationDevice": "EmulationDevice",

    "Wearable": "Wearable",
    "HoloLens": "HoloLens",
    "MobileDevice": "MobileDevice",
    "Desktop": "Desktop"
}

export {
    ClientConnectionType
}