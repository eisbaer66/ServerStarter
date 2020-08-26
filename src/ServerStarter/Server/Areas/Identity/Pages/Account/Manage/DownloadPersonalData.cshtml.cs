using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Models;

namespace ServerStarter.Server.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;
        private readonly IQueryable<UserQueueStatistics> _statistics;

        public DownloadPersonalDataModel(
            UserManager<ApplicationUser> userManager,
            ILogger<DownloadPersonalDataModel> logger,
            IQueryable<UserQueueStatistics> statistics)
        {
            _userManager = userManager;
            _logger = logger;
            _statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

            // Only include personal data for download
            var personalData = GetPersonalDataProperties(user);

            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            var statistics = await _statistics.Where(s => s.UserId == user.Id)
                                              .FirstOrDefaultAsync();
            var statisticsProperties = GetPersonalDataProperties(statistics);
            var pd = new
                     {
                         User= personalData,
                         Statistics = statisticsProperties,
                     };

            Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(pd), "application/json");
        }

        private Dictionary<string, string> GetPersonalDataProperties<T>(T item)
        {
            var personalData = new Dictionary<string, string>();
            if (item == null)
                return null;

            var personalDataProps = typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(item)?.ToString() ?? "null");
            }

            return personalData;
        }
    }
}
