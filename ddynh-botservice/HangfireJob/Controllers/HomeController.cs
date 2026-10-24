using HangfireJob.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using example.invalid;
using System.Security.Principal;
using SearchScope = System.DirectoryServices.SearchScope;

namespace HangfireJob.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public IConfiguration _config { get; }
        public IOptions<LDAP> _ldap { get; }
        public HomeController(ILogger<HomeController> logger, IOptions<LDAP> ldap)
        {
            _logger = logger;
            _ldap = ldap;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string account, string password)
        {
            if (ValidateLDAPUser(_ldap.Value.Server,
                                 _ldap.Value.Port,
                                 $"{_ldap.Value.UserDomain}\\{account}",
                                 password))
            {
                HttpContext.Session.SetString("User", account);
                return Redirect("/hangfire");
            }
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.SetString("User", "");
            return Redirect("/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        static bool ValidateLDAPUser(string ldapserver, string port, string userId, string password)
        {
            try
            {
                using (var ldapConnection = new LdapConnection(
                        new LdapDirectoryIdentifier($"{ldapserver}:{port}")))
                {
                    ldapConnection.AuthType = AuthType.Basic;
                    ldapConnection.AutoBind = false;
                    ldapConnection.Timeout = new TimeSpan(0, 0, 0, 15);
                    var credential = new NetworkCredential(userId, password);
                    ldapConnection.Bind(credential);
                    Console.WriteLine("Successfully authenticated to ldap server " + ldapserver);
                    return true;
                }

            }
            catch (LdapException e)
            {
                Console.WriteLine(("Error with ldap server " + ldapserver + e.ToString()));
                return false;
            }
        }
    }
}