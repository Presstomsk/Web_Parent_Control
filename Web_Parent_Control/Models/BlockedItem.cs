using System;

namespace Web_Parent_Control.Models
{
    public class BlockedItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public bool Blocked { get; set; }
        public string Site { get; set; }
    }
}
