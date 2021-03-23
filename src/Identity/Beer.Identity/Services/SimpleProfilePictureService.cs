using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.Identity.Services
{
    public class SimpleProfilePictureService : IProfilePictureService
    {
        private readonly IWebHostEnvironment environment;

        public SimpleProfilePictureService(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        public IEnumerable<(string Url, string Name)> GetPossibleProfilePicture()
        {
            var result = new List<(string Url, string Name)>();
            var path = Path.Combine(environment.WebRootPath, "img", "pp");
            foreach (var item in Directory.GetFiles(path, "*.png"))
            {
                FileInfo fileInfo = new FileInfo(item);

                String url = "/img/pp/" + fileInfo.Name;
                String niceName = fileInfo.Name.Replace("-", " ").Substring(0, fileInfo.Name.Length-4);

                result.Add((url, niceName));
            }

            return result;
        }
    }
}
