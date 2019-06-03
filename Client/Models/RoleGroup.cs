using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Models
{
    public class RoleGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public List<Role> Roles { get; set; }

        [ForeignKey("ContactPersonId")]
        public long ContactPersonId { get; set; }
        public virtual ContactPerson ContactPerson { get; set; }
    }
}
