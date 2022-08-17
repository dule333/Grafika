using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class MapHandler
    {
        public static int ArraySize = 500;
        public static EntityWrapper[,] BigArray;
        private double maxX, maxY, minX, minY;
        private int missedCount = 0;
        Converter converter = new Converter();
        private bool[,] visitedMatrix, primeMatrix;
        ArrayPosition position;

        Queue<ArrayPosition> queue = new Queue<ArrayPosition>();
        private LineType CalculateJoin(LineType line1, LineType line2)
        {
            LineType result;
            result = line1 | line2;
            return result;
        }
        private void FirstInitMatrix()
        {
            for (int i = 0; i < ArraySize; i++)
            {
                for (int j = 0; j < ArraySize; j++)
                {
                    primeMatrix[i, j] = false;
                }
            }
            visitedMatrix = new bool[ArraySize, ArraySize];

        }
        private void InitMatrix()
        {
            Array.Copy(primeMatrix, visitedMatrix, primeMatrix.Length);
        }
        private void FillArrayEntities(bool noNodes = false)
        {
            converter.Convert(noNodes);
            converter.SetBounds(out maxX, out maxY, out minX, out minY);

            foreach (var item in Converter.powerEntities)
            {
                PlaceEntity(item);
            }
        }

        private void PlaceEntity(EntityWrapper entity)
        {
            int X = (int) ((entity.powerEntity.X - minX) / ((maxX - minX) / ArraySize));
            int Y = (int) ((entity.powerEntity.Y - minY) / ((maxY - minY) / ArraySize)); //Y je obrnuto postavljen na canvasu, needs a mirror to be accurate
            if(BigArray[X, Y] == null)
            {
                BigArray[X, Y] = new EntityWrapper();
                BigArray[X, Y].entityPlaced = true;
                BigArray[X, Y].powerEntity = entity.powerEntity;
                BigArray[X, Y].entityType = entity.entityType;
                return;
            }
            for(int i = -2; i < 3; i++)
            {
                if(BigArray[X + (i % 2), Y + (i / 2)] == null)
                {
                    BigArray[X + (i % 2), Y + (i / 2)] = new EntityWrapper();
                    BigArray[X + (i % 2), Y + (i / 2)].entityPlaced = true;
                    BigArray[X + (i % 2), Y + (i / 2)].powerEntity = entity.powerEntity;
                    BigArray[X + (i % 2), Y + (i / 2)].entityType = entity.entityType;
                    return;
                }
            }
        }

        public void CalculateEntities(bool noNodes)
        {
            BigArray = new EntityWrapper[ArraySize, ArraySize];
            FillArrayEntities(noNodes);
            primeMatrix = new bool[ArraySize, ArraySize];
            FirstInitMatrix();
            FillArrayLines();
        }

        private ArrayPosition Search(Tuple<int, int> firstEnd, Tuple<int, int> secondEnd, bool intersect = false)
        {
            InitMatrix();
            queue.Clear();
            queue.Enqueue(new ArrayPosition() { Parent = null, X = firstEnd.Item1, Y = firstEnd.Item2 });
            visitedMatrix[firstEnd.Item1, firstEnd.Item2] = true;

            int count = 0;
            int maxRange = ((secondEnd.Item1 - firstEnd.Item1)*(secondEnd.Item1 - firstEnd.Item1) + 
                (secondEnd.Item2 - firstEnd.Item2) * (secondEnd.Item2 - firstEnd.Item2)) * 4;


            while (queue.Count > 0)
            {
                count++;
                position = queue.Dequeue();
                if(position.X == secondEnd.Item1 && position.Y == secondEnd.Item2)
                    return position;
                if (count >= maxRange)
                    return null;
                for(int i = -2; i < 3; i++)
                {
                    int newX = position.X + i / 2, newY = position.Y + i % 2;
                    if (newX < 0 || newY < 0 || newX >= ArraySize || newY >= ArraySize)
                        continue;
                    

                    if (!visitedMatrix[newX, newY] && (intersect || BigArray[newX, newY] == null || !BigArray[newX, newY].lineDrawn))
                    {
                        visitedMatrix[newX, newY] = true;
                        queue.Enqueue(new ArrayPosition() { Parent = position, X = newX, Y = newY });
                    }
                }
            }
            return null;
        }

        private void FillArrayLines()
        {
            List<LineEntity> tempList = new List<LineEntity>(Converter.lineEntities.Values);
            foreach (LineEntity item in tempList)
            {
                PlaceLines(item);
            }
            tempList = new List<LineEntity>(Converter.lineEntities2.Values);
            Console.WriteLine("Done with cycle 1");
            foreach (LineEntity item in tempList)
            {
                PlaceLines(item, true);
            }
            Converter.lineEntities2.Clear();
        }

        private void PlaceLines(LineEntity entity, bool intersect = false)
        {
            ArrayPosition position, tempPosition;
            EntityWrapper temp;
            LineType lineType;
            Tuple<int, int> tempFirst, tempSecond;
            tempFirst = GetTuple(entity.FirstEnd);
            tempSecond = GetTuple(entity.SecondEnd);
            if (tempFirst == null || tempSecond == null)
            {
                Converter.lineEntities2.Add(missedCount++, entity);
                return;
            }
            tempPosition = position = Search(tempFirst, tempSecond, intersect);
            if (position == null)
            {
                Converter.lineEntities2.Add(missedCount++, entity);
                return;
            }
            int length = 0;
            while (tempPosition != null)
            {
                length++;
                tempPosition = tempPosition.Parent;
            }
            tempPosition = position;
            for(int i = 0; i < length; i++)
            {
                temp = BigArray[position.X, position.Y];
                if (temp == null)
                    temp = new EntityWrapper();
                if (i == length - 1)
                {
                    lineType = CalculateLineType(tempFirst, new Tuple<int,int>(position.X, position.Y), 
                        new Tuple<int, int>(tempPosition.X, tempPosition.Y));
                }
                else if(i == 0)
                {
                    lineType = CalculateLineType(new Tuple<int, int>(position.Parent.X, position.Parent.Y), 
                        new Tuple<int, int>(position.X, position.Y), tempSecond);
                }
                else 
                {
                    lineType = CalculateLineType(new Tuple<int, int>(position.Parent.X, position.Parent.Y),
                        new Tuple<int, int>(position.X, position.Y),
                        new Tuple<int, int>(tempPosition.X, tempPosition.Y));
                    tempPosition = tempPosition.Parent;
                }
                if (!temp.lineDrawn)
                {
                    temp.lineType = lineType;
                    temp.lineDrawn = true;
                    temp.lines = new List<LineEntity>();
                    temp.lines.Add(entity);
                    temp.lineEntities = new List<Tuple<long,long>>();
                    temp.lineEntities.Add(new Tuple<long, long>(entity.FirstEnd, entity.SecondEnd));

                }
                else
                {
                    temp.lineType = CalculateJoin(BigArray[position.X, position.Y].lineType, lineType);
                    temp.lines.Add(entity);
                    temp.lineEntities.Add(new Tuple<long, long>(entity.FirstEnd, entity.SecondEnd));
                }
                BigArray[position.X, position.Y] = temp;
                position = position.Parent;
            }
        }

        private LineType CalculateLineType(Tuple<int, int> tuple1, Tuple<int, int> tuple2, Tuple<int, int> tuple3)
        {
            LineType lineType;
            if ((tuple1.Item1 == tuple2.Item1) && (tuple1.Item1 == tuple3.Item1))
                lineType = LineType.NS;
            else if ((tuple1.Item2 == tuple2.Item2) && (tuple1.Item2 == tuple3.Item2))
                lineType = LineType.WE;
            else if (((tuple2.Item1 > tuple3.Item1) && (tuple1.Item2 > tuple2.Item2)) ||
                ((tuple1.Item1 < tuple2.Item1) && (tuple2.Item2 < tuple3.Item2)))
                lineType = LineType.NW;
            else if (((tuple2.Item1 < tuple3.Item1) && (tuple1.Item2 > tuple2.Item2)) ||
                ((tuple1.Item1 > tuple2.Item1) && (tuple2.Item2 < tuple3.Item2)))
                lineType = LineType.NE;
            else if (((tuple2.Item1 > tuple3.Item1) && (tuple1.Item2 < tuple2.Item2)) ||
                ((tuple1.Item1 < tuple2.Item1) && (tuple2.Item2 > tuple3.Item2)))
                lineType = LineType.SW;
            else 
                lineType = LineType.SE;
            return lineType;

        }

        public static Tuple<int, int> GetTuple(long Id)
        {
            Tuple<int, int> tuple;
            for(int i = 0; i < ArraySize; i++)
                for(int j = 0; j < ArraySize; j++)
                    if(BigArray[i,j] != null && BigArray[i, j].powerEntity != null && BigArray[i,j].powerEntity.Id == Id)
                    {
                        tuple = new Tuple<int, int>(i, j);
                        return tuple;
                    }    
            return null;
        }
    }
}
