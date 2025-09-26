using Microsoft.AspNetCore.Mvc;
using WorkScheduleApp.Services;

namespace WorkScheduleApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserService _userService;
        public AuthController(UserService us) { _userService = us; }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập tài khoản và mật khẩu.";
                return View();
            }

            var user = _userService.GetUser(username);
            if (user == null)
            {
                ViewBag.Error = "Tài khoản không tồn tại.";
                return View();
            }

            if (user.LockedUntil.HasValue && user.LockedUntil.Value > System.DateTime.UtcNow)
            {
                ViewBag.Error = $"Tài khoản bị khóa đến {user.LockedUntil.Value.ToLocalTime()}.";
                return View();
            }

            bool ok = _userService.VerifyPassword(username, password);
            if (ok)
            {
                HttpContext.Session.SetString("username", username);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var u2 = _userService.GetUser(username);
                if (u2.LockedUntil.HasValue && u2.LockedUntil.Value > System.DateTime.UtcNow)
                    ViewBag.Error = "Tài khoản đã bị khóa do đăng nhập sai nhiều lần.";
                else
                    ViewBag.Error = "Sai mật khẩu.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string username, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }
            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu nhập lại không khớp.";
                return View();
            }
            var existing = _userService.GetUser(username);
            if (existing != null)
            {
                ViewBag.Error = "Tài khoản đã tồn tại.";
                return View();
            }

            _userService.CreateUser(username, password);
            ViewBag.Message = "Đăng ký thành công, vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("username");
            return RedirectToAction("Login");
        }
    }
}
