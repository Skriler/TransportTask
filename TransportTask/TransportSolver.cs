using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace TransportTask
{
    public class TransportSolver
    {
        private static readonly int START_MIN_VALUE = 999999;

        public int SizeA { get; private set; }
        public int SizeB { get; private set; }

        public int[,] Potentials { get; private set; }
        public int[,] Values { get; private set; }
        public bool[,] IsVisited { get; private set; }
        public int[] TotalA { get; private set; }
        public int[] TotalB { get; private set; }

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

        public int[,] CalculateValues()
        {
            Values = new int[SizeA, SizeB];
            IsVisited = new bool[SizeA, SizeB];

            int currentSum = 0;
            int totalSum = TotalB.Sum();

            do
            {
                Point minPotentialPos = FindMinPotentialPosition();
                if (TotalA[minPotentialPos.X] >= TotalB[minPotentialPos.Y])
                {
                    Values[minPotentialPos.X, minPotentialPos.Y] = TotalB[minPotentialPos.Y];
                    TotalA[minPotentialPos.X] -= TotalB[minPotentialPos.Y];
                    currentSum += TotalB[minPotentialPos.Y];
                    TotalB[minPotentialPos.Y] = 0;
                }
                else
                {
                    Values[minPotentialPos.X, minPotentialPos.Y] = TotalA[minPotentialPos.X];
                    TotalB[minPotentialPos.Y] -= TotalA[minPotentialPos.X];
                    currentSum += TotalA[minPotentialPos.X];
                    TotalA[minPotentialPos.X] = 0;
                }
                IsVisited[minPotentialPos.X, minPotentialPos.Y] = true;
            } while (currentSum < totalSum);

            return Values;
        }

        public int[,] CreateCycleAndRecalculateValues(Point cycleStartPoint)
        {
            List<Point> cycleRoute = GetCycleRoute(cycleStartPoint);

            if (cycleRoute == null)
                return Values;

            int minValue = FindMinValue(cycleRoute);

            for (int i = 0; i < cycleRoute.Count; ++i)
            {
                if (i % 2 == 0)
                    Values[cycleRoute[i].X, cycleRoute[i].Y] += minValue;
                else
                    Values[cycleRoute[i].X, cycleRoute[i].Y] -= minValue;
            }

            return Values;
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
                        if (Values[i, j] == 0 || IsPointContains(cycleRoute, i, j))
                            continue;

                        if (lastPoint.X == i && cycleDirection != CycleDirection.Y)
                        {
                            newPoint = new Point(i, j);

                            if (IsCycleDeadEnd(cycleRoute, newPoint, CycleDirection.Y))
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

                            if (IsCycleDeadEnd(cycleRoute, newPoint, CycleDirection.X))
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

        private bool IsCycleDeadEnd(List<Point> cycleRoute, Point lastPoint, CycleDirection cycleDirection)
        {
            if (cycleRoute.Count >= 3 && IsCycleClosed(cycleRoute[0], lastPoint))
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
                    if (Values[i, j] == 0 || IsPointContains(cycleRouteCopy, i, j))
                        continue;

                    if (lastPoint.X == i && cycleDirection != CycleDirection.Y)
                    {
                        newPoint = new Point(i, j);

                        if (IsCycleDeadEnd(cycleRouteCopy, newPoint, CycleDirection.Y))
                            continue;

                        return false;
                    }
                    else if (lastPoint.Y == j && cycleDirection != CycleDirection.X)
                    {
                        newPoint = new Point(i, j);

                        if (IsCycleDeadEnd(cycleRouteCopy, newPoint, CycleDirection.X))
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

                if (Values[points[i].X, points[i].Y] >= minValue)
                    continue;

                minValue = Values[points[i].X, points[i].Y];
            }

            return minValue;
        }
    }
}
