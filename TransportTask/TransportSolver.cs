using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using TransportTask.Enums;

namespace TransportTask
{
    public class TransportSolver
    {
        private static readonly int START_MIN_VALUE = 999999;

        public int SizeA { get; private set; }
        public int SizeB { get; private set; }

        public int[,] Potentials { get; private set; }
        public Cell[,] Values { get; private set; }
        public bool[,] IsVisited { get; private set; }
        public int[] TotalA { get; private set; }
        public int[] TotalB { get; private set; }

        public int[] TempTotalA { get; private set; }
        public int[] TempTotalB { get; private set; }

        public TransportSolver(int[,] potentials, int[] totalA, int[] totalB)
        {
            SizeA = potentials.GetLength(0);
            SizeB = potentials.GetLength(1);

            Potentials = new int[SizeA, SizeB];
            TotalA = new int[SizeA];
            TotalB = new int[SizeB];

            Array.Copy(potentials, Potentials, potentials.Length);
            Array.Copy(totalA, TotalA, TotalA.Length);
            Array.Copy(totalB, TotalB, TotalB.Length);
        }

        public Cell[,] CalculateValues()
        {
            Point zeroPos = new Point(0, 0);
            int filledCellsAmount;

            do
            {
                FillValuesArray();

                filledCellsAmount = GetFilledCellsAmount();

                if (filledCellsAmount == SizeA + SizeB - 1)
                    break;

                while (filledCellsAmount < SizeA + SizeB - 1)
                {
                    zeroPos = AddZeroInValues(zeroPos);
                    ++filledCellsAmount;
                }
            } while (!IsContainCycle(filledCellsAmount));

            return Values;
        }

        public Cell[,] CreateCycleAndRecalculateValues(Point cycleStartPoint)
        {
            List<Point> cycleRoute = GetCycleRoute(cycleStartPoint);

            if (cycleRoute == null)
                return Values;

            int minValue = FindMinValue(cycleRoute);

            for (int i = 0; i < cycleRoute.Count; ++i)
            {
                if (i % 2 == 0)
                {
                    Values[cycleRoute[i].X, cycleRoute[i].Y].Value += minValue;
                    Values[cycleRoute[i].X, cycleRoute[i].Y].Status = CellStatus.Filled;
                }
                else
                {
                    Values[cycleRoute[i].X, cycleRoute[i].Y].Value -= minValue;

                    if (Values[cycleRoute[i].X, cycleRoute[i].Y].Value == 0)
                        Values[cycleRoute[i].X, cycleRoute[i].Y].Status = CellStatus.Changed;
                }
            }

            int filledCellsAmount = GetFilledCellsAmount();

            while (filledCellsAmount < SizeA + SizeB - 1)
            {
                SetChangedCellsToReqStatus(CellStatus.Filled, true);
                ++filledCellsAmount;
            }

            SetChangedCellsToReqStatus(CellStatus.Empty, false);

            return Values;
        }

        private void InitializeValues()
        {
            Values = new Cell[SizeA, SizeB];

            for (int i = 0; i < SizeA; ++i)
            {
                for (int j = 0; j < SizeB; ++j)
                {
                    Values[i, j] = new Cell();
                }
            }
        }

