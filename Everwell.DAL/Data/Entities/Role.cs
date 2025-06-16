using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everwell.DAL.Data.Entities;

public enum RoleName
{
    Customer,
    Consultant,
    Staff,
    Manager,
    Admin
}

[Table("Roles")]
public class Role
{
    [Key]
    [Required]
    [Column("id")]
    public int Id { get; set; }
    
    [Required]
    [Column("name")]
    public RoleName Name { get; set; }
}