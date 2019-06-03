using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Models
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Value { get; set; }

        [ForeignKey("RoleGroupId")]
        public long RoleGroupId { get; set; }
        public virtual RoleGroup RoleGroup { get; set; }
    }
}
