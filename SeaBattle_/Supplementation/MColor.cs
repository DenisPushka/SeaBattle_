using System.Drawing;

namespace SeaBattle_.Supplementation
{
    /// <summary>
    /// Цвета для карты.
    /// </summary>
    public abstract class MColor
    {
        /// <summary>
        /// Корабль цел.
        /// </summary>
        public static Color Ship = Color.Goldenrod;
        /// <summary>
        /// Корабли подбит.
        /// </summary>
        public static Color DamageShip = Color.Brown;
        /// <summary>
        /// Произведен выстрел в море.
        /// </summary>
        public static Color Point = Color.Teal;
        
        /// <summary>
        /// Цвет клетки по умолчанию. 
        /// </summary>
        public static Color Cell = Color.Aqua;
    }
}