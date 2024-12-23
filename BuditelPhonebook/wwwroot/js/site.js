document.getElementById("roleSelect").addEventListener("change", function () {
    const selectedRole = this.options[this.selectedIndex].text; // Get the selected role text
    const subjectGroup = document.getElementById("subjectGroup");
    const subject = document.getElementById("subject");

    if (selectedRole === "Учител") {
        subjectGroup.style.display = "block";
        subject.style.display = "block";
    } else {
        subjectGroup.style.display = "none";
        subject.style.display = "none";
    }
});