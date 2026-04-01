const refreshInterval = 3000; // interval in ms for fetching from api
const stateClasses = {
    Safe: "status_safe",
    Unsafe: "status_unsafe",
    Dangerous: "status_dangerous"
};
const sensorNames = ["gas", "flame", "motion"]

// system disarm dialog
let disarmButton = document.getElementById("disarmButton");

let dialogWindow = document.getElementById("dialogWindow");
let dialogContainer = document.getElementById("dialogContainer");

let cancelDialogButton = document.getElementById("cancelDialogButton");
let confirmDialogButton = document.getElementById("confirmDialogButton");

let header = document.getElementsByTagName("header");

disarmButton.addEventListener("click", (event) => {
    event.stopPropagation();
    dialogContainer.style.cssText = "display: flex;\n" +
        `height: calc(100vh - ${header[0].clientHeight}px);`;
});

cancelDialogButton.addEventListener("click", (event) => {
    dialogContainer.style.cssText = "display: none;";
});

dialogWindow.addEventListener("click", (event) => {
    event.stopPropagation();
});

document.body.addEventListener("click", (event) => {
    dialogContainer.style.cssText = "display: none;";
});

// getting sensor data
async function refreshSensors() {
    let readings;

    try {
        let response = await fetch("/api/sensor/readings", {
            method: "GET",
            headers: { "Accept": "application/json" }
        });

        if (!response.ok) {
            console.log(`Sensor poll returned HTTP ${response.status}`);
            return;
        }

        readings = await response.json();
    } catch (error) {
        console.log("Sensor poll failed:".toUpperCase(), error);
        return;
    }

    // abort if we don't get the sensor array
    if (!Array.isArray(readings)) return;

    for (let sensor of readings) {
        let sensorName = sensorNames[sensor.sensorType];
        if (!sensorName) continue; // skip unkown types

        // update reading value
        let readingElement = document.getElementById(`reading-${sensorName}`);
        if (readingElement != null) {
            let unit = sensor.unit ? `${sensor.unit}` : ``;
            readingElement.textContent = `${sensor.rawValue} ${unit}`;
        }

        // update status
        let statusElement = document.getElementById(`status-${sensorName}`);
        if (statusElement) {
            statusElement.textContent = sensor.stateName;

            // remove any previous status
            statusElement.classList.remove("status_safe", "status_unsafe", "status_dangerous");

            //add new status class
            let cssClass = stateClasses[sensor.stateName];
            if (cssClass != null)
                statusElement.classList.add(cssClass);
        }
    }
}

// dismiss system status popup 
let systemBanner = document.getElementById("systemBanner");
let bannerCloseButton = document.getElementById("bannerCloseButton");

function closeMessage(banner) {
    if (!banner || banner.classList.contains("banner_hiding")) return;

    // mark as hidden to avoid replaying animation
    banner.classList.add("banner_hiding");

    // remove once animation finishes
    setTimeout(() => banner.remove(), 400);
}

if (systemBanner) {
    // close on click
    bannerCloseButton?.addEventListener("click", () =>
        closeMessage(systemBanner)
    );

    // auto close
    setTimeout(() => closeMessage(systemBanner), 5000);
}

// start polling
refreshSensors();
setInterval(refreshSensors, refreshInterval);