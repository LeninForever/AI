using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace AI.Models
{
    internal class UnionGameField
    {
        private static string _pathStartInstanse = @"C:\Users\Igor\Desktop\уЧеба_4курс\1 семестр\ИИ\AI\AI\res\StartInstanseField.txt";
        private static string _pathFinalInstanse = @"C:\Users\Igor\Desktop\уЧеба_4курс\1 семестр\ИИ\AI\AI\res\FinalInstanseField.txt";

        protected List<byte> _gameField;
        public List<byte> GameField { get { return _gameField; } }
        private static int _size;
        public int Size { get { return _size; } }
        public int RotatePointsCount { get { return (_size - 1) * (_size - 1); } }
        private bool wasRotated = false;
        public int Hash { get; set; }

        public int Depth { get; set; }

        public UnionGameField ForgottenPath { get; set; } = null;



        public UnionGameField? Parent { get; set; }

        public static UnionGameField FinalField { get; private set; } = new UnionGameField(4, null, CreateOptionsEnum.FINAL);

        public UnionGameField(int size, UnionGameField? parent, CreateOptionsEnum createOption = CreateOptionsEnum.NONE)
        {
            _size = size;
            Parent = parent;
            Depth = parent == null ? 0 : parent.Depth + 1;

            switch (createOption)
            {
                case CreateOptionsEnum.FROMFILE:
                    _gameField = CreateFromFile(_pathStartInstanse);
                    break;
                case CreateOptionsEnum.RANDOM:
                    throw new NotImplementedException();
                    break;
                case CreateOptionsEnum.FINAL:
                    _gameField = CreateFromFile(_pathFinalInstanse);
                    break;
                case CreateOptionsEnum.NONE:
                default:
                    _gameField = new List<byte>(size * size);
                    break;
            }
            if (_gameField.Count != 0)
                Hash = GetHashCode(_gameField);

        }
        public static int GetHashCode(List<byte> gameField)
        {
            
            StringBuilder stringBuilder = new StringBuilder(gameField.Count + 1);
            foreach (var el in gameField)
            {
                stringBuilder.Append((char)el);
            }
          

            return HashCode.Combine(stringBuilder.ToString());
        }
        public static UnionGameField Copy(UnionGameField from)
        {
            UnionGameField gameField = new UnionGameField(from.Size, from.Parent);
            for (int i = 0; i < from.Size * from.Size; i++)
            {
                gameField._gameField.Add(from._gameField[i]);
            }
            gameField.Hash = GetHashCode(gameField._gameField);
            return gameField;
        }
        private static List<byte> CreateFromFile(string path)
        {
            string field;
            var listCell = new List<byte>(_size * _size);

            using (var streamReader = new StreamReader(path))
            {
                field = streamReader.ReadToEnd();
            }

            var lines = field.Split("\r\n");
            foreach (var line in lines)
            {
                var numbers = line.Split(' ');
                foreach (var number in numbers)
                {
                    listCell.Add(byte.Parse(number));
                }
            }

            return listCell;
        }


        public override int GetHashCode()
        {

            /*StringBuilder stringBuilder = new StringBuilder(_gameField.Count);
            foreach (var el in _gameField)
            {
                stringBuilder.Append(el);
            }*/
            return Hash;// HashCode.Combine(stringBuilder,ToString()); // HashCode.Combine(Hash);
        }
        public override bool Equals(object? obj)
        {
            var other = obj as UnionGameField;
            if (other == null)
                throw new NullReferenceException();

            /* for (int i = 0; i < _gameField.Count; i++)
             {
                 if (_gameField[i] != other._gameField[i])
                     return false;
             }*/
           
            return other.Hash == this.Hash;
        }
        public override string ToString()
        {
            var builder = new StringBuilder(_gameField.Count);
            for (int i = 1; i <= _gameField.Count; i++)
            {
                //if (i % 4 == 0)
                //  builder.AppendLine(_gameField[i - 1].ToString());
                //else
                builder.Append(_gameField[i - 1].ToString() + " ");
            }
            return builder.ToString().Substring(0, builder.ToString().Length);
        }
        public void Rotate(int rotateNode, DirectionEnum direction)
        {
           
            int leftUp = rotateNode + rotateNode / (_size - 1);
            int rightUp = leftUp + 1;
            int leftDown = leftUp + _size;
            int rightDown = leftDown + 1;

            var temp = _gameField[rightUp];

            if (direction == DirectionEnum.CLOCKWISE)
            {
                _gameField[rightUp] = _gameField[leftUp];
                _gameField[leftUp] = _gameField[leftDown];
                _gameField[leftDown] = _gameField[rightDown];
                _gameField[rightDown] = temp;
            }
            else
            {
                _gameField[rightUp] = _gameField[rightDown];
                _gameField[rightDown] = _gameField[leftDown];
                _gameField[leftDown] = _gameField[leftUp];
                _gameField[leftUp] = temp;
            }
            Hash = GetHashCode(_gameField);
        }

        public int CalculateHeuristic(UnionGameField to)
        {
            if (to == null)
                throw new ArgumentNullException();

            int summaryPath = 0;
            int x1 = 0;
            int x2 = 0;
            int y1 = 0;
            int y2 = 0;

            for (int i = 0; i < to._gameField.Count; i++)
            {
                x1 = i % 4;
                y1 = i / 4;
                x2 = (to._gameField[i] - 1) % 4;
                y2 = (to._gameField[i] - 1) / 4;

                summaryPath += (Math.Abs(x2 - x1) + Math.Abs(y2 - y1));
            }

            return (int)Math.Ceiling((double)summaryPath / 4);
            //return summaryPath;
        }


        public bool IsFinalInstanse()
        {
            return Equals(FinalField);

        }
        public int Path { get; set; }
    }

    public class UnionGameFieldComparer : System.Collections.Generic.IComparer<GameFieldInstanse>
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
