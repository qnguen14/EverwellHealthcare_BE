using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Data.Entities
{
    [Table("Service")]
    public class Service
    {
        [Key]
        [Required]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Column("description")]
        [StringLength(256)]
        public string Description { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; }
        
        [Required]
        [Column("created_at")]
        public DateOnly CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateOnly UpdatedAt { get; set; }
    }
}
