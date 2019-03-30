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
    
    public partial class Status
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Status()
        {
            this.ProcessRuns = new HashSet<ProcessRun>();
            this.StepRuns = new HashSet<StepRun>();
            this.TaskProcessRuns = new HashSet<TaskProcessRun>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> Create_At { get; set; }
        public Nullable<System.DateTime> Update_At { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProcessRun> ProcessRuns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StepRun> StepRuns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TaskProcessRun> TaskProcessRuns { get; set; }
    }
}