        private void FillValuesArray()
        {
            InitializeValues();
            IsVisited = new bool[SizeA, SizeB];

            TempTotalA = new int[SizeA];
            TempTotalB = new int[SizeB];
            Array.Copy(TotalA, TempTotalA, TotalA.Length);
            Array.Copy(TotalB, TempTotalB, TotalB.Length);

            int currentSum = 0;
            int totalSum = TotalB.Sum();

            do
            {
                Point minPotentialPos = FindMinPotentialPosition();
                if (TotalA[minPotentialPos.X] >= TotalB[minPotentialPos.Y])
                {
                    if (TotalB[minPotentialPos.Y] != 0)
                    {
                        Values[minPotentialPos.X, minPotentialPos.Y].Value = TotalB[minPotentialPos.Y];
                        Values[minPotentialPos.X, minPotentialPos.Y].Status = CellStatus.Filled;
                        TotalA[minPotentialPos.X] -= TotalB[minPotentialPos.Y];
                        currentSum += TotalB[minPotentialPos.Y];
                        TotalB[minPotentialPos.Y] = 0;
                    }
                }
                else
                {
                    if (TotalA[minPotentialPos.X] != 0)
                    {
                        Values[minPotentialPos.X, minPotentialPos.Y].Value = TotalA[minPotentialPos.X];
                        Values[minPotentialPos.X, minPotentialPos.Y].Status = CellStatus.Filled;
                        TotalB[minPotentialPos.Y] -= TotalA[minPotentialPos.X];
                        currentSum += TotalA[minPotentialPos.X];
                        TotalA[minPotentialPos.X] = 0;
                    }
                }
                IsVisited[minPotentialPos.X, minPotentialPos.Y] = true;
            } while (currentSum < totalSum);

            TotalA = TempTotalA;
            TotalB = TempTotalB;
        }

        private Point FindMinPotentialPosition()
        {
            Point minPotentialPos = new Point();
            int minPotential = START_MIN_VALUE;

            for (int i = 0; i < SizeA; ++i)
            {
                for (int j = 0; j < SizeB; ++j)
                {
                    if (Potentials[i, j] >= minPotential || IsVisited[i, j])
                        continue;

                    minPotential = Potentials[i, j];
                    minPotentialPos.X = i;
                    minPotentialPos.Y = j;
                }
            }

            return minPotentialPos;
        }

        private Point AddZeroInValues(Point zeroPos)
        {
            if (zeroPos.X >= SizeA || zeroPos.Y >= SizeB)
                return zeroPos;

            do
            {
                if (Values[zeroPos.X, zeroPos.Y].Status != CellStatus.Empty)
                {
                    zeroPos = AddOnePos(zeroPos);
                }
                else
                {
                    Values[zeroPos.X, zeroPos.Y].Value = 0;
                    Values[zeroPos.X, zeroPos.Y].Status = CellStatus.Filled;
                    zeroPos = AddOnePos(zeroPos);
                    return zeroPos;
                }
            } while (true);
        }

        private Point AddOnePos(Point point)
        {
            ++point.Y;

            if (point.Y >= SizeB)
            {
                ++point.X;
                point.Y = 0;

                if (point.X >= SizeB)
                    return point;
            }

            return point;
        }

        private bool IsContainCycle(int minValueAmount)
        {
            List<Point> cycleRoute = new List<Point>();

            for (int i = 0; i < SizeA; ++i)
            {
                for (int j = 0; j < SizeB; ++j)
                {
                    if (Values[i, j].Status != CellStatus.Empty)
                        continue;

                    if (!IsCycleDeadEnd(cycleRoute, new Point(i, j), CycleDirection.None, minValueAmount))
                        return true;
                }
            }

            return false;
        }

        private List<Point> GetCycleRoute(Point cycleStartPoint)
        {
            int iterationsAmount = 0;
            Point newPoint;
            CycleDirection cycleDirection = CycleDirection.None;
            List<Point> cycleRoute = new List<Point>();
            cycleRoute.Add(cycleStartPoint);
            Point lastPoint = cycleStartPoint;

            do
            {
                for (int i = 0; i < SizeA; ++i)
                {
                    for (int j = 0; j < SizeB; ++j)
                    {
                        if (Values[i, j].Status == CellStatus.Empty || IsPointContains(cycleRoute, i, j))
                            continue;

                        if (lastPoint.X == i && cycleDirection != CycleDirection.Y)
                        {
                            newPoint = new Point(i, j);

                            if (IsCycleDeadEnd(cycleRoute, newPoint, CycleDirection.Y, 3))
                                continue;

                            cycleRoute.Add(newPoint);
                            lastPoint = newPoint;
                            cycleDirection = CycleDirection.Y;
                            i = 0;
                            j = 0;
                        }
                        else if (lastPoint.Y == j && cycleDirection != CycleDirection.X)
                        {
                            newPoint = new Point(i, j);

                            if (IsCycleDeadEnd(cycleRoute, newPoint, CycleDirection.X, 3))
                                continue;

                            cycleRoute.Add(newPoint);
                            lastPoint = newPoint;
                            cycleDirection = CycleDirection.X;
                            i = 0;
                            j = 0;
                        }

                        if (cycleRoute.Count >= 4 && IsCycleClosed(cycleStartPoint, lastPoint))
                            return cycleRoute;
                    }
                }
                ++iterationsAmount;
            } while (iterationsAmount > SizeA * SizeB);

            return null;
        }

