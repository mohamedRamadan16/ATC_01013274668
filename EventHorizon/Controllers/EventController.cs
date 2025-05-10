using AutoMapper;
using EventHorizon.DataAccess.Persistence;
using EventHorizon.DataAccess.Repository.IRepository;
using EventHorizon.Models.DTOs.Event;
using EventHorizon.Models.Models;
using Microsoft.AspNetCore.Mvc;
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
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GeneralResponse>> GetAll()
        {
            try
            {
                List<Event>? events = await eventRepository.GetAllAsync();

                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
                if (events == null || events.Count == 0)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return _response;
                }

                _response.Result = events;
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<GeneralResponse>> Create([FromBody] EventCreateDTO eventCreateDTO)
        {
            if (eventCreateDTO == null)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return _response;
            }

            try
            {
                Event _event = _mapper.Map<Event>(eventCreateDTO);
                _event.CreatedAt = DateTime.Now;
                _event.UpdatedAt = DateTime.Now;
                //_event.OwnerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _event.OwnerId = "8865c445-b137-445b-bacc-87e8f1a5b668"; // just for testing
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

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GeneralResponse>> Update([FromBody] EventUpdateDTO updateDTO)
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
                if (_event == null)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return _response;
                }
                _event = _mapper.Map(updateDTO, _event);
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

        [HttpDelete("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GeneralResponse>> Delete(Guid id)
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
