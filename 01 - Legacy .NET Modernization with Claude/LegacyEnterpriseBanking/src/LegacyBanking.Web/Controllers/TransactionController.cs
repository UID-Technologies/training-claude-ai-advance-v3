using System.Web.Http;
using LegacyBanking.Data.Repositories;

namespace LegacyBanking.Web.Controllers
{
    public class TransactionController : ApiController
    {
        [HttpGet]
        [Route("api/transactions/search")]
        public IHttpActionResult Search(
            string accountNumber,
            string status)
        {
            return Ok(
                new TransactionRepository()
                    .Search(accountNumber, status));
        }
    }
}
