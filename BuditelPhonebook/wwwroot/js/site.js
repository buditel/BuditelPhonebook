//document.addEventListener("DOMContentLoaded", function () {
//    const roleContainer = document.getElementById("roleContainer");
//    const subjectGroup = document.getElementById("subjectGroup");
//    const subject = document.getElementById("subject");

//    const subjectGroupInput = document.querySelector('[name="SubjectGroup"]');
//    const subjectInput = document.querySelector('[name="Subject"]');

//    if (!roleContainer || !subjectGroup || !subject || !subjectGroupInput || !subjectInput) {
//        console.error("One or more required elements are missing. Check your HTML structure.");
//        return;
//    }

//    function updateVisibility() {
//        const roleSelects = roleContainer.querySelectorAll("select"); // Get all role dropdowns
//        let isTeacherSelected = false;

//        roleSelects.forEach(select => {
//            const selectedText = select.options[select.selectedIndex]?.text.trim() || "";
//            if (selectedText.includes("Учител")) {
//                isTeacherSelected = true;
//            }
//        });

//        if (isTeacherSelected) {
//            subjectGroup.style.display = "block";
//            subject.style.display = "block";
//            subjectGroupInput.setAttribute("required", "true");
//            subjectInput.setAttribute("required", "true");
//        } else {
//            subjectGroup.style.display = "none";
//            subject.style.display = "none";
//            subjectGroupInput.removeAttribute("required");
//            subjectInput.removeAttribute("required");
//        }
//    }

//    // Run on page load (for validation error reloads)
//    updateVisibility();

//    // Attach event listener to all role dropdowns dynamically
//    roleContainer.addEventListener("change", function (event) {
//        if (event.target.tagName === "SELECT") {
//            updateVisibility();
//        }
//    });

//    // Ensure newly added dropdowns trigger visibility update
//    document.getElementById("addRole").addEventListener("click", function () {
//        setTimeout(updateVisibility, 50); // Small delay to ensure DOM updates
//    });
//});


document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("searchInput");
    const searchBtn = document.getElementById("searchBtn");
    const resultsContainer = document.getElementById("results");

    let currentPage = 1;

    // Function to fetch search results with pagination
    const fetchSearchResults = async (query, page = 1) => {
        try {
            const response = await fetch(`/Phonebook/Index?search=${encodeURIComponent(query)}&page=${page}`, {
                headers: { "X-Requested-With": "XMLHttpRequest" },
            });

            if (!response.ok) {
                throw new Error("Failed to fetch results.");
            }

            const partialViewHtml = await response.text();

            if (partialViewHtml.trim()) {
                resultsContainer.innerHTML = partialViewHtml;
                currentPage = page; // Update current page

                window.scrollTo({ top: 0, behavior: "smooth" });
            }
        } catch (error) {
            console.error("Error fetching search results:", error);
            resultsContainer.innerHTML = `<div class="alert alert-danger text-center">Възникна грешка при зареждането на резултатите.</div>`;
        }
    };

    // Event handler for real-time search
    searchInput.addEventListener("input", function () {
        const query = searchInput.value.trim();

        if (query.length > 1) {
            fetchSearchResults(query, 1); // Reset to page 1 for new search
        } else {
            fetchSearchResults("", 1);
        }
    });

    // Event handler for search button
    searchBtn.addEventListener("click", function () {
        const query = searchInput.value.trim();
        fetchSearchResults(query, 1); // Reset to page 1 for search button click
    });

    // Event delegation for pagination links
    resultsContainer.addEventListener("click", function (e) {
        if (e.target.tagName === "A" && e.target.classList.contains("page-link")) {
            e.preventDefault();
            const page = parseInt(e.target.getAttribute("data-page"), 10);
            const query = searchInput.value.trim();
            fetchSearchResults(query, page);
        }
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("searchInputDeleted");
    const searchBtn = document.getElementById("searchBtnDeleted");
    const resultsContainer = document.getElementById("resultsDeleted");

    let currentPage = 1;

    // Function to fetch search results with pagination
    const fetchSearchResults = async (query, page = 1) => {
        try {
            const response = await fetch(`/Admin/DeletedIndex?search=${encodeURIComponent(query)}&page=${page}`, {
                headers: { "X-Requested-With": "XMLHttpRequest" },
            });

            if (!response.ok) {
                throw new Error("Failed to fetch results.");
            }

            const partialViewHtml = await response.text();

            if (partialViewHtml.trim()) {
                resultsContainer.innerHTML = partialViewHtml;
                currentPage = page; // Update current page

                window.scrollTo({ top: 0, behavior: "smooth" });
            }
        } catch (error) {
            console.error("Error fetching search results:", error);
            resultsContainer.innerHTML = `<div class="alert alert-danger text-center">Възникна грешка при зареждането на резултатите.</div>`;
        }
    };

    // Event handler for real-time search
    searchInput.addEventListener("input", function () {
        const query = searchInput.value.trim();

        if (query.length > 1) {
            fetchSearchResults(query, 1); // Reset to page 1 for new search
        } else {
            fetchSearchResults("", 1);
        }
    });

    // Event handler for search button
    searchBtn.addEventListener("click", function () {
        const query = searchInput.value.trim();
        fetchSearchResults(query, 1); // Reset to page 1 for search button click
    });

    // Event delegation for pagination links
    resultsContainer.addEventListener("click", function (e) {
        if (e.target.tagName === "A" && e.target.classList.contains("page-link")) {
            e.preventDefault();
            const page = parseInt(e.target.getAttribute("data-page"), 10);
            const query = searchInput.value.trim();
            fetchSearchResults(query, page);
        }
    });
});




