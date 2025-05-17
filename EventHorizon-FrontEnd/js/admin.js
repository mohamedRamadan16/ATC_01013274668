document.addEventListener("DOMContentLoaded", () => {
  // Check if user is admin
  const token = localStorage.getItem("token");
  console.log("Token:", token ? "Present" : "Missing");

  if (!token) {
    console.log("User not logged in");
    window.location.href = "login.html?redirect=admin";
    return;
  }

  // Function to check if user is admin
  function isAdmin(token) {
    try {
      const base64Url = token.split(".")[1];
      const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split("")
          .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
          .join("")
      );
      const payload = JSON.parse(jsonPayload);

      // Check both possible claim names
      return (
        payload[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ] === "Admin" || payload.role === "Admin"
      );
    } catch (error) {
      console.error("Error decoding token:", error);
      return false;
    }
  }

  if (!isAdmin(token)) {
    console.log("User is not admin");
    alert("You don't have permission to access the admin panel");
    window.location.href = "index.html";
    return;
  }

  console.log("User is admin, proceeding to admin panel");

  // DOM Elements
  const eventsTableBody = document.getElementById("eventsTableBody");
  const addEventBtn = document.getElementById("addEventBtn");
  const eventModal = document.getElementById("eventModal");
  const deleteModal = document.getElementById("deleteModal");
  const eventForm = document.getElementById("eventForm");
  const modalTitle = document.getElementById("modalTitle");
  const closeButtons = document.querySelectorAll(".close");
  const confirmDeleteBtn = document.getElementById("confirmDelete");
  const cancelDeleteBtn = document.getElementById("cancelDelete");
  const imagePreview = document.getElementById("imagePreview");
  const eventImageInput = document.getElementById("eventImage");
  const eventCategorySelect = document.getElementById("eventCategory");
  const logoutBtn = document.getElementById("logoutBtn");

  // Add logout event listener
  if (logoutBtn) {
    logoutBtn.addEventListener("click", () => {
      localStorage.removeItem("token");
      window.location.href = "index.html";
    });
  }

  let currentEventId = null;
  let categories = [];

  // Load events and categories
  loadEvents();
  loadCategories();

  // Event Listeners
  addEventBtn.addEventListener("click", () => {
    showEventModal();
  });

  eventForm.addEventListener("submit", handleEventSubmit);

  closeButtons.forEach((button) => {
    button.addEventListener("click", () => {
      eventModal.style.display = "none";
      deleteModal.style.display = "none";
    });
  });

  confirmDeleteBtn.addEventListener("click", handleDelete);
  cancelDeleteBtn.addEventListener("click", () => {
    deleteModal.style.display = "none";
  });

  // Image file preview
  eventImageInput.addEventListener("change", (e) => {
    const file = e.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e) => {
        imagePreview.src = e.target.result;
        imagePreview.style.display = "block";
      };
      reader.readAsDataURL(file);
    } else {
      imagePreview.style.display = "none";
    }
  });

  // Close modals when clicking outside
  window.onclick = (event) => {
    if (event.target === eventModal) {
      eventModal.style.display = "none";
    }
    if (event.target === deleteModal) {
      deleteModal.style.display = "none";
    }
  };

  // Functions
  async function loadEvents() {
    console.log("Loading events...");
    showLoading(eventsTableBody, 6);

    try {
      const response = await fetch("https://localhost:7193/api/event", {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      console.log("Response status:", response.status);

      if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }

      const data = await response.json();
      console.log("Response data:", data);

      if (data.isSuccess) {
        console.log("Events loaded successfully:", data.result);
        if (Array.isArray(data.result)) {
          renderEvents(data.result);
        } else {
          console.error(
            "Expected array of events but got:",
            typeof data.result
          );
          showError(
            eventsTableBody,
            "Invalid data format received from server"
          );
        }
      } else {
        console.error("Failed to load events:", data.message);
        showError(eventsTableBody, data.message || "Failed to load events");
      }
    } catch (error) {
      console.error("Error loading events:", error);
      showError(eventsTableBody, "Error loading events. Please try again.");
    }
  }

  async function loadCategories() {
    try {
      const response = await fetch("https://localhost:7193/api/category", {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }

      const data = await response.json();
      console.log("Categories data:", data);

      if (data.isSuccess && Array.isArray(data.result)) {
        categories = data.result;

        // Clear existing options
        eventCategorySelect.innerHTML =
          '<option value="">Select a category</option>';

        // Add categories to dropdown
        categories.forEach((category) => {
          const option = document.createElement("option");
          option.value = category.id;
          option.textContent = category.name;
          eventCategorySelect.appendChild(option);
        });
      } else {
        console.error("Failed to load categories:", data.message);
      }
    } catch (error) {
      console.error("Error loading categories:", error);
    }
  }

  function showLoading(element, colSpan = 1) {
    element.innerHTML = `
      <tr>
        <td colspan="${colSpan}" style="text-align: center; padding: 2rem;">
          <div class="loading-spinner"></div> Loading...
        </td>
      </tr>
    `;
  }

  function showError(element, message, colSpan = 6) {
    element.innerHTML = `
      <tr>
        <td colspan="${colSpan}" style="text-align: center; color: #dc3545; padding: 1rem;">
          ${message}
        </td>
      </tr>
    `;
  }

  function renderEvents(events) {
    console.log("Rendering events:", events);
    if (!eventsTableBody) {
      console.error("Events table body element not found!");
      return;
    }

    eventsTableBody.innerHTML = "";

    if (!Array.isArray(events) || events.length === 0) {
      const row = document.createElement("tr");
      row.innerHTML = `
        <td colspan="6" style="text-align: center;">No events found</td>
      `;
      eventsTableBody.appendChild(row);
      return;
    }

    events.forEach((event) => {
      console.log("Processing event:", event);
      const row = document.createElement("tr");

      // Handle different property names that might come from the API
      const title = event.name || event.title || "N/A";
      const date = event.eventDate || event.date || null;
      const location = event.venue || event.location || "N/A";
      const price = typeof event.price === "number" ? event.price : 0;

      // Find category name if categoryId exists
      let categoryName = "N/A";
      if (event.categoryId && categories.length > 0) {
        const category = categories.find((c) => c.id === event.categoryId);
        if (category) {
          categoryName = category.name;
        }
      }

      row.innerHTML = `
        <td>${title}</td>
        <td>${date ? new Date(date).toLocaleString() : "N/A"}</td>
        <td>${location}</td>
        <td>${categoryName}</td>
        <td>$${price.toFixed(2)}</td>
        <td>
          <div class="action-buttons">
            <button class="edit-btn" data-id="${event.id}">Edit</button>
            <button class="delete-btn" data-id="${event.id}">Delete</button>
          </div>
        </td>
      `;
      eventsTableBody.appendChild(row);
    });

    // Add event listeners to buttons
    document.querySelectorAll(".edit-btn").forEach((btn) => {
      btn.addEventListener("click", () => {
        const eventId = btn.dataset.id;
        console.log("Edit button clicked for event:", eventId);
        showEventModal(eventId);
      });
    });

    document.querySelectorAll(".delete-btn").forEach((btn) => {
      btn.addEventListener("click", () => {
        const eventId = btn.dataset.id;
        console.log("Delete button clicked for event:", eventId);
        showDeleteModal(eventId);
      });
    });
  }

  async function showEventModal(eventId = null) {
    currentEventId = eventId;
    modalTitle.textContent = eventId ? "Edit Event" : "Add New Event";
    console.log("Showing event modal for event:", eventId);

    // Reset form and image preview
    eventForm.reset();
    document.getElementById("eventId").value = ""; // Ensure add mode clears the ID
    imagePreview.style.display = "none";

    if (eventId) {
      try {
        console.log("Fetching event details for ID:", eventId);
        const response = await fetch(
          `https://localhost:7193/api/event/${eventId}`,
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        console.log("Response status:", response.status);

        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();
        console.log("Event details:", data);

        if (data.isSuccess) {
          const event = data.result;

          // Handle different property names that might come from the API
          document.getElementById("eventId").value = event.id;
          document.getElementById("eventTitle").value =
            event.name || event.title || "";

          // Format date for datetime-local input
          const eventDate = event.eventDate || event.date;
          if (eventDate) {
            // Convert to local ISO string and remove seconds/milliseconds
            const dateObj = new Date(eventDate);
            const localISOString = new Date(
              dateObj.getTime() - dateObj.getTimezoneOffset() * 60000
            )
              .toISOString()
              .slice(0, 16);
            document.getElementById("eventDate").value = localISOString;
          }

          document.getElementById("eventLocation").value =
            event.venue || event.location || "";
          document.getElementById("eventPrice").value = event.price || 0;
          document.getElementById("eventDescription").value =
            event.description || "";

          // Set category if available
          if (event.categoryId) {
            document.getElementById("eventCategory").value = event.categoryId;
          }

          // Show current image in preview if available
          if (event.imageUrl) {
            imagePreview.src = event.imageUrl;
            imagePreview.style.display = "block";
          }
        } else {
          console.error("Failed to load event details:", data.message);
          alert("Failed to load event details: " + data.message);
        }
      } catch (error) {
        console.error("Error loading event:", error);
        alert("Error loading event details. Please try again.");
        return;
      }
    }

    eventModal.style.display = "block";
  }

  function showDeleteModal(eventId) {
    currentEventId = eventId;
    console.log("Showing delete modal for event:", eventId);
    deleteModal.style.display = "block";
  }

  async function handleEventSubmit(e) {
    e.preventDefault();
    console.log("Handling event submit");

    // Disable submit button and show loading state
    const submitBtn = eventForm.querySelector("button[type='submit']");
    const originalBtnText = submitBtn.textContent;
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<div class="loading-spinner"></div> Saving...';

    try {
      // Get form values
      const eventId = document.getElementById("eventId").value;
      const name = document.getElementById("eventTitle").value;
      const dateTimeValue = document.getElementById("eventDate").value;
      const venue = document.getElementById("eventLocation").value.trim();
      const price = Number.parseFloat(
        document.getElementById("eventPrice").value
      );
      const description = document
        .getElementById("eventDescription")
        .value.trim();
      const categoryId = document.getElementById("eventCategory").value;
      const imageFile = document.getElementById("eventImage").files[0];

      // Validate required fields
      if (!name || !venue || !dateTimeValue || !description) {
        throw new Error("Please fill in all required fields");
      }

      // Create FormData object to send the data
      const formData = new FormData();

      // For updates, include the ID
      if (eventId) {
        formData.append("Id", eventId);
      }

      formData.append("Name", name);
      formData.append("Description", description);
      formData.append("venue", venue); // Note: lowercase 'venue' to match backend
      formData.append("price", price);

      // Format date correctly
      const dateObj = new Date(dateTimeValue);
      formData.append("EventDate", dateObj.toISOString());

      // Only append CategoryId if a category is selected
      if (categoryId) {
        formData.append("CategoryId", categoryId);
      }

      // Only append Image if a file is selected
      if (imageFile) {
        formData.append("Image", imageFile);
      }

      console.log("Form data prepared with the following fields:");
      for (const [key, value] of formData.entries()) {
        console.log(`${key}: ${value instanceof File ? value.name : value}`);
      }

      let url, method;

      if (eventId) {
        // Update existing event
        url = "https://localhost:7193/api/event"; // Base URL for events
        method = "PUT"; // Use PUT for updates
      } else {
        // Create new event
        url = "https://localhost:7193/api/event";
        method = "POST"; // Use POST for creates
      }

      console.log(`Submitting to URL: ${url} with method: ${method}`);

      const response = await fetch(url, {
        method: method,
        headers: {
          // Don't set Content-Type header when using FormData
          Authorization: `Bearer ${token}`,
        },
        body: formData,
      });
      console.log("Response status:", response.status);

      // Try to get detailed error information
      if (!response.ok) {
        let errorMessage = `HTTP error! Status: ${response.status}`;
        try {
          const errorData = await response.json();
          console.log("Error response data:", errorData);

          if (errorData.message) {
            errorMessage = errorData.message;
          } else if (errorData.errors) {
            // Handle validation errors
            const errors = Object.values(errorData.errors).flat();
            errorMessage = errors.join(", ");
          }
        } catch (e) {
          console.log("Could not parse error response as JSON");
        }
        throw new Error(errorMessage);
      }

      const data = await response.json();
      console.log("Response data:", data);

      if (data.isSuccess) {
        console.log("Event saved successfully");
        eventModal.style.display = "none";
        loadEvents();
        alert(
          eventId
            ? "Event updated successfully!"
            : "Event created successfully!"
        );
      } else {
        console.error("Failed to save event:", data.message);
        alert("Failed to save event: " + data.message);
      }
    } catch (error) {
      console.error("Error saving event:", error);
      alert("Error saving event: " + error.message);
    } finally {
      // Re-enable submit button
      submitBtn.disabled = false;
      submitBtn.textContent = originalBtnText;
    }
  }

  async function handleDelete() {
    if (!currentEventId) return;
    console.log("Handling delete for event:", currentEventId);

    // Disable delete button and show loading state
    const deleteBtn = confirmDeleteBtn;
    const originalBtnText = deleteBtn.textContent;
    deleteBtn.disabled = true;
    deleteBtn.innerHTML = '<div class="loading-spinner"></div> Deleting...';

    try {
      const response = await fetch(
        `https://localhost:7193/api/event/${currentEventId}`,
        {
          method: "DELETE",
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      console.log("Response status:", response.status);

      if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }

      const data = await response.json();
      console.log("Response data:", data);

      if (data.isSuccess) {
        console.log("Event deleted successfully");
        deleteModal.style.display = "none";
        loadEvents();
        alert("Event deleted successfully!");
      } else {
        console.error("Failed to delete event:", data.message);
        alert("Failed to delete event: " + data.message);
      }
    } catch (error) {
      console.error("Error deleting event:", error);
      alert("Error deleting event. Please try again.");
    } finally {
      // Re-enable delete button
      deleteBtn.disabled = false;
      deleteBtn.textContent = originalBtnText;
    }
  }
});
