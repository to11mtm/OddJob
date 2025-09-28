using GlutenFree.OddJob.Manager.Blazor.Models;
using Microsoft.AspNetCore.Mvc;

namespace GlutenFree.OddJob.Manager.Blazor.Controllers
{
    [ApiController]
    [Route("api")]
    public class OddJobApiController : ControllerBase
    {
        private readonly OddJobRemotingHandler _remotingHandler;
        public OddJobApiController(OddJobRemotingHandler remotingHandler)
        {
            _remotingHandler = remotingHandler;
        }

        /// <summary>
        /// Gets the list of method names for a given queue.
        /// </summary>
        /// <param name="queueName">The queue name to filter methods by.</param>
        /// <returns>List of method names.</returns>
        [HttpGet("methods")]
        public async Task<ActionResult<List<string>>> GetMethods([FromQuery] string queueName)
        {
            try
            {
                var result = await _remotingHandler.Handle(new GetMethodsForQueueNameRequest { QueueName = queueName });
                return Ok(result.ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving methods: {ex.Message}");
            }
        }

        [HttpGet("queues")]
        public async Task<ActionResult<string[]>> GetQueues()
        {
            try
            {
                var result = await _remotingHandler.Handle(new QueueNameListRequest());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving queues: {ex.Message}");
            }
        }

        [HttpPost("jobs/search")]
        public async Task<ActionResult<JobMetadataResult[]>> SearchJobs([FromBody] JobSearchCriteria criteria)
        {
            try
            {
                var result = await _remotingHandler.Handle(criteria);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error searching jobs: {ex.Message}");
            }
        }

        [HttpPost("jobs/update")]
        public async Task<ActionResult<bool>> UpdateJob([FromBody] JobUpdateViewModel updateModel)
        {
            try
            {
                var result = await _remotingHandler.Handle(updateModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating job: {ex.Message}");
            }
        }

        [HttpPost("jobs/timeline")]
        public async Task<ActionResult<JobTimelineResult>> GetJobTimeline([FromBody] JobTimelineRequest request)
        {
            try
            {
                var result = await _remotingHandler.Handle(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving job timeline: {ex.Message}");
            }
        }
    }
}
