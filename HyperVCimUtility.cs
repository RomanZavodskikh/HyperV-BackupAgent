using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Generic;

namespace BackupService
{
    static class HyperVCimUtility
    {
        public static class WmiScopes
        {
            public const string virt_v2 = @"root\virtualization\v2";
        }

        public enum SnapshotType
        {
            FullSnapshot = 2,
            DiskSnapshot = 3
        };

        private static CimInstance GetFirstObjectFromCollection(IEnumerable<CimInstance> collection)
        {
            foreach (var e in collection)
                return e;

            throw new Exception();
        }

        private static IEnumerable<CimInstance> SearchWmiObjects(string request, string sc)
        {
            CimSession session = CimSession.Create(null);
            IEnumerable<CimInstance> collection = session.QueryInstances(sc, "WQL", request);                        

            return collection;
        }

        private static CimInstance SearchFirstWmiObject(string request, string sc)
        {            
            try
            {
                var collection = SearchWmiObjects(request, sc);
                var instance = GetFirstObjectFromCollection(collection);

                return instance;
            }
            catch(Exception)
            {
                throw new Exception(string.Format("no result for wmi objects searching \nrequest: {0}\nscope:{1}", request, sc));
            }            
        }

        public static CimInstance GetVm(string vmName)
        {
            var request  = string.Format("SELECT * from Msvm_ComputerSystem where ElementName = '{0}'", vmName);
            var instance = SearchFirstWmiObject(request, WmiScopes.virt_v2);

            return instance;
        }

        public static string GetVmId(CimInstance Msvm_ComputerSystem)
        {
            return Msvm_ComputerSystem.CimInstanceProperties["Name"].Value.ToString();
        }

        public static string GetSnapshotId(CimInstance Msvm_VirtualSystemSettingData)
        {
            return Msvm_VirtualSystemSettingData.CimInstanceProperties["ConfigurationID"].Value.ToString();
        }

        public static string GetVmName(CimInstance Msvm_VirtualSystemSettingData)
        {
            return Msvm_VirtualSystemSettingData.CimInstanceProperties["ElementName"].Value.ToString();
        }

        public static string GetVmVmcxPath(CimInstance Msvm_ComputerSystem)
        {
            var Msvm_VirtualSystemSettings = GetVmVirtualSystemSettingData(Msvm_ComputerSystem);
            var path = GetVmcxPath(Msvm_VirtualSystemSettings);

            return path;
        }

        public static string GetVmcxPath(CimInstance Msvm_VirtualSystemSettings)
        {
            string ConfigDataRoot = Msvm_VirtualSystemSettings.CimInstanceProperties["ConfigurationDataRoot"].Value.ToString();
            string ConfigFile = Msvm_VirtualSystemSettings.CimInstanceProperties["ConfigurationFile"].Value.ToString();

            return ConfigDataRoot + "\\" + ConfigFile;
        }


        public static CimInstance GetVmVirtualSystemSettingData(CimInstance Msvm_ComputerSystem)
        {
            var session    = CimSession.Create(null);
            var collection = session.EnumerateAssociatedInstances(WmiScopes.virt_v2, Msvm_ComputerSystem, 
                "Msvm_SettingsDefineState", "Msvm_VirtualSystemSettingData", null, null);
            
            try
            {
                var Msvm_VirtualSystemSettingData = GetFirstObjectFromCollection(collection);
                return Msvm_VirtualSystemSettingData;
            }
            catch(Exception)
            {
                throw new Exception(string.Format("no result for associated searching \nAssociatedClass:{0}\nResultClassName:{1}", 
                    "Msvm_SettingsDefineState", "Msvm_VirtualSystemSettingData"));
            }
        }

        public static string GetPathToVirtualDisk(CimInstance Msvm_StorageAllocationSettingData)
        {
            if (Msvm_StorageAllocationSettingData == null)
                return null;

            var path = ((string[])Msvm_StorageAllocationSettingData.CimInstanceProperties["HostResource"].Value)[0];
            return path;
        }

        public static CimInstance GetVmStorageAllocationSettings(CimInstance Msvm_VirtualSystemSettingData)
        {
            var session = CimSession.Create(null);
            var collection = session.EnumerateAssociatedInstances(WmiScopes.virt_v2, Msvm_VirtualSystemSettingData, 
                "Msvm_VirtualSystemSettingDataComponent", "Msvm_StorageAllocationSettingData", null, null);
            
            try
            {
                var Msvm_StorageAllocationSettingData = GetFirstObjectFromCollection(collection);
                return Msvm_StorageAllocationSettingData;
            }
            catch(Exception)
            {
                return null;
            }
        }

        public static CimInstance[] GetSnapshotsVirtualSettings(CimInstance Msvm_ComputerSystem)
        {
            var session = CimSession.Create(null);
            var collection = session.EnumerateAssociatedInstances(WmiScopes.virt_v2, Msvm_ComputerSystem,
                null, "Msvm_SummaryInformation", null, null);

            var Msvm_SummaryInformation = GetFirstObjectFromCollection(collection);
            var Msvm_VirtualSystemSettingDataArray = (CimInstance[])(Msvm_SummaryInformation.CimInstanceProperties["Snapshots"].Value);

            return Msvm_VirtualSystemSettingDataArray;
        }

        public static string GetSnapshotParentId(CimInstance Msvm_VirtualSystemSettingData)
        {
            var parentPath = Msvm_VirtualSystemSettingData.CimInstanceProperties["Parent"].Value;
            if (parentPath != null)
            {
                var tmp = parentPath.ToString().Split(':');
                string parentId = tmp[tmp.Length - 1];
                parentId = parentId.Remove(parentId.Length - 1);

                return parentId;
            }
            else
                return "";
        }

        public static List<VirtualMachine> GetVms()
        {
            var vmList = new List<VirtualMachine>();
            string request = "Select * from Msvm_ComputerSystem where Caption='Virtual Machine'";

            var collection = SearchWmiObjects(request, WmiScopes.virt_v2);
            foreach (var Msvm_ComputerSystem in collection)
            {
                var Msvm_VirtualSystemSettings = GetVmVirtualSystemSettingData(Msvm_ComputerSystem);
                string vmName                  = GetVmName(Msvm_VirtualSystemSettings);
                var vm                         = new VirtualMachine(vmName);
                vmList.Add(vm);
            }

            return vmList;
        }
    }
}