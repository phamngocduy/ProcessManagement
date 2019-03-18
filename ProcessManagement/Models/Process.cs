//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProcessManagement.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Process
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Process()
        {
            this.Roles = new HashSet<Role>();
            this.Steps = new HashSet<Step>();
        }
    
        public int Id { get; set; }
        public int IdGroup { get; set; }
        public string IdOwner { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DataJson { get; set; }
        public System.DateTime Created_At { get; set; }
        public System.DateTime Updated_At { get; set; }
        public string Avatar { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Group Group { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Role> Roles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Step> Steps { get; set; }
    }
}
