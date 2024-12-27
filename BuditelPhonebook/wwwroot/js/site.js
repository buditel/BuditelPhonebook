document.addEventListener("DOMContentLoaded", function () {
    const roleSelect = document.getElementById("roleSelect");
    const subjectGroup = document.getElementById("subjectGroup");
    const subject = document.getElementById("subject");

    const subjectGroupInput = document.querySelector('[name="SubjectGroup"]');
    const subjectInput = document.querySelector('[name="Subject"]');

    // Function to update visibility and required attributes
    function updateVisibility() {
        const selectedRole = roleSelect.options[roleSelect.selectedIndex].text;

        if (selectedRole === "Учител") {
            subjectGroup.style.display="block";
            subject.style.display = "block";

            subjectGroupInput.setAttribute("required", "true");
            subjectInput.setAttribute("required", "true");
        } else {
            subjectGroup.style.display = "none";
            subject.style.display = "none";

            subjectGroupInput.removeAttribute("required");
            subjectInput.removeAttribute("required");
        }
    }

    // Initialize visibility on page load (for the first load or after validation errors)
    updateVisibility();

    // Add event listener for role selection change
    roleSelect.addEventListener("change", updateVisibility);
});
