using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WorkScheduleApp.Data;
using WorkScheduleApp.Models;
namespace WorkScheduleApp.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _cfg;
        private readonly int _maxFailed;
        private readonly int _lockoutMinutes;

        public UserService(AppDbContext db, IConfiguration cfg)
        {
            _db = db;
            _cfg = cfg;
            _maxFailed = cfg.GetValue<int>("Lockout:MaxFailed", 5);
            _lockoutMinutes = cfg.GetValue<int>("Lockout:LockoutMinutes", 15);
        }

        public AppUser GetUser(string username)
        {
            return _db.AppUsers.AsNoTracking().FirstOrDefault(u => u.Username == username);
        }

        public bool VerifyPassword(string username, string password)
        {
            var u = _db.AppUsers.FirstOrDefault(x => x.Username == username);
            if (u == null) return false;

            // check lockout
            if (u.LockedUntil.HasValue && u.LockedUntil.Value > DateTime.UtcNow) return false;

            // verify PBKDF2
            try
            {
                var salt = Convert.FromBase64String(u.Salt);
                var expected = Convert.FromBase64String(u.PasswordHash);
                using var derive = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
                var computed = derive.GetBytes(expected.Length);
                if (FixedTimeEquals(computed, expected))
                {
                    // reset failed attempts
                    u.FailedAttempts = 0;
                    u.LockedUntil = null;
                    _db.SaveChanges();
                    return true;
                }
                else
                {
                    // increment
                    u.FailedAttempts++;
                    if (u.FailedAttempts >= _maxFailed)
                    {
                        u.LockedUntil = DateTime.UtcNow.AddMinutes(_lockoutMinutes);
                    }
                    _db.SaveChanges();
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            uint diff = (uint)a.Length ^ (uint)b.Length;
            int len = Math.Min(a.Length, b.Length);
            for (int i = 0; i < len; i++) diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }

        public void CreateUser(string username, string password)
        {
            if (_db.AppUsers.Any(u => u.Username == username)) return;
            byte[] salt = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            using var derive = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var hash = derive.GetBytes(32);

            var u = new AppUser
            {
                Username = username,
                Salt = Convert.ToBase64String(salt),
                PasswordHash = Convert.ToBase64String(hash),
                FailedAttempts = 0,
                LockedUntil = null
            };
            _db.AppUsers.Add(u);
            _db.SaveChanges();
        }

        // admin helper: reset lock
        public void ResetLock(string username)
        {
            var u = _db.AppUsers.FirstOrDefault(x => x.Username == username);
            if (u == null) return;
            u.FailedAttempts = 0;
            u.LockedUntil = null;
            _db.SaveChanges();
        }
    }
}
