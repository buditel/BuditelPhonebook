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
    const suggestionsDropdown = document.getElementById("suggestions");

    // Function to fetch search results
    const fetchSearchResults = async (query) => {
        try {
            const response = await fetch(`/Phonebook/Index?search=${encodeURIComponent(query)}`, {
                headers: { "X-Requested-With": "XMLHttpRequest" },
            });

            if (!response.ok) {
                throw new Error("Failed to fetch results.");
            }

            const partialViewHtml = await response.text();

            if (partialViewHtml.trim()) {
                resultsContainer.innerHTML = partialViewHtml;
            } else {
                resultsContainer.innerHTML = `<div class="alert alert-info text-center">Няма съвпадения.</div>`;
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
            fetchSearchResults(query);
        } else {
            fetchSearchResults("");
        }
    });

    // Event handler for search button
    searchBtn.addEventListener("click", function () {
        const query = searchInput.value.trim();
        fetchSearchResults(query);
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