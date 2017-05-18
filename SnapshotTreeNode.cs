using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BackupService
{
    [DataContract]
    public class SnapshotTreeNode
    {
        [DataMember]
        public Snapshot snapshot;
        [DataMember]
        public List<SnapshotTreeNode> childs;

        public SnapshotTreeNode( Snapshot snapshot)
        {
            this.snapshot = snapshot;
            childs        = new List<SnapshotTreeNode>();
        }
    }
}
