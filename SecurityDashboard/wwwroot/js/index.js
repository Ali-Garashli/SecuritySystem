let profileButton = document.getElementById("profileButton");
let profileOptions = document.getElementById("profileOptions");


profileButton.addEventListener("click", (event) => {
    event.stopPropagation();

    if (profileOptions.style.display == "flex")
        profileOptions.style.display = "none";
    else
        profileOptions.style.display = "flex";
});

document.body.addEventListener("click", (event) => {
    if (profileOptions.style.display == "flex" &&
        event.target != profileOptions
    )
        profileOptions.style.display = "none";
});