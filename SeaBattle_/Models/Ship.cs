namespace SeaBattle_.Models
{
    /// <summary>
    /// Корабль.
    /// </summary>
    public class Ship
    {
        /// <summary>
        ///  1 - по вертикали <br/> 2 - по горизонтали
        /// </summary>
        public int Rotation { get; set; }
        
        /// <summary>
        /// Клетки на которых располагается корабль.
        /// </summary>
        public Cell[] Cells { get; set; }
        /// <summary>
        /// Ширина корабля.
        /// </summary>
        public int Width { get; set; }
        
        /// <summary>
        /// Высота корабля.
        /// </summary>
        public int Height { get; set; }
        
        /// <summary>
        /// Количество жизней у корабля.
        /// </summary>
        public int Hp { get; set; }
        
        /// <summary>
        /// Жив ли корабль.
        /// </summary>
        public bool IsDead { get; set; }

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Ship()
        {
            Width = Height = 1;
            IsDead = false;
        }

        /// <summary>
        /// Конструтор с 2 параметрами.
        /// </summary>
        /// <param name="rotation">Поворот.</param>
        /// <param name="cells">Клетки в которых располагается корабль.</param>
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

        /// <summary>
        /// Изменение поворота у корабля.
        /// </summary>
        /// <param name="rotation">Поворот.</param>
        public void UpdateRotationForCheck(int rotation)
        {
            Rotation = rotation;
            Width = Height = 1;
            if (rotation == 1)
                Height = Cells.Length;
            else
                Width = Cells.Length;
        }

        /// <summary>
        /// Изменение расположения корабля.
        /// </summary>
        /// <param name="rotation">Поворот.</param>
        /// <param name="i">Длина корабля.</param>
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