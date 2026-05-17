using Core.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCGApi.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    [Authorize(Policy = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogsController(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetByEntityType([FromQuery] string entityType = "Game")
        {
            var logs = await _auditLogRepository.GetByEntityTypeAsync(entityType);
            return Ok(logs);
        }
    }
}
