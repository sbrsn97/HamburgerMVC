﻿using Hamburger.Models;
using Hamburger.Models.Entities;
using Hamburger.Models.ViewModels;
using Hamburger.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hamburger.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly SignInManager<User> signInManager;
        private readonly IUserService _userService;
        public readonly IEmailService _emailService;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, IPasswordHasher<User> passwordHasher, IUserService userService, IEmailService emailService)
        {
            this.signInManager = signInManager;
            this.passwordHasher = passwordHasher;
            this.userManager = userManager;
            this._userService = userService;
            this._emailService = emailService;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(UserVM vm)
        {
            if (ModelState.IsValid)
            {
                User appUser = new User()
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    UserName = vm.UserName,
                    Email = vm.Email,
                    Address = vm.Address,
                    PhoneNumber = vm.PhoneNumber,
                };
                bool EmailInUse = _userService.IsEmailInUse(appUser.Email);
                IdentityResult identityResult = await userManager.CreateAsync(appUser, vm.Password);
                if (identityResult.Succeeded && EmailInUse == false)
                {
                    var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(appUser);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = appUser.Id, token = confirmationToken }, Request.Scheme
                        );
                    string content = "Confirmation link:  " + confirmationLink;
                    SendEmail(appUser.Email, content);

                    await userManager.AddToRoleAsync(appUser, "Standard");
                    ViewBag.ErrorTitle = "Registration successful";
                    ViewBag.ErrorMessage = "Before you can Login, please confirm your email by clicking on the confirmation link we've sent to your email adress.";
                    return View("Error");
                }
            }
            TempData["error"] = "An error occurred.";
            return View(vm);
        }
        public async Task<IActionResult> Login()
        {
            return RedirectToAction("Register");
        }
        [HttpPost]
        public async Task<IActionResult> Login(UserVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.LoginVM.Email) || string.IsNullOrWhiteSpace(vm.LoginVM.Password))
            {
                return View("Register", vm);
            }
            if (vm.LoginVM != null)
            {
                User user = await userManager.FindByEmailAsync(vm.LoginVM.Email);
                if (user != null)
                {
                    await signInManager.SignOutAsync();
                    var result = await signInManager.PasswordSignInAsync(user, vm.LoginVM.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            return RedirectToAction("Login");
        }
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Error()
        {
            return View();
        }
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("index", "Home");
            }
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"The User ID {userId} is invalid";
                return View("NotFound");
            }
            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View();
            }
            ViewBag.ErrorTile = "Email can not be confirmed.";
            return View("Error");
        }
        public void SendEmail(string email,string content)
        {
            var message = new Message(new string[] {email},"Confirmation Link",content);
            _emailService.SendEmail(message);        
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
		public async Task<IActionResult> ForgotPassword(UserVM vm)
		{
			var user = await userManager.FindByEmailAsync(vm.Email);
            if (user != null && await userManager.IsEmailConfirmedAsync(user))
            {
			    var passwordToken = await userManager.GeneratePasswordResetTokenAsync(user);
				var resetLink = Url.Action("ResetPassword", "Account",
						new { userId = user.Id, token = passwordToken }, Request.Scheme
						);
				string content = "Password reset link:  " + resetLink;
				SendEmail(user.Email, content);
				ViewBag.ErrorMessage = "Password reset link has been sent to your email adress.";
                return View("NotFound");
			}	 
			return View();
		}
		public IActionResult ResetPassword()
		{
			return View();
		}
        [HttpPost]
		public async Task<IActionResult> ResetPassword(UserVM vm)
		{
			var user = await userManager.FindByEmailAsync(vm.Email);
			if (user == null)
			{
				var passwordToken = await userManager.GeneratePasswordResetTokenAsync(user);
				var resetLink = Url.Action("ResetPassword", "Account",
						new { userId = user.Id, token = passwordToken }, Request.Scheme
						);
				string content = "Password reset link:  " + resetLink;
                SendEmail(user.Email, content);              
			}
			return View();
		}
	}
}
