using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace BobDeathmic.Controllers
{
    public class DownloadController : Controller
    {
        private IHostingEnvironment env;
        public DownloadController(IHostingEnvironment env)
        {
            this.env = env;
        }
        [Authorize(Roles = "Dev,Admin")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Dev,Admin")]
        [RequestSizeLimit(60_914_560)]
        public async Task<bool> UploadAPKFile(List<IFormFile> Files)
        {
            long size = Files.Sum(f => f.Length);

            foreach (var formFile in Files)
            {
                if (formFile.Length > 0)
                {
                    var filePath = Path.Combine(new string[] { env.WebRootPath, "Downloads","APK", formFile.FileName });
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return true;
        }
        [HttpGet]
        public IActionResult DownloadTest(string FileName)
        {
            var filePath = Path.Combine(new string[] { env.WebRootPath, "Downloads", "character paladin.png" });
            return PhysicalFile(filePath, MimeTypes.GetMimeType(filePath), Path.GetFileName(filePath));
        }
    }
}