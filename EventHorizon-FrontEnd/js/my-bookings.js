const BASE_URL = "https://localhost:7193/api";

const loginBtn = document.querySelector(".login-btn");
const registerBtn = document.querySelector(".register-btn");
const loginModal = document.getElementById("loginModal");
const registerModal = document.getElementById("registerModal");
const closeButtons = document.querySelectorAll(".close");
const loginForm = document.getElementById("loginForm");
const registerForm = document.getElementById("registerForm");
const userInfo = document.getElementById("userInfo");
const logoutBtn = document.getElementById("logoutBtn");
const bookingsGrid = document.getElementById("bookingsGrid");

function getToken() {
  return localStorage.getItem("jwtToken");
}
function setToken(token) {
  localStorage.setItem("jwtToken", token);
}
function removeToken() {
  localStorage.removeItem("jwtToken");
}

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
  if (!userInfo) return;
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

logoutBtn.addEventListener("click", () => {
  removeToken();
  updateAuthUI();
  renderBookings();
});

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

function openModal(modal) {
  modal.style.display = "block";
  document.body.style.overflow = "hidden";
}
function closeModal(modal) {
  modal.style.display = "none";
  document.body.style.overflow = "auto";
}

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
      renderBookings();
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

async function fetchUserBookings() {
  const token = getToken();
  if (!token) return null;
  try {
    const res = await fetch(`${BASE_URL}/booking`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    const data = await res.json();
    if (data.isSuccess && data.result) {
      return data.result;
    }
    return [];
  } catch (err) {
    alert("Failed to fetch bookings: " + err.message);
    return [];
  }
}

async function cancelBooking(eventId) {
  const token = getToken();
  if (!token) return;
  if (!confirm("Are you sure you want to cancel this booking?")) return;
  try {
    const res = await fetch(`${BASE_URL}/booking/${eventId}`, {
      method: "DELETE",
      headers: { Authorization: `Bearer ${token}` },
    });
    const data = await res.json();
    if (data.isSuccess) {
      alert("Booking cancelled.");
      renderBookings();
    } else {
      alert(
        "Failed to cancel booking: " +
          (data.errors ? data.errors.join(", ") : "Unknown error")
      );
    }
  } catch (err) {
    alert("Cancel error: " + err.message);
  }
}

function createBookingCard(booking) {
  const event = booking.event || booking.Event;
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
        <p class="event-price">${event.price ? "$" + event.price : "Free"}</p>
        <button class="main-btn" onclick="cancelBooking('${
          event.id
        }')">Cancel Booking</button>
      </div>
    </div>
  `;
}

async function renderBookings() {
  const token = getToken();
  if (!token) {
    bookingsGrid.innerHTML =
      '<p style="text-align:center;">Please log in to view your bookings.</p>';
    return;
  }
  bookingsGrid.innerHTML = "Loading...";
  const bookings = await fetchUserBookings();
  if (!bookings || !bookings.length) {
    bookingsGrid.innerHTML =
      '<p style="text-align:center;">No bookings found.</p>';
    return;
  }
  let html = "";
  for (const booking of bookings) {
    html += createBookingCard(booking);
  }
  bookingsGrid.innerHTML = html;
}

window.cancelBooking = cancelBooking;

document.addEventListener("DOMContentLoaded", () => {
  updateAuthUI();
  renderBookings();
});
