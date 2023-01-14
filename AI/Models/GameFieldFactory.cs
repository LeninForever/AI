using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace AI.Models
{
    internal class GameFieldInstanseFactory
    {

        public GameFieldInstanse CreateStartFieldInstanse(int size, CreateOptionsEnum createOptions)
        {

            return new GameFieldInstanse(new GameField(size, createOptions), null);
        }

        public UnionGameField CreateStartField(int size, CreateOptionsEnum createOptions)
        {
            return new UnionGameField(size, null, createOptions);
        }

        public List<GameFieldInstanse> CreateNewFieldInstanses(GameFieldInstanse parent, int maxDepth)
        {
            if (parent.Depth == maxDepth)
                return new List<GameFieldInstanse>();


            var nextStepInstanses = new List<GameFieldInstanse>(parent.Current.RotatePointsCount);
            for (int i = 0; i < parent.Current.RotatePointsCount; i++)
            {
                nextStepInstanses.Add(Rotate(i, DirectionEnum.CLOCKWISE, parent));
                nextStepInstanses.Add(Rotate(i, DirectionEnum.COUNTERCLOCKWISE, parent));
            }
            return nextStepInstanses;
        }
        public GameFieldInstanse Rotate(int rotateNode, DirectionEnum direction, GameFieldInstanse parent)
        {
            GameField gameField = GameField.Copy(parent.Current);
            gameField.Rotate(rotateNode, direction);
            return new GameFieldInstanse(gameField, parent);

        }

        public void CreateNewFieldInstanses(UnionGameField parent, int maxDepth, Stack<UnionGameField> open, HashSet<UnionGameField> closed, HashSet<UnionGameField> openCopy)
        {
            if (parent.Depth == maxDepth)
                return;

            for (int i = 0; i < parent.RotatePointsCount; i++)
            {
                parent.Rotate(i, DirectionEnum.CLOCKWISE);
                if (!openCopy.Contains(parent) && !closed.Contains(parent))
                {
                    var rotatedNode = UnionGameField.Copy(parent);
                    parent.Rotate(i, DirectionEnum.COUNTERCLOCKWISE);
                    rotatedNode.Parent = parent;
                    rotatedNode.Depth++;
                    //   rotatedNode.Hash = UnionGameField.GetHashCode(rotatedNode.GameField);
                    openCopy.Add(rotatedNode);
                    open.Push(rotatedNode);
                }
                else
                {
                    parent.Rotate(i, DirectionEnum.COUNTERCLOCKWISE);
                }

                parent.Rotate(i, DirectionEnum.COUNTERCLOCKWISE);

                if (!openCopy.Contains(parent) && !closed.Contains(parent))
                {
                    var rotatedNode = UnionGameField.Copy(parent);
                    parent.Rotate(i, DirectionEnum.CLOCKWISE);
                    rotatedNode.Parent = parent;
                    rotatedNode.Depth++;
                    //    rotatedNode.Hash = UnionGameField.GetHashCode(rotatedNode.GameField);
                    openCopy.Add(rotatedNode);
                    open.Push(rotatedNode);
                }
                else
                    parent.Rotate(i, DirectionEnum.CLOCKWISE);

            }

        }

    }
}
