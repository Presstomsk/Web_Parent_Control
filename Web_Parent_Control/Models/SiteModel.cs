using System;

namespace Web_Parent_Control.Models
{
    public class SiteModel
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public string Host { get; set; }
        public bool Flag { get; set; } = false;
    }
}