document.addEventListener('DOMContentLoaded', function () {
    var fileInput = document.getElementById('fileInput');
    var fileNameLabel = document.querySelector('.file-name');

    fileInput.addEventListener('change', function () {
        var fileName = fileInput.files[0] ? fileInput.files[0].name : 'Изберете файл (по желание)...';
        fileNameLabel.textContent = fileName;
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const fileInput = document.getElementById("fileInput");
    const previewContainer = document.getElementById("previewContainer");
    const previewImage = document.getElementById("previewImage");
    const existingPictureBase64 = document.getElementById("existingPicture"); // Hidden field holding the existing picture
    const personPictureInput = document.getElementById("PersonPicture"); // The actual file input for new picture

    previewContainer.style.display = 'none';

    fileInput.addEventListener("change", function (event) {
        const file = event.target.files[0];

        if (file) {
            const reader = new FileReader();

            reader.onload = function (e) {
                previewImage.src = e.target.result; // Set the preview image source
                previewContainer.style.display = 'block'; // Show the preview container
            };

            reader.readAsDataURL(file); // Read the file as a Data URL
        } else {
            // Hide the preview container if no file is selected
            previewContainer.style.display = 'none';
            previewImage.src = ''; // Clear the preview image source
        }
    });
});



window.onload = function () {
    const images = document.querySelectorAll('.img-thumbnail');
    images.forEach(img => {
        img.style.width = '150px';
        img.style.height = '150px';
        img.style.objectFit = 'contain';
    });
};

document.addEventListener('DOMContentLoaded', function () {
    flatpickr('.datetime-picker', {
        dateFormat: "d.m.Y."
    });
});


document.addEventListener('DOMContentLoaded', function () {
    // Ensure the customDateContainer is hidden on page load
    const customDateContainer = document.getElementById('customDateContainer');
    if (customDateContainer) {
        customDateContainer.style.display = 'none';
    }

    // Initialize Flatpickr
    flatpickr('#customDate', {
        dateFormat: "d.m.Y.",
    });

    const dateSelect = document.getElementById('dateSelect');
    const selectedDateInput = document.getElementById('SelectedDate');
    const today = new Date();

    // Format the date as "d.m.Y."
    const formatDate = (date) => {
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return `${day}.${month}.${year}.`;
    };

    // Default to today, formatted as "d.m.Y."
    selectedDateInput.value = formatDate(today);

    dateSelect.addEventListener('change', function () {
        if (this.value === 'custom') {
            customDateContainer.style.display = 'block';
        } else {
            customDateContainer.style.display = 'none';
            selectedDateInput.value = formatDate(today); // Set today's date in "d.m.Y." format
        }
    });

    // Update hidden input when a custom date is selected
    const customDateInput = document.getElementById('customDate');
    customDateInput.addEventListener('change', function () {
        selectedDateInput.value = this.value;
    });
});


document.addEventListener('DOMContentLoaded', function () {
    document.getElementById("results").addEventListener("click", function (event) {
        const target = event.target.closest(".see-latest-change");
        if (target) {
            event.preventDefault();
            const personId = target.getAttribute("data-id");
            const modalElement = document.getElementById("latestChangeModal");
            const modalBody = modalElement.querySelector(".modal-content");

            fetch(`/Admin/SeeLatestChange?id=${personId}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error("Грешка при зареждане на последна промяна.");
                    }
                    return response.text();
                })
                .then(html => {
                    modalBody.innerHTML = html;
                    const modal = new bootstrap.Modal(modalElement);
                    modal.show();

                    // Ensure proper cleanup when modal is closed
                    modalElement.addEventListener("hidden.bs.modal", function () {
                        modal.dispose();
                        modalBody.innerHTML = ""; // Clear modal content
                        removeBackdrop(); // Ensure no lingering backdrop
                        restoreBodyStyles(); // Fix scrolling issue
                    }, { once: true });
                })
                .catch(error => {
                    modalBody.innerHTML = `<div class="modal-body text-danger">${error.message}</div>`;
                });
        }
    });
});

// Function to remove any leftover Bootstrap modal backdrops
function removeBackdrop() {
    const backdrops = document.querySelectorAll(".modal-backdrop");
    backdrops.forEach(backdrop => backdrop.remove());
}

// Function to fully restore scrolling behavior
function restoreBodyStyles() {
    document.body.classList.remove("modal-open"); // Ensure modal-open is removed
    document.body.style.overflow = ""; // Restore scrolling
    document.body.style.paddingRight = ""; // Reset padding added by Bootstrap
}






document.addEventListener('DOMContentLoaded', function () {
    const removePictureButton = document.getElementById('removePictureButton');
    const existingPictureContainer = document.getElementById('existingPictureContainer');
    const existingPicture = document.getElementById('existingPicture');

    if (removePictureButton) {
        removePictureButton.addEventListener('click', function () {
            existingPictureContainer.style.display = 'none';
            existingPicture.value = null;
        });
    }
});

document.addEventListener("DOMContentLoaded", function () {
    let container = document.getElementById("departmentContainer");

    function addDepartment() {
        let departmentGroups = container.querySelectorAll(".department-group");
        let index = departmentGroups.length;

        let newGroup = document.createElement("div");
        newGroup.classList.add("department-group", "d-flex", "align-items-center", "mt-2");

        let newSelect = document.createElement("select");
        newSelect.name = `Departments[${index}]`;
        newSelect.classList.add("form-control", "dropdown-select", "me-2");

        let defaultOption = document.createElement("option");
        defaultOption.value = "";
        defaultOption.textContent = "Изберете отдел...";
        newSelect.appendChild(defaultOption);

        let firstSelect = container.querySelector("select");
        if (firstSelect) {
            firstSelect.querySelectorAll("option").forEach(option => {
                if (option.value !== "") {
                    let newOption = document.createElement("option");
                    newOption.value = option.value;
                    newOption.textContent = option.textContent;
                    newSelect.appendChild(newOption);
                }
            });
        }

        let removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.classList.add("btn", "btn-sm", "btn-danger", "ms-2", "remove-department");
        removeButton.textContent = "Премахни";
        removeButton.onclick = function () {
            container.removeChild(newGroup);
            updateDepartmentIndexes();
        };

        newGroup.appendChild(newSelect);
        newGroup.appendChild(removeButton);
        container.appendChild(newGroup);
    }

    document.getElementById("addDepartment").addEventListener("click", addDepartment);

    function attachRemoveEvents() {
        container.querySelectorAll(".remove-department").forEach(button => {
            button.onclick = function () {
                container.removeChild(this.parentElement);
                updateDepartmentIndexes();
            };
        });
    }

    function updateDepartmentIndexes() {
        let selects = container.querySelectorAll(".department-group select");
        selects.forEach((select, index) => {
            select.name = `Departments[${index}]`;
        });

        // Remove any old hidden inputs
        document.querySelectorAll("input[name^='Departments']").forEach(input => input.remove());

        // Add new hidden inputs for each department
        selects.forEach((select, index) => {
            let hiddenInput = document.createElement("input");
            hiddenInput.type = "hidden";
            hiddenInput.name = `Departments[${index}]`;
            hiddenInput.value = select.value;
            document.querySelector("form").appendChild(hiddenInput);
        });
    }

    attachRemoveEvents();

    document.querySelector("form").addEventListener("submit", function () {
        updateDepartmentIndexes();
    });
});


document.addEventListener("DOMContentLoaded", function () {
    let container = document.getElementById("roleContainer");

    function addRole() {
        let roleGroups = container.querySelectorAll(".role-group");
        let index = roleGroups.length;

        let newGroup = document.createElement("div");
        newGroup.classList.add("role-group", "d-flex", "align-items-center", "mt-2");

        let newSelect = document.createElement("select");
        newSelect.name = `Roles[${index}]`;
        newSelect.classList.add("form-control", "dropdown-select", "me-2");

        let defaultOption = document.createElement("option");
        defaultOption.value = "";
        defaultOption.textContent = "Изберете длъжност...";
        newSelect.appendChild(defaultOption);

        let firstSelect = container.querySelector("select");
        if (firstSelect) {
            firstSelect.querySelectorAll("option").forEach(option => {
                if (option.value !== "") {
                    let newOption = document.createElement("option");
                    newOption.value = option.value;
                    newOption.textContent = option.textContent;
                    newSelect.appendChild(newOption);
                }
            });
        }

        let removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.classList.add("btn", "btn-sm", "btn-danger", "ms-2", "remove-role");
        removeButton.textContent = "Премахни";
        removeButton.onclick = function () {
            container.removeChild(newGroup);
            updateRoleIndexes();
        };

        newGroup.appendChild(newSelect);
        newGroup.appendChild(removeButton);
        container.appendChild(newGroup);
    }

    document.getElementById("addRole").addEventListener("click", addRole);

    function attachRemoveEvents() {
        container.querySelectorAll(".remove-role").forEach(button => {
            button.onclick = function () {
                container.removeChild(this.parentElement);
                updateRoleIndexes();
            };
        });
    }

    function updateRoleIndexes() {
        let selects = container.querySelectorAll(".role-group select");
        selects.forEach((select, index) => {
            select.name = `Roles[${index}]`;
        });

        // Remove any old hidden inputs
        document.querySelectorAll("input[name^='Roles']").forEach(input => input.remove());

        // Add new hidden inputs for each department
        selects.forEach((select, index) => {
            let hiddenInput = document.createElement("input");
            hiddenInput.type = "hidden";
            hiddenInput.name = `Roles[${index}]`;
            hiddenInput.value = select.value;
            document.querySelector("form").appendChild(hiddenInput);
        });
    }

    attachRemoveEvents();

    document.querySelector("form").addEventListener("submit", function () {
        updateRoleIndexes();
    });
});

//document.addEventListener("DOMContentLoaded", function () {
//    let container = document.getElementById("subjectContainer");

//    function addSubject() {
//        let subjectGroups = container.querySelectorAll(".subject-group");
//        let index = subjectGroups.length;

//        let newGroup = document.createElement("div");
//        newGroup.classList.add("subject-group", "d-flex", "align-items-center", "mt-2");

//        let newSelect = document.createElement("select");
//        newSelect.name = `Subjects[${index}]`;
//        newSelect.classList.add("form-control", "dropdown-select", "me-2");

//        let defaultOption = document.createElement("option");
//        defaultOption.value = "";
//        defaultOption.textContent = "Изберете предмет...";
//        newSelect.appendChild(defaultOption);

//        let firstSelect = container.querySelector("select");
//        if (firstSelect) {
//            firstSelect.querySelectorAll("option").forEach(option => {
//                if (option.value !== "") {
//                    let newOption = document.createElement("option");
//                    newOption.value = option.value;
//                    newOption.textContent = option.textContent;
//                    newSelect.appendChild(newOption);
//                }
//            });
//        }

//        let removeButton = document.createElement("button");
//        removeButton.type = "button";
//        removeButton.classList.add("btn", "btn-sm", "btn-danger", "ms-2", "remove-subject");
//        removeButton.textContent = "Премахни";
//        removeButton.onclick = function () {
//            container.removeChild(newGroup);
//            updateSubjectIndexes();
//        };

//        newGroup.appendChild(newSelect);
//        newGroup.appendChild(removeButton);
//        container.appendChild(newGroup);
//    }

//    document.getElementById("addSubject").addEventListener("click", addSubject);

//    function attachRemoveEvents() {
//        container.querySelectorAll(".remove-subject").forEach(button => {
//            button.onclick = function () {
//                container.removeChild(this.parentElement);
//                updateSubjectIndexes();
//            };
//        });
//    }

//    function updateSubjectIndexes() {
//        let selects = container.querySelectorAll(".subject-group select");
//        selects.forEach((select, index) => {
//            select.name = `Subjects[${index}]`;
//        });

//        // Remove any old hidden inputs
//        document.querySelectorAll("input[name^='Subjects']").forEach(input => input.remove());

//        // Add new hidden inputs for each department
//        selects.forEach((select, index) => {
//            let hiddenInput = document.createElement("input");
//            hiddenInput.type = "hidden";
//            hiddenInput.name = `Subjects[${index}]`;
//            hiddenInput.value = select.value;
//            document.querySelector("form").appendChild(hiddenInput);
//        });
//    }

//    attachRemoveEvents();

//    document.querySelector("form").addEventListener("submit", function () {
//        updateSubjectIndexes();
//    });
//});

document.addEventListener("DOMContentLoaded", function () {
    const roleContainer = document.getElementById("roleContainer");
    const subjectGroup = document.getElementById("subjectGroup");
    const subjectParentContainer = document.getElementById("subject");
    const subjectContainer = document.getElementById("subjectContainer");
    const addSubjectButton = document.getElementById("addSubject");
    const subjectGroupInput = document.querySelector('[name="SubjectGroup"]');

    if (!roleContainer || !subjectGroup || !subjectContainer || !subjectGroupInput) {
        console.error("One or more required elements are missing. Check your HTML structure.");
        return;
    }

    // ========================== ROLE SELECTION HANDLING ==========================

    function isTeacherSelected() {
        return Array.from(roleContainer.querySelectorAll("select")).some(select =>
            select.options[select.selectedIndex]?.text.includes("Учител")
        );
    }

    function updateVisibility() {
        if (isTeacherSelected()) {
            subjectGroup.style.display = "block";
            subjectParentContainer.style.display = "block";
            subjectGroupInput.setAttribute("required", "true");
            subjectContainer.querySelectorAll("select").forEach(select => select.setAttribute("required", "true"));
        } else {
            subjectGroup.style.display = "none";
            subjectParentContainer.style.display = "none";
            subjectGroupInput.removeAttribute("required");
            subjectContainer.querySelectorAll("select").forEach(select => select.removeAttribute("required"));
        }
    }

    updateVisibility();

    roleContainer.addEventListener("change", updateVisibility);
    document.getElementById("addRole").addEventListener("click", () => {
        setTimeout(() => {
            attachRoleRemoveListeners();
            updateVisibility();
        }, 50);
    });

    function attachRoleRemoveListeners() {
        document.querySelectorAll(".remove-role").forEach(button => {
            button.removeEventListener("click", handleRoleRemove);
            button.addEventListener("click", handleRoleRemove);
        });
    }

    function handleRoleRemove(event) {
        event.target.closest(".role-group").remove();
        updateVisibility();
    }

    attachRoleRemoveListeners();

    // ========================== SUBJECT SELECTION HANDLING ==========================

    function addSubject() {
        let index = subjectContainer.querySelectorAll(".subject-group").length;

        let newGroup = document.createElement("div");
        newGroup.classList.add("subject-group", "d-flex", "align-items-center", "mt-2");

        let newSelect = document.createElement("select");
        newSelect.name = `Subjects[${index}]`;
        newSelect.classList.add("form-control", "dropdown-select", "me-2");

        let defaultOption = new Option("Изберете предмет...", "");
        newSelect.appendChild(defaultOption);

        let availableSubjects = getAvailableSubjects();
        let selectedSubjects = getSelectedSubjects();

        availableSubjects.forEach(subject => {
            if (!selectedSubjects.has(subject)) {
                newSelect.appendChild(new Option(subject, subject));
            }
        });

        let removeButton = createRemoveButton(() => {
            subjectContainer.removeChild(newGroup);
            updateSubjectIndexes();
            updateAvailableSubjects();
            updateVisibility(); // Update visibility when subject is removed
        });

        newSelect.addEventListener("change", updateAvailableSubjects);
        newGroup.append(newSelect, removeButton);
        subjectContainer.appendChild(newGroup);

        updateSubjectIndexes();
        updateAvailableSubjects();
    }

    function getAvailableSubjects() {
        return Array.from(document.querySelector("select[name='Subjects[0]']").options).map(opt => opt.value);
    }

    function getSelectedSubjects() {
        return new Set(Array.from(subjectContainer.querySelectorAll("select")).map(select => select.value));
    }

    function createRemoveButton(onClick) {
        let removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.classList.add("btn", "btn-sm", "btn-danger", "ms-2", "remove-subject");
        removeButton.textContent = "Премахни";
        removeButton.addEventListener("click", onClick);
        return removeButton;
    }

    function updateAvailableSubjects() {
        let selectedSubjects = getSelectedSubjects();

        subjectContainer.querySelectorAll("select").forEach(select => {
            let currentSelection = select.value;
            select.querySelectorAll("option").forEach(option => {
                option.hidden = option.value && option.value !== currentSelection && selectedSubjects.has(option.value);
            });
        });
    }

    function updateSubjectIndexes() {
        document.querySelectorAll(".subject-group select").forEach((select, index) => {
            select.name = `Subjects[${index}]`;
        });

        document.querySelectorAll("input[name^='Subjects']").forEach(input => input.remove());

        document.querySelectorAll(".subject-group select").forEach((select, index) => {
            let hiddenInput = document.createElement("input");
            hiddenInput.type = "hidden";
            hiddenInput.name = `Subjects[${index}]`;
            hiddenInput.value = select.value;
            document.querySelector("form").appendChild(hiddenInput);
        });

        updateVisibility();
    }

    addSubjectButton.addEventListener("click", addSubject);

    document.querySelectorAll(".remove-subject").forEach(button => {
        button.addEventListener("click", function () {
            subjectContainer.removeChild(button.parentElement);
            updateSubjectIndexes();
            updateAvailableSubjects();
            updateVisibility(); // Ensure subjects update immediately when removed
        });
    });

    subjectContainer.addEventListener("change", updateAvailableSubjects);
    document.querySelector("form").addEventListener("submit", updateSubjectIndexes);
    updateAvailableSubjects();
});


document.addEventListener("DOMContentLoaded", function () {
    // Set default selection for all dropdowns that are empty
    document.querySelectorAll("select").forEach(select => {
        if (!select.value) {
            select.value = "";
        }
    });
});