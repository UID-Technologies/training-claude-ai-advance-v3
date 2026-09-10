using System;
using System.Web.Http;
using LegacyBanking.Business;
using LegacyBanking.Domain.Models;

namespace LegacyBanking.Web.Controllers
{
    public class TransferController : ApiController
    {
        [HttpPost]
        [Route("api/transfers")]
        public IHttpActionResult Transfer(TransferRequest request)
        {
            try
            {
                // Legacy issue:
                // no injected service and no authorization policy.
                var service = new BankingService();

                var reference =
                    service.TransferMoney(request);

                return Ok(new
                {
                    Success = true,
                    Reference = reference
                });
            }
            catch (Exception ex)
            {
                // Legacy issue: exposes stack trace/internal details.
                return BadRequest(ex.ToString());
            }
        }
    }
}
