using System.Collections.Generic;
using UnityEngine;

namespace YRA
{
    public class MeshReferences : Singleton<MeshReferences>
    {
        public GameObject top;
        public GameObject bottom;
        public List<GameObject> fillerCollection = new List<GameObject>();

        public GameObject GetMeshByBlockType(BlockType type)
        {
            switch (type)
            {
                case BlockType.Top:
                    return top;
                case BlockType.Filler:
                    return fillerCollection[Random.Range(0, fillerCollection.Count)];
                case BlockType.Bottom:
                    return bottom;
                default:
                    return null;
            }
        }
    }
}
