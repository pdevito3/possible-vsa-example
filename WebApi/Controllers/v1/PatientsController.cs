namespace WebApi.Controllers.v1
{
    using Application.Dtos.Patient;
    using Application.Wrappers;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using static WebApi.Features.Patients.CreatePatient;
    using static WebApi.Features.Patients.DeletePatient;
    using static WebApi.Features.Patients.GetPatient;
    using static WebApi.Features.Patients.GetPatientList;
    using static WebApi.Features.Patients.PatchPatient;
    using static WebApi.Features.Patients.UpdatePatient;

    [ApiController]
    [Route("api/Patients")]
    [ApiVersion("1.0")]
    //[AllowAnonymous]
    public class PatientsController : Controller
    {
        private readonly IMediator _mediator;

        public PatientsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Gets a list of all Patients.
        /// </summary>
        /// <response code="200">Patient list returned successfully.</response>
        /// <response code="400">Patient has missing/invalid values.</response>
        /// <response code="401">This request was not able to be authenticated.</response>
        /// <response code="403">The required permissions to access this resource were not present in the given request.</response>
        /// <response code="500">There was an error on the server while creating the Patient.</response>
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
        [ProducesResponseType(typeof(Response<PagedList<PatientDto>>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        [Authorize(Policy = "CanReadPatients")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpGet(Name = "GetPatients")]
        public async Task<IActionResult> GetPatients([FromQuery] PatientParametersDto patientParametersDto)
        {
            var patientQuery = new PatientListQuery(patientParametersDto);
            var patientsDto = await _mediator.Send(patientQuery);

            var paginationMetadata = new
            {
                totalCount = patientsDto.TotalCount,
                pageSize = patientsDto.PageSize,
                currentPageSize = patientsDto.CurrentPageSize,
                currentStartIndex = patientsDto.CurrentStartIndex,
                currentEndIndex = patientsDto.CurrentEndIndex,
                pageNumber = patientsDto.PageNumber,
                totalPages = patientsDto.TotalPages,
                hasPrevious = patientsDto.HasPrevious,
                hasNext = patientsDto.HasNext
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));

            var response = new Response<PagedList<PatientDto>>(patientsDto);

            return Ok(response);
        }

        /// <summary>
        /// Gets a single Patient by ID.
        /// </summary>
        /// <response code="200">Patient record returned successfully.</response>
        /// <response code="400">Patient has missing/invalid values.</response>
        /// <response code="401">This request was not able to be authenticated.</response>
        /// <response code="403">The required permissions to access this resource were not present in the given request.</response>
        /// <response code="500">There was an error on the server while creating the Patient.</response>
        [ProducesResponseType(typeof(Response<PatientDto>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        [Authorize(Policy = "CanReadPatients")]
        [Produces("application/json")]
        [HttpGet("{patientId}", Name = "GetPatient")]
        public async Task<ActionResult<PatientDto>> GetPatient(Guid patientId, CancellationToken cancellationToken)
        {
            var patientQuery = new PatientQuery(patientId);
            var patientDto = await _mediator.Send(patientQuery, cancellationToken);

            if (patientDto == null)
            {
                return NotFound();
            }

            var response = new Response<PatientDto>(patientDto);

            return Ok(response);
        }

        /// <summary>
        /// Creates a new Patient record.
        /// </summary>
        /// <response code="201">Patient created.</response>
        /// <response code="400">Patient has missing/invalid values.</response>
        /// <response code="401">This request was not able to be authenticated.</response>
        /// <response code="403">The required permissions to access this resource were not present in the given request.</response>
        /// <response code="500">There was an error on the server while creating the Patient.</response>
        [ProducesResponseType(typeof(Response<PatientDto>), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        [Authorize(Policy = "CanAddPatients")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPost]
        public async Task<IActionResult> AddPatient([FromBody] PatientForCreationDto patientForCreation, CancellationToken cancellationToken)
        {
            try
            {
                var creationCommand = new PatientForCreationCommand(patientForCreation);
                var patientDto = await _mediator.Send(creationCommand, cancellationToken);
                var response = new Response<PatientDto>(patientDto);

                return CreatedAtRoute("GetPatient",
                    new { patientDto.PatientId },
                    response);
            }
            catch (Exception e)
            {
                // add a specific catch for Notfound
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Deletes an existing Patient record.
        /// </summary>
        /// <response code="201">Patient deleted.</response>
        /// <response code="400">Patient has missing/invalid values.</response>
        /// <response code="401">This request was not able to be authenticated.</response>
        /// <response code="403">The required permissions to access this resource were not present in the given request.</response>
        /// <response code="500">There was an error on the server while creating the Patient.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        [Authorize(Policy = "CanDeletePatients")]
        [Produces("application/json")]
        [HttpDelete("{patientId}")]
        public async Task<ActionResult> DeletePatient(Guid patientId, CancellationToken cancellationToken)
        {
            var patientQuery = new PatientForDeleteCommand(patientId);
            await _mediator.Send(patientQuery, cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Updates an entire existing Patient.
        /// </summary>
        /// <response code="201">Patient updated.</response>
        /// <response code="400">Patient has missing/invalid values.</response>
        /// <response code="401">This request was not able to be authenticated.</response>
        /// <response code="403">The required permissions to access this resource were not present in the given request.</response>
        /// <response code="500">There was an error on the server while creating the Patient.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        [Authorize(Policy = "CanUpdatePatients")]
        [Produces("application/json")]
        [HttpPut("{patientId}")]
        public async Task<IActionResult> UpdatePatient(Guid patientId, PatientForUpdateDto patient)
        {
            // add error handling
            var updateCommand = new PatientForUpdateCommand(patientId, patient);
            await _mediator.Send(updateCommand);
            return NoContent();
        }

        /// <summary>
        /// Updates specific properties on an existing Patient.
        /// </summary>
        /// <response code="201">Patient updated.</response>
        /// <response code="400">Patient has missing/invalid values.</response>
        /// <response code="401">This request was not able to be authenticated.</response>
        /// <response code="403">The required permissions to access this resource were not present in the given request.</response>
        /// <response code="500">There was an error on the server while creating the Patient.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        [Authorize(Policy = "CanUpdatePatients")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPatch("{patientId}")]
        public async Task<IActionResult> PartiallyUpdatePatient(Guid patientId, JsonPatchDocument<PatientForUpdateDto> patchDoc, CancellationToken cancellationToken)
        {
            var patientPatchCommand = new PatientForPatchCommand(patientId, patchDoc);
            await _mediator.Send(patientPatchCommand, cancellationToken);

            return NoContent();
        }
    }
}