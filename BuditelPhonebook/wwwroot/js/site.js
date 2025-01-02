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

document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.getElementById('searchInput');
    const suggestions = document.getElementById('suggestions');

    // Function to fetch suggestions from the API
    async function fetchSuggestions(query) {
        try {
            const response = await fetch(`/api/Suggestions/GetSuggestions?query=${encodeURIComponent(query)}`);
            if (!response.ok) {
                throw new Error('Failed to fetch suggestions');
            }
            return await response.json();
        } catch (error) {
            console.error(error);
            return [];
        }
    }

    // Function to handle input event
    searchInput.addEventListener('input', async function () {
        const query = searchInput.value.trim();

        if (query.length === 0) {
            suggestions.style.display = 'none';  // Hide suggestions when input is empty
            return;
        }

        const data = await fetchSuggestions(query);
        suggestions.innerHTML = ''; // Clear existing suggestions

        if (data.length > 0) {
            // If there are suggestions, display them
            data.forEach(item => {
                const suggestion = document.createElement('div');
                suggestion.className = 'dropdown-item';
                suggestion.innerHTML = `<strong>${item.fullName}</strong><br>${item.subject || ''} - ${item.department}`;
                suggestion.addEventListener('click', function () {
                    window.location.href = `/Phonebook/Details/${item.id}`;
                });
                suggestions.appendChild(suggestion);
            });
            suggestions.style.display = 'block'; // Show suggestions
        } else {
            // If no data found, show "not found" message
            const nothingFound = document.createElement('div');
            nothingFound.className = 'dropdown-item';
            nothingFound.innerHTML = `Търсенето не е намерено.`;
            suggestions.appendChild(nothingFound);
            suggestions.style.display = 'block'; // Show the "not found" message
        }
    });

    // Hide suggestions when clicking outside
    document.addEventListener('click', function (event) {
        if (!event.target.closest('#searchInput') && !event.target.closest('#suggestions')) {
            suggestions.style.display = 'none';
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