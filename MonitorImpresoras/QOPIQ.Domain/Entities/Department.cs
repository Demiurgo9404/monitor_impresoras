using System;
using System.Collections.Generic;
using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Representa un departamento o área funcional en la organización.
    /// </summary>
    public class Department : BaseAuditableEntity
    {
        /// <summary>
        /// Nombre del departamento.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Código único del departamento.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Descripción del departamento.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ID del departamento padre (para jerarquías).
        /// </summary>
        public Guid? ParentDepartmentId { get; set; }

        /// <summary>
        /// Departamento padre.
        /// </summary>
        public virtual Department ParentDepartment { get; set; }

        /// <summary>
        /// Subdepartamentos.
        /// </summary>
        public virtual ICollection<Department> SubDepartments { get; set; } = new List<Department>();

        /// <summary>
        /// Usuarios asignados a este departamento.
        /// </summary>
        public virtual ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Impresoras asignadas a este departamento.
        /// </summary>
        public virtual ICollection<Printer> Printers { get; set; } = new List<Printer>();
    }
}
