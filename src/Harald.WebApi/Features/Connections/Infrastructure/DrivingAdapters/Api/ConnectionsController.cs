using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Harald.Infrastructure.Slack;
using Harald.WebApi.Domain;
using Harald.WebApi.Domain.Queries;
using Harald.WebApi.Features.Connections.Domain.Model;
using Harald.WebApi.Features.Connections.Domain.Queries;
using Harald.WebApi.Features.Connections.Infrastructure.DrivingAdapters.Api.Model;
using Harald.WebApi.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Harald.WebApi.Features.Connections.Infrastructure.DrivingAdapters.Api
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ConnectionsController : ControllerBase
    {
        private readonly ISlackService _slackService;
        private readonly ISlackFacade _slackFacade;
        private readonly ICapabilityRepository _capabilityRepository;

        private readonly IQueryHandler<FindConnectionsByClientTypeClientIdChannelTypeChannelId, IEnumerable<Connection>>
            _findConnectionsByClientTypeClientIdChannelTypeChannelIdQueryHandler;

        public ConnectionsController(
            IQueryHandler<FindConnectionsByClientTypeClientIdChannelTypeChannelId, IEnumerable<Connection>>
                findConnectionsByClientTypeClientIdChannelTypeChannelIdQueryHandler,
            ISlackFacade slackFacade,
            ICapabilityRepository capabilityRepository,
            ISlackService slackService)
        {
            _findConnectionsByClientTypeClientIdChannelTypeChannelIdQueryHandler =
                findConnectionsByClientTypeClientIdChannelTypeChannelIdQueryHandler;

            _slackFacade = slackFacade;
            _capabilityRepository = capabilityRepository;
            _slackService = slackService;
        }

        [HttpGet]
        public async Task<IActionResult> GetConnections(
            [FromQuery] string clientType,
            [FromQuery] string clientId,
            [FromQuery] string channelType,
            [FromQuery] string channelId
        )
        {
            var query = new FindConnectionsByClientTypeClientIdChannelTypeChannelId();

            if (!string.IsNullOrEmpty(clientType))
            {
                query.ClientType = ClientType.Create(clientType);
            }

            if (!string.IsNullOrEmpty(clientId))
            {
                query.ClientId = ClientId.Create(clientId);
            }

            if (!string.IsNullOrEmpty(channelType))
            {
                query.ChannelType = ChannelType.Create(channelType);
            }

            if (!string.IsNullOrEmpty(channelId))
            {
                query.ChannelId = ChannelId.Create(channelId);
            }

            IEnumerable<Connection> connections;

            try
            {
                connections =
                    await _findConnectionsByClientTypeClientIdChannelTypeChannelIdQueryHandler.HandleAsync(query);
            }
            catch (ValidationException validationException)
            {
                return StatusCode(
                    (int) HttpStatusCode.UnprocessableEntity,
                    new {message = validationException.MessageToUser}
                );
            }

            var connectionDtos = connections.Select(ConnectionDto.CreateFromConnection);

            return Ok(new ItemsEnvelope<ConnectionDto>(connectionDtos));
        }

        [HttpPost]
        public async Task<IActionResult> AddConnection(ConnectionDto connection)
        {
            if (string.IsNullOrEmpty(connection.ClientId))
            {
                return BadRequest("ClientId ID is required.");
            }

            if (string.IsNullOrEmpty(connection.ClientType))
            {
                return BadRequest("ClientType is required.");
            }
            
            if (string.IsNullOrEmpty(connection.ClientName))
            {
                return BadRequest("ClientName is required.");
            }            

            if (string.IsNullOrEmpty(connection.ChannelId))
            {
                return BadRequest("Channel ID is required.");
            }

            if (string.IsNullOrEmpty(connection.ChannelType))
            {
                return BadRequest("ChannelType is required.");
            }

            if (string.IsNullOrEmpty(connection.ChannelName))
            {
                return BadRequest("ChannelName is required.");
            }

            var userGroup = await _slackService.EnsureUserGroupExists(connection.ClientName);
            var capability = Capability.Create(connection.ClientId, connection.ClientName,
                connection.ChannelId, userGroup.Id);

            await _capabilityRepository.Add(capability);

            var response = await _slackFacade.JoinChannel(((ChannelName)connection.ChannelName).ToString());
            
            return Accepted(response.Channel);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteConnection(
            [FromQuery] string clientType,
            [FromQuery] string clientId,
            [FromQuery] string channelType,
            [FromQuery] string channelId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                return BadRequest(new {message = "ClientId ID is required."});
            }

            ClientType clientTypeValueObject;
            ChannelType channelTypeValueObject;

            try
            {
                clientTypeValueObject = string.IsNullOrEmpty(clientType) ? null : (ClientType) clientType;
                channelTypeValueObject = string.IsNullOrEmpty(channelType) ? null : (ChannelType) channelType;
            }
            catch (ValidationException validationException)
            {
                return StatusCode(
                    (int) HttpStatusCode.UnprocessableEntity,
                    new {message = validationException.MessageToUser}
                );
            }

            var getMatchedConnectionsQuery = new FindConnectionsByClientTypeClientIdChannelTypeChannelId(
                clientTypeValueObject,
                (ClientId) clientId,
                channelTypeValueObject,
                (ChannelId) channelId);

            var matchedConnections =
                await _findConnectionsByClientTypeClientIdChannelTypeChannelIdQueryHandler.HandleAsync(
                    getMatchedConnectionsQuery);

            foreach (var connection in matchedConnections)
            {
                await _slackFacade.LeaveChannel(connection.ChannelId.ToString());

                var getAllChannelConnectionsQuery = new FindConnectionsByClientTypeClientIdChannelTypeChannelId(
                    null,
                    null,
                    connection.ChannelType,
                    connection.ChannelId);

                var allChannelConnections =
                    await _findConnectionsByClientTypeClientIdChannelTypeChannelIdQueryHandler.HandleAsync(
                        getAllChannelConnectionsQuery);

                var capability = Capability.Create(connection.ClientId, connection.ClientName,
                    connection.ChannelId, "");
                await _capabilityRepository.Remove(capability);

                if (allChannelConnections.All(c => c.ClientId.ToString().Equals(clientId)))
                {
                    var channelsAll = await _slackFacade.GetConversations();
                    var channelsWhereConnectionIdAndChannelCreatorMatches = channelsAll.Channels.Where(ch =>
                        ch.Creator.Equals(_slackFacade.GetBotUserId())
                        &&
                        ch.Id.Equals(connection.ChannelId));

                    if (channelsWhereConnectionIdAndChannelCreatorMatches.Any())
                    {
                        await _slackFacade.ArchiveChannel(connection.ChannelId.ToString());
                    }
                }
            }

            return Accepted();
        }
    }
}