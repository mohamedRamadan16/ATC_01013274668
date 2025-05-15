// BASE API URL
const BASE_URL = "https://localhost:7193/api";

// DOM Elements
const loginBtn = document.querySelector(".login-btn");
const registerBtn = document.querySelector(".register-btn");
const loginModal = document.getElementById("loginModal");
const registerModal = document.getElementById("registerModal");
const closeButtons = document.querySelectorAll(".close");
const loginForm = document.getElementById("loginForm");
const registerForm = document.getElementById("registerForm");
const eventsGrid = document.querySelector(".events-grid");
const prevPageBtn = document.getElementById("prevPageBtn");
const nextPageBtn = document.getElementById("nextPageBtn");
const pageInfo = document.getElementById("pageInfo");
const searchInput = document.getElementById("searchInput");
const searchBtn = document.getElementById("searchBtn");

let currentPage = 1;
let pageSize = 6;
let currentSearch = "";

function getToken() {
  return localStorage.getItem("jwtToken");
}
function setToken(token) {
  localStorage.setItem("jwtToken", token);
}
function removeToken() {
  localStorage.removeItem("jwtToken");
}

function openModal(modal) {
  modal.style.display = "block";
  document.body.style.overflow = "hidden";
}
function closeModal(modal) {
  modal.style.display = "none";
  document.body.style.overflow = "auto";
}

loginBtn.addEventListener("click", () => openModal(loginModal));
registerBtn.addEventListener("click", () => openModal(registerModal));
closeButtons.forEach((button) => {
  button.addEventListener("click", () => {
    const modal = button.closest(".modal");
    closeModal(modal);
  });
});
window.addEventListener("click", (e) => {
  if (e.target.classList.contains("modal")) {
    closeModal(e.target);
  }
});

// AUTH: Login
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

// AUTH: Register
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

function createEventCard(event, booked) {
  return `
        <div class="event-card" data-event-id="${event.id}">
            <div class="event-image">
                <img src="${event.imageUrl || event.image}" alt="${event.name}">
                <span class="event-category">${
                  event.category?.name || event.category || ""
                }</span>
            </div>
            <div class="event-details">
                <h3>${event.name}</h3>
                <p class="event-date">${event.date || event.dateTime || ""}</p>
                <p class="event-venue">${event.venue}</p>
                <p class="event-price">${
                  event.price ? "$" + event.price : ""
                }</p>
                ${
                  booked
                    ? `<span class="booked-label">Booked</span>`
                    : `<button class="book-now-btn" onclick="bookEvent('${
                        event.id || event.id
                      }')">Book Now</button>`
                }
            </div>
        </div>
    `;
}

async function populateEvents() {
  eventsGrid.innerHTML = "Loading...";
  const events = await fetchEvents(pageSize, currentPage, currentSearch);
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
searchBtn.addEventListener("click", () => {
  currentSearch = searchInput.value.trim();
  currentPage = 1;
  populateEvents();
});
searchInput.addEventListener("keydown", (e) => {
  if (e.key === "Enter") {
    currentSearch = searchInput.value.trim();
    currentPage = 1;
    populateEvents();
  }
});

document.addEventListener("DOMContentLoaded", () => {
  populateEvents();
});
