using System.Collections.Generic;
using UnityEngine;

namespace YRA {
    public class PuzzleGenerator : MonoBehaviour
    {
        private float offset = 1f;
        [SerializeField] private List<Vector3> _usedPositions;
        private List<Vector3> _positions = new List<Vector3>();
        private List<FoldingBlock> _puzzleCollection = new List<FoldingBlock>();

        public List<FoldingBlock> GeneratePuzzle(int level, Transform parent, GameObject blockPrefab)
        {
            _puzzleCollection = new List<FoldingBlock>();
            int gridSize = Mathf.Min(3 + (level - 1) / 2, 6);
            int numFillerBlocks = Mathf.Min(3 + (level - 1) / 2, gridSize * gridSize - 2);

            _positions = GenerateGridPositions(gridSize);
            _usedPositions = new List<Vector3>();

            //TOP AND BOTTOM
            CreateBlock(GetRandomPosition(_positions, _usedPositions), BlockType.Bottom, parent, blockPrefab, _puzzleCollection);
            CreateBlock(GetAdjacentPosition(_usedPositions[0]), BlockType.Top, parent, blockPrefab, _puzzleCollection);

            //FILLER
            for (int i = 0; i < numFillerBlocks; i++)
            {
                Vector3 pos = GetRandomPositionNearObject(_usedPositions);
                CreateBlock(pos, BlockType.Filler, parent, blockPrefab, _puzzleCollection);
            }

            return _puzzleCollection;
        }

        public static bool IsOutsideGridBounds_Bounds(Vector3 testPoint, Vector3[] gridArray)
        {
            if (gridArray == null || gridArray.Length == 0)
                return true;
            Bounds bounds = new Bounds(gridArray[0], Vector3.zero);
            foreach (Vector3 pos in gridArray)
            {
                bounds.Encapsulate(pos);
            }
            return !bounds.Contains(testPoint);
        }

        bool CheckIfConnected(Vector3 pos, Vector3 pos2)
        {
            return Vector3.Distance(pos, pos2) <= 1.1f;
        }

        List<Vector3> GenerateGridPositions(int gridSize)
        {
            List<Vector3> positions = new List<Vector3>();
            float offset = (gridSize - 1) * 1f;
            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    positions.Add(new Vector3(x - Mathf.Round(offset / 2), 0, z - Mathf.Round(offset / 2)));
                }
            }
            return positions;
        }

        Vector3 GetAdjacentPosition(Vector3 reference)
        {
            List<Vector3> remaining = new List<Vector3>();
            foreach (var pos in _positions)
            {
                if (pos == reference) continue;
                if (CheckIfConnected(pos, reference))
                    remaining.Add(pos);
            }
            Vector3 selectedPos = remaining[UnityEngine.Random.Range(0, remaining.Count)];
            _usedPositions.Add(selectedPos);
            return selectedPos;
        }

        List<Vector3> GetRemainingEmptyPos()
        {
            List<Vector3> remaining = new List<Vector3>();
            foreach (var pos in _positions)
            {
                if (!_usedPositions.Contains(pos))
                    remaining.Add(pos);
            }
            return remaining;
        }

        Vector3 GetRandomPosition(List<Vector3> available, List<Vector3> used)
        {
            List<Vector3> remaining = GetRemainingEmptyPos();
            if (remaining.Count == 0)
                return Vector3.zero;
            Vector3 selectedPos = remaining[UnityEngine.Random.Range(0, remaining.Count)];
            used.Add(selectedPos);
            return selectedPos;
        }

        Vector3 GetRandomPositionNearObject(List<Vector3> used)
        {
            List<Vector3> remaining = GetRemainingEmptyPos();
            List<Vector3> near = new List<Vector3>();
            foreach (var item in used)
            {
                foreach (var itema in remaining)
                {
                    if (CheckIfConnected(item, itema))
                    {
                        near.Add(itema);
                    }
                }
            }
            if (near.Count == 0)
                return Vector3.zero;
            Vector3 selectedPos = near[UnityEngine.Random.Range(0, near.Count)];
            used.Add(selectedPos);
            return selectedPos;
        }

        void CreateBlock(Vector3 position, BlockType type, Transform parent, GameObject prefab, List<FoldingBlock> puzzle)
        {
            GameObject blockObj = Instantiate(prefab);
            blockObj.transform.position = position;
            blockObj.transform.rotation = Quaternion.identity;
            FoldingBlock block = blockObj.GetComponent<FoldingBlock>();
            if (block == null)
                block = blockObj.AddComponent<FoldingBlock>();
            block.blockType = type;
            puzzle.Add(block);
            blockObj.name = $"block#{puzzle.IndexOf(block)}";

            if (blockObj.GetComponent<Collider>() == null)
                blockObj.AddComponent<BoxCollider>();

            block.InitializeComponents();
            //Disable TOP and BOT
            block.State.IsSelectable = block.blockType == BlockType.Filler;
        }
    }
}