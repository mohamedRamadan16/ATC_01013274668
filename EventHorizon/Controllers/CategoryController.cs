using AutoMapper;
using EventHorizon.DataAccess.Persistence;
using EventHorizon.DataAccess.Repository.IRepository;
using EventHorizon.Models.DTOs.Category;
using EventHorizon.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventHorizon.Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly GeneralResponse _response;
        private readonly IMapper _mapper;
        public CategoryController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _response = new GeneralResponse();
            this.categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        //[ProducesResponseType(200)]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GeneralResponse>> GetAll()
        {
            try
            {
                List<Category>? categories = await categoryRepository.GetAllAsync();

                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
                if (categories == null || categories.Count == 0)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return _response;
                }
                
                _response.Result = _mapper.Map<List<CategoryDTO>>(categories);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpGet("{id:int}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GeneralResponse>> Get([FromRoute] int id)
        {
            try
            {
                if(id <= 0)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return _response;
                }
                Category? category = await categoryRepository.GetAsync(c => c.Id == id);
                if(category == null)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return _response;
                }
                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
                _response.Result = category;
            }
            catch
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string>() { "An error occurred while retrieving the category." };
            }
            return _response;
        }

        [Authorize(Roles = RolesConstant.Admin)]
        [HttpPost]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<GeneralResponse>> Create([FromBody] CategoryCreateDTO categoryCreateDTO)
        {
            try
            {
                if (categoryCreateDTO == null)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return _response;
                }
                Category category = new(){  Name = categoryCreateDTO.Name };
                await categoryRepository.CreateAsync(category);
                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
                _response.Result = category;
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [Authorize(Roles = RolesConstant.Admin)]
        [HttpPut]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GeneralResponse>> Update([FromBody] CategoryUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || updateDTO.Id <= 0)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return _response;
                }
                Category? category = await categoryRepository.GetAsync(c => c.Id == updateDTO.Id, tracked:false);
                if (category == null)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return _response;
                }
                category = _mapper.Map(updateDTO, category);
                await categoryRepository.UpdateAsync(category);
                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<CategoryDTO>(category);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [Authorize(Roles = RolesConstant.Admin)]
        [HttpDelete("{id:int}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GeneralResponse>> Delete(int id)
        {
            if(id <= 0)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return _response;
            }
            try
            {
                Category? category = await categoryRepository.GetAsync(c => c.Id == id);
                if (category == null)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return _response;
                }
                await categoryRepository.RemoveAsync(category);
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
