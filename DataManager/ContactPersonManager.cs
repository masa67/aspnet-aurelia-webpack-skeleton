using Client.Models;
using Client.Repository;
using System.Collections.Generic;
using System.Linq;

namespace Client.DataManager
{
    public class ContactPersonManager : IDataRepository<ContactPerson>
    {
        readonly CustomerContext _customerContext;

        public ContactPersonManager(CustomerContext context)
        {
            _customerContext = context;
        }

        public IEnumerable<ContactPerson> GetAll()
        {
            return _customerContext.ContactPersons.ToList();
        }

        public ContactPerson Get(long id)
        {
            return _customerContext.ContactPersons
                  .FirstOrDefault(e => e.ContactPersonId == id);
        }

        public void Add(ContactPerson entity)
        {
            _customerContext.ContactPersons.Add(entity);
            _customerContext.SaveChanges();
        }

        public void Update(ContactPerson contactPerson, ContactPerson entity)
        {
            contactPerson.Name = entity.Name;

            _customerContext.SaveChanges();
        }

        public void Delete(ContactPerson contactPerson)
        {
            _customerContext.ContactPersons.Remove(contactPerson);
            _customerContext.SaveChanges();
        }
    }
}
