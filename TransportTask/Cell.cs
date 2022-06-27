using TransportTask.Enums;

namespace TransportTask
{
    public class Cell
    {
        public int Value { get; set; }
        public CellStatus Status { get; set; }

        public Cell(int value) : base()
        {
            Value = value;
        }

        public Cell()
        {
            Value = 0;
            Status = CellStatus.Empty;
        }
    }
}
