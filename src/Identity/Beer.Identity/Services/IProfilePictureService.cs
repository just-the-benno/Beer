using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.Identity.Services
{
    public interface IProfilePictureService
    {
        public IEnumerable<(String Url, String Name)> GetPossibleProfilePicture();
    }
}
