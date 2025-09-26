using System;

namespace WorkScheduleApp.Models
{
    public class AppUser
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; } // base64
        public string Salt { get; set; } // base64
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
    }
}
