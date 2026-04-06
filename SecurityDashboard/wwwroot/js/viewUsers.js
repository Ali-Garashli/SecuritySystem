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


// SEARCH BAR
document.addEventListener("DOMContentLoaded", () => {
    let search = document.getElementById("searchBar");

    search.addEventListener("input", async (event) => {
        if (!event.target.value.match(/^[A-Za-z]+$/)) // ignores non alpha characters
            search.value = search.value.replace(/[^a-zA-Z]/g, '');

        let response = await fetch(`/User/FilterUsers?searchTerm=${search.value}`);
        let html = await response.text();

        let parser = new DOMParser();
        let doc = parser.parseFromString(html, "text/html");

        let newUsersList = doc.getElementById("usersList");
        let currentUsersList = document.getElementById("usersList");

        currentUsersList.replaceChildren(...newUsersList.children);
    });
});