using SeaBattle_.Supplementation;

namespace SeaBattle_.Models
{
    /// <summary>
    /// Игрок.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Карта.
        /// </summary>
        public Map Map { get; }
        
        /// <summary>
        /// Состояние игры.
        /// </summary>
        public ConditionGame ConditionGame { get; set; }
        
        /// <summary>
        /// ИИ.
        /// </summary>
        public bool IsAi { get; }
        
        /// <summary>
        /// Массив с уничтоженными кораблями.
        /// </summary>
        public int[] ShipsDead { get; } = new int[4];

        public Player(Map map, bool isAi)
        {
            Map = map;
            ConditionGame = ConditionGame.BeginGame;
            IsAi = isAi;
        }
    }
}