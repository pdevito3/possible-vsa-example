namespace WebApi.Controllers.v1
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using Application.Dtos.Sample;
    using Application.Interfaces.Sample;
    using Application.Validation.Sample;
    using Domain.Entities;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using Application.Wrappers;

    [ApiController]
    [Route("api/Samples")]
    [ApiVersion("1.0")]
    public class SamplesController: Controller
    {
        private readonly ISampleRepository _sampleRepository;
        private readonly IMapper _mapper;

        public SamplesController(ISampleRepository sampleRepository
            , IMapper mapper)
        {
            _sampleRepository = sampleRepository ??
                throw new ArgumentNullException(nameof(sampleRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }
        
        /// <summary>
        /// Gets a list of all Samples.
        /// </summary>
        /// <response code="200">Sample list returned successfully.</response>
        /// <response code="400">Sample has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Sample.</response>
        /// <remarks>
        /// Requests can be narrowed down with a variety of query string values:
        /// ## Query String Parameters
        /// - **PageNumber**: An integer value that designates the page of records that should be returned.
        /// - **PageSize**: An integer value that designates the number of records returned on the given page that you would like to return. This value is capped by the internal MaxPageSize.
        /// - **SortOrder**: A comma delimited ordered list of property names to sort by. Adding a `-` before the name switches to sorting descendingly.
        /// - **Filters**: A comma delimited list of fields to filter by formatted as `{Name}{Operator}{Value}` where
        ///     - {Name} is the name of a filterable property. You can also have multiple names (for OR logic) by enclosing them in brackets and using a pipe delimiter, eg. `(LikeCount|CommentCount)>10` asks if LikeCount or CommentCount is >10
        ///     - {Operator} is one of the Operators below
        ///     - {Value} is the value to use for filtering. You can also have multiple values (for OR logic) by using a pipe delimiter, eg.`Title@= new|hot` will return posts with titles that contain the text "new" or "hot"
        ///
        ///    | Operator | Meaning                       | Operator  | Meaning                                      |
        ///    | -------- | ----------------------------- | --------- | -------------------------------------------- |
        ///    | `==`     | Equals                        |  `!@=`    | Does not Contains                            |
        ///    | `!=`     | Not equals                    |  `!_=`    | Does not Starts with                         |
        ///    | `>`      | Greater than                  |  `@=*`    | Case-insensitive string Contains             |
        ///    | `&lt;`   | Less than                     |  `_=*`    | Case-insensitive string Starts with          |
        ///    | `>=`     | Greater than or equal to      |  `==*`    | Case-insensitive string Equals               |
        ///    | `&lt;=`  | Less than or equal to         |  `!=*`    | Case-insensitive string Not equals           |
        ///    | `@=`     | Contains                      |  `!@=*`   | Case-insensitive string does not Contains    |
        ///    | `_=`     | Starts with                   |  `!_=*`   | Case-insensitive string does not Starts with |
        /// </remarks>
        [ProducesResponseType(typeof(Response<IEnumerable<SampleDto>>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpGet(Name = "GetSamples")]
        public async Task<IActionResult> GetSamples([FromQuery] SampleParametersDto sampleParametersDto)
        {
            var samplesFromRepo = await _sampleRepository.GetSamplesAsync(sampleParametersDto);

            var paginationMetadata = new
            {
                totalCount = samplesFromRepo.TotalCount,
                pageSize = samplesFromRepo.PageSize,
                currentPageSize = samplesFromRepo.CurrentPageSize,
                currentStartIndex = samplesFromRepo.CurrentStartIndex,
                currentEndIndex = samplesFromRepo.CurrentEndIndex,
                pageNumber = samplesFromRepo.PageNumber,
                totalPages = samplesFromRepo.TotalPages,
                hasPrevious = samplesFromRepo.HasPrevious,
                hasNext = samplesFromRepo.HasNext
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));

            var samplesDto = _mapper.Map<IEnumerable<SampleDto>>(samplesFromRepo);
            var response = new Response<IEnumerable<SampleDto>>(samplesDto);

            return Ok(response);
        }
        
        /// <summary>
        /// Gets a single Sample by ID.
        /// </summary>
        /// <response code="200">Sample record returned successfully.</response>
        /// <response code="400">Sample has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Sample.</response>
        [ProducesResponseType(typeof(Response<SampleDto>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        [HttpGet("{sampleId}", Name = "GetSample")]
        public async Task<ActionResult<SampleDto>> GetSample(Guid sampleId)
        {
            var sampleFromRepo = await _sampleRepository.GetSampleAsync(sampleId);

            if (sampleFromRepo == null)
            {
                return NotFound();
            }

            var sampleDto = _mapper.Map<SampleDto>(sampleFromRepo);
            var response = new Response<SampleDto>(sampleDto);

            return Ok(response);
        }
        
        /// <summary>
        /// Creates a new Sample record.
        /// </summary>
        /// <response code="201">Sample created.</response>
        /// <response code="400">Sample has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Sample.</response>
        [ProducesResponseType(typeof(Response<SampleDto>), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPost]
        public async Task<ActionResult<SampleDto>> AddSample([FromBody]SampleForCreationDto sampleForCreation)
        {
            var validationResults = new SampleForCreationDtoValidator().Validate(sampleForCreation);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            var sample = _mapper.Map<Sample>(sampleForCreation);
            await _sampleRepository.AddSample(sample);
            var saveSuccessful = await _sampleRepository.SaveAsync();

            if(saveSuccessful)
            {
                var sampleFromRepo = await _sampleRepository.GetSampleAsync(sample.SampleId);
                var sampleDto = _mapper.Map<SampleDto>(sampleFromRepo);
                var response = new Response<SampleDto>(sampleDto);
                
                return CreatedAtRoute("GetSample",
                    new { sampleDto.SampleId },
                    response);
            }

            return StatusCode(500);
        }
        
        /// <summary>
        /// Deletes an existing Sample record.
        /// </summary>
        /// <response code="201">Sample deleted.</response>
        /// <response code="400">Sample has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Sample.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        [HttpDelete("{sampleId}")]
        public async Task<ActionResult> DeleteSample(Guid sampleId)
        {
            var sampleFromRepo = await _sampleRepository.GetSampleAsync(sampleId);

            if (sampleFromRepo == null)
            {
                return NotFound();
            }

            _sampleRepository.DeleteSample(sampleFromRepo);
            await _sampleRepository.SaveAsync();

            return NoContent();
        }
        
        /// <summary>
        /// Updates an entire existing Sample.
        /// </summary>
        /// <response code="201">Sample updated.</response>
        /// <response code="400">Sample has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Sample.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        [HttpPut("{sampleId}")]
        public async Task<IActionResult> UpdateSample(Guid sampleId, SampleForUpdateDto sample)
        {
            var sampleFromRepo = await _sampleRepository.GetSampleAsync(sampleId);

            if (sampleFromRepo == null)
            {
                return NotFound();
            }

            var validationResults = new SampleForUpdateDtoValidator().Validate(sample);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            _mapper.Map(sample, sampleFromRepo);
            _sampleRepository.UpdateSample(sampleFromRepo);

            await _sampleRepository.SaveAsync();

            return NoContent();
        }
        
        /// <summary>
        /// Updates specific properties on an existing Sample.
        /// </summary>
        /// <response code="201">Sample updated.</response>
        /// <response code="400">Sample has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Sample.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPatch("{sampleId}")]
        public async Task<IActionResult> PartiallyUpdateSample(Guid sampleId, JsonPatchDocument<SampleForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var existingSample = await _sampleRepository.GetSampleAsync(sampleId);

            if (existingSample == null)
            {
                return NotFound();
            }

            var sampleToPatch = _mapper.Map<SampleForUpdateDto>(existingSample); // map the sample we got from the database to an updatable sample model
            patchDoc.ApplyTo(sampleToPatch, ModelState); // apply patchdoc updates to the updatable sample

            if (!TryValidateModel(sampleToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(sampleToPatch, existingSample); // apply updates from the updatable sample to the db entity so we can apply the updates to the database
            _sampleRepository.UpdateSample(existingSample); // apply business updates to data if needed

            await _sampleRepository.SaveAsync(); // save changes in the database

            return NoContent();
        }
    }
}