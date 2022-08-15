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
        public static EntityWrapper[,] BigArray = new EntityWrapper[ArraySize, ArraySize];
        private double maxX, maxY, minX, minY;
        private int missedCount = 0;
        Converter converter = new Converter();
        private bool[,] visitedMatrix = new bool[ArraySize, ArraySize];
        private LineType CalculateJoin(LineType line1, LineType line2)
        {
            LineType result;
            result = line1 | line2;
            return result;
        }
        private void InitMatrix()
        {
            for (int i = 0; i < ArraySize; i++)
            {
                for (int j = 0; j < ArraySize; j++)
                {
                    visitedMatrix[i, j] = false;
                }
            }
        }
        private void FillArrayEntities()
        {
            converter.Convert();
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

        public void CalculateEntities()
        {
            FillArrayEntities();
            FillArrayLines();
        }

        private ArrayPosition Search(Tuple<int, int> firstEnd, Tuple<int, int> secondEnd, bool intersect = false)
        {
            InitMatrix();
            ArrayPosition startingPosition = new ArrayPosition() { Parent = null, X = firstEnd.Item1, Y = firstEnd.Item2};
            Queue<ArrayPosition> queue = new Queue<ArrayPosition>();
            ArrayPosition position = null;
            queue.Enqueue(startingPosition);
            visitedMatrix[firstEnd.Item1, firstEnd.Item2] = true;
            int count = 0;
            int maxRange = (int)Math.Sqrt((secondEnd.Item1 * secondEnd.Item1 - firstEnd.Item1 * firstEnd.Item1) + 
                (secondEnd.Item2 * secondEnd.Item2 - firstEnd.Item2 * firstEnd.Item2));
            maxRange = maxRange * maxRange * 10;


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
                    if (position.X + i / 2 < 0 || position.Y + i % 2 < 0 || position.X + i / 2 >= ArraySize || position.Y + i % 2 >= ArraySize)
                        continue;

                    if (!visitedMatrix[position.X + i / 2, position.Y + i % 2] && (intersect || BigArray[position.X + i / 2, position.Y + i % 2] == null || !BigArray[position.X + i / 2, position.Y + i % 2].lineDrawn))
                    {
                        visitedMatrix[position.X + i / 2, position.Y + i % 2] = true;
                        queue.Enqueue(new ArrayPosition() { Parent = position, X = position.X + i / 2, Y = position.Y + i % 2 });
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
            foreach (LineEntity item in tempList)
            {
                PlaceLines(item, true);
            }
            Converter.lineEntities2.Clear();
        }

        private void PlaceLines(LineEntity entity, bool intersect = false)
        {
            ArrayPosition position, tempPosition;
            Tuple<int, int> tempSpot;
            LineType lineType;
            Tuple<int, int> tempFirst, tempSecond;
            tempFirst = GetTuple(entity.FirstEnd);
            tempSecond = GetTuple(entity.SecondEnd);
            if (tempFirst == null || tempSecond == null)
                return;
            tempPosition = position = Search(tempFirst, tempSecond, intersect);
            if (position == null)
                return;
            int length = 0;
            while (tempPosition != null)
            {
                length++;
                tempPosition = tempPosition.Parent;
            }
            tempPosition = position;
            for(int i = 0; i < length; i++)
            { 
                if(i == length - 1)
                {
                    tempSpot = GetTuple(entity.FirstEnd);
                    lineType = CalculateLineType(tempSpot, new Tuple<int,int>(position.X, position.Y), 
                        new Tuple<int, int>(tempPosition.X, tempPosition.Y));
                }
                else if(i == 0)
                {
                    tempSpot = GetTuple(entity.SecondEnd);
                    lineType = CalculateLineType(new Tuple<int, int>(position.Parent.X, position.Parent.Y), 
                        new Tuple<int, int>(position.X, position.Y), tempSpot);
                }
                else 
                {
                    lineType = CalculateLineType(new Tuple<int, int>(position.Parent.X, position.Parent.Y),
                        new Tuple<int, int>(position.X, position.Y),
                        new Tuple<int, int>(tempPosition.X, tempPosition.Y));
                    tempPosition = tempPosition.Parent;
                }
                if (BigArray[position.X, position.Y] == null)
                    BigArray[position.X, position.Y] = new EntityWrapper();
                if (!BigArray[position.X, position.Y].lineDrawn)
                {
                    BigArray[position.X, position.Y].lineType = lineType;
                    BigArray[position.X, position.Y].lineDrawn = true;
                    BigArray[position.X, position.Y].lines = new List<LineEntity>();
                    BigArray[position.X, position.Y].lines.Add(entity);
                    BigArray[position.X, position.Y].lineEntities = new List<Tuple<long,long>>();
                    BigArray[position.X, position.Y].lineEntities.Add(new Tuple<long, long>(entity.FirstEnd, entity.SecondEnd));

                }
                else
                { 
                    BigArray[position.X, position.Y].lineType = CalculateJoin(BigArray[position.X, position.Y].lineType, lineType);
                    BigArray[position.X, position.Y].lines.Add(entity);
                    BigArray[position.X, position.Y].lineEntities.Add(new Tuple<long, long>(entity.FirstEnd, entity.SecondEnd));
                }
                position = position.Parent;
            }
            if(!intersect)
                Converter.lineEntities2.Add(missedCount++, entity);
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
