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
            subjectGroup.style.display = "block";
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
    document.querySelectorAll('.see-latest-change').forEach(element => {
        element.addEventListener('click', function (event) {
            event.preventDefault();

            const personId = this.getAttribute('data-id');
            const modalBody = document.querySelector('#latestChangeModal .modal-content');

            fetch(`/Admin/SeeLatestChange?id=${personId}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Грешка при зареждане на последна промяна.');
                    }
                    return response.text();
                })
                .then(html => {
                    modalBody.innerHTML = html;
                })
                .catch(error => {
                    modalBody.innerHTML = `<div class="modal-body text-danger">${error.message}</div>`;
                });
        });
    });
});

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


