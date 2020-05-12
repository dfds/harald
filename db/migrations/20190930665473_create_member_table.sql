CREATE TABLE "CapabilityMember" (
"Id" uuid NOT NULL,
"Email" text NULL,
"CapabilityId" uuid NULL,
"CapabilitySlackChannelId" text NULL,
CONSTRAINT "PK_CapabilityMember" PRIMARY KEY ("Id"),
CONSTRAINT "FK_CapabilityMember_CapabilityId_CapabilitySlackChannelId" FOREIGN KEY ("CapabilityId", "CapabilitySlackChannelId") REFERENCES "Capability" ("Id", "SlackChannelId") ON DELETE RESTRICT