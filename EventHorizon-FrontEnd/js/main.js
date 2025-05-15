// BASE API URL
const BASE_URL = "https://localhost:7193/api"; // Updated to match backend

// DOM Elements
const loginBtn = document.querySelector(".login-btn");
const registerBtn = document.querySelector(".register-btn");
const loginModal = document.getElementById("loginModal");
const registerModal = document.getElementById("registerModal");
const closeButtons = document.querySelectorAll(".close");
const loginForm = document.getElementById("loginForm");
const registerForm = document.getElementById("registerForm");
const eventsGrid = document.querySelector(".events-grid");
const showAllBtn = document.getElementById("showAllBtn");
const prevPageBtn = document.getElementById("prevPageBtn");
const nextPageBtn = document.getElementById("nextPageBtn");
const pageInfo = document.getElementById("pageInfo");
const searchInput = document.getElementById("searchInput");
const searchBtn = document.getElementById("searchBtn");
const userInfo = document.getElementById("userInfo");
const logoutBtn = document.getElementById("logoutBtn");

let currentPage = 1;
let pageSize = 3;
let showingAll = false;
let currentSearch = "";

// Helper: Get JWT token
function getToken() {
  return localStorage.getItem("jwtToken");
}

// Helper: Set JWT token
function setToken(token) {
  localStorage.setItem("jwtToken", token);
}

// Helper: Remove JWT token
function removeToken() {
  localStorage.removeItem("jwtToken");
}

// Modal Functions
function openModal(modal) {
  modal.style.display = "block";
  document.body.style.overflow = "hidden";
}

function closeModal(modal) {
  modal.style.display = "none";
  document.body.style.overflow = "auto";
}

// Event Listeners for Modals
loginBtn.addEventListener("click", () => openModal(loginModal));
registerBtn.addEventListener("click", () => openModal(registerModal));

closeButtons.forEach((button) => {
  button.addEventListener("click", () => {
    const modal = button.closest(".modal");
    closeModal(modal);
  });
});

// Close modal when clicking outside
window.addEventListener("click", (e) => {
  if (e.target.classList.contains("modal")) {
    closeModal(e.target);
  }
});

// AUTH: Login (username & password only)
loginForm.addEventListener("submit", async (e) => {
  e.preventDefault();
  const username = document.getElementById("username").value;
  const password = document.getElementById("password").value;

  try {
    const res = await fetch(`${BASE_URL}/account/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ userName: username, password }),
    });
    const data = await res.json();
    if (data.isSuccess && data.result && data.result.token) {
      setToken(data.result.token);
      closeModal(loginModal);
      alert("Login successful!");
      updateAuthUI();
      populateEvents();
    } else {
      let errorMsg = "Login failed.";
      if (data.errors && Array.isArray(data.errors))
        errorMsg += "\n" + data.errors.join("\n");
      alert(errorMsg);
    }
  } catch (err) {
    alert("Login error: " + err.message);
  }
});

// AUTH: Register (RegisterAccountDTO: UserName, Email, Password)
registerForm.addEventListener("submit", async (e) => {
  e.preventDefault();
  const username = document.getElementById("regUsername").value;
  const email = document.getElementById("regEmail").value;
  const password = document.getElementById("regPassword").value;

  try {
    const res = await fetch(`${BASE_URL}/account/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ userName: username, email, password }),
    });
    const data = await res.json();
    if (data.isSuccess) {
      alert("Registration successful! Please log in.");
      closeModal(registerModal);
      updateAuthUI();
    } else {
      let errorMsg = "Registration failed.";
      if (data.errors && Array.isArray(data.errors))
        errorMsg += "\n" + data.errors.join("\n");
      alert(errorMsg);
    }
  } catch (err) {
    alert("Registration error: " + err.message);
  }
});

// Fetch Events with pagination and search
async function fetchEvents(pageSize, pageNumber, searchQuery = "") {
  let url = `${BASE_URL}/event?pageSize=${pageSize}&pageNumber=${pageNumber}`;
  if (searchQuery) url += `&searchQuery=${encodeURIComponent(searchQuery)}`;
  try {
    const res = await fetch(url);
    const data = await res.json();
    if (data.isSuccess && data.result) {
      return data.result;
    }
    return [];
  } catch (err) {
    alert("Failed to fetch events: " + err.message);
    return [];
  }
}

// Check if event is booked
async function isEventBooked(eventId) {
  const token = getToken();
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

// Book Event
async function bookEvent(eventId) {
  const token = getToken();
  if (!token) {
    alert("Please log in to book events.");
    return;
  }
  try {
    const res = await fetch(`${BASE_URL}/booking`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ eventId }),
    });
    const data = await res.json();
    if (data.isSuccess) {
      alert("Event booked successfully!");
      populateEvents();
    } else {
      alert(
        "Booking failed: " +
          (data.errors ? data.errors.join(", ") : "Unknown error")
      );
    }
  } catch (err) {
    alert("Booking error: " + err.message);
  }
}

