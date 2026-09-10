using System;

namespace LegacyBanking.Domain.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
