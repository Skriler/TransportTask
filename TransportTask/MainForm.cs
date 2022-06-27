using System;
using System.Windows.Forms;
using TransportTask.TaskConstants;
using TransportTask.Enums;

namespace TransportTask
{
    public partial class MainForm : Form
    {
        private int amountOfA = 5;
        private int amountOfB = 5;
        private TransportValues transportValues;
        private TextBox[,] txtBxsPotential;
        private TextBox[,] txtBxsValue;
        private TextBox[] txtBxsTotalA;
        private TextBox[] txtBxsTotalB;
        private Label[] lblsA;
        private Label[] lblsB;

        public MainForm()
        {
            InitializeComponent();
            InitializeArrays(amountOfA, amountOfB);

            FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void btnInitializeFirstTask_Click(object sender, EventArgs e)
        {
            ClearTxtBxTransportValues();

            transportValues = new TransportValues(
                FirstTaskConstants.POTENTIALS,
                FirstTaskConstants.TOTAL_A,
                FirstTaskConstants.TOTAL_B
                );

            HideFields(transportValues.SizeA, transportValues.SizeB);
            SetTxtBxStartValues();
        }

        private void btnInitializeSecondTask_Click(object sender, EventArgs e)
        {
            ClearTxtBxTransportValues();

            transportValues = new TransportValues(
                SecondTaskConstants.POTENTIALS,
                SecondTaskConstants.TOTAL_A,
                SecondTaskConstants.TOTAL_B
                );

            HideFields(transportValues.SizeA, transportValues.SizeB);
            SetTxtBxStartValues();
        }

        private void btnInitializeThirdTask_Click(object sender, EventArgs e)
        {
            ClearTxtBxTransportValues();

            transportValues = new TransportValues(
                ThirdTaskConstants.POTENTIALS,
                ThirdTaskConstants.TOTAL_A,
                ThirdTaskConstants.TOTAL_B
                );

            HideFields(transportValues.SizeA, transportValues.SizeB);
            SetTxtBxStartValues();
        }

        private void btnInitializeFourthTask_Click(object sender, EventArgs e)
        {
            ClearTxtBxTransportValues();

            transportValues = new TransportValues(
                FourthTaskConstants.POTENTIALS,
                FourthTaskConstants.TOTAL_A,
                FourthTaskConstants.TOTAL_B
                );

            HideFields(transportValues.SizeA, transportValues.SizeB);
            SetTxtBxStartValues();
        }

        private void btnAmountSettingsSubmit_Click(object sender, EventArgs e)
        {
            if (txtBxAmountA.Text == "" || !char.IsDigit(char.Parse(txtBxAmountA.Text)) ||
                txtBxAmountB.Text == "" || !char.IsDigit(char.Parse(txtBxAmountB.Text)))
            {
                MessageBox.Show(
                    "You must enter the numbers!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                return;
            }

            amountOfA = int.Parse(txtBxAmountA.Text);
            amountOfB = int.Parse(txtBxAmountB.Text);
            
            if (amountOfA > ProjectSettings.MAX_A_AMOUNT || 
                amountOfA < ProjectSettings.MIN_A_AMOUNT || 
                amountOfB > ProjectSettings.MAX_B_AMOUNT || 
                amountOfB < ProjectSettings.MIN_B_AMOUNT)
            {
                MessageBox.Show(
                    "Numbers must be between " + ProjectSettings.MIN_A_AMOUNT + " and " + ProjectSettings.MAX_A_AMOUNT + "!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                return;
            }

            InitializeArrays(amountOfA, amountOfB);
            HideFields(amountOfA, amountOfB);
        }

        private void btnClearValues_Click(object sender, EventArgs e)
        {
            ClearTxtBxTransportValues();
        }

        private void btnCalculateValues_Click(object sender, EventArgs e)
        {
            GetValuesFromFields();

            if (transportValues == null)
                return;

            if (!transportValues.IsAEqualsB())
            {
                MessageBox.Show(
                    "A must equals B!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                return;
            }

            transportValues.CalculateValues();
            ClearTxtBxTransportValues();
            SetTxtBxTransportValues();
        }

        private void btnCalculatePotentials_Click(object sender, EventArgs e)
        {
            if (transportValues == null || transportValues.Values == null)
            {
                MessageBox.Show(
                    "You must enter the values first!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                return;
            }

            transportValues.CalculatePotentials();
            SetPotentialValues();
        }

        private void btnCreateCycle_Click(object sender, EventArgs e)
        {
            if (transportValues == null || transportValues.PotentialValues == null)
            {
                MessageBox.Show(
                    "You must enter the values first!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                return;
            }

            if (transportValues.IsAnswerCorrect())
            {
                MessageBox.Show(
                    "Plan is optimal!",
                    "Answer correct",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                    );
                return;
            }

            transportValues.CreateCycleAndRecalculateValues();
            ClearTxtBxTransportValues();
            SetTxtBxTransportValues();
        }

        private void btnQuickSolution_Click(object sender, EventArgs e)
        {
            GetValuesFromFields();

            if (transportValues == null)
                return;

            if (!transportValues.IsAEqualsB())
            {
                MessageBox.Show(
                    "A must equals B!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                return;
            }

            int iterationsAmount = 0;

            transportValues.CalculateValues();
            do
            {
                transportValues.CalculatePotentials();

                if (transportValues.IsAnswerCorrect())
                    break;

                transportValues.CreateCycleAndRecalculateValues();
                ++iterationsAmount;
            } while (iterationsAmount < ProjectSettings.MAX_ITERATIONS);

            ClearTxtBxTransportValues();
            SetTxtBxTransportValues();
        }

        private void btnCheckAnswer_Click(object sender, EventArgs e)
        {
            if (transportValues == null || transportValues.PotentialValues == null)
            {
                MessageBox.Show(
                    "You must enter the values first!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                return;
            }

            if (transportValues.IsAnswerCorrect())
            {
                MessageBox.Show(
                    "Plan is optimal!",
                    "Answer correct",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                    );
            }
            else
            {
                MessageBox.Show(
                    "Plan is not optimal!",
                    "Answer is not correct",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                    );
                return;
            }
        }

        private void GetValuesFromFields()
        {
            int[,] potentials = new int[amountOfA, amountOfB];
            int[] totalA = new int[amountOfA];
            int[] totalB = new int[amountOfB];

            try
            {
                for (int i = 0; i < txtBxsValue.GetLength(0); ++i)
                {
                    for (int j = 0; j < txtBxsValue.GetLength(1); ++j)
                    {
                        potentials[i, j] = int.Parse(txtBxsPotential[i, j].Text);

                        if (i == 0)
                            totalB[j] = int.Parse(txtBxsTotalB[j].Text);
                    }

                    totalA[i] = int.Parse(txtBxsTotalA[i].Text);
                }
            }
            catch(Exception)
            {
                MessageBox.Show(
                    "You must enter the numbers!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                return;
            }

            transportValues = new TransportValues(potentials, totalA, totalB);
        }

        private void InitializeArrays(int sizeA, int sizeB)
        {
            amountOfA = sizeA;
            amountOfB = sizeB;

            txtBxsPotential = new TextBox[sizeA, sizeB];
            txtBxsValue = new TextBox[sizeA, sizeB];
            txtBxsTotalA = new TextBox[sizeA];
            txtBxsTotalB = new TextBox[sizeB];
            lblsA = new Label[sizeA];
            lblsB = new Label[sizeB];

            for (int i = 0; i < sizeA; ++i)
            {
                for (int j = 0; j < sizeB; ++j)
                {
                    GroupBox currentGrpBx = Controls["grpBxA" + (i + 1) + "B" + (j + 1)] as GroupBox;

                    txtBxsPotential[i, j] = currentGrpBx.Controls["txtBoxPotentialA" + (i + 1) + "B" + (j + 1)] as TextBox;
                    txtBxsValue[i, j] = currentGrpBx.Controls["txtBoxValueA" + (i + 1) + "B" + (j + 1)] as TextBox;

                    if (i == 0)
                    {
                        txtBxsTotalB[j] = Controls["txtBoxTotalB" + (j + 1)] as TextBox;
                        lblsB[j] = Controls["lblB" + (j + 1)] as Label;
                    }
                }

                txtBxsTotalA[i] = Controls["txtBoxTotalA" + (i + 1)] as TextBox;
                lblsA[i] = Controls["lblA" + (i + 1)] as Label;
            }
        }

        private void SetTxtBxStartValues()
        {
            if (transportValues == null)
                return;

            InitializeArrays(transportValues.SizeA, transportValues.SizeB);

            for (int i = 0; i < transportValues.SizeA; ++i)
            {
                for (int j = 0; j < transportValues.SizeB; ++j)
                {
                    txtBxsPotential[i, j].Text = transportValues.Potentials[i, j].ToString();

                    if (i == 0)
                        txtBxsTotalB[j].Text = transportValues.TotalB[j].ToString();
                }

                txtBxsTotalA[i].Text = transportValues.TotalA[i].ToString();
            }
        }

        private void HideFields(int sizeA, int sizeB)
        {
            ShowAllFields();

            if (sizeA < 5)
                SetVisibilityA5Group(false);

            if (sizeA < 4)
                SetVisibilityA4Group(false);

            if (sizeB < 5)
                SetVisibilityB5Group(false);

            if (sizeB < 4)
                SetVisibilityB4Group(false);
        }

        private void ShowAllFields()
        {
            SetVisibilityA4Group(true);
            SetVisibilityA5Group(true);
            SetVisibilityB4Group(true);
            SetVisibilityB5Group(true);
        }

        private void SetVisibilityA4Group(bool isVisible)
        {
            lblA4.Visible = isVisible;
            grpBxA4B1.Visible = isVisible;
            grpBxA4B2.Visible = isVisible;
            grpBxA4B3.Visible = isVisible;
            grpBxA4B4.Visible = isVisible;
            grpBxA4B5.Visible = isVisible;
            txtBoxTotalA4.Visible = isVisible;
        }

        private void SetVisibilityA5Group(bool isVisible)
        {
            lblA5.Visible = isVisible;
            grpBxA5B1.Visible = isVisible;
            grpBxA5B2.Visible = isVisible;
            grpBxA5B3.Visible = isVisible;
            grpBxA5B4.Visible = isVisible;
            grpBxA5B5.Visible = isVisible;
            txtBoxTotalA5.Visible = isVisible;
        }

        private void SetVisibilityB4Group(bool isVisible)
        {
            lblB4.Visible = isVisible;
            grpBxA1B4.Visible = isVisible;
            grpBxA2B4.Visible = isVisible;
            grpBxA3B4.Visible = isVisible;
            grpBxA4B4.Visible = isVisible;
            grpBxA5B4.Visible = isVisible;
            txtBoxTotalB4.Visible = isVisible;
        }

        private void SetVisibilityB5Group(bool isVisible)
        {
            lblB5.Visible = isVisible;
            grpBxA1B5.Visible = isVisible;
            grpBxA2B5.Visible = isVisible;
            grpBxA3B5.Visible = isVisible;
            grpBxA4B5.Visible = isVisible;
            grpBxA5B5.Visible = isVisible;
            txtBoxTotalB5.Visible = isVisible;
        }

        private void SetTxtBxTransportValues()
        {
            if (transportValues == null)
                return;

            for (int i = 0; i < transportValues.SizeA; ++i)
            {
                for (int j = 0; j < transportValues.SizeB; ++j)
                {
                    if (transportValues.Values[i, j].Status == CellStatus.Empty)
                        continue;

                    txtBxsValue[i, j].Text = transportValues.Values[i, j].Value.ToString();
                }
            }
        }

        private void ClearTxtBxTransportValues()
        {
            if (txtBxsValue == null || transportValues == null)
                return;

            for (int i = 0; i < transportValues.SizeA; ++i)
            {
                for (int j = 0; j < transportValues.SizeB; ++j)
                {
                    txtBxsValue[i, j].Text = "";

                    if (i == 0)
                        lblsB[j].Text = "B" + (j + 1);
                }

                lblsA[i].Text = "A" + (i + 1);
            }
        }

        private void SetPotentialValues()
        {
            for (int i = 0; i < transportValues.SizeA; ++i)
            {
                for (int j = 0; j < transportValues.SizeB; ++j)
                {
                    if (i == 0)
                        lblsB[j].Text = "v" + (j + 1) + "=" + transportValues.ValuesV[j].ToString();

                    if (transportValues.Values[i, j].Status != CellStatus.Empty)
                        continue;

                    txtBxsValue[i, j].Text = "p=" + transportValues.PotentialValues[i, j].ToString();
                }

                lblsA[i].Text = "u" + (i + 1) + "=" + transportValues.ValuesU[i].ToString();
            }
        }
    }
}
