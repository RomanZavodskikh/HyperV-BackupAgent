using System;
using System.Collections.Generic;

namespace BackupService
{
    public class SnapshotTreeBuilder
    {
        private Dictionary<string, SnapshotTreeNode> CreateSnapshotDictionary(Dictionary<String,Snapshot> snapshots)
        {
            var dict = new Dictionary<string, SnapshotTreeNode>();

            foreach (KeyValuePair<string,Snapshot> pair in snapshots)
            {
                dict.Add(pair.Key, new SnapshotTreeNode(pair.Value));
            }

            return dict;
        }

        public SnapshotTreeNode Build(Dictionary<String, Snapshot> snapshots)
        {
            Dictionary<string, SnapshotTreeNode> dict = CreateSnapshotDictionary(snapshots);
            string rootId = "";

            foreach(KeyValuePair<string, SnapshotTreeNode> pair in dict)
            {
                string parentId = pair.Value.snapshot.parentId;
                if(parentId.Length == 0)
                {
                    rootId = pair.Value.snapshot.id;
                    continue;
                }

                dict[parentId].childs.Add(pair.Value);
            }

            return dict[rootId];
        }
    }
}
