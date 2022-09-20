//#region placement slider configs
let distanceSlider = {
    start: [1.5],
    behaviour: "snap",
    orientation: "vertical",
    range: {
        "min": 1,
        "max": 6,
    },
    direction: "rtl",
    tooltips: false,
    pips: {
        mode: "values",
        density: 2,
        values: [1, 2, 3, 4, 5, 6],
        format: wNumb({
            decimals: 0,
            suffix: ' m',
        })
    }
};

let heightSlider = {
    start: [0],
    behaviour: "snap",
    orientation: "vertical",
    range: {
        "min": 0,
        "max": 1,
    },
    tooltips: false,
    pips: {
        mode: "values",
        density: 2,
        values: [0, 0.25, 0.5, 0.75, 1],
        format: wNumb({
            decimals: 2,
            suffix: ' m',
        })
    }
};

let posXSlider = {
    start: [0],
    behaviour: "snap",
    orientation: "vertical",
    range: {
        "min": -2.5,
        "max": 2.5,
    },
    tooltips: false,
    pips: {
        mode: "values",
        density: 2,
        values: [-2.5, -2, -1.5, -1, -0.5, 0, 0.5, 1, 1.5, 2, 2.5],
        format: wNumb({
            decimals: 1,
            suffix: ' m',
        })
    }
};

let egoRotationSlider = {
    start: [0],
    behaviour: "snap",
    orientation: "vertical",
    range: {
        "min": 0,
        "max": 360,
    },
    tooltips: false,
    pips: {
        mode: "values",
        density: 2,
        values: [0, 90, 180, 270, 360],
        format: wNumb({
            decimals: 0,
            suffix: ' °',
        })
    }
};

let tiltSlider = {
    start: [0],
    behaviour: "snap",
    orientation: "vertical",
    range: {
        "min": 0,
        "max": 90,
    },
    tooltips: false,
    pips: {
        mode: "values",
        density: 2,
        values: [0, 15, 30, 45, 60, 75, 90],
        format: wNumb({
            decimals: 0,
            suffix: ' °',
        })
    }
};

let yawSlider = {
    start: [0],
    behaviour: "snap",
    orientation: "vertical",
    range: {
        "min": 0,
        "max": 360,
    },
    tooltips: false,
    pips: {
        mode: "values",
        density: 2,
        values: [0, 90, 180, 270, 360],
        format: wNumb({
            decimals: 0,
            suffix: ' °',
        })
    }
};

let scaleSlider = {
    start: [1],
    behaviour: "snap",
    orientation: "vertical",
    range: {
        "min": 0.2,
        "max": 1.1,
    },
    direction: "rtl",
    tooltips: false,
    pips: {
        mode: "values",
        density: 2,
        values: [0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1, 1.1],
        format: wNumb({
            decimals: 2,
        })
    }
};
//#endregion

class Config {
    /**
     * @type object(string, object)
     * @public
     */
    static PlacementSliderConfigs = {
        "distance-slider":      distanceSlider,
        "height-slider":        heightSlider,
        "posX-slider":          posXSlider,
        "egoRotation-slider":   egoRotationSlider,
        "tilt-slider":          tiltSlider,
        "yaw-slider":           yawSlider,
        "scale-slider":         scaleSlider,
    };
}

export {
    Config
}