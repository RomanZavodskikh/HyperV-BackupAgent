using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace BackupService
{
    public class HomeController : Controller
    {
        public HomeController(IConfiguration Configuration, ILoggerFactory loggerFactory)
        {
            this.Configuration = Configuration;
            storageClient = new StorageClient(Configuration["storage"]);
            logger = loggerFactory.CreateLogger("BackupService");
        }

        public IConfiguration Configuration;
        public StorageClient storageClient;
        public ILogger logger;

        [HttpGet]
        public string HyperVInfo()
        {
            try
            {
                logger.LogInformation("Getting Hyper-V information");
                List<VirtualMachine> vmList = HyperVCimUtility.GetVms();

                MemoryStream stream = new MemoryStream();
                var serializer = new DataContractJsonSerializer(typeof(List<VirtualMachine>));

                serializer.WriteObject(stream, vmList);
                stream.Position = 0;
                StreamReader sr = new StreamReader(stream);

                string virtualMachinesJson = sr.ReadToEnd();
                logger.LogInformation("Got Hyper-V information");

                return virtualMachinesJson;
            }
            catch(Exception e)
            {
                var err = string.Format("Failed to get Hyper-V information. Error:{0}", e.Message);
                logger.LogError(err);
                Response.StatusCode = 500;

                return "";
            }

        }

        [HttpPost]
        public string BackupSnapshot(string vmName, string snapshotId)
        {
            try
            {
                logger.LogInformation(string.Format("Backuping snapshot with id: {0} of virtual machine:{1}", snapshotId, vmName));
                var vm = new VirtualMachine(vmName);
                var snapshot = vm.snapshots[snapshotId];

                storageClient.uploadSnapshot(snapshot.virtualDiskPath, vm.name);
                logger.LogInformation(string.Format("Backupped snapshot with id: {0} of virtual machine:{1}", snapshotId, vmName));

                return "";
            }
            catch(Exception e)
            {
                var err = string.Format("Failed to backup snapshot with id: {0} of virtual machine:{1}. Error:{2}", snapshotId, vmName, e.Message);
                logger.LogError(err);
                Response.StatusCode = 500;

                return "";
            }
        }

        [HttpPost]
        public string DownloadSnapshot(string vmName, string snapshotId)
        {
            try
            {
                logger.LogInformation(string.Format("Restoring snapshot with id: {0} of virtual machine:{1}", snapshotId, vmName));
                var vm = new VirtualMachine(vmName);
                var snapshot = vm.snapshots[snapshotId];

                storageClient.DownloadSnapshot(snapshot);
                logger.LogInformation(string.Format("Restorred snapshot with id: {0} of virtual machine:{1}", snapshotId, vmName));

                return "";
            }
            catch(Exception e)
            {
                var err = string.Format("Failed to restore snapshot with id: {0} of virtual machine:{1}. Error:{2}", snapshotId, vmName, e.Message);
                logger.LogError(err);
                Response.StatusCode = 500;

                return "";
            }
        }
    }
}
