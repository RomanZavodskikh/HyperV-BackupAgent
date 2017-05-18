using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;

namespace BackupService
{
    public class StorageClient
    {
        private readonly string pc;
        private readonly string address;

        public StorageClient(string address)
        {
            pc = System.Environment.MachineName;
            this.address = address;
        }

        public void uploadSnapshot(string file, string vmName)
        {
            if (!File.Exists(file))
            {
                throw new Exception("attempt to upload not existed file");
            }

            var uri = string.Format("{0}/upload?m={1}&vm={2}", address, pc, vmName);

            var fileContent = new ByteArrayContent(File.ReadAllBytes(file));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            var requestContent = new MultipartFormDataContent();
            requestContent.Add(fileContent, "file", Path.GetFileName(file));

            var client = new HttpClient();
            var response = client.PostAsync(uri, requestContent);

            response.Result.EnsureSuccessStatusCode();
        }

        public void DownloadSnapshot(Snapshot snapshot)
        {
            DownloadFile(snapshot.vmName, snapshot.virtualDiskPath, snapshot.virtualDisk);
        }

        public void DownloadFile(string vmName, string localUrl, string remoteFileName)
        {
            if (File.Exists(localUrl))
            {
                throw new Exception("attempt to rewrite file by backup");
            }

            string remoteUrl = string.Format("{0}/download?m={1}&vm={2}&sn={3}",
                                              address, pc, vmName, remoteFileName);
            var client = new HttpClient();
            var response = client.GetAsync(remoteUrl);
            var result = response.Result;

            result.EnsureSuccessStatusCode();
            byte[] bytes = result.Content.ReadAsByteArrayAsync().Result;

            File.WriteAllBytes(localUrl, bytes);
        }
    }
}