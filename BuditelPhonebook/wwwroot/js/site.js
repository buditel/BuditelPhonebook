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
            suggestions.style.display = 'none';
            return;
        }

        const data = await fetchSuggestions(query);
        suggestions.innerHTML = ''; // Clear existing suggestions

        if (data.length > 0) {
            data.forEach(item => {
                const suggestion = document.createElement('div');
                suggestion.className = 'dropdown-item';
                suggestion.innerHTML = `<strong>${item.FullName}</strong><br>${item.Subject || ''} - ${item.Department}`;
                suggestion.addEventListener('click', function () {
                    searchInput.value = item.FullName; // Set input value
                    suggestions.style.display = 'none'; // Hide suggestions
                });
                suggestions.appendChild(suggestion);
            });
            suggestions.style.display = 'block'; // Show suggestions
        } else {
            suggestions.style.display = 'none';
        }
    });

    // Hide suggestions when clicking outside
    document.addEventListener('click', function (event) {
        if (!event.target.closest('#searchInput') && !event.target.closest('#suggestions')) {
            suggestions.style.display = 'none';
        }
    });
});

