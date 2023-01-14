using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Models
{
    internal class GameFieldInstanse
    {




        public int Depth { get; private set; }

        public GameFieldInstanse ForgottenPath { get; set; } = null;

        public GameField Current { get; private set; }

        public GameFieldInstanse? Parent { get; private set; }

        public static GameField FinalField { get; private set; }

        public GameFieldInstanse(GameField current, GameFieldInstanse? parent)
        {
            Current = current;
            Parent = parent;
            Depth = parent == null ? 0 : parent.Depth + 1;

            if (FinalField == null)
                FinalField = new GameField(current.Size, CreateOptionsEnum.FINAL);

            Path = Depth + current.CalculateHeuristic(this.Current);
        }
        public bool IsFinalInstanse()
        {
            return (Current.Equals(FinalField));
        }

        public override int GetHashCode()
        {
            return Current.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj != null)
                return Current.GetHashCode() == obj.GetHashCode();
            else if (Current == null)
                return true;
            else return false;
        }

        public override string ToString()
        {
            return Current == null ? string.Empty : Current.ToString();
        }

        public int Path { get; set; }



    }

    public class GameFieldInstanseComparer : System.Collections.Generic.IComparer<GameFieldInstanse>
    {
        int IComparer<GameFieldInstanse>.Compare(GameFieldInstanse? x, GameFieldInstanse? y)
        {
            if (x == null)
                throw new ArgumentNullException("x");
            if (y == null)
                throw new ArgumentNullException("y");

            int xPath = x.Depth;
            int xHeuristic = x.Current.CalculateHeuristic(GameFieldInstanse.FinalField);
            int yPath = y.Depth;
            int yHeuristic = y.Current.CalculateHeuristic(GameFieldInstanse.FinalField);

            int resultX = xPath + xHeuristic;
            int resultY = yPath + yHeuristic;

            return resultX - resultY;

        }
    }
}
