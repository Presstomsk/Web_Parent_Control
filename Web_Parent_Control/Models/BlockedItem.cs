using System;

namespace Web_Parent_Control.Models
{
    public class BlockedItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime BlockDate { get; set; } = DateTime.Now;       
        public bool Blocked { get; set; }
        public string Site { get; set; }
        public Guid? UserId { get; set; } 
    }
}
