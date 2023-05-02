using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Harald.WebApi.Domain
{
    public class Capability
    {
        private List<CapabilityMember> _members = new List<CapabilityMember>();

        public IEnumerable<CapabilityMember> Members => _members.AsReadOnly();
        public string Id { get; private set; }
        public string Name { get; private set; }
        public ChannelId SlackChannelId { get; private set; }
        public string SlackUserGroupId { get; private set; }
        
        // For Entity Framework
        private Capability()
        {
        }

        private Capability(string id, string name, string slackChannelId, string slackUserGroupId)
        {
            Id = id;
            Name = name;
            SlackChannelId = new ChannelId(slackChannelId);
            SlackUserGroupId = slackUserGroupId;
        }

        public static Capability Create(string id, string name, string slackChannelId, string slackUserGroupId)
        {
            var capability = new Capability(
                id: id,
                name: name,
                slackChannelId: slackChannelId,
                slackUserGroupId: slackUserGroupId
            );

            return capability;
        }

        public ValueTask AddMember(string email) 
        {
            if (!_members.Any(m => m.Email == email))
            { 
                _members.Add(new CapabilityMember(email));
            }

            return new ValueTask();
        }

        public ValueTask RemoveMember(string email)
        {
            _members.Remove(_members.Find(m => m.Email == email));

            return new ValueTask();
        }

        public void SetUserGroupID(string slackUserGroupId)
        {
            SlackUserGroupId = slackUserGroupId;
        }
    }
}