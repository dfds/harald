using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harald.Infrastructure.Slack;
using Harald.Infrastructure.Slack.Dto;
using Harald.WebApi.Domain;
using Harald.WebApi.Domain.Queries;
using Harald.WebApi.Features.Connections.Domain.Model;
using Harald.WebApi.Features.Connections.Domain.Queries;

namespace Harald.WebApi.Features.Connections.Infrastructure.Persistence
{
    public class FindConnectionsByClientTypeClientIdChannelTypeChannelIdHandler : IQueryHandler<
        FindConnectionsByClientTypeClientIdChannelTypeChannelId, IEnumerable<Connection>>
    {
        private readonly ICapabilityRepository _capabilityRepository;
        private readonly ISlackFacade _slackFacade;

        public FindConnectionsByClientTypeClientIdChannelTypeChannelIdHandler(
            ICapabilityRepository capabilityRepository,
            ISlackFacade slackFacade
        )
        {
            _capabilityRepository = capabilityRepository;
            _slackFacade = slackFacade;
        }

        public async Task<IEnumerable<Connection>> HandleAsync(
            FindConnectionsByClientTypeClientIdChannelTypeChannelId query
        )
        {
            var capabilities = await QueryCapabilities(query);

            if (capabilities.Any() == false)
            {
                return Enumerable.Empty<Connection>();
            }

            var ConversationIds = capabilities.Select(c => c.SlackChannelId.ToString());

            var channelDtos = await GetSlackConversationsByIds(ConversationIds);

            var connections = MergeIntoConnections(capabilities, channelDtos);

            return connections;
        }

        private async Task<IEnumerable<Capability>> QueryCapabilities(
            FindConnectionsByClientTypeClientIdChannelTypeChannelId query)
        {
            if (query.ChannelType != null && !query.ChannelType.IsEmpty && !(query.ChannelType is ChannelTypeSlack))
            {
                return Enumerable.Empty<Capability>();
            }

            if (query.ClientType != null && !query.ClientType.IsEmpty && !(query.ClientType is ClientTypeCapability))
            {
                return Enumerable.Empty<Capability>();
            }

            var capabilities = await _capabilityRepository.GetAll();

            if (query.ChannelId != null && !query.ChannelId.IsEmpty)
            {
                capabilities = capabilities
                    .Where(c =>
                        c.SlackChannelId.Equals(query.ChannelId)
                    );
            }

            if (query.ClientId == null || query.ClientId.IsEmpty) {return capabilities;}


            string capabilityId = query.ClientId;

            capabilities = capabilities
                .Where(c =>
                    c.Id.Equals(capabilityId));


            return capabilities;
        }

        private async Task<IEnumerable<ChannelDto>> GetSlackConversationsByIds(IEnumerable<string> channelIds)
        {
            var conversations = await _slackFacade.GetConversations();

            var relevantConversations = conversations.Channels.Where(c => channelIds.Contains(c.Id));


            return relevantConversations;
        }


        private IEnumerable<Connection> MergeIntoConnections(
            IEnumerable<Capability> capabilities,
            IEnumerable<ChannelDto> channelDtos
        )
        {
            var connections = new List<Connection>();

            foreach (var channelDto in channelDtos)
            {
                var relevantCapability = capabilities.Where(c => c.SlackChannelId == channelDto.Id);

                connections.AddRange(
                    relevantCapability.Select(capability =>
                        new Connection(
                            new ClientTypeCapability(),
                            new ClientName(capability.Name),
                            new ClientId(capability.Id.ToString()),
                            new ChannelTypeSlack(),
                            ChannelName.Create(channelDto.Name),
                            new ChannelId(capability.SlackChannelId)
                        )
                    )
                );
            }

            return connections;
        }
    }
}