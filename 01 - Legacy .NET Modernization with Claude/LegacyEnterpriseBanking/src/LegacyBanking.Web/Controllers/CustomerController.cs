using System.Web.Http;
using LegacyBanking.Business;
using LegacyBanking.Data.Repositories;

namespace LegacyBanking.Web.Controllers
{
    public class CustomerController : ApiController
    {
        [HttpGet]
        [Route("api/customers/search")]
        public IHttpActionResult Search(string q)
        {
            var result =
                new CustomerRepository()
                    .SearchByName(q);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/customers/{id}/dashboard")]
        public IHttpActionResult Dashboard(int id)
        {
            return Ok(
                new BankingService()
                    .GetCustomerDashboard(id));
        }

        [HttpPost]
        [Route("api/customers/register")]
        public IHttpActionResult Register(dynamic request)
        {
            // Legacy issue: dynamic request model.
            var id =
                new CustomerOnboardingService().Register(
                    (string)request.fullName,
                    (string)request.email,
                    (string)request.mobile,
                    (string)request.nationalId);

            return Ok(new { CustomerId = id });
        }
    }
}
