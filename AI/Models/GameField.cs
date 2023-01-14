using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Models
{
    internal class GameField
    {
        private static string _pathStartInstanse = @"C:\Users\Igor\Desktop\уЧеба_4курс\1 семестр\ИИ\AI\AI\res\StartInstanseField.txt";
        private static string _pathFinalInstanse = @"C:\Users\Igor\Desktop\уЧеба_4курс\1 семестр\ИИ\AI\AI\res\FinalInstanseField.txt";

        private List<Cell> _gameField;

        private int _size;



        public int Size { get { return _size; } }

        public int RotatePointsCount { get { return (_size - 1) * (_size - 1); } }

        public GameField(int size, CreateOptionsEnum createOption = CreateOptionsEnum.NONE)
        {
            _size = size;
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
                    _gameField = new List<Cell>(size * size);
                    break;
            }

        }

        public static GameField Copy(GameField from)
        {
            GameField gameField = new GameField(from.Size);
            for (int i = 0; i < from.Size * from.Size; i++)
            {
                gameField._gameField.Add(from._gameField[i]);
            }
            return gameField;
        }
        private List<Cell> CreateFromFile(string path)
        {
            string field;
            var listCell = new List<Cell>(_size * _size);

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
                    listCell.Add(new Cell(int.Parse(number), null));
                }
            }

            return listCell;
        }


        public override int GetHashCode()
        {
            var fieldAsArray = _gameField.Select(x => x.Number).ToArray();
            StringBuilder stringBuilder = new StringBuilder(fieldAsArray.Length);
            foreach (var el in fieldAsArray)
            {
                stringBuilder.Append(el.ToString());
            }

            return HashCode.Combine(stringBuilder.ToString());
        }
        public override bool Equals(object? obj)
        {
            return (obj?.GetHashCode() ?? 0) == this.GetHashCode();
        }
        public override string ToString()
        {
            var builder = new StringBuilder(_gameField.Count);
            for (int i = 1; i <= _gameField.Count; i++)
            {
                if (i % 4 == 0)
                    builder.AppendLine(_gameField[i - 1].Number.ToString());
                else
                    builder.Append(_gameField[i - 1].Number.ToString() + " ");
            }
            return builder.ToString().Substring(0, builder.ToString().Length -2);
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

        }

        public int CalculateHeuristic(GameField to)
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
                x2 = (to._gameField[i].Number - 1) % 4;
                y2 = (to._gameField[i].Number - 1) / 4;

                summaryPath += (Math.Abs(x2 - x1) + Math.Abs(y2 - y1));
            }

             return (int)Math.Ceiling((double)summaryPath / 4);
            //return summaryPath;
        }


        /*
        1) Узлы поворота - матрица (size-1)x(size-1)
        2) Позиция левой верхней ячейки: номер точки поворота + ряд
        3) Позиция правой верхней ячейки: левая ячейка + 1
        4) Позиция нижней левой ячейки: номер точки поворота + ряд + size
        5) Позиция нижней правой ячейки: номер точки поворота + ряд + size + 1
        6) Вращение по часовой: ЛВ -> ПВ -> ПН -> ЛН -> ЛВ
        7) Вращение против часовой: ЛВ -> ЛН -> ПН -> ПВ -> ЛВ
         */
    }
}
