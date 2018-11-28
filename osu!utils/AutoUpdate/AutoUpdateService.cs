using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Osu.Utils.AutoUpdate
{
    /// <summary>
    /// The AutoUpdateService class handling check and download osu tournament update
    /// </summary>
    public class AutoUpdateService
    {
        private readonly string PROJECT_URL = @"https://git.cartooncraft.fr/shARPII/script-chan";
        private readonly string DOWNLOAD_REGEX = @"^.+[?=\(](.+(?=\)))";

        private readonly Uri _uri;
        private string _downloadLink;

        public AutoUpdateService(string url)
        {
            _uri = new Uri(url);
        }

        public async Task<string> GetNewVersion()
        {
            using (var wc = new WebClient())
            {
                var version = "ko";
                try
                {
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    string json = await wc.DownloadStringTaskAsync(_uri);
                    var obj = JObject.Parse("{ \"versions\":" + json + "}");
                    var token = obj["versions"].First;

                    version = token["release"]["tag_name"].ToString();
                    var description = token["release"]["description"].ToString();

                    Regex regex = new Regex(DOWNLOAD_REGEX);
                    if (regex.IsMatch(description))
                    {
                        var match = regex.Match(description);
                        _downloadLink = PROJECT_URL + match.Groups[1].Value;
                    }
                }
                catch(Exception e)
                {
                }

                return version;
            }
        }

        public bool DownloadNewVersion()
        {
            try
            {
                var backupfolder = "\\backup";

                var file = System.IO.Path.GetTempPath() + "script_chan_update.zip";
                if (File.Exists(file))
                    File.Delete(file);

                using (var client = new WebClient())
                {
                    client.DownloadFile(_downloadLink, file);
                }

                var folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                if (Directory.Exists(folder + backupfolder))
                {
                    Directory.Delete(folder + backupfolder, true);
                }
                Directory.CreateDirectory(folder + backupfolder);
                foreach (string f in Directory.EnumerateFiles(folder))
                {
                    if (f.Contains(".db"))
                        File.Copy(f, folder + backupfolder + "\\" + Path.GetFileName(f));
                    else
                        File.Move(f, folder + backupfolder + "\\" + Path.GetFileName(f));

                }

                ZipFile.ExtractToDirectory(file, folder);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            
        }
    }
}
