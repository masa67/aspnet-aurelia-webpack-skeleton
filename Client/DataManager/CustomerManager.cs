using Client.Models;
using Client.Query;
using Client.Repository;
using Microsoft.EntityFrameworkCore;
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
            return _customerContext.Customers.Include(c => c.ContactPersons).ToList();
        }

        public Customer Get(long id)
        {
            return _customerContext.Customers
                .Include(c => c.ContactPersons)
                .FirstOrDefault(e => e.Id == id);
        }

        public IEnumerable<Customer> Query(Client.Query.Query query)
        {
            var predicate = QueryHelper.GenerateWhere<Customer>(query);

            return _customerContext.Customers.Where(predicate).ToList();
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
