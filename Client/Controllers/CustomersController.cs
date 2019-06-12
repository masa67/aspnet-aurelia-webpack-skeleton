using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Client.Controllers
{
    public class CustomersController : ODataController
    {
        private readonly CustomerContext _db;

        public CustomersController(CustomerContext context)
        {
            _db = context;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_db.Customers);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            return Ok(_db.Customers.FirstOrDefault(c => c.Id == key));
        }
    }
}
