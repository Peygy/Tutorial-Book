using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MainApp.Services;
using MainApp.Models;

namespace MainApp.Controllers
{
    // Controller to manage tutorial parts 
    [Authorize(Roles = "admin, editor")]
    public class PartController : Controller
    {
        private IPartsService partService;

        public PartController(TopicsContext topicsData, MongoService contentData, ILogger<PartsService> log)
        {
            // Data context for parts -> topicsData & contentData
            // Logger for exceptions -> log
            partService = new PartsService(topicsData, contentData, log);
        }



        [HttpGet]
        public async Task<IActionResult> ViewParts(string table, int partId, string? title, string? filtre)
        {
            HttpContext.Session.Remove("refererurl");

            var part = await partService.GetPartAsync(partId, table, title, filtre);
            if (part == null) { return BadRequest(); }
            part.Parent ??= new PartModel { Id = 0, Table = table };
            ViewBag.Part = part;
            
            if (table == "posts" && partId != 0)
            {
                ViewBag.Content = await partService.GetPostContentAsync(((Post)part).ContentId);
                return View("ViewPost");
            }

            return View();
        }



        [HttpGet]
        public async Task<IActionResult> AddPart(string table, int partId)
        {
            HttpContext.Session.SetString("refererurl", Request.Headers["Referer"].ToString());

            if (partId == 0)
            {
                ViewBag.Parents = await partService.GetAllParentsAsync(table);
                ViewBag.Part = new PartModel { Table = table };
                return View();
            }

            ViewBag.Part = await partService.GetPartAsync(partId, table, null, null);
            if (ViewBag.Part == null) { return BadRequest(); }

            return View();          
        }

        [HttpPost]
        public async Task<IActionResult> AddPart(PartModel part, string content)
        {
            if (part.Title == null) { return View(part); }
            if (await partService.AddPartAsync(part, content))
            {
                return Redirect(HttpContext.Session.GetString("refererurl"));
            }
            
            return BadRequest();
        }



        [HttpGet]
        public async Task<IActionResult> UpdatePart(string table, int partId)
        {
            HttpContext.Session.SetString("refererurl", Request.Headers["Referer"].ToString());

            var part = await partService.GetPartAsync(partId, table, null, null);
            if (part == null) { return BadRequest(); }
            part.Parent ??= new PartModel { Id = 0, Table = table };
            ViewBag.Parents = (await partService.GetAllParentsAsync(table)).Where(s => s.Id != part.ParentId).ToList();

            if (table == "posts")
            {
                ViewBag.Content = await partService.GetPostContentAsync(((Post)part).ContentId);
            }

            return View(part);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePart(PartModel part, string newContent)
        {
            if (await partService.UpdatePartAsync(part, newContent))
            {           
                return Redirect(HttpContext.Session.GetString("refererurl"));
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("/api/check/title/{partId:int}&{parentTable}&{title}")]
        public async Task<IActionResult> CheckTitleExist(int partId, string parentTable, string title) 
            => Json(await partService.CheckTitleExistanceAsync(partId, parentTable, title));

        [HttpDelete]
        [Route("/api/part/remove/{partId:int}&{table}")]
        public async Task<IActionResult> DeletePart(int partId, string table)
            => Json(await partService.RemovePartAsync(partId, table));
    }
}
