using Core.Models;
using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AT_WebMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext applicationDb;

        public HomeController(ApplicationDbContext applicationDb)
        {
            this.applicationDb = applicationDb;
        }

        // GET: HomeController
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CriarUsuario()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarUsuario(DetalheUsuario detalheUsuario)
        {
            DetalheUsuario detalheUsuario1 = new DetalheUsuario();
            detalheUsuario.PathImage = await UploadImage(detalheUsuario.Image);

            detalheUsuario.Image = null;

            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(detalheUsuario), Encoding.UTF8, "application/json");

                using (var response = await httpClient.PostAsync("https://localhost:44312/api/Auth/Register", content))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    detalheUsuario1 = JsonConvert.DeserializeObject<DetalheUsuario>(apiResponse);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditarUsuario(string Id)
        {
            Id = HttpContext.Session.GetString("Id");
            DetalheUsuario detalheUsuario1 = new DetalheUsuario();

            using (var httpClient = new HttpClient())
            {
                var tokenDeAcesso = HttpContext.Session.GetString("Token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);

                StringContent content = new StringContent(JsonConvert.SerializeObject(Id), Encoding.UTF8, "application/json");

                using (var response = await httpClient.PostAsync("https://localhost:44312/api/Auth/DetalhesPerfil", content))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    detalheUsuario1 = JsonConvert.DeserializeObject<DetalheUsuario>(apiResponse);
                }
            }

            ViewData["NomeUsuarioLogado"] = HttpContext.Session.GetString("UserName");
            ViewData["ImagemPerfil"] = HttpContext.Session.GetString("PathImage");
            return View(detalheUsuario1);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(DetalheUsuario detalheUsuario)
        {
            DetalheUsuario detalheUsuario1 = new DetalheUsuario();

            detalheUsuario.PathImage = await UploadImage(detalheUsuario.Image);
            detalheUsuario.Id = HttpContext.Session.GetString("Id");
            detalheUsuario.Image = null;

            using (var httpClient = new HttpClient())
            {
                var tokenDeAcesso = HttpContext.Session.GetString("Token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);

                StringContent content = new StringContent(JsonConvert.SerializeObject(detalheUsuario), Encoding.UTF8, "application/json");

                using (var response = await httpClient.PostAsync("https://localhost:44312/api/Auth/EditUser", content))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    detalheUsuario1 = JsonConvert.DeserializeObject<DetalheUsuario>(apiResponse);
                }
                HttpContext.Session.SetString("UserName", detalheUsuario.PrimeiroNome + " " + detalheUsuario.SegundoNome);
                HttpContext.Session.SetString("PathImage", detalheUsuario.PathImage);
            }
            return RedirectToAction(nameof(Feed));
        }

        public async Task<IActionResult> DetalhesPerfil(string Id)
        {
            DetalheUsuario detalheUsuario1 = new DetalheUsuario();

            Id = HttpContext.Session.GetString("Id");

            using (var httpClient = new HttpClient())
            {
                var tokenDeAcesso = HttpContext.Session.GetString("Token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);

                StringContent content = new StringContent(JsonConvert.SerializeObject(Id), Encoding.UTF8, "application/json");

                using (var response = await httpClient.PostAsync("https://localhost:44312/api/Auth/DetalhesPerfil", content))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    detalheUsuario1 = JsonConvert.DeserializeObject<DetalheUsuario>(apiResponse);
                }
            }
            ViewData["NomeUsuarioLogado"] = HttpContext.Session.GetString("UserName");
            ViewData["ImagemPerfil"] = HttpContext.Session.GetString("PathImage");
            return View(detalheUsuario1);
        }

        public async Task<IActionResult> AdicionarAmigo()
        {
            var amizade = applicationDb.Amizade.ToList();
            List<Amizade> meusAmigos = new List<Amizade>();

            foreach (Amizade i in amizade)
            {
                if (i.UsuarioIdA == HttpContext.Session.GetString("Id"))
                {
                    meusAmigos.Add(i);
                }
            }
            if (meusAmigos.Count == 0)
            {
                meusAmigos = null;
            }

            ViewData["meusAmigos"] = meusAmigos;

            List<DetalheUsuario> usuarios = new List<DetalheUsuario>();
            using (var httpClient = new HttpClient())
            {
                var tokenDeAcesso = HttpContext.Session.GetString("Token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);

                using (var response = await httpClient.GetAsync("https://localhost:44312/api/Auth/AddAmigos"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    usuarios = JsonConvert.DeserializeObject<List<DetalheUsuario>>(apiResponse);
                }
            }
            ViewData["NomeUsuarioLogado"] = HttpContext.Session.GetString("UserName");
            ViewData["UsuarioLogado"] = HttpContext.Session.GetString("Id");
            ViewData["ImagemPerfil"] = HttpContext.Session.GetString("PathImage");
            return View(usuarios);
        }

        public async Task<IActionResult> ConfirmaAmizade(string id)
        {
            using (var httpClient = new HttpClient())
            {
                var tokenDeAcesso = HttpContext.Session.GetString("Token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);
                string idSessao = HttpContext.Session.GetString("Id");
                StringContent content = new StringContent(JsonConvert.SerializeObject(idSessao), Encoding.UTF8, "application/json");

                using var response = await httpClient.PostAsync("https://localhost:44312/api/Auth/ConfirmaAmizade/" + id, content);

                return RedirectToAction(nameof(AdicionarAmigo));
            }
        }


        public async Task<IActionResult> MinhaListaAmigos()
        {
            var amizade = applicationDb.Amizade.ToList();
            List<Amizade> meusAmigos = new List<Amizade>();

            foreach (Amizade i in amizade)
            {
                if (i.UsuarioIdA == HttpContext.Session.GetString("Id"))
                {
                    meusAmigos.Add(i);
                }
            }

            ViewData["meusAmigos"] = meusAmigos;

            List<DetalheUsuario> usuarios = new List<DetalheUsuario>();
            using (var httpClient = new HttpClient())
            {
                var tokenDeAcesso = HttpContext.Session.GetString("Token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);

                using (var response = await httpClient.GetAsync("https://localhost:44312/api/Auth/AddAmigos"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    usuarios = JsonConvert.DeserializeObject<List<DetalheUsuario>>(apiResponse);
                }
            }
            ViewData["NomeUsuarioLogado"] = HttpContext.Session.GetString("UserName");
            ViewData["UsuarioLogado"] = HttpContext.Session.GetString("Id");
            ViewData["ImagemPerfil"] = HttpContext.Session.GetString("PathImage");
            return View(usuarios);
        }

        private async Task<string> UploadImage(IFormFile imageFile)
        {

            var reader = imageFile.OpenReadStream();
            var cloundStorageAccount = CloudStorageAccount.Parse(@"DefaultEndpointsProtocol=https;AccountName=grstore;AccountKey=TpT2g5Gbgo9x+WkJ1ISdqKSvkR/iFujClMt0QC/dy/fZS0FoGeIPpeHvTOZ1UN7CmP081fqPRcycd0gXkl+mdg==;EndpointSuffix=core.windows.net");
            var blobClient = cloundStorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("post-images");
            await container.CreateIfNotExistsAsync();
            var blob = container.GetBlockBlobReference(Guid.NewGuid().ToString());
            await blob.UploadFromStreamAsync(reader);
            var uri = blob.Uri.ToString();
            return uri;
        }


        [HttpPost]
        public async Task<IActionResult> FazerLogin(CredencialLogin credencial)
        {
            DetalheUsuario detalheUsuario = new DetalheUsuario();
            using (var httpClient = new HttpClient())
            {
                var tokenDeAcesso = HttpContext.Session.GetString("Token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);

                StringContent content = new StringContent(JsonConvert.SerializeObject(credencial), Encoding.UTF8, "application/json");

                using var response = await httpClient.PostAsync("https://localhost:44312/api/Auth/Login", content);

                if (((int)response.StatusCode) == 200)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    detalheUsuario = JsonConvert.DeserializeObject<DetalheUsuario>(apiResponse);
                    IDictionary<string,string> token = JsonConvert.DeserializeObject<IDictionary<string,string>>(apiResponse);
                    
                    foreach(KeyValuePair<string,string> i in token)
                    {
                        if(i.Key == "token")
                        {
                            string t = i.Value;
                            HttpContext.Session.SetString("Token", t);
                        }
                        
                    }                                      
                    HttpContext.Session.SetString("UserName", detalheUsuario.PrimeiroNome + " " + detalheUsuario.SegundoNome);
                    HttpContext.Session.SetString("Id", detalheUsuario.Id);
                    HttpContext.Session.SetString("PathImage", detalheUsuario.PathImage);
                    
                    return RedirectToAction(nameof(Feed));
                }
                else
                {
                    ModelState.AddModelError("LoginError", "Usuário ou senha incorreto!");
                    return View("~/Views/Home/Index.cshtml");
                }
            }
        }

        public async Task<IActionResult> DeletarAmizade(string id)
        {
            using (var httpClient = new HttpClient())
            {
                var tokenDeAcesso = HttpContext.Session.GetString("Token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);

                string idSessao = HttpContext.Session.GetString("Id");
                StringContent content = new StringContent(JsonConvert.SerializeObject(idSessao), Encoding.UTF8, "application/json");

                using var response = await httpClient.PostAsync("https://localhost:44312/api/Auth/DeletarAmizade/" + id, content);

                return RedirectToAction(nameof(MinhaListaAmigos));
            }
        }

        public async Task<IActionResult> DetalhesAmigo(string id)
        {
            DetalheUsuario usuario = new DetalheUsuario();

            using (var httpClient = new HttpClient())
            {
                var tokenDeAcesso = HttpContext.Session.GetString("Token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);

                StringContent content = new StringContent(JsonConvert.SerializeObject(id), Encoding.UTF8, "application/json");

                using (var response = await httpClient.PostAsync("https://localhost:44312/api/Auth/DetalhesAmigo", content))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    usuario = JsonConvert.DeserializeObject<DetalheUsuario>(apiResponse);
                }

                ViewData["NomeUsuarioLogado"] = HttpContext.Session.GetString("UserName");
                ViewData["ImagemPerfil"] = HttpContext.Session.GetString("PathImage");
                return View(usuario);
            }
        }

        public async Task<IActionResult> Feed()
        {
            List<Post> posts = new List<Post>();
            using (var httpClient = new HttpClient())
            {
                var tokenDeAcesso = HttpContext.Session.GetString("Token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);

                using (var response = await httpClient.GetAsync("https://localhost:44312/api/Posts/Feed"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    posts = JsonConvert.DeserializeObject<List<Post>>(apiResponse);

                    if (posts.Count() == 0)
                    {
                        Post post = new Post()
                        {
                            NomeUsuario = "",
                            Mensagem = "Ainda não existem publicações, seja o primeiro a adicionar!",
                            PathImagePost = "https://grstore.blob.core.windows.net/post-images/55eaf0bd-92af-4567-8fdb-5da6b5f77a54",
                            dataPublicação = DateTime.Now
                        };
                        posts.Add(post);
                    }
                }
            }
            ViewData["IdUsuarioLogado"] = HttpContext.Session.GetString("Id");
            ViewData["NomeUsuarioLogado"] = HttpContext.Session.GetString("UserName");
            ViewData["ImagemPerfil"] = HttpContext.Session.GetString("PathImage");
            return View(posts);
        }

        public IActionResult CriarPost()
        {
            ViewData["NomeUsuarioLogado"] = HttpContext.Session.GetString("UserName");
            ViewData["ImagemPerfil"] = HttpContext.Session.GetString("PathImage");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarPost(Post post)
        {
            Post post1 = new Post();
            post.PathImagePost = await UploadImage(post.ImagePost);
            post.NomeUsuario = HttpContext.Session.GetString("UserName");
            post.UsuarioId = HttpContext.Session.GetString("Id");
            post.dataPublicação = DateTime.Now;
            post.ImagePost = null;

            using (var httpClient = new HttpClient())
            {
                var tokenDeAcesso = HttpContext.Session.GetString("Token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);

                StringContent content = new StringContent(JsonConvert.SerializeObject(post), Encoding.UTF8, "application/json");

                using (var response = await httpClient.PostAsync("https://localhost:44312/api/Posts/CriarPost", content))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    post1 = JsonConvert.DeserializeObject<Post>(apiResponse);
                }
            }
            return RedirectToAction(nameof(Feed));
        }


        // GET: HomeController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }


        // GET: HomeController/Edit/5
        public async Task<IActionResult> EditPost(Guid id)
        {
            Post post = new Post();
            using (var httpClient = new HttpClient())
            {
                var tokenDeAcesso = HttpContext.Session.GetString("Token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);

                using (var response = await httpClient.GetAsync("https://localhost:44312/api/Posts/" + id))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    post = JsonConvert.DeserializeObject<Post>(apiResponse);
                }
            }
            ViewData["NomeUsuarioLogado"] = HttpContext.Session.GetString("UserName");
            ViewData["ImagemPerfil"] = HttpContext.Session.GetString("PathImage");
            return View(post);
        }

        // POST: HomeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(Guid id, Post post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }
            post.NomeUsuario = HttpContext.Session.GetString("UserName");
            post.dataPublicação = DateTime.Now;
            post.UsuarioId = HttpContext.Session.GetString("Id");
            post.PathImagePost = await UploadImage(post.ImagePost);
            post.ImagePost = null;
            try
            {
                Post post1 = new Post();
                using (var httpClient = new HttpClient())
                {
                    var tokenDeAcesso = HttpContext.Session.GetString("Token");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);

                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(post.PostId.ToString()), "PostId");
                    content.Add(new StringContent(post.UsuarioId.ToString()), "UsuarioId");
                    content.Add(new StringContent(post.NomeUsuario), "NomeUsuario");
                    content.Add(new StringContent(post.PathImagePost), "PathImagePost");
                    content.Add(new StringContent(post.Mensagem), "Mensagem");
                    content.Add(new StringContent(Convert.ToString(post.dataPublicação)), "dataPublicação");

                    using (var response = await httpClient.PutAsync("https://localhost:44312/api/Posts/Update", content))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        post1 = JsonConvert.DeserializeObject<Post>(apiResponse);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return RedirectToAction(nameof(Feed));
        }

        // POST: HomeController/Delete/5
        public async Task<IActionResult> ExcluirPost(Guid id)
        {
            using (var httpClient = new HttpClient())
            {
                var tokenDeAcesso = HttpContext.Session.GetString("Token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDeAcesso);

                using (var response = await httpClient.DeleteAsync("https://localhost:44312/api/Posts/DeletePost/" + id))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                }
            }
            return RedirectToAction(nameof(Feed));
        }

        public async Task<IActionResult> Logoff()
        {
            HttpContext.Session.SetString("Token", " ");
            return RedirectToAction(nameof(Index));
        }
    }
}
