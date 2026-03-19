let disarmButton = document.getElementById("disarmButton");

let dialogWindow = document.getElementById("dialogWindow");
let dialogContainer = document.getElementById("dialogContainer");

let cancelDialogButton = document.getElementById("cancelDialogButton");

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