﻿using GoTransport.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GoTransport.Domain.Entities.App;

public partial class Role : IdentityRole<long>, IAuditableEntity
{
    /// <summary>
    /// ¿Está activo? (1-True, 0-False)
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Fecha de creación del registro
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha de la última modificación del registro
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Usuario que realizó la creación del registro
    /// </summary>
    public string CreationUser { get; set; } = null!;

    /// <summary>
    /// Usuario que realizó la última modificación del registro
    /// </summary>
    public string UpdateUser { get; set; } = null!;

    public virtual ICollection<UserRole> UserRoles { get; } = new List<UserRole>();

    public virtual ICollection<RoleClaim> RoleClaims { get; } = new List<RoleClaim>();
}