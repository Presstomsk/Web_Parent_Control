using System;

namespace Web_Parent_Control.Models
{
    public class File
    {
        public Guid Id { get; set; } 
        public string FilePath { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }
}
