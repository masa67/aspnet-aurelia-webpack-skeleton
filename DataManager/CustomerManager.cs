using Client.Models;
using Client.Repository;
using System.Collections.Generic;
using System.Linq;

namespace Client.DataManager
{
    public class CustomerManager : IDataRepository<Customer>
    {
        readonly CustomerContext _customerContext;

        public CustomerManager(CustomerContext context)
        {
            _customerContext = context;
        }

        public IEnumerable<Customer> GetAll()
        {
            return _customerContext.Customers.ToList();
        }

        public Customer Get(long id)
        {
            return _customerContext.Customers
                  .FirstOrDefault(e => e.CustomerId == id);
        }

        public void Add(Customer entity)
        {
            _customerContext.Customers.Add(entity);
            _customerContext.SaveChanges();
        }

        public void Update(Customer customer, Customer entity)
        {
            customer.Name = entity.Name;
            customer.NameExtension = entity.NameExtension;

            _customerContext.SaveChanges();
        }

        public void Delete(Customer customer)
        {
            _customerContext.Customers.Remove(customer);
            _customerContext.SaveChanges();
        }
    }
}
