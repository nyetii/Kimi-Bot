using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Core.Services
{
    public class Settings
    {
        public Profile Profile { get; set; } = new Profile();
    }

    public class Profile
    {
        public string? Status { get; set; } = null;
        public string? Link { get; set; } = null;
        public ActivityType ActivityType { get; set; } = (ActivityType)4;
        public UserStatus UserStatus { get; set; } = (UserStatus)1;
    }
}