// Create Event Card
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

// Populate Events Grid
async function populateEvents(pageSizeParam, pageNumberParam, searchParam) {
  const pageSizeToUse = pageSizeParam !== undefined ? pageSizeParam : pageSize;
  const pageNumberToUse =
    pageNumberParam !== undefined ? pageNumberParam : currentPage;
  const searchToUse = searchParam !== undefined ? searchParam : currentSearch;
  eventsGrid.innerHTML = "Loading...";
  const events = await fetchEvents(pageSizeToUse, pageNumberToUse, searchToUse);
  if (!events.length) {
    eventsGrid.innerHTML = "<p>No events found.</p>";
    return;
  }
  let html = "";
  for (const event of events) {
    const booked = await isEventBooked(event.id || event.id);
    html += createEventCard(event, booked);
  }
  eventsGrid.innerHTML = html;
  pageInfo.textContent = `Page ${currentPage}`;
}

// Show All Events Button Handler
showAllBtn.addEventListener("click", async () => {
  if (!showingAll) {
    showingAll = true;
    showAllBtn.textContent = "Show Fewer Events";
    pageSize = 10;
    currentPage = 1;
    await populateEvents(pageSize, currentPage, currentSearch);
  } else {
    showingAll = false;
    showAllBtn.textContent = "Show All Events";
    pageSize = 3;
    currentPage = 1;
    await populateEvents(pageSize, currentPage, currentSearch);
  }
});

prevPageBtn.addEventListener("click", () => {
  if (currentPage > 1) {
    currentPage--;
    populateEvents(pageSize, currentPage, currentSearch);
  }
});
nextPageBtn.addEventListener("click", () => {
  currentPage++;
  populateEvents(pageSize, currentPage, currentSearch);
});

if (searchBtn && searchInput) {
  searchBtn.addEventListener("click", () => {
    currentSearch = searchInput.value.trim();
    currentPage = 1;
    populateEvents(pageSize, currentPage, currentSearch);
  });
  searchInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
      currentSearch = searchInput.value.trim();
      currentPage = 1;
      populateEvents(pageSize, currentPage, currentSearch);
    }
  });
}

// Initialize
document.addEventListener("DOMContentLoaded", () => {
  updateAuthUI();
  populateEvents();

  // Add smooth scrolling for Explore Events button
  exploreEventsBtn.addEventListener("click", () => {
    const eventsSection = document.getElementById("events");
    eventsSection.scrollIntoView({ behavior: "smooth" });
  });
});

// Smooth Scroll for Navigation
document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
  anchor.addEventListener("click", function (e) {
    e.preventDefault();
    const target = document.querySelector(this.getAttribute("href"));
    if (target) {
      target.scrollIntoView({
        behavior: "smooth",
      });
    }
  });
});

function parseJwt(token) {
  if (!token) return null;
  try {
    const base64Url = token.split(".")[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split("")
        .map(function (c) {
          return "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2);
        })
        .join("")
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

function updateAuthUI() {
  const token = getToken();
  const loginBtn = document.querySelector(".login-btn");
  const registerBtn = document.querySelector(".register-btn");
  if (token) {
    const payload = parseJwt(token);
    let username =
      payload &&
      (payload["unique_name"] ||
        payload["name"] ||
        payload["UserName"] ||
        payload["sub"]);
    if (!username && payload)
      username =
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];
    userInfo.style.display = "inline-block";
    userInfo.textContent = username ? `👤 ${username}` : "👤 User";
    loginBtn.style.display = "none";
    registerBtn.style.display = "none";
    logoutBtn.style.display = "inline-block";
  } else {
    userInfo.style.display = "none";
    userInfo.textContent = "";
    loginBtn.style.display = "inline-block";
    registerBtn.style.display = "inline-block";
    logoutBtn.style.display = "none";
  }
}

logoutBtn.addEventListener("click", async () => {
  try {
    const token = getToken();
    if (token) {
      const res = await fetch(`${BASE_URL}/account/logout`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
      });
      const data = await res.json();
      if (data.isSuccess) {
        removeToken();
        updateAuthUI();
        populateEvents();
      } else {
        alert(
          "Logout failed: " +
            (data.errors ? data.errors.join("\n") : "Unknown error")
        );
      }
    } else {
      removeToken();
      updateAuthUI();
      populateEvents();
    }
  } catch (err) {
    console.error("Logout error:", err);
    // Still remove token and update UI even if the API call fails
    removeToken();
    updateAuthUI();
    populateEvents();
  }
});
