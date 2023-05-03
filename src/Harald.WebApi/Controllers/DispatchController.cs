using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Harald.WebApi.Domain;
using Harald.Infrastructure.Slack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Harald.Infrastructure.Slack.Exceptions;

namespace Harald.WebApi.Controllers
{
    [Route("api/v1/messages")]
    [ApiController]
    public class DispatchController : ControllerBase
    {
        private readonly ISlackFacade _slackFacade;
        private readonly ICapabilityRepository _capabilityRepository;

        public DispatchController(
            ISlackFacade slackFacade,
            ICapabilityRepository capabilityRepository)
        {
            _slackFacade = slackFacade;
            _capabilityRepository = capabilityRepository;
        }

        [HttpPost]
        public async Task<IActionResult> DispatchMessage(DispatchMessageInput input)
        {
            if (input.CapabilityId.Equals(""))
            {
                return BadRequest("capabilityId is required.");
            }

            var capabilities = await _capabilityRepository.GetById(input.CapabilityId);

            if (capabilities.Any() == false)
            {
                return UnprocessableEntity($"No channel for capability ID '{input.CapabilityId}' Found");
            }

            foreach (var capability in capabilities)
            {
                try
                {
                    var sendNotificationToChannelResponse = await _slackFacade.SendNotificationToChannel(capability.SlackChannelId.ToString(), input.Message);

                    if (!sendNotificationToChannelResponse.Ok)
                    {
                        return StatusCode(
                            StatusCodes.Status503ServiceUnavailable,
                            $"An error occured trying to send notification: {sendNotificationToChannelResponse.Error}");
                        }
                    }
                catch (Exception exp)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        $"An error occured trying to send notification: {exp.Message}");
                }
            }

            return Accepted();
        }
    }

    public class DispatchMessageInput
    {
        [Required] public string CapabilityId { get; set; }

        [Required] public string Message { get; set; }
    }
}