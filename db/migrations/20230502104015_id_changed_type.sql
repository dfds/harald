-- 2023-05-02 10:40:15 : capability_id_changed_type

ALTER TABLE "CapabilityMember"
DROP CONSTRAINT "FK_CapabilityMember_CapabilityId_CapabilitySlackChannelId";

alter table public."Capability" alter column "Id" type varchar(255);
alter table public."CapabilityMember" alter column "Id" type varchar(255);
alter table public."CapabilityMember" alter column "CapabilityId" type varchar(255);

ALTER TABLE "CapabilityMember"
ADD CONSTRAINT "FK_CapabilityMember_CapabilityId_CapabilitySlackChannelId" FOREIGN KEY ("CapabilityId", "CapabilitySlackChannelId") REFERENCES "Capability" ("Id", "SlackChannelId") ON DELETE RESTRICT;