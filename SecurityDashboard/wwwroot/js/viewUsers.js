// DELETE BUTTON
let deleteButtons = document.getElementsByClassName("delete_link");
let dialogWindow = document.getElementById("dialogWindow");
let dialogContainer = document.getElementById("dialogContainer");
let cancelDialogButton = document.getElementById("cancelDialogButton");
let confirmDialogButton = document.getElementById("confirmDialogButton");
let header = document.getElementsByTagName("header");
let selectedUserId = null;

for (let element of deleteButtons)
    element.addEventListener("click", (event) => {
        event.stopPropagation();
        dialogContainer.style.cssText = "display: flex;\n" +
                                        `height: calc(100vh - ${header[0].clientHeight}px);\n` +
                                        `top: ${header[0].clientHeight}px`;

        selectedUserId = element.getAttribute("data-id");
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

confirmDialogButton.addEventListener("click", () => {
    if (selectedUserId) {
        window.location.href = `/User/Delete/${selectedUserId}`;
        selectedUserId = null;
    }
});