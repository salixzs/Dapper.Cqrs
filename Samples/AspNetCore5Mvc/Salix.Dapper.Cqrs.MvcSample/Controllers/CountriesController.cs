using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Salix.Dapper.Cqrs.MvcSample.BusinessLogic;

namespace Salix.Dapper.Cqrs.MvcSample.Controllers
{
    public class CountriesController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICountriesLogic _businessLogic;

        public CountriesController(ILogger<HomeController> logger, ICountriesLogic businessLogic)
        {
            _logger = logger;
            _businessLogic = businessLogic;
        }

        public IActionResult Get(int pageSize, int pageNum)
        {
            return View();
        }
    }
}
