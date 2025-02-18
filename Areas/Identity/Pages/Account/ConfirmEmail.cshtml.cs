// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace WebApplication8.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ConfirmEmailModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            if (!string.IsNullOrEmpty(code))
            {
                var result = await _userManager.ConfirmEmailAsync(user, code);

                StatusMessage = result.Succeeded
                    ? "شكرًا لتأكيد بريدك الإلكتروني. يمكنك الآن تسجيل الدخول."
                    : "خطأ: رمز تأكيد غير صالح. يرجى طلب رابط تأكيد جديد.";
            }
            else
            {
                StatusMessage = "Error: يرجى التحقق من بريدك الإلكتروني لتأكيده، ثم حاول تسجيل الدخول مرة أخرى";
            }

            return Page();
        }
    }
}
