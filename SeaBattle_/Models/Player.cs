using SeaBattle_.Supplementation;

namespace SeaBattle_.Models
{
    public class Player
    {
        public Map Map { get; }
        public ConditionGame ConditionGame { get; set; }
        public bool IsAi { get; }

        public int[] ShipsDead { get; } = new int[4];

        public Player(Map map, bool isAi)
        {
            Map = map;
            ConditionGame = ConditionGame.BeginGame;
            IsAi = isAi;
        }
    }
}