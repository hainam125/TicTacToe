using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour {
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private int boardSize = 3;
    [SerializeField] private float pieceSize = 1;
    [SerializeField] private Sprite player1Sprite;
    [SerializeField] private Sprite player2Sprite;
    [SerializeField] private bool isPlayer1First;

    private Piece[] pieces;
    private List<Piece> available;
    private int availaleCount;

    private char[] board;
    private bool player1Turn;
    private char player1 = 'O';
    private char player2 = 'X';
    private char empty = ' ';
    private char tie = 't';
    private char gameResult;
    private Dictionary<char, int> scoresMap;


    private void Start() {
        gameResult = empty;
        player1Turn = isPlayer1First;
        board = new char[] {
            ' ', ' ', ' ',
            ' ', ' ', ' ',
            ' ', ' ', ' ',
        };
        scoresMap = new Dictionary<char, int>() {
            { player1, -10 },
            { player2, +10 },
            { tie,       0 },
        };
        Generate();
        if (!isPlayer1First) AIMove();
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0) && gameResult == empty && player1Turn) {
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var tileX = Mathf.FloorToInt(worldPos.x + boardSize * 0.5f);
            var tileY = Mathf.FloorToInt(worldPos.y + boardSize * 0.5f);
            if (tileX >= 0 && tileX < boardSize && tileY >= 0 && tileY < boardSize) {
                var piece = pieces[GetIdx(tileX, tileY)];
                if (piece.IsEmpty) {
                    Move(piece);
                    if (gameResult == empty && available.Count > 0) AIMove();
                }
            }
        }
    }

    private void AIMove() {
        //var piece = available[Random.Range(0, available.Count)];
        float bestScore = float.MinValue;
        Piece bestMove = null;
        for (int i = 0; i < available.Count; i++) {
            var move = available[i];
            TryTestMove(move, player2);
            var score = Minimax(0, false);
            RestoreTestMove(move);
            if (score > bestScore) {
                bestScore = score;
                bestMove = move;
            }
        }
        Move(bestMove);
    }

    private float Minimax(int depth, bool isMaximizing) {
        var result = CheckWinner();
        if (result != empty) {
            return scoresMap[result];
        }
        if (isMaximizing) {
            float bestScore = float.MinValue;
            for (int i = 0; i < available.Count; i++) {
                var move = available[i];
                if (move.type != empty) continue;
                TryTestMove(move, player2);
                var score = Minimax(depth + 1, false);
                RestoreTestMove(move);
                bestScore = Mathf.Max(score, bestScore);
            }
            return bestScore;
        }
        else {
            float bestScore = float.MaxValue;
            for(int i = 0; i < available.Count; i++) {
                var move = available[i];
                if (move.type != empty) continue;
                TryTestMove(move, player1);
                var score = Minimax(depth + 1, true);
                RestoreTestMove(move);
                bestScore = Mathf.Min(score, bestScore);
            }
            return bestScore;
        }
    }

    private void Move(Piece piece) {
        piece.view.sprite = player1Turn ? player1Sprite : player2Sprite;
        piece.type = player1Turn ? player1 : player2;
        player1Turn = !player1Turn;
        available.Remove(piece);
        availaleCount--;
        gameResult = CheckWinner();
        if (gameResult == tie) {
            Debug.Log("tie");
        }
        else if (gameResult != empty) {
            Debug.Log(gameResult);
        }
    }

    private bool CheckEqual(Piece piece1, Piece piece2, Piece piece3) {
        return piece1.type == piece2.type && piece2.type == piece3.type && piece1.type != empty;
    }

    private char CheckWinner() {
        var winPlayer = empty;
        //vertical
        for(int i = 0; i < 3; i++) {
            if(CheckEqual(pieces[GetIdx(i,0)],pieces[GetIdx(i, 1)],pieces[GetIdx(i, 2)])) {
                winPlayer = pieces[GetIdx(i, 0)].type;
            }
        }
        //horizontal
        for (int i = 0; i < 3; i++) {
            if (CheckEqual(pieces[GetIdx(0, i)],pieces[GetIdx(1, i)],pieces[GetIdx(2, i)])) {
                winPlayer = pieces[GetIdx(0, i)].type;
            }
        }
        //diagonal
        if (CheckEqual(pieces[GetIdx(0, 0)], pieces[GetIdx(1, 1)], pieces[GetIdx(2, 2)])) {
            winPlayer = pieces[GetIdx(0, 0)].type;
        }
        if (CheckEqual(pieces[GetIdx(0, 2)], pieces[GetIdx(1, 1)], pieces[GetIdx(2, 0)])) {
            winPlayer = pieces[GetIdx(0, 2)].type;
        }

        if (winPlayer == empty && availaleCount == 0) {
            return tie;
        }
        else if (winPlayer != empty) {
            return winPlayer;
        }
        return empty;
    }

    private void TryTestMove(Piece move, char type) {
        move.type = type;
        availaleCount--;
    }

    private void RestoreTestMove(Piece move) {
        move.type = empty;
        availaleCount++;
    }

    private void Generate() {
        pieces = new Piece[boardSize * boardSize];
        available = new List<Piece>(pieces.Length);
        for (int y = 0; y < boardSize; y++) {
            for (int x = 0; x < boardSize; x++) {
                var pos = new Vector3(-boardSize * 0.5f + (x+0.5f) * pieceSize, -boardSize * 0.5f + (y+0.5f) * pieceSize);
                var pieceObj = Instantiate(piecePrefab, pos, Quaternion.identity, transform);
                pieceObj.transform.localScale = Vector3.one * 3;
                var renderer = pieceObj.GetComponent<SpriteRenderer>();
                var idx = GetIdx(x, y);
                var type = board[idx];
                if (type.Equals(player1)) renderer.sprite = player1Sprite;
                else if (type.Equals(player2)) renderer.sprite = player2Sprite;
                var piece = pieces[idx] = new Piece(renderer, type);
                if(type == empty) available.Add(piece);
            }
        }
        availaleCount = available.Count;
    }

    private int GetIdx(int x, int y) {
        return x + (boardSize - 1 - y) * boardSize;
    }
}

public class Piece {
    public SpriteRenderer view;
    public char type;
    public bool IsEmpty => type == ' ';

    public Piece(SpriteRenderer renderer, char peiceType) {
        view = renderer;
        type = peiceType;
    }

}
