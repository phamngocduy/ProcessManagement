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
    
    public partial class Notify
    {
        public int Id { get; set; }
        public string IdUser { get; set; }
        public string Content { get; set; }
        public string viContent { get; set; }
        public string ActionLink { get; set; }
        public bool isRead { get; set; }
        public System.DateTime Created_At { get; set; }
        public System.DateTime Updated_At { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
    }
}