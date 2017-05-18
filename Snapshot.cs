using Microsoft.Management.Infrastructure;
using System.Runtime.Serialization;
using System.IO;

namespace BackupService
{
    [DataContract]
    public class Snapshot
    {
        [DataMember]
        public readonly string name;
        [DataMember]
        public readonly string vmName;
        [DataMember]
        public readonly string id;
        [DataMember]
        public readonly string parentId;
        [DataMember]
        public readonly string virtualDisk;
        public readonly string virtualDiskPath;
        [DataMember]
        public readonly string vmcx;
        public readonly string vmcxPath;

        public Snapshot(CimInstance Msvm_VirtualSystemSettings, string vmName)
        {
            var Msvm_StorageAllocationSettingData = HyperVCimUtility.GetVmStorageAllocationSettings(Msvm_VirtualSystemSettings);

            this.vmName                       = vmName;
            name                              = HyperVCimUtility.GetVmName(Msvm_VirtualSystemSettings);
            id                                = HyperVCimUtility.GetSnapshotId(Msvm_VirtualSystemSettings);
            parentId                          = HyperVCimUtility.GetSnapshotParentId(Msvm_VirtualSystemSettings);           
            virtualDiskPath                   = HyperVCimUtility.GetPathToVirtualDisk(Msvm_StorageAllocationSettingData);
            virtualDisk                       = Path.GetFileName(virtualDiskPath);
            vmcxPath                          = HyperVCimUtility.GetVmcxPath(Msvm_VirtualSystemSettings);
            vmcx                              = Path.GetFileName(vmcxPath);
        }
    }
}
