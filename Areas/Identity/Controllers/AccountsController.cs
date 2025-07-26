using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookStore.Areas.Identity.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Identity")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IEmailSender emailSender;
        private readonly IUnitOfWork unitOfWork;

        public AccountsController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            this.emailSender = emailSender;
            this.unitOfWork = unitOfWork;
        }

        [HttpPost("Register")] 
        public async Task<IActionResult> Register([FromForm]RegisterRequest registerRequest)
        {
            var user = registerRequest.Adapt<ApplicationUser>();
           
            var result = await unitOfWork.UserManager.CreateAsync(user, registerRequest.Password);

            if (result.Succeeded)
            {
                await unitOfWork.UserManager.AddToRoleAsync(user, SD.Customer);

                // Send Confirmation Email
                var token = await  unitOfWork.UserManager.GenerateEmailConfirmationTokenAsync(user);
                var link = Url.Action("ConfirmEmail", "Accounts", new { userId = user.Id, token = token, area = "Identity" }, Request.Scheme);

                await emailSender.SendEmailAsync(user!.Email ?? "", "Confirm Your Account's Email", $"<h1> Please Confirm Your Account By Clicking <a href='{link}'>here</a></h1>");

                return Ok("User Created Successfully");
            }
            return BadRequest(result.Errors);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await unitOfWork.UserManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            var result = await unitOfWork.UserManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded) { 
                    return Ok("Your Email Confirmed Successfully");
            }else
            {
                return BadRequest($"{String.Join(",", result.Errors)}");
            }
        }
       
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromForm]LoginRequest loginRequest)
        {
            var user = await unitOfWork.UserManager.FindByEmailAsync(loginRequest.Email);
            if (user is null) return NotFound("Invalid Email");

            var result = await unitOfWork.SignInManager.PasswordSignInAsync(user, loginRequest.Password, loginRequest.RememberMe, true);

            if (result.Succeeded)
            {
                if (!user.EmailConfirmed)
                {
                    return BadRequest("Confirm Your Email, Please");
                }

                if (!user.LockoutEnabled)
                {
                    return BadRequest($"You have been blocked untill {user.LockoutEnd}");
                }
                var roles = await unitOfWork.UserManager.GetRolesAsync(user);

                var claims = new List<Claim> {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Name, user.UserName!),
                        new Claim(ClaimTypes.Email, user.Email!),
                        new Claim(ClaimTypes.Role, String.Join(" ", roles))
                };

                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("BhrawiBookShopBhrawiBookShopBhrawiBookShopBhrawi"));

                var signInCredential = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "https://localhost:7143",
                    audience: "https://localhost:4200,https://localhost:5000",
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(3),
                    signingCredentials: signInCredential
                    );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
            else if (result.IsLockedOut)
            {
                return BadRequest("Too Many attempts, try again after 5 min");
            }

            return BadRequest("Invalid Email OR Password");
        }
        [HttpPost("SignOut")]
        public new async Task<IActionResult> SignOut()
        {
            await unitOfWork.SignInManager.SignOutAsync();
            return Ok("Logged out Successfully");
        }

        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation([FromForm] ResendEmailConfirmationRequest resendEmailConfirmationRequest)
        {

            var user = await unitOfWork.UserManager.FindByEmailAsync(resendEmailConfirmationRequest.Email);

            if (user is null)
                return BadRequest("Invalid Email");


            // Send Confirmation Email
            var token = await unitOfWork.UserManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Accounts", new { userId = user.Id, token = token, area = "Identity" }, Request.Scheme);

            await emailSender.SendEmailAsync(user!.Email ?? "", "Confirm Your Account's Email", $"<h1> Please Confirm Your Account By Clicking <a href='{link}'>here</a></h1>");

            return Ok("Sent Email Successfully");
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromForm] ForgetPasswordRequest forgetPasswordRequest)
        {
            var user = await unitOfWork.UserManager.FindByEmailAsync(forgetPasswordRequest.Email);

            if (user is null)
                return BadRequest("Invalid Email");

            // Send OTP Email
            var otpNumber = new Random().Next(0, 999999).ToString("D6");

            var totalNumberOfOTPs = (await unitOfWork.ApplicationUserOTPRepository.GetAsync(e => e.ApplicationUserId == user.Id && DateTime.UtcNow.Day == e.SendDate.Day));

            if (totalNumberOfOTPs.Count() > 5)
            {
                return BadRequest("Many Requests of OTPs");
            }

            await unitOfWork.ApplicationUserOTPRepository.CreateAsync(new()
            {
                ApplicationUserId = user.Id,
                OTPNumber = otpNumber,
                Reason = "ForgetPassword",
                SendDate = DateTime.UtcNow,
                Status = false,
                ValidTo = DateTime.UtcNow.AddMinutes(30)
            });

            await emailSender.SendEmailAsync(user!.Email ?? "", "Reset Password OTP", $"<h1>Reset Password using OTP: {otpNumber}</h1>");

            return Ok("Send OTP to your Email Successfully");
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordRequest resetPasswordRequest)
        {
            var user = await unitOfWork.UserManager.FindByIdAsync(resetPasswordRequest.UserId);

            if (user is null)
                return NotFound();

            var lastOTP = (await unitOfWork.ApplicationUserOTPRepository.GetAsync(e => e.ApplicationUserId == resetPasswordRequest.UserId)).OrderBy(e => e.Id).LastOrDefault();

            if (lastOTP is not null)
            {
                if (lastOTP.OTPNumber == resetPasswordRequest.OTP && (lastOTP.ValidTo - DateTime.UtcNow).TotalMinutes < 30 && !lastOTP.Status)
                {
                    var token = await unitOfWork.UserManager.GeneratePasswordResetTokenAsync(user);
                    var result = await unitOfWork.UserManager.ResetPasswordAsync(user, token, resetPasswordRequest.Password);

                    if (result.Succeeded)
                    {
                        lastOTP.Status = true;
                        await unitOfWork.ApplicationUserOTPRepository.CommitAsync();
                        return Ok("Your Password Resetted Successfully");
                    }
                    else
                    {
                        return BadRequest($"{String.Join(",", result.Errors)}");
                    }
                }
            }

            return BadRequest("Invalid OR Expired OTP");
        }
    }
}
