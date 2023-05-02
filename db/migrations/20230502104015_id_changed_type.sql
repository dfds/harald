-- 2023-05-02 10:40:15 : capability_id_changed_type

alter table public."Capability" alter column "Id" type varchar(255);
alter table public."CapabilityMember" alter column "Id" type varchar(255);
alter table public."CapabilityMember" alter column "CapabilityId" type varchar(255);
