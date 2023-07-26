using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    Move bestMove;
    Board board;
    Timer timer;

    int searchDepth = 4;

    int largeNumber = 9999999;
    int[] pieceValues = { 0, 100, 300, 310, 500, 900, 10000 };
    bool isWhite = false;

    public Move Think(Board _board, Timer _timer)
    {
        board = _board;
        timer = _timer;
        isWhite = board.IsWhiteToMove;

        NegamaxEvaluation(searchDepth, -largeNumber, largeNumber, isWhite ? 1 : -1);

        Move[] moves = board.GetLegalMoves();

        bool moveFound = false;
        foreach(Move move in moves)
        {
           if (bestMove == move) { moveFound = true; break; }
        }

        if (!moveFound)
        {
            bestMove = moves[0];
            Console.WriteLine("Prevented illegal move.");
        }
        
        return bestMove;
    }

    private int NegamaxEvaluation(int depth, int alpha, int beta, int color)
    {
        Move[] legalMoves = board.GetLegalMoves();

        if (depth == 0 || legalMoves.Length == 0)
        {
            int sum = 0;

            if (board.IsInCheckmate()) return -99999999;

            for (int i = 0; ++i < 7;)
            {
                sum += (board.GetPieceList((PieceType)i, true).Count - board.GetPieceList((PieceType)i, false).Count) * pieceValues[i];
            }

            return color * sum;
        }

        int bestEvalution = int.MinValue;

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);

            if (board.IsDraw())
            {
                board.UndoMove(move);
                continue;
            }

            int eval = -NegamaxEvaluation(depth - 1, -beta, -alpha, -color);

            if (TotalPieceCount(color == 1) < 8 && move.MovePieceType == PieceType.Pawn)
            {
                // Console.WriteLine("Encouraging pawns to move forward");
                eval += 200; // Encourage pawns to move forward
            }

            if (board.GetPieceList(PieceType.Pawn, color == 1).Count < 5 && TotalPieceCount(color == 1) < 8)
            {
                Square startSquare = move.StartSquare;

                // Numbers are vertical
                // Alphabets are horizontal

                bool pieceExists = false;
                for(int i = 1; ++i < 8;)
                {
                    if (board.GetPiece(new Square(move.StartSquare.File, i)).PieceType != PieceType.None) {
                        pieceExists = true; break;
                    }

                    if (TotalPieceCount(color == 1) < 8 && move.MovePieceType == PieceType.Pawn)
                    {
                        eval += 800; // Encourage pawns to move forward
                    }
                }
            }

            board.UndoMove(move);

            if (bestEvalution < eval)
            {
                bestEvalution = eval;
                if (depth == searchDepth)
                {
                    bestMove = move;
                }
            }

            alpha = Math.Max(alpha, eval);
            if (alpha >= beta) break;
        }

        return bestEvalution;
    }

    private int TotalPieceCount(bool white)
    {
        int count = 0;

        for (int i = 0; ++i < 7;)
        {
            count += board.GetPieceList((PieceType)i, white).Count;
        }

        return count;
    }
}