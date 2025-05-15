function getToken() {
  return localStorage.getItem("token");
}

function getUsernameFromToken(token) {
  if (!token) return null;
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return (
      payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
      payload["unique_name"] ||
      payload["username"] ||
      payload["name"] ||
      null
    );
  } catch {
    return null;
  }
}

function updateAuthUI() {
  const token = getToken();
  const userInfo = document.getElementById("userInfo");
  const loginBtn = document.querySelector(".login-btn");
  const registerBtn = document.querySelector(".register-btn");
  const logoutBtn = document.getElementById("logoutBtn");

  if (!userInfo || !loginBtn || !registerBtn || !logoutBtn) return;

  if (token) {
    const username = getUsernameFromToken(token);
    userInfo.style.display = "inline";
    userInfo.textContent = username ? `👤 ${username}` : "";
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

// Listen for logout
// (delegated in case the button is dynamically rendered)
document.addEventListener("click", function (e) {
  if (e.target && e.target.id === "logoutBtn") {
    localStorage.removeItem("token");
    updateAuthUI();
    window.location.reload();
  }
});

document.addEventListener("DOMContentLoaded", updateAuthUI);

document.addEventListener("DOMContentLoaded", function () {
  // Modal logic
  const loginBtn = document.querySelector(".login-btn");
  const registerBtn = document.querySelector(".register-btn");
  const loginModal = document.getElementById("loginModal");
  const registerModal = document.getElementById("registerModal");
  const closeButtons = document.querySelectorAll(".close");

  if (loginBtn && loginModal) {
    loginBtn.addEventListener("click", () => {
      loginModal.style.display = "block";
      document.body.style.overflow = "hidden";
    });
  }
  if (registerBtn && registerModal) {
    registerBtn.addEventListener("click", () => {
      registerModal.style.display = "block";
      document.body.style.overflow = "hidden";
    });
  }
  closeButtons.forEach((button) => {
    button.addEventListener("click", () => {
      const modal = button.closest(".modal");
      modal.style.display = "none";
      document.body.style.overflow = "auto";
    });
  });
  window.onclick = function (event) {
    if (event.target.classList.contains("modal")) {
      event.target.style.display = "none";
      document.body.style.overflow = "auto";
    }
  };

  // Login form handler
  const loginForm = document.getElementById("loginForm");
  if (loginForm) {
    loginForm.addEventListener("submit", async function (e) {
      e.preventDefault();
      const username = document.getElementById("username").value;
      const password = document.getElementById("password").value;
      try {
        const response = await fetch(
          "https://localhost:7193/api/account/login",
          {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ userName: username, password }),
          }
        );
        const data = await response.json();
        if (data.isSuccess && data.result?.token) {
          localStorage.setItem("token", data.result.token);
          updateAuthUI();
          // Close modal
          loginModal.style.display = "none";
          document.body.style.overflow = "auto";
          window.location.reload();
        } else {
          alert(data.message || "Login failed");
        }
      } catch (err) {
        alert("Login error: " + err.message);
      }
    });
  }
});
