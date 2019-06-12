using Newtonsoft.Json;
using ProcessManagement.Models;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace ProcessManagement.Controllers
{
    public class APIController : Controller
    {
        // GET: API
        public string ReadData(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = null;
            readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
            string data = readStream.ReadToEnd().ToString();
            response.Close();
            readStream.Close();
            return data;
        }
        public static string GetAvatar(string email)
        {
            APIController api = new APIController();
            string avatarData = api.ReadData("https://fitlogin.vanlanguni.edu.vn/GroupManagement/api/getUserImage?searchString=" + email);
            AvatarBase64 ava = JsonConvert.DeserializeObject<AvatarBase64>(avatarData);
            return ava.Avatar;
        }
    }
}