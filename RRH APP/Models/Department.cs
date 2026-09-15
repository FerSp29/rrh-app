using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRH_APP.Models;

[Table("departments")]
public class Department
{
    public string Name { get; set; }

    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public decimal? Budget { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
