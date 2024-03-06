using System.Text;
using System.Threading.Tasks;
using Harald.WebApi.Domain;
using Harald.WebApi.Domain.Events;
using Harald.Infrastructure.Slack;

namespace Harald.WebApi.Application.EventHandlers
{
    public class SlackAwsContextAccountCreatedEventHandler : IEventHandler<AWSContextAccountCreatedDomainEvent>
    {
        private readonly ISlackFacade _slackFacade;
        private readonly ICapabilityRepository _capabilityRepository;

        public SlackAwsContextAccountCreatedEventHandler(ISlackFacade slackFacade,
            ICapabilityRepository capabilityRepository)
        {
            _slackFacade = slackFacade;
            _capabilityRepository = capabilityRepository;
        }

        public async Task HandleAsync(AWSContextAccountCreatedDomainEvent domainEvent)
        {
            var addDeployCredentialsBash = $"ROOT_ID={domainEvent.Payload.CapabilityRootId} ACCOUNT_ID={domainEvent.Payload.AccountId} ./kube-config-generator.sh";

            var sb = new StringBuilder();

            sb.AppendLine($"*An AWS Context account has been created for ContextId: {domainEvent.Payload.ContextId}*");
            sb.AppendLine($"\n_Generate k8s service account_");
            sb.AppendLine($"Ensure you have set the correct Kube Config for Hellman cluster: ");
            sb.AppendLine($"`export KUBECONFIG=~/.kube/hellman-saml.config`");
            sb.AppendLine($"Execute kube-config-generator.sh script from github.com/dfds/ce-toolbox/k8s-service-account-config-to-ssm");
            sb.AppendLine($"`{addDeployCredentialsBash}`");

            await _slackFacade.SendNotificationToChannel(_slackFacade.GetDefaultNotificationChannelId(), sb.ToString());

            // Send message to Capability Slack channel
            var capabilities = await _capabilityRepository.GetById(domainEvent.Payload.CapabilityId);

            foreach (var capability in capabilities)
            {
                await _slackFacade.SendNotificationToChannel(capability.SlackChannelId.ToString(),
                    $"Status update\n{SlackContextAddedToCapabilityDomainEventHandler.CreateTaskTable(true, false, false)}");
            }
        }
    }
}
