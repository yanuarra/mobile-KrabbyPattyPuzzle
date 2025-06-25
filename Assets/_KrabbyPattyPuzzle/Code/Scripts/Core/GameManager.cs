using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YRA
{
    public class GameManager : Singleton<GameManager>
    {
        [Header("Game Settings")]
        [SerializeField] private GameObject _blockPrefab;
        [SerializeField] private Transform _gameBoard;
        [SerializeField] private PuzzleGenerator puzzleGenerator;
        [SerializeField] private List<FoldingBlock> _currentPuzzle;
        [SerializeField] private List<GameState> _gameHistory;
        [SerializeField] private List<FoldingBlock> _foldedBlocks = new List<FoldingBlock>();
        [SerializeField] private int _currentLevel = 1;
        private int _score = 0;
        private int _moves = 0;
        private bool _isGameActive = true;
        FoldingBlock _top;
        FoldingBlock _bot;
        [System.Serializable]
        public struct GameState
        {
            public List<BlockState> blockStates;
            public int score;
            public int moves;
        }

        [System.Serializable]
        public struct BlockState
        {
            public Transform parent;
            public Vector3 position;
            public Quaternion rotation;
            public bool isFolded;
            public bool isSelectable;
            public int foldOrder;
        }

        void Start()
        {
            if (puzzleGenerator == null)
                puzzleGenerator = FindAnyObjectByType<PuzzleGenerator>();

            StartCoroutine(GenerateNewLevel());
            _gameHistory = new List<GameState>();
        }

        private IEnumerator GenerateNewLevel()
        {
            ClearBoard();
            yield return new WaitForEndOfFrame();
            _currentPuzzle = puzzleGenerator.GeneratePuzzle(_currentLevel, _gameBoard, _blockPrefab);
            yield return new WaitForEndOfFrame();
          
            _top = _currentPuzzle.Where(p => p.blockType.Equals(BlockType.Top)).FirstOrDefault();
            _bot = _currentPuzzle.Where(p => p.blockType.Equals(BlockType.Bottom)).FirstOrDefault();
            _moves = 0;
            _isGameActive = true;
            _gameHistory.Clear();
            SaveGameState();
            UpdateUI();

            foreach (var block in _currentPuzzle)
            {
                block.SetOnFoldedEvent(SaveGameState);
            }
        }

        void ClearBoard()
        {
            _foldedBlocks?.Clear();
            foreach (var item in _currentPuzzle)
            {
                Destroy(item.gameObject);
            }
            _currentPuzzle?.Clear();
        }

        public int GetFoldedBlockCount() => _foldedBlocks.Count;

        public void AddFoldedCollection(FoldingBlock block)
        {
            if (!_foldedBlocks.Contains(block))
                _foldedBlocks.Add(block);
            CheckRemainingBlocks();
        }

        public void RemoveFoldedCollection(FoldingBlock block)
        {
            if (_foldedBlocks.Contains(block))
                _foldedBlocks.Remove(block);
            CheckRemainingBlocks();
        }
        
        void CheckRemainingBlocks()
        {
            _moves++;
            if (_top.State.IsFolded || _bot.State.IsFolded || _foldedBlocks.Count == _currentPuzzle.Count - 1)
            {
                WinLevel();
                return;
            }
            int foldedFiller = 0;
            foreach (var item in _foldedBlocks)
            {
                if (item.blockType == BlockType.Filler)
                    foldedFiller++;

                _top.State.IsSelectable = _currentPuzzle.Count - 2 == foldedFiller;
                _bot.State.IsSelectable = _currentPuzzle.Count - 2 == foldedFiller;
            }
        }

        void UpdateUI()
        {
            MenuSystem.Instance.UpdateUI(
                _score.ToString(),
                _currentLevel.ToString(),
                _gameHistory.Count > 1
                );
        }

        void SaveGameState()
        {
            GameState state = new GameState
            {
                blockStates = new List<BlockState>(),
                score = this._score,
                moves = this._moves
            };

            foreach (var block in _currentPuzzle)
            {
                state.blockStates.Add(new BlockState
                {
                    parent = block.transform.parent,
                    position = block.transform.localPosition,
                    rotation = block.transform.rotation,
                    isFolded = block.State.IsFolded,
                    isSelectable = block.State.IsSelectable,
                });
            }

            _gameHistory.Add(state);
            if (_gameHistory.Count > 20)
                _gameHistory.RemoveAt(0);
            UpdateUI();
        }

        public void UndoMove()
        {
            if (_gameHistory.Count <= 1) return;
            GameState previousState = _gameHistory[_gameHistory.Count - 2];
            for (int i = 0; i < _currentPuzzle.Count; i++)
            {
                var block = _currentPuzzle[i];
                var blockState = previousState.blockStates[i];
                block.transform.SetParent(blockState.parent, true);
                block.transform.localPosition = blockState.position;
                block.transform.rotation = blockState.rotation;
                block.State.IsFolded = blockState.isFolded;
                block.State.IsSelectable = blockState.isSelectable;
                
                block.ToggleBlockCollider (!blockState.isFolded);
                if (!block.State.IsFolded)
                    RemoveFoldedCollection(block);
            }

            _score = previousState.score;
            _moves = previousState.moves;
            _gameHistory.RemoveAt(_gameHistory.Count - 1);
            UpdateUI();
        }

        public void SkipLevel()
        {
            _score = Mathf.Max(0, _score - 50);
            NextLevel();
        }

        void NextLevel()
        {
            MenuSystem.Instance.HideAllPanels();
            _currentLevel++;
             StartCoroutine(GenerateNewLevel());
        }
        
        void WinLevel()
        {
            _isGameActive = false;
            int levelBonus = _currentLevel * 100;
            int moveBonus = Mathf.Max(0, (50 - _moves) * 10);
            _score += levelBonus + moveBonus;
            string message = $"Level {_currentLevel} Complete!\nScore: {_score}\nMoves: {_moves}";
            MenuSystem.Instance.SetWinPanel(message);
            Invoke(nameof(NextLevel), 2f);
        }
    }
}
