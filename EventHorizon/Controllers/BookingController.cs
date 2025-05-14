using EventHorizon.DataAccess.Persistence;
using EventHorizon.DataAccess.Repository.IRepository;
using EventHorizon.Models.DTOs.Event;
using EventHorizon.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventHorizon.Controllers
{
    [ApiController]
    [Route("api/booking")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IUserEventRepository _userEventRepository;
        private readonly GeneralResponse _response;
        public BookingController(IUserEventRepository userEventRepository)
        {
            _userEventRepository = userEventRepository;
            _response = new GeneralResponse();
        }

        [HttpPost]
        public async Task<ActionResult<GeneralResponse>> BookEvent([FromBody] BookEventDTO dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.Unauthorized;
                return _response;
            }
            try
            {
                var booking = await _userEventRepository.BookEventAsync(userId, dto.EventId);
                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
                _response.Result = booking;
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string> { ex.Message };
            }
            return _response;
        }

        [HttpGet]
        public async Task<ActionResult<GeneralResponse>> GetUserBookings()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.Unauthorized;
                return _response;
            }
            try
            {
                var bookings = await _userEventRepository.GetUserBookingsAsync(userId);
                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
                _response.Result = bookings;
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string> { ex.Message };
            }
            return _response;
        }

        [HttpGet("{eventId}")]
        public async Task<ActionResult<GeneralResponse>> IsEventBooked(Guid eventId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.Unauthorized;
                return _response;
            }
            try
            {
                var isBooked = await _userEventRepository.IsEventBookedAsync(userId, eventId);
                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
                _response.Result = isBooked;
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string> { ex.Message };
            }
            return _response;
        }

        [HttpDelete("{eventId}")]
        public async Task<ActionResult<GeneralResponse>> CancelBooking(Guid eventId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.Unauthorized;
                return _response;
            }
            try
            {
                var result = await _userEventRepository.CancelBookingAsync(userId, eventId);
                if (!result)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.Errors = new List<string> { "Booking not found." };
                }
                else
                {
                    _response.isSuccess = true;
                    _response.statusCode = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string> { ex.Message };
            }
            return _response;
        }
    }
} 