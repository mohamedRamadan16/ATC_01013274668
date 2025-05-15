// Constants
const BASE_URL = "https://localhost:7193/api";
const EVENTS_PER_PAGE = 6;

// DOM Elements
const eventsContainer = document.getElementById("eventsContainer");
const searchInput = document.getElementById("searchInput");
const searchBtn = document.getElementById("searchBtn");
const prevPageBtn = document.getElementById("prevPage");
const nextPageBtn = document.getElementById("nextPage");
const pageInfo = document.getElementById("pageInfo");
const loginBtn = document.querySelector(".login-btn");
const registerBtn = document.querySelector(".register-btn");
const logoutBtn = document.getElementById("logoutBtn");
const loginModal = document.getElementById("loginModal");
const registerModal = document.getElementById("registerModal");
const closeButtons = document.querySelectorAll(".close");
const loginForm = document.getElementById("loginForm");
const registerForm = document.getElementById("registerForm");

// State
let currentPage = 1;
let totalPages = 1;
let currentSearchQuery = "";

// Event Listeners
document.addEventListener("DOMContentLoaded", () => {
  checkAuthState();
  populateEvents();
  setupEventListeners();
});

function setupEventListeners() {
  // Search and pagination
  searchBtn.addEventListener("click", handleSearch);
  searchInput.addEventListener("keypress", (e) => {
    if (e.key === "Enter") {
      handleSearch();
    }
  });
  prevPageBtn.addEventListener("click", () => {
    if (currentPage > 1) {
      currentPage--;
      populateEvents();
    }
  });
  nextPageBtn.addEventListener("click", () => {
    currentPage++;
    populateEvents();
  });

  // Auth modals
  loginBtn.addEventListener("click", () => openModal(loginModal));
  registerBtn.addEventListener("click", () => openModal(registerModal));
  closeButtons.forEach((button) => {
    button.addEventListener("click", () => {
      const modal = button.closest(".modal");
      closeModal(modal);
    });
  });

  // Auth forms
  loginForm.addEventListener("submit", handleLogin);
  registerForm.addEventListener("submit", handleRegister);
  logoutBtn.addEventListener("click", handleLogout);
}

function openModal(modal) {
  modal.style.display = "block";
  document.body.style.overflow = "hidden";
}

function closeModal(modal) {
  modal.style.display = "none";
  document.body.style.overflow = "auto";
}

async function handleLogin(e) {
  e.preventDefault();
  const username = document.getElementById("username").value;
  const password = document.getElementById("password").value;

  try {
    const response = await fetch(`${BASE_URL}/account/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ userName: username, password }),
    });

    const data = await response.json();
    if (data.isSuccess && data.result?.token) {
      localStorage.setItem("token", data.result.token);
      closeModal(loginModal);
      checkAuthState();
      populateEvents();
    } else {
      alert(data.message || "Login failed");
    }
  } catch (error) {
    console.error("Login error:", error);
    alert("Login failed. Please try again.");
  }
}

async function handleRegister(e) {
  e.preventDefault();
  const username = document.getElementById("regUsername").value;
  const email = document.getElementById("regEmail").value;
  const password = document.getElementById("regPassword").value;

  try {
    const response = await fetch(`${BASE_URL}/account/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ userName: username, email, password }),
    });

    const data = await response.json();
    if (data.isSuccess) {
      alert("Registration successful! Please log in.");
      closeModal(registerModal);
    } else {
      alert(data.message || "Registration failed");
    }
  } catch (error) {
    console.error("Registration error:", error);
    alert("Registration failed. Please try again.");
  }
}

