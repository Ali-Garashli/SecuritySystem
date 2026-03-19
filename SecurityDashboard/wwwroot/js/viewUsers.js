// DELETE BUTTON
let deleteButtons = document.getElementsByClassName("delete_button");
let dialogWindow = document.getElementById("dialogWindow");
let dialogContainer = document.getElementById("dialogContainer");
let cancelDialogButton = document.getElementById("cancelDialogButton");
let header = document.getElementsByTagName("header");

for (let element of deleteButtons)
    element.addEventListener("click", (event) => {
        event.stopPropagation();
        dialogContainer.style.cssText = "display: flex;\n" +
                                        `height: calc(100vh - ${header[0].clientHeight}px);\n` +
                                        `top: ${header[0].clientHeight}px`;
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

// EDIT BUTTON
let editButtons = document.getElementsByClassName("edit_button");

for (let element of editButtons)
    element.addEventListener("click", (event) => {
        let userInfo = element.parentElement.previousElementSibling.children;

        let buttons = element.parentElement;
        let originalContainer = buttons.previousElementSibling;
        let pp = userInfo[0];
        originalContainer.remove();

        let form = document.createElement("form");
        form.classList.add("user_info");
        
        // add image
        form.appendChild(pp);

        // add image input
        let ppInput = document.createElement("input");
        ppInput.type = "file";
        ppInput.accept = "image/*";
        ppInput.style.cssText = "position: absolute;\n" +
                                "left: 0;\n" +
                                "width: 40px;\n" +
                                "height: 40px;\n" +
                                "z-index: 1;\n" +
                                "opacity: 0;\n" +
                                "cursor: pointer;"
        form.children[0].classList.add("profile_pic_edit_cont");
        form.children[0].firstElementChild.before(ppInput);

        // add name input
        let usernameContainer = document.createElement("div");
        usernameContainer.classList.add("input_container");
        usernameContainer.style.marginBottom = "-8px";

        let usernameLabel = document.createElement("label");
        usernameLabel.htmlFor = "username";
        usernameLabel.innerHTML = "<b>Name: </b>"

        let usernameInput = document.createElement("input");
        usernameInput.type = "text";
        usernameInput.name = "username";
        usernameInput.value = userInfo[0].lastChild.textContent;
        
        usernameContainer.appendChild(usernameLabel);
        usernameContainer.appendChild(usernameInput);
        form.appendChild(usernameContainer);

        // add password input
        let passwordContainer = document.createElement("div");
        passwordContainer.classList.add("input_container");

        let passwordLabel = document.createElement("label");
        passwordLabel.htmlFor = "password";
        passwordLabel.innerHTML = "<b>Password: </b>"
        passwordLabel.style.position = "relative";
        passwordLabel.style.top = "4px";

        let passwordInput = document.createElement("input");
        passwordInput.type = "password";
        passwordInput.name = "password";
        passwordInput.value = userInfo[1].lastChild.textContent;
        
        passwordContainer.appendChild(passwordLabel);
        passwordContainer.appendChild(passwordInput);
        form.appendChild(passwordContainer);

        // add form
        buttons.before(form);

        // replace edit button
        element.style.display = "none";
        
        element.nextElementSibling.style.display = "block";
        element.nextElementSibling.addEventListener("click", (event) => {
            // bring back original container
            originalContainer.children[0].before(pp);
            originalContainer.children[1].lastChild.textContent = usernameInput.value;
            originalContainer.children[2].lastChild.textContent = passwordInput.value;

            form.before(originalContainer);

            // remove form
            form.remove();

            // replace confirm button
            element.nextElementSibling.style.display = "none";
            element.style.display = "block";
        });
    });

// CONFIRM BUTTONS
// let confirmButtons = document.getElementsByClassName("confirm_button");

// for (let element of confirmButtons)
//     element.addEventListener("click", (event) => {


//         // replace confirm button
//         element.previousElementSibling.style.display = "block";
//         element.style.display = "none";
//     });