using System;
using Microsoft.Management.Infrastructure;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;

namespace BackupService
{
    [DataContract]
    public class VirtualMachine
    {
        [DataMember]
        public readonly string name;
        [DataMember]
        public readonly string id;
        [DataMember]
        public readonly string virtualDiskPath;
        [DataMember]
        public readonly string virtualDisk;        
        [DataMember]
        public readonly string vmcxPath;
        [DataMember]
        public readonly string vmcx;

        public readonly Dictionary<String,Snapshot> snapshots;
        [DataMember]
        public readonly SnapshotTreeNode snapshotTree;

        public VirtualMachine(string vmName)
        {
            var Msvm_ComputerSystem               = HyperVCimUtility.GetVm(vmName);
            var Msvm_VirtualSystemSettings        = HyperVCimUtility.GetVmVirtualSystemSettingData(Msvm_ComputerSystem);
            var Msvm_StorageAllocationSettingData = HyperVCimUtility.GetVmStorageAllocationSettings(Msvm_VirtualSystemSettings);

            name                                  = vmName;
            id                                    = HyperVCimUtility.GetVmId(Msvm_ComputerSystem);
            vmcxPath                              = HyperVCimUtility.GetVmVmcxPath(Msvm_ComputerSystem);
            vmcx                                  = Path.GetFileName(vmcxPath);            
            virtualDiskPath                       = HyperVCimUtility.GetPathToVirtualDisk(Msvm_StorageAllocationSettingData);
            virtualDisk                           = Path.GetFileName(virtualDiskPath);

            snapshots = new Dictionary<String,Snapshot>();
            CimInstance[] snapshotSettings = HyperVCimUtility.GetSnapshotsVirtualSettings(Msvm_ComputerSystem);

            if( snapshotSettings != null )
            {
                foreach (var Msvm_VirtualSystemSettingData in snapshotSettings)
                {
                    var snapshot = new Snapshot(Msvm_VirtualSystemSettingData, name);
                    snapshots.Add(snapshot.id, snapshot);
                }

                var treeBuilder = new SnapshotTreeBuilder();
                snapshotTree = treeBuilder.Build(snapshots);
            }           
        }
    }
}