async function handleLogout() {
  const token = localStorage.getItem("token");
  if (token) {
    try {
      const response = await fetch(`${BASE_URL}/account/logout`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      const data = await response.json();
      if (data.isSuccess) {
        localStorage.removeItem("token");
        checkAuthState();
        populateEvents();
      }
    } catch (error) {
      console.error("Logout error:", error);
    }
  }
  localStorage.removeItem("token");
  checkAuthState();
  populateEvents();
}

function checkAuthState() {
  const token = localStorage.getItem("token");
  loginBtn.style.display = token ? "none" : "block";
  registerBtn.style.display = token ? "none" : "block";
  logoutBtn.style.display = token ? "block" : "none";
}

async function handleSearch() {
  currentSearchQuery = searchInput.value.trim();
  currentPage = 1;
  await populateEvents();
}

async function populateEvents() {
  try {
    eventsContainer.innerHTML = '<div class="loading">Loading events...</div>';
    const { events } = await fetchEvents(
      currentPage,
      EVENTS_PER_PAGE,
      currentSearchQuery
    );
    displayEvents(events);
    updatePagination(events);
  } catch (error) {
    console.error("Error populating events:", error);
    eventsContainer.innerHTML =
      '<p class="error-message">Failed to load events. Please try again later.</p>';
  }
}

async function fetchEvents(pageNumber, pageSize, searchQuery) {
  try {
    let url = `${BASE_URL}/event?pageNumber=${pageNumber}&pageSize=${pageSize}`;
    if (searchQuery) {
      url += `&searchQuery=${encodeURIComponent(searchQuery)}`;
    }

    const response = await fetch(url);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    if (!data.isSuccess) {
      throw new Error(data.message || "Failed to fetch events");
    }

    // Support both old and new backend response
    if (Array.isArray(data.result)) {
      return {
        events: data.result,
        totalCount: data.totalCount || data.result.length,
      };
    } else if (data.result && data.result.events) {
      return { events: data.result.events, totalCount: data.result.totalCount };
    } else {
      return { events: [], totalCount: 0 };
    }
  } catch (error) {
    console.error("Error fetching events:", error);
    throw error;
  }
}

// Helper to check if event is booked
async function isEventBooked(eventId) {
  const token = localStorage.getItem("token");
  if (!token) return false;
  try {
    const res = await fetch(`${BASE_URL}/booking/${eventId}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    const data = await res.json();
    return data.isSuccess && data.result === true;
  } catch {
    return false;
  }
}

// Render events and show booked state
async function displayEvents(events) {
  if (!events || events.length === 0) {
    eventsContainer.innerHTML = '<p class="no-events">No events found</p>';
    return;
  }
  let html = "";
  for (const event of events) {
    const booked = await isEventBooked(event.id);
    html += createEventCard(event, booked);
  }
  eventsContainer.innerHTML = html;
}

function createEventCard(event, booked) {
  let formattedDate = "Date TBA";
  let formattedTime = "";

  try {
    const dateValue = event.eventDate || event.date || event.dateTime;
    if (dateValue) {
      const eventDate = new Date(dateValue);
      if (!isNaN(eventDate.getTime())) {
        formattedDate = eventDate.toLocaleDateString("en-US", {
          weekday: "short",
          month: "short",
          day: "numeric",
          year: "numeric",
        });

        formattedTime = eventDate.toLocaleTimeString("en-US", {
          hour: "2-digit",
          minute: "2-digit",
          hour12: true,
        });
      }
    }
  } catch (error) {
    console.error("Error formatting date:", error);
  }

  return `
        <div class="event-card" data-event-id="${event.id}">
            <div class="event-image">
                <img src="${event.imageUrl || event.image}" alt="${event.name}">
                <span class="event-category">${
                  event.category?.name || event.category || ""
                }</span>
                <div class="event-date-badge">
                    <i class="fas fa-calendar"></i>
                    ${formattedDate}
                </div>
            </div>
            <div class="event-details">
                <h3>${event.name}</h3>
                <div class="event-info">
                    <div class="event-info-item">
                        <i class="fas fa-map-marker-alt"></i>
                        <span>${event.venue || "Venue TBA"}</span>
                    </div>
                    <div class="event-info-item">
                        <i class="fas fa-clock"></i>
                        <span>${formattedTime || "Time TBA"}</span>
                    </div>
                </div>
                <p class="event-price">${
                  event.price ? "$" + event.price : "Free"
                }</p>
                ${
                  booked
                    ? `<span class="booked-label">Booked</span>`
                    : `<button class="book-now-btn main-btn" onclick="bookEvent('${
                        event.id || event.id
                      }')">Book Now</button>`
                }
            </div>
        </div>
    `;
}

function updatePagination(events) {
  pageInfo.textContent = `Page ${currentPage}`;
  prevPageBtn.disabled = currentPage === 1;
  nextPageBtn.disabled = !events || events.length < EVENTS_PER_PAGE;
}

async function bookEvent(eventId) {
  const token = localStorage.getItem("token");
  if (!token) {
    alert("Please login to book events");
    openModal(loginModal);
    return;
  }

  try {
    const payload = { eventId };
    const response = await fetch(`${BASE_URL}/booking`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(payload),
    });

    const data = await response.json();
    if (data.isSuccess) {
      alert("Event booked successfully!");
      window.location.reload();
    } else {
      alert(data.message || "Failed to book event");
    }
  } catch (error) {
    console.error("Error booking event:", error);
    alert("Failed to book event. Please try again later.");
  }
}
