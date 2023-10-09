using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb;
using DotNetCoreSqlDb.Data;
using DotNetCoreSqlDb.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Net.Http.Headers;

namespace DotNetCoreSqlDb.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyDatabaseContext _context;
        private readonly IDistributedCache _cache;
        private readonly string _JokesCacheKey = "JokesList";

        public HomeController(MyDatabaseContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: Todos
        public async Task<IActionResult> Index()
        {
            var jokes = new List<Joke>();
            byte[]? jokesByteArray;

            jokesByteArray = await _cache.GetAsync(_JokesCacheKey);
            if (jokesByteArray != null && jokesByteArray.Length > 0)
            { 
                jokes = ConvertData<Joke>.ByteArrayToObjectList(jokesByteArray);
            }
            else 
            {
                jokes = await _context.Joke.ToListAsync();
                jokesByteArray = ConvertData<Joke>.ObjectListToByteArray(jokes);
                await _cache.SetAsync(_JokesCacheKey, jokesByteArray);
            }

            return View(jokes);
        }

        // GET: Todos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            byte[]? jokesByteArray;
            Joke? joke;

            if (id == null)
            {
                return NotFound();
            }

            jokesByteArray = await _cache.GetAsync(GetJokeCacheKey(id));

            if (jokesByteArray != null && jokesByteArray.Length > 0)
            {
                joke = ConvertData<Joke>.ByteArrayToObject(jokesByteArray);
            }
            else 
            {
                joke = await _context.Joke
                .FirstOrDefaultAsync(m => m.ID == id);
            if (joke == null)
            {
                return NotFound();
            }

                jokesByteArray = ConvertData<Joke>.ObjectToByteArray(joke);
                await _cache.SetAsync(GetJokeCacheKey(id), jokesByteArray);
            }

            

            return View(joke);
        }

        // GET: Todos/Create
        public async Task<IActionResult> Create()
        {
            var joke = new Joke();

            HttpClient httpClient = new()
            {
                BaseAddress = new Uri("https://official-joke-api.appspot.com")
            };
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await httpClient.GetAsync("/random_joke");
            if (response.IsSuccessStatusCode)
            {
                Joke? newJoke = await response.Content.ReadFromJsonAsync<Joke>();
                if (newJoke != null)
                {
                    newJoke.CreatedDate = DateTime.Now;
                    _context.Add<Joke>(newJoke);
                    await _context.SaveChangesAsync();
                    await _cache.RemoveAsync(_JokesCacheKey);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool JokeExists(int id)
        {
            return _context.Joke.Any(e => e.ID == id);
        }

        private string GetJokeCacheKey(int? id)
        {
            return _JokesCacheKey+"_&_"+id;
        }
    }

    
}
