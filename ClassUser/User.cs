﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModeratorApp.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // "Client" или "Moderator"
    }
    public class UserData
    {
        public List<User> Users { get; set; }
    }


}
