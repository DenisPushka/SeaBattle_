namespace SeaBattle_.Models
{
    public class Ship
    {
        /// <summary>
        ///  1 - по вертикали <br/> 2 - по горизонтали
        /// </summary>
        public int Rotation { get; set; }
        public Cell[] Cells { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        
        public int Hp { get; set; }
        
        public bool IsDead { get; set; }

        public Ship()
        {
            Width = Height = 1;
            IsDead = false;
        }

        public Ship(int rotation, Cell[] cells)
        {
            Cells = cells;
            Width = Height = 1;
            Rotation = rotation;
            if (rotation == 1)
                Height = cells.Length;
            else
                Width = cells.Length;
        }

        public void UpdateRotationForCheck(int rotation)
        {
            Rotation = rotation;
            Width = Height = 1;
            if (rotation == 1)
                Height = Cells.Length;
            else
                Width = Cells.Length;
        }

        public void UpdateRotationForInstallationToPlace(int rotation, int i)
        {
            switch (rotation)
            {
                case 1:
                    Height = i;
                    Width = 1;
                    Rotation = 1;
                    break;
                case 2:
                    Width = i;
                    Height = 1;
                    Rotation = 2;
                    break;
            }
        }
    }
}