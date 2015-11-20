"use strict";

var $k = $k || {
    Light: function (watts) {
        function LightClass(watts) {
            this.maxInputInWatts = watts;
            this.brightnessLevel = 0;
        }

        var currentWatts = 0;
        var brightnessStepSize = 2;
        LightClass.prototype.IncreaseBrightness = function () {
            if ((this.maxInputInWatts - currentWatts) >= brightnessStepSize) {
                currentWatts += 1;
                this.brightnessLevel += brightnessStepSize;
                return true; // success
            }
            else {
                return false; // fail
            }
        }

        return new LightClass(watts);
    }
};

// The key here is window.setInterval and setTimeout
(function Main() {
    var myLight = new $k.Light(10);
    var intervalId = window.setInterval(TurnUpLight, 1000);

    function TurnUpLight() {
        if (myLight.IncreaseBrightness())
            console.log(myLight.brightnessLevel);
        else
            window.clearInterval(intervalId);
    }
}());