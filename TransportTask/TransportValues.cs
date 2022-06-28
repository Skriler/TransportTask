using System;
using System.Drawing;
using System.Linq;
using TransportTask.Enums;

namespace TransportTask
{
    public class TransportValues
    {
        private static readonly int EMPTY_V_U_VALUE = -999999;

        public int SizeA { get; private set; }
        public int SizeB { get; private set; }

        public int[,] Potentials { get; private set; }
        public Cell[,] Values { get; private set; }
        public int[] TotalA { get; private set; }
        public int[] TotalB { get; private set; }

        public int[,] PotentialValues { get; private set; }
        public int[] ValuesU { get; private set; }
        public int[] ValuesV { get; private set; }

        private TransportSolver transportSolver;

        public TransportValues(int[,] potentials, int[] totalA, int[] totalB)
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

        public bool IsAEqualsB()
        {
            return TotalA.Sum() == TotalB.Sum();
        }

        public void CalculateValues()
        {
            InitializeValues();
            transportSolver = new TransportSolver(
                Potentials,
                TotalA,
                TotalB
                );
            
            Cell[,] TempValues = transportSolver.CalculateValues();
            Array.Copy(TempValues, Values, TempValues.Length);
        }

        public void CalculatePotentials()
        {
            PotentialValues = new int[SizeA, SizeB];
            ValuesU = Enumerable.Repeat(EMPTY_V_U_VALUE, SizeA).ToArray();
            ValuesV = Enumerable.Repeat(EMPTY_V_U_VALUE, SizeB).ToArray();

            CalculateUV();
            CalculatePotentialValues();
        }

        public void CreateCycleAndRecalculateValues()
        {
            if (transportSolver == null)
                return;

            Values = new Cell[SizeA, SizeB];

            Point maxPotentialValue = FindMaxPotentialValue(); 
            Cell[,] TempValues = transportSolver.CreateCycleAndRecalculateValues(maxPotentialValue);
            Array.Copy(TempValues, Values, TempValues.Length);
        }

        public bool IsAnswerCorrect()
        {
            for (int i = 0; i < ValuesU.Length; ++i)
            {
                for (int j = 0; j < ValuesV.Length; ++j)
                {
                    if (PotentialValues[i, j] > 0)
                        return false;
                }
            }

            return true;
        }

        public int GetTargetFunction()
        {
            int result = 0;

            for (int i = 0; i < SizeA; ++i)
            {
                for (int j = 0; j < SizeB; ++j)
                {
                    if (Values[i, j].Status != CellStatus.Filled)
                        continue;

                    result += Values[i, j].Value * Potentials[i, j];
                }
            }

            return result;
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

        private void CalculateUV()
        {
            ValuesU[0] = 0;
            do
            {
                for (int i = 0; i < ValuesU.Length; ++i)
                {
                    for (int j = 0; j < ValuesV.Length; ++j)
                    {
                        if (Values[i, j].Status == CellStatus.Empty)
                            continue;

                        if (ValuesU[i] != EMPTY_V_U_VALUE && ValuesV[j] == EMPTY_V_U_VALUE)
                        {
                            ValuesV[j] = Potentials[i, j] - ValuesU[i];
                        }
                        else if (ValuesV[j] != EMPTY_V_U_VALUE && ValuesU[i] == EMPTY_V_U_VALUE)
                        {
                            ValuesU[i] = Potentials[i, j] - ValuesV[j];
                        }
                    }
                }
            }
            while (!IsUVFilled());
        }

        private bool IsUVFilled()
        {
            for (int i = 0; i < ValuesU.Length; ++i)
            {
                if (ValuesU[i] == EMPTY_V_U_VALUE)
                    return false;
            }

            for (int j = 0; j < ValuesV.Length; ++j)
            {
                if (ValuesV[j] == EMPTY_V_U_VALUE)
                    return false;
            }

            return true;
        }

        private void CalculatePotentialValues()
        {
            for (int i = 0; i < ValuesU.Length; ++i)
            {
                for (int j = 0; j < ValuesV.Length; ++j)
                {
                    if (Values[i, j].Status != CellStatus.Empty)
                    {
                        PotentialValues[i, j] = -1;
                        continue;
                    }

                    PotentialValues[i, j] = ValuesU[i] + ValuesV[j] - Potentials[i, j];
                }
            }
        }

        private Point FindMaxPotentialValue()
        {
            Point maxPotentialPos = new Point();
            int maxPotentialValue = -1;

            for (int i = 0; i < SizeA; ++i)
            {
                for (int j = 0; j < SizeB; ++j)
                {
                    if (PotentialValues[i, j] <= maxPotentialValue)
                        continue;

                    maxPotentialValue = PotentialValues[i, j];
                    maxPotentialPos.X = i;
                    maxPotentialPos.Y = j;
                }
            }

            return maxPotentialPos;
        }
    }
}
