using System;
using System.Windows.Forms;
using SeaBattle_.Models;
using SeaBattle_.Supplementation;

namespace SeaBattle_
{
    public partial class Main : Form
    {
        /// <summary>
        /// Размерность поля.
        /// </summary>
        private const int SizeMap = 10;

        public Main()
        {
            InitializeComponent();
            InputData.KeyPress += CheckEnterKeyPress;
        }

        /// <summary>
        /// Игра.
        /// </summary>
        private Game _game;

        /// <summary>
        /// Точка входа.
        /// </summary>
        private async void Start()
        {
            // Расстановка кораблей
            var map = new Map(this, SizeMap, 25);
            await map.ShipsPlacementRandom(FieldPart.Map);

            var map2 = new Map(this, SizeMap, 25);
            if (new Random().Next(11) < 8)
                await map2.ShipsPlacementNotRandom(FieldPart.Map);
            else
                await map2.ShipsPlacementRandom(FieldPart.Map);

            // Отрисовка
            map.DrawField(FieldPart.Map);
            map.DrawField(FieldPart.Radar);
            map.DrawFieldChar(FieldPart.Map);
            map.DrawFieldChar(FieldPart.Radar);

            var currentPlayer = new Player(map, false);
            var nextPlayer = new Player(map2, true);

            _game = new Game(currentPlayer, nextPlayer, SizeMap, this);
            _game.Start();
        }

        /// <summary>
        /// Эвент на нажатие клавиши.
        /// </summary>
        private void CheckEnterKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                if (InputData.Text == "")
                {
                    MessageBox.Show(@"Введите поле!", @"Ошибка!", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var arrayChar = new char[2];
                arrayChar[0] = char.ToUpper(InputData.Text[0]);
                arrayChar[1] = InputData.Text[1];
                var move = Cell.TranslateToCell(arrayChar);
                
                if (!_game.CheckMove(move))
                {
                    MessageBox.Show(@"Смените поле для атаки!", @"Ошибка!", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                _game.Move(move);
                InputData.Text = "";
            }
        }

        /// <summary>
        /// Отображение ходов
        /// </summary>
        /// <param name="cell">Ход</param>
        public void VieMove(Cell cell)
        {
            
        }
        
        /// <summary>
        /// Кнопка запуска игры.
        /// </summary>
        private void buttonStart_Click(object sender, EventArgs e) => Start();
    }
}