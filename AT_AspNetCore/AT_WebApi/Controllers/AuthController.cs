using AT_WebApi.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Data;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace AT_WebApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtBearerTokenSettings jwtBearerTokenSettings;
        private readonly UserManager<DetalheUsuario> userManager;
        private readonly ApplicationDbContext applicationDb;

        public AuthController(IOptions<JwtBearerTokenSettings> jwtTokenOptions, UserManager<DetalheUsuario> userManager, ApplicationDbContext applicationDb)
        {
            this.jwtBearerTokenSettings = jwtTokenOptions.Value;
            this.userManager = userManager;
            this.applicationDb = applicationDb;
        }

        [Authorize]
        [Route("AddAmigos")]
        public async Task<ActionResult<IEnumerable<DetalheUsuario>>> AdicionarAmigo()
        {
            var t = await userManager.Users.ToListAsync();
            return t;
        }

        [Authorize]
        [Route("ConfirmaAmizade/{id}")]
        public async Task EstabelecerRelacaoAsync(string id, [FromBody] string idSessao)
        {
            Amizade amizade = new Amizade()
            {
                UsuarioIdA = idSessao,
                UsuarioIdB = id
            };
            applicationDb.Amizade.Add(amizade);
            await applicationDb.SaveChangesAsync();
        }

        [Authorize]
        [Route("DeletarAmizade/{id}")]
        public async Task TirarRelacaoAsync(string id, [FromBody] string idSessao)
        {
            Amizade amizade = new Amizade()
            {
                UsuarioIdA = idSessao,
                UsuarioIdB = id
            };
            applicationDb.Amizade.Remove(amizade);
            await applicationDb.SaveChangesAsync();
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] DetalheUsuario detalheUsuario)
        {
            if (!ModelState.IsValid || detalheUsuario == null)
            {
                return new BadRequestObjectResult(new { Message = "User Registration Failed" });
            }

            var result = await userManager.CreateAsync(detalheUsuario, detalheUsuario.Password);
            if (!result.Succeeded)
            {
                var dictionary = new ModelStateDictionary();
                foreach (IdentityError error in result.Errors)
                {
                    dictionary.AddModelError(error.Code, error.Description);
                }

                return new BadRequestObjectResult(new { Message = "User Registration Failed", Errors = dictionary });
            }

            return Ok(new { Message = "User Registration Successful" });
        }

        [Authorize]
        [HttpPost]
        [Route("EditUser")]
        public async Task<IActionResult> EditUser([FromBody] DetalheUsuario detalheUsuario)
        {
            if (!ModelState.IsValid || detalheUsuario == null)
            {
                return new BadRequestObjectResult(new { Message = "User Registration Failed" });
            }



            var identityUser = await userManager.Users.ToListAsync();
            DetalheUsuario identityUserEncontrado = new DetalheUsuario();
            foreach (DetalheUsuario d in identityUser)
            {
                if (d.Id == detalheUsuario.Id)
                {
                    identityUserEncontrado = d;
                    break;
                }
            }

            identityUserEncontrado.UserName = detalheUsuario.UserName;
            identityUserEncontrado.Password = detalheUsuario.Password;
            identityUserEncontrado.Email = detalheUsuario.Email;
            identityUserEncontrado.PrimeiroNome = detalheUsuario.PrimeiroNome;
            identityUserEncontrado.SegundoNome = detalheUsuario.SegundoNome;
            identityUserEncontrado.Nascimento = detalheUsuario.Nascimento;
            identityUserEncontrado.AssuntosDeInteresse = detalheUsuario.AssuntosDeInteresse;
            identityUserEncontrado.PathImage = detalheUsuario.PathImage;

            await userManager.UpdateAsync(identityUserEncontrado);

            return Ok(new { Message = "User Registration Successful" });
        }

        [Authorize]
        [HttpPost]
        [Route("DetalhesPerfil")]
        public async Task<IActionResult> DetalhesPerfil([FromBody] string idSessao)
        {
            if (!ModelState.IsValid || idSessao == null)
            {
                return new BadRequestObjectResult(new { Message = "User Registration Failed" });
            }

            var identityUser = await userManager.Users.ToListAsync();
            DetalheUsuario identityUserEncontrado = new DetalheUsuario();
            foreach (DetalheUsuario d in identityUser)
            {
                if (d.Id == idSessao)
                {
                    identityUserEncontrado = d;
                    break;
                }
            }

            return Ok(identityUserEncontrado);
        }

        [Authorize]
        [Route("DetalhesAmigo")]
        public async Task<IActionResult> DetalhesAmigo([FromBody] string Id)
        {
            var identityUser = await userManager.Users.ToListAsync();
            DetalheUsuario identityUserEncontrado = new DetalheUsuario();
            foreach (DetalheUsuario d in identityUser)
            {
                if (d.Id == Id)
                {
                    identityUserEncontrado = d;
                    break;
                }
            }

            //return Ok(new { UserName = identityUserEncontrado.UserName, Email = identityUserEncontrado.Email, PathImage = identityUserEncontrado.PathImage, SegundoNome = identityUserEncontrado.SegundoNome, Nascimento = identityUserEncontrado.Nascimento, AssuntosDeInteresse = identityUserEncontrado.AssuntosDeInteresse });

            return Ok(identityUserEncontrado);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] CredencialLogin credentials)
        {

            DetalheUsuario identityUser;

            if (!ModelState.IsValid || credentials == null || (identityUser = await ValidateUser(credentials)) == null)
            {
                return new BadRequestObjectResult(new { Message = "Login failed" });
            }

            var token = GenerateToken(identityUser);
            return Ok(new { Token = token, Message = "Success", id = identityUser.Id, PrimeiroNome = identityUser.PrimeiroNome, SegundoNome = identityUser.SegundoNome, PathImage = identityUser.PathImage });
        }

        [Authorize]
        [HttpPost]
        [Route("Logout")]
        public IActionResult Logout()
        {
            // Well, What do you want to do here ?
            // Wait for token to get expired OR 
            // Maintain token cache and invalidate the tokens after logout method is called
            return Ok(new { Token = "", Message = "Logged Out" });
        }

        private async Task<DetalheUsuario> ValidateUser(CredencialLogin credentials)
        {           
            var identityUser = await userManager.Users.ToListAsync();
            DetalheUsuario identityUserEncontrado = new DetalheUsuario();
            foreach (DetalheUsuario d in identityUser)
            {
                if(d.UserName.ToUpper() == credentials.Username.ToUpper())
                {
                    identityUserEncontrado = d;
                }
            }
   
            if (identityUserEncontrado != null)
            {
                var result = userManager.PasswordHasher.VerifyHashedPassword(identityUserEncontrado, identityUserEncontrado.PasswordHash, credentials.Password);
                return result == PasswordVerificationResult.Failed ? null : identityUserEncontrado;
            }

            return null;
        }


        private object GenerateToken(DetalheUsuario identityUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtBearerTokenSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, identityUser.UserName.ToString()),
                    new Claim(ClaimTypes.Email, identityUser.Email)
                }),

                Expires = DateTime.UtcNow.AddSeconds(jwtBearerTokenSettings.ExpiryTimeInSeconds),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = jwtBearerTokenSettings.Audience,
                Issuer = jwtBearerTokenSettings.Issuer
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

