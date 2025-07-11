﻿using AutoMapper;
using EventHorizon.DataAccess.Persistence;
using EventHorizon.DataAccess.Repository.IRepository;
using EventHorizon.Models.DTOs.Event;
using EventHorizon.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace EventHorizon.Controllers
{
    [ApiController]
    [Route("api/event")]
    public class EventController : ControllerBase
    {
        private readonly IEventRepository eventRepository;
        private readonly GeneralResponse _response;
        private readonly IMapper _mapper;
        public EventController(IEventRepository eventRepository, IMapper mapper)
        {
            _response = new GeneralResponse();
            this.eventRepository = eventRepository;
            _mapper = mapper;
        }

        [HttpGet]
        //[ProducesResponseType(200)]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GeneralResponse>> GetAll(
                [FromQuery] string? searchQuery = null,
                [FromQuery] int pageSize = 0,
                [FromQuery] int pageNumber = 1)
        {
            try
            {
                Expression<Func<Event, bool>>? filter = null;

                // search by name, venue and description
                if (!string.IsNullOrEmpty(searchQuery))
                    filter = e => e.Venue.ToLower().Contains(searchQuery.ToLower()) || e.Name.ToLower().Contains(searchQuery.ToLower()) || e.Description.ToLower().Contains(searchQuery.ToLower());

                var events = await eventRepository.GetAllAsync(filter, pageSize: pageSize, pageNumber: pageNumber);

                if (events == null || events.Count == 0)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return _response;
                }

                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
                _response.Result = events;
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string> { ex.Message };
            }

            return _response;
        }

        [HttpGet("{id:guid}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GeneralResponse>> Get([FromRoute] Guid id)
        {
            try
            {
                Event? _event = await eventRepository.GetAsync(c => c.Id == id);
                if (_event == null)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return _response;
                }
                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
                _response.Result =  _mapper.Map<EventDTO>(_event);
            }
            catch
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string>() { "An error occurred while retrieving the event." };
            }
            return _response;
        }

        //[Authorize(Roles = $"{RolesConstant.Admin},{RolesConstant.Owner}")]
        [Authorize(Roles = $"{RolesConstant.Admin}")]
        [HttpPost]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<GeneralResponse>> Create([FromForm] EventCreateDTO eventCreateDTO)
        {
            if (eventCreateDTO == null)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return _response;
            }

            try
            {
                // Upload logic
                string imageUrl = null!;
                if (eventCreateDTO.Image != null)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    Directory.CreateDirectory(uploadsFolder); // Ensure directory exists
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(eventCreateDTO.Image.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await eventCreateDTO.Image.CopyToAsync(stream);
                    }
                    imageUrl = "/images/" + fileName;
                }

                Event _event = _mapper.Map<Event>(eventCreateDTO);
                _event.CreatedAt = DateTime.Now;
                _event.UpdatedAt = DateTime.Now;
                _event.OwnerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _event.ImageUrl = imageUrl;
                //_event.OwnerId = "8865c445-b137-445b-bacc-87e8f1a5b668"; // just for testing
                await eventRepository.CreateAsync(_event);
                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
                _response.Result = _event;
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        //[Authorize(Roles = $"{RolesConstant.Admin},{RolesConstant.Owner}")]
        [Authorize(Roles = $"{RolesConstant.Admin}")]
        [HttpPut]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GeneralResponse>> Update([FromForm] EventUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return _response;
                }
                Event? _event = await eventRepository.GetAsync(c => c.Id == updateDTO.Id, tracked: false);
                //string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (_event == null)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return _response;
                }

                // Upload logic
                string imageUrl = null!;
                if (updateDTO.Image != null)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    Directory.CreateDirectory(uploadsFolder); // Ensure directory exists
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(updateDTO.Image.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await updateDTO.Image.CopyToAsync(stream);
                    }
                    imageUrl = "/images/" + fileName;
                }

                // Authorize the owner of the event
                //if(_event.OwnerId != currentUserId)
                //{
                //    _response.isSuccess = false;
                //    _response.statusCode = HttpStatusCode.Forbidden;
                //    _response.Errors = new List<string>() { "You are not authorized to update this event." };
                //    return _response;
                //}

                _event = _mapper.Map(updateDTO, _event);

                _event.UpdatedAt = DateTime.Now;
                _event.OwnerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _event.ImageUrl = imageUrl;

                await eventRepository.UpdateAsync(_event);
                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<EventDTO>(_event);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        //[Authorize(Roles = $"{RolesConstant.Admin},{RolesConstant.Owner}")]
        [Authorize(Roles = $"{RolesConstant.Admin}")]
        [HttpDelete("{id:Guid}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GeneralResponse>> Delete([FromRoute]Guid id)
        {
            try
            {
                Event? _event = await eventRepository.GetAsync(c => c.Id == id);
                //string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (_event == null)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return _response;
                }

                //// Authorize the owner of the event
                //if (_event.OwnerId != currentUserId)
                //{
                //    _response.isSuccess = false;
                //    _response.statusCode = HttpStatusCode.Forbidden;
                //    _response.Errors = new List<string>() { "You are not authorized to update this event." };
                //    return _response;
                //}

                await eventRepository.RemoveAsync(_event);
                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string>() { ex.ToString() };
            }
            return _response;
        }
    }
}
