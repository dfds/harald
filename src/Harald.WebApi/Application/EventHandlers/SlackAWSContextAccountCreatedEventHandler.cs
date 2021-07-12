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
            var addUserCmd =
                $"Get-ADUser \"CN=IT BuildSource DevEx,OU=DFDS AS,OU=Mailboxes,OU=Accounts,OU=DFDS,DC=dk,DC=dfds,DC=root\" | Set-ADUser -Add @{{proxyAddresses=\"smtp:{domainEvent.Payload.RoleEmail}\"}}";
            var installToolsCmd =
                $"Get-WindowsCapability -Online | ? {{$_.Name -like 'Rsat.ActiveDirectory.DS-LDS.Tools*'}} | Add-WindowsCapability -Online";
            var addDeployCredentialsBash = $"AWS_ROLE_CLOUD_ADMIN=\"[Cloud Administrator Role ARN]\"\\\n" +
                                           $"AWS_ROLE_ADFS_ADMIN=\"[ADFS Administrator Role ARN]\"\\\n" +
                                           $"poetry run python ./kube_config_generator.py -r {domainEvent.Payload.CapabilityRootId} -a {domainEvent.Payload.AccountId}";
            var addDeployCredentialsPS = $"$ENV:AWS_ROLE_CLOUD_ADMIN=\"[Cloud Administrator Role ARN]\"\\\n" +
                                         $"$ENV:AWS_ROLE_ADFS_ADMIN=\"[ADFS Administrator Role ARN]\"\\\n" +
                                         $"poetry run python .\kube_config_generator.py -r {domainEvent.Payload.CapabilityRootId} -a {domainEvent.Payload.AccountId}";

            // poetry run python kube_config_generator.py -r {domainEvent.Payload.CapabilityRootId} -a {domainEvent.Payload.AccountId}
            var sb = new StringBuilder();

            sb.AppendLine($"*An AWS Context account has been created for ContextId: {domainEvent.Payload.ContextId}*");
            sb.AppendLine("\n_Add email address to shared mailbox_");
            sb.AppendLine("Execute the following Powershell command:");
            sb.AppendLine($"`{addUserCmd}`");
            // sb.AppendLine($"Should you not have RSAT tools installed, please do so with command:");
            // sb.AppendLine(installToolsCmd);
            sb.AppendLine($"\n_Generate k8s service account_");
            sb.AppendLine($"Execute the Python script from github.com/dfds/ce-toolbox/k8s-service-account-config-to-ssm.  Please ensure that the two environment variables are modified to include the correct Role ARNs.");
            sb.AppendLine($"Bash: `{addDeployCredentialsBash}`");
            sb.AppendLine($"Powershell: `{addDeployCredentialsPS}`");

            var hardCodedDedChannelId = new ChannelId("GFYE9B99Q");
            await _slackFacade.SendNotificationToChannel(hardCodedDedChannelId.ToString(), sb.ToString());

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
