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
    }
}
