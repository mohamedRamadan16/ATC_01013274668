// Shared event card component for all pages
function createEventCard(event, booked = false, options = {}) {
  const eventDate = new Date(event.eventDate);
  const formattedDate = eventDate.toLocaleDateString("en-US", {
    weekday: "short",
    month: "short",
    day: "numeric",
    year: "numeric",
  });
  const formattedTime = eventDate.toLocaleTimeString("en-US", {
    hour: "2-digit",
    minute: "2-digit",
  });

  // Blue date badge with calendar icon
  const dateBadge = `<div class='event-date-badge'><i class='fas fa-calendar'></i> ${formattedDate}</div>`;

  // Booked label or Book Now button
  let footerHtml = "";
  if (booked) {
    footerHtml = `<span class='booked-label'><i class='fas fa-check-circle'></i> Booked</span>`;
  } else if (options.showBookBtn !== false) {
    footerHtml = `<button class='book-now-btn main-btn' onclick="bookEvent('${event.id}')">
      <i class='fas fa-ticket-alt'></i> Book Now
    </button>`;
  }

  return `
    <div class="event-card">
      <div class="event-image">
        <img src="${event.imageUrl || "images/default-event.jpg"}" alt="${
    event.name
  }" />
        ${dateBadge}
      </div>
      <div class="event-details">
        <h3>${event.name}</h3>
        <div class="event-info">
          <div class="event-info-item">
            <i class="fas fa-map-marker-alt"></i>
            <span>${event.venue}</span>
          </div>
          <div class="event-info-item">
            <i class="fas fa-clock"></i>
            <span>${formattedTime}</span>
          </div>
        </div>
        <p class="event-price">$${event.price}</p>
        ${footerHtml}
      </div>
    </div>
  `;
}
