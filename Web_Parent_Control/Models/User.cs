using System;
using System.Collections.Generic;
using System.Numerics;

namespace Web_Parent_Control.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Login { get; set; }
        public string Password { get; set; }
        public string ClientPC { get; set; }
        public virtual ICollection<SiteModel> Sites { get; set; }
        public virtual ICollection<FileModel> Files { get; set; }

        public User()
        {
            Sites = new List<SiteModel>();
            Files = new List<FileModel>();
        }
    }
}
