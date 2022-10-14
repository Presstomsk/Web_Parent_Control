using System;

namespace Web_Parent_Control.Models
{
    public class FileModel
    {
        public Guid Id { get; set; } 
        public string FilePath { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public Guid? UserId { get; set; }        
        public virtual User User { get; set; }
    }
}