        private bool IsPointContains(List<Point> points, int X, int Y)
        {
            for (int i = 0; i < points.Count; ++i)
            {
                if (points[i].X == X && points[i].Y == Y)
                    return true;
            }

            return false;
        }

        private bool IsCycleDeadEnd(List<Point> cycleRoute, Point lastPoint, CycleDirection cycleDirection, int minValueAmount)
        {
            if (cycleRoute.Count >= minValueAmount && IsCycleClosed(cycleRoute[0], lastPoint))
                return false;

            Point newPoint;
            List<Point> cycleRouteCopy = new List<Point>();
            for (int i = 0; i < cycleRoute.Count; ++i)
                cycleRouteCopy.Add(cycleRoute[i]);
            cycleRouteCopy.Add(lastPoint);

            for (int i = 0; i < SizeA; ++i)
            {
                for (int j = 0; j < SizeB; ++j)
                {
                    if (Values[i, j].Status == CellStatus.Empty || IsPointContains(cycleRouteCopy, i, j))
                        continue;

                    if (lastPoint.X == i && cycleDirection != CycleDirection.Y)
                    {
                        newPoint = new Point(i, j);

                        if (IsCycleDeadEnd(cycleRouteCopy, newPoint, CycleDirection.Y, minValueAmount))
                            continue;

                        return false;
                    }
                    else if (lastPoint.Y == j && cycleDirection != CycleDirection.X)
                    {
                        newPoint = new Point(i, j);

                        if (IsCycleDeadEnd(cycleRouteCopy, newPoint, CycleDirection.X, minValueAmount))
                            continue;

                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsCycleClosed(Point cycleStartPoint, Point lastPoint)
        {
            if (cycleStartPoint.X == lastPoint.X || cycleStartPoint.Y == lastPoint.Y)
                return true;

            return false;
        }

        private int FindMinValue(List<Point> points)
        {
            int minValue = START_MIN_VALUE;

            for (int i = 1; i < points.Count; ++i)
            {
                if (i % 2 == 0)
                    continue;

                if (Values[points[i].X, points[i].Y].Value >= minValue)
                    continue;

                minValue = Values[points[i].X, points[i].Y].Value;
            }

            return minValue;
        }

        private int GetFilledCellsAmount()
        {
            int filledCellsAmount = 0;

            for (int i = 0; i < Values.GetLength(0); ++i)
            {
                for (int j = 0; j < Values.GetLength(1); ++j)
                {
                    if (Values[i, j].Status != CellStatus.Filled)
                        continue;

                    ++filledCellsAmount;
                }
            }

            return filledCellsAmount;
        }

        private void SetChangedCellsToReqStatus(CellStatus reqStatus, bool isOnlyOneCell)
        {
            for (int i = 0; i < Values.GetLength(0); ++i)
            {
                for (int j = 0; j < Values.GetLength(1); ++j)
                {
                    if (Values[i, j].Status != CellStatus.Changed)
                        continue;

                    Values[i, j].Status = reqStatus;

                    if (isOnlyOneCell)
                        return;
                }
            }
        }
    }
}
