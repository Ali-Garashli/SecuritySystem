const refreshInterval = 1000; // interval in ms for fetching from api
const stateClasses = {
    Safe: "status_safe",
    Unsafe: "status_unsafe",
    Dangerous: "status_dangerous"
};
const sensorNames = ["gas", "flame", "motion"]

// DISARM DIALOG
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

// SENSONRS
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

            // add new status class
            let cssClass = stateClasses[sensor.stateName];
            if (cssClass != null)
                statusElement.classList.add(cssClass);
        }
    }
}

// HISTORY
let historyList = document.getElementById("historyList");
let historyEmpty = document.getElementById("historyEmpty");

// get the reading time of the first list item
let newestTimestamp = historyList?.querySelector("li")
                                 ?.getAttribute("data-time") // data-time is in iso 8601 format
                      ?? null;

function formatDate(string) {
    let date = new Date(string);

    // return data in the default C# DateTime format
    return date.toLocaleString("en-US", {
        month: "numeric",
        day: "numeric",
        year: "numeric",
        hour: "numeric",
        minute: "2-digit",
        second: "2-digit",
        hour12: false
    });
}

function createHistoryItem(item) {
    let li = document.createElement("li");
    li.className = "history_item";

    let reading = item.unit
                    ? `${item.readingValue} ${item.unit}`
                    : `${item.readingValue}`;

    let p_ReadingTime = document.createElement("p");
    p_ReadingTime.innerText = formatDate(item.readingTime);

    let p_SensorType = document.createElement("p");
    p_SensorType.innerText = item.sensorType;

    let p_Reading = document.createElement("p");
    p_Reading.innerText = reading;

    let p_State = document.createElement("p");
    p_State.className = item.stateClass;
    p_State.innerText = item.state;

    li.appendChild(p_ReadingTime);
    li.appendChild(p_SensorType);
    li.appendChild(p_Reading);
    li.appendChild(p_State);

    return li;
}

async function refreshHistory() {
    if (!historyList) return;

    let history;
    try {
        const response = await fetch("/api/sensor/history", {
            method: "GET",
            headers: { "Accept": "application/json" }
        });

        if (!response.ok) {
            console.log(`History poll returned HTTP ${response.status}`);
            return;
        }

        history = await response.json();
    } catch (error) {
        console.log("History poll failed:".toUpperCase(), error);
        return;
    }

    // abort if we don't get the sensor array
    if (!Array.isArray(history)) return;

    // hide the history empty message if a history item is added
    if (historyEmpty) {
        if (history.length == 0)
            historyEmpty.style.display = "";
        else
            historyEmpty.style.display = "none";
    }

    if (history.length == 0) {
        historyList.innerHTML = "";
        newestTimestamp = null;
        return;
    }

    let newItems; // list of history items to load

    // pick only newer items
    if (newestTimestamp != null) {
        newItems = history.filter(item =>
                               new Date(item.readingTime) > new Date(newestTimestamp)
                           );
    }
    else
        newItems = history; // load everything on first load

    if (newItems.length == 0) return;

    // update timestamp
    newestTimestamp = history[0].readingTime;

    // prepend the new items
    for (let item of newItems)
        historyList.prepend(createHistoryItem(item));

    // keep only the newest 50 items
    while (historyList.children.length > 50)
        historyList.removeChild(historyList.lastElementChild);
}

// SYSTEM BANNER
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

// START POLLING
refreshSensors();
refreshHistory();
setInterval(refreshSensors, refreshInterval);
setInterval(refreshHistory, refreshInterval);