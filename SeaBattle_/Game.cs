using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeaBattle_.Models;
using SeaBattle_.Supplementation;

namespace SeaBattle_
{
    /// <summary>
    /// Игра.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Текущий игрок.
        /// </summary>
        private Player CurrentPlayer { get; set; }

        /// <summary>
        /// Соперник.
        /// </summary>
        private Player NextPlayer { get; set; }

        /// <summary>
        /// Размерность поля.
        /// </summary>
        private int SizeMap { get; }

        /// <summary>
        /// Отображатель.
        /// </summary>
        private readonly Main _main;

        /// <summary>
        /// Список использованных файлов с ходами.
        /// </summary>
        private readonly List<MoveMobFile> _filesWithMoves = new List<MoveMobFile>();

        /// <summary>
        /// Количество файлов с ходами.
        /// </summary>
        private int _filesCount;

        /// <summary>
        /// Можно ли стрелять по диагонали.
        /// </summary>
        private bool _isNotMoveToDiagonal;

        public Game(Player currentPlayer, Player nextPlayer, int sizeMap, Main main)
        {
            CurrentPlayer = currentPlayer;
            NextPlayer = nextPlayer;
            SizeMap = sizeMap;
            _main = main;
            _filesCount = Directory.GetFiles("FilesWithMove",
                "*", SearchOption.TopDirectoryOnly).Length;
        }

        /// <summary>
        /// Присвоение значений, что игроки в игре.
        /// </summary>
        public void Start()
        {
            CurrentPlayer.ConditionGame = ConditionGame.InGame;
            NextPlayer.ConditionGame = ConditionGame.InGame;
        }

        /// <summary>
        /// Ход.
        /// </summary>
        /// <param name="move">Клетка куда сходили.</param>
        public async void Move(Cell move)
        {
            while (true)
                if (CurrentPlayer.ConditionGame != ConditionGame.GameOver ||
                    NextPlayer.ConditionGame != ConditionGame.GameOver)
                {
                    // Ai
                    if (CurrentPlayer.IsAi)
                    {
                        if (await MoveAi())
                        {
                            if (CheckWin())
                            {
                                Uno();
                                CurrentPlayer.Map.DrawField(FieldPart.Map);
                                CurrentPlayer.Map.DrawField(FieldPart.Radar);
                                return;
                            }

                            // Если успешно выстрелил
                            continue;
                        }

                        // Промах
                        Uno();
                        CurrentPlayer.Map.DrawField(FieldPart.Map);
                        CurrentPlayer.Map.DrawField(FieldPart.Radar);
                        break;
                    }

                    // Пользователь
                    if (move != null)
                    {
                        if (Attack.DoAttack(CurrentPlayer, NextPlayer, move, this))
                        {
                            if (CheckWin())
                                return;

                            // Успешный выстрел
                            CurrentPlayer.Map.DrawField(FieldPart.Map);
                            CurrentPlayer.Map.DrawField(FieldPart.Radar);
                            break;
                        }

                        // Промах
                        Uno();
                    }
                }
        }

        /// <summary>
        /// Проверка на выигрыш
        /// </summary>
        private bool CheckWin()
        {
            if (GameOver(CurrentPlayer))
            {
                CurrentPlayer.ConditionGame = ConditionGame.GameOver;
                NextPlayer.ConditionGame = ConditionGame.GameOver;
                MessageBox.Show(@"Вы проиграли!", @"Конец игры!", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверка окончания игры.
        /// </summary>
        /// <param name="player">Игрок, который проверятеся на победу.</param>
        /// <returns>true - в случае выигрыша.</returns>
        private static bool GameOver(Player player) =>
            player.ShipsDead[0] == 4 && player.ShipsDead[1] == 3 &&
            player.ShipsDead[2] == 2 && player.ShipsDead[3] == 1;

        /// <summary>
        /// Ход AI
        /// </summary>
        /// <returns>в случае успешного выстрела - true</returns>>
        private async Task<bool> MoveAi()
        {
            // Получение карты в которой происходит выстрел
            var radar = CurrentPlayer.Map.GetFieldPart(FieldPart.Radar);
            CalculationWeight(radar);
            // Обычный ход AI
            var moveAi = CellSearchByWeight(radar);
            // Берем последний файл из списка файлов............как тут не материться?
            var lastFile = _filesWithMoves.Count == 0
                ? null
                : _filesWithMoves[_filesWithMoves.Count - 1];

            // Добавляем файл (работает только при первой итерации)
            if (lastFile == null)
            {
                _filesWithMoves.Add(Attack.AttackFromFile(_filesWithMoves));
                lastFile = _filesWithMoves[_filesWithMoves.Count - 1];
            }

            // отвечает за то, что ходы будут из файла
            var moveFromFile = _filesWithMoves.Count <= _filesCount && lastFile?.CountMove != 0;

            // Если ход типичный (вес == 1), и можно брать ход из файла
            if (moveAi.Weight == 1 && moveFromFile)
            {
                // Пошла логика стрельбы из файла
                if (await MoveMobFromFile(lastFile, radar))
                {
                    _main.VieMove(moveAi);
                    // если попал
                    return true;
                }

                // Выполняется, когда некуда стрелять по диагонали
                if (_isNotMoveToDiagonal)
                {
                    if (lastFile != null) lastFile.CountMove = 0;
                    return true;
                }

                _main.VieMove(moveAi);
                // если промах 
                return false;
            }

            // Если вес больше 1
            if (moveAi.Weight > 1)
            {
                _main.VieMove(moveAi);
                return Attack.DoAttack(CurrentPlayer, NextPlayer, moveAi, this);
            }

            // Добавление файла 
            if (_filesWithMoves.Count != _filesCount)
            {
                _replay = 0;
                _filesWithMoves.Add(Attack.AttackFromFile(_filesWithMoves));
                return true;
            }

            // Логика на обнаружение кораблей, эффективна для многопалубных
            moveAi = SearchCell(radar, moveAi);

            // Обычная стрельба
            _main.VieMove(moveAi);
            return Attack.DoAttack(CurrentPlayer, NextPlayer, moveAi, this);
        }

        /// <summary>
        /// Алгоритм для лучшего поиска однопалубных
        /// </summary>
        /// <param name="radar"></param>
        /// <param name="moveAi"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static Cell SearchForOneCell(Cell[,] radar, Cell moveAi, int x, int y)
        {
            var moveAiBuffer = new Cell { X = x, Y = y };
            // проверяем верхние точки
            if (Map.CheckBorders(x - 1, y - 1) && radar[x - 1, y - 1].Color == MColor.Cell)
                moveAiBuffer.Weight += 5;
            if (Map.CheckBorders(x, y - 1) && radar[x, y - 1].Color == MColor.Cell)
                moveAiBuffer.Weight += 5;
            if (Map.CheckBorders(x + 1, y - 1) && radar[x + 1, y - 1].Color == MColor.Cell)
                moveAiBuffer.Weight += 5;
            // проверяем боковые точки

            if (Map.CheckBorders(x - 1, y) && radar[x - 1, y].Color == MColor.Cell)
                moveAiBuffer.Weight += 5;
            if (Map.CheckBorders(x + 1, y) && radar[x + 1, y].Color == MColor.Cell)
                moveAiBuffer.Weight += 5;

            // проверяем нижние точки
            if (Map.CheckBorders(x - 1, y + 1) && radar[x - 1, y + 1].Color == MColor.Cell)
                moveAiBuffer.Weight += 5;
            if (Map.CheckBorders(x, y + 1) && radar[x, y + 1].Color == MColor.Cell)
                moveAiBuffer.Weight += 5;
            if (Map.CheckBorders(x + 1, y + 1) && radar[x + 1, y + 1].Color == MColor.Cell)
                moveAiBuffer.Weight += 5;

            if (moveAi.Weight <= moveAiBuffer.Weight)
                moveAi = moveAiBuffer;
            return moveAi;
        }

        /// <summary>
        /// Поиск приоритетного поля, когда закончились выстрелы по диагонали. Метод эффективен для многопалубных
        /// </summary>
        /// <param name="radar">Поле соперника.</param>
        /// <param name="moveAi">Ход ИИ.</param>
        private Cell SearchCell(Cell[,] radar, Cell moveAi)
        {
            for (var x = 0; x < SizeMap; x++)
            for (var y = 0; y < SizeMap; y++)
            {
                if (!Attack.CheckAttack(radar, new Cell { X = x, Y = y }))
                    continue;
                // Какой корабль ищем (1-палубный, 2-ух палубный...)
                var lengthShip = SearchShipCount();

                // Для однопалубного 
                if (lengthShip == 1)
                {
                    moveAi = SearchForOneCell(radar, moveAi, x, y);
                }
                else
                {
                    // Вниз
                    var ship = new Ship(1, new Cell[lengthShip]);
                    if (CheckPlaceShip(ship, radar, x, y))
                        radar[x, y].Weight += 5;

                    // Вверх
                    if (CheckPlaceShip(ship, radar, x, y - lengthShip + 1))
                        radar[x, y].Weight += 5;

                    // Вправо
                    ship.UpdateRotationForCheck(2);
                    if (CheckPlaceShip(ship, radar, x, y))
                        radar[x, y].Weight += 5;

                    // Влево
                    if (CheckPlaceShip(ship, radar, x - lengthShip + 1, y))
                        radar[x, y].Weight += 5;
                }

                if (radar[x, y].Weight >= moveAi.Weight)
                {
                    moveAi.X = x;
                    moveAi.Y = y;
                    moveAi.Weight = radar[x, y].Weight;
                }
            }

            return moveAi;
        }

        /// <summary>
        /// Проверка расположения корабля, метод нужен для определения расположения корабля в 4 стороны
        /// </summary>
        /// <param name="ship">Корабль</param>
        /// <param name="radar">Радар</param>
        /// <param name="x">Координата х</param>
        /// <param name="y">Координата х</param>
        /// <returns>Если расположение возможно - true</returns>
        private static bool CheckPlaceShip(Ship ship, Cell[,] radar, int x, int y)
        {
            var end = ship.Rotation == 1 ? ship.Height : ship.Width;
            var s = 0;

            while (s < end)
            {
                var b =
                    // Вертикаль
                    ship.Rotation == 1
                        ? Map.CheckBorders(x, y + s) && Map.CheckShipAndPoint(radar, x, y + s)
                        // Горизонталь
                        : Map.CheckBorders(x + s, y) && Map.CheckShipAndPoint(radar, x + s, y);

                if (!b)
                    return false;
                s++;
            }

            return true;
        }

        /// <summary>
        /// Какой корабль ищем (1-палубный, 2-ух палубный...)
        /// </summary>
        /// <returns>Количество палуб у корабля для поиска</returns>
        private int SearchShipCount()
        {
            if (CurrentPlayer.ShipsDead[3] <= 0)
                return 4;
            if (CurrentPlayer.ShipsDead[2] <= 1)
                return 3;
            return CurrentPlayer.ShipsDead[1] <= 2 ? 2 : 1;
        }

        /// <summary>
        /// Для счета повторений при перестреле по диагонали
        /// </summary>
        private int _replay;

        /// <summary>
        /// Ход из файла
        /// </summary>
        /// <param name="mobFile">файл с ходами</param>
        /// <param name="radar">карта на которой производится выстрел</param>
        /// <returns>в случае попадания/уничтожения - true</returns>
        private async Task<bool> MoveMobFromFile(MoveMobFile mobFile, Cell[,] radar)
        {
            // Клетки по которым можно стрелять
            var cells = SearchEmptyCells(mobFile.Str, radar);
            // Если на диагонали нет места для обстрела
            if (cells.Count == 0 || _replay == 10000)
            {
                _isNotMoveToDiagonal = true;
                _replay = 0;
                return false;
            }

            var cell = cells[new Random().Next(0, cells.Count)];

            if (await CheckingNearbyPoints(cell, radar))
            {
                _replay = 0;
                _isNotMoveToDiagonal = false;
                mobFile.CountMove--;
                return Attack.DoAttack(CurrentPlayer, NextPlayer, cell, this);
            }

            _replay++;
            return true;
        }

        /// <summary>
        /// Проверка на рядом лежащие точки
        /// </summary>
        /// <param name="cell">Поле выстрела</param>
        /// <param name="radar">Карта</param>
        /// <returns>в случае если рядом нет точек - true</returns>
        private static Task<bool> CheckingNearbyPoints(Cell cell, Cell[,] radar)
        {
            // проверка на рядом лежащие точки
            var a = Map.CheckBorders(cell.X - 1, cell.Y - 1) &&
                    Map.CheckShipAndPoint(radar, cell.X - 1, cell.Y - 1);
            var h = Map.CheckBorders(cell.X + 1, cell.Y + 1) &&
                    Map.CheckShipAndPoint(radar, cell.X + 1, cell.Y + 1);

            // Для всех аттак по 0 горизонтали
            if (cell.Y - 1 == -1)
                a = true;

            // Для всех аттак по 0 вертикали
            if (cell.X - 1 == -1)
                a = true;

            // Для всех аттак по 9 горизонтали
            if (cell.Y + 1 == 10)
                a = true;

            // Для всех аттак по 9 вертикали
            if (cell.X + 1 == 10)
                h = true;

            return Task.FromResult(a && h);
        }

        /// <summary>
        /// Проверка обстрела из передаваемой строки  
        /// </summary>
        /// <param name="strCells">Строка с полями для обстрела</param>
        /// <param name="radar">Карта</param>
        /// <returns>Возвращает список полей по которым можно стрелять</returns>
        private static List<Cell> SearchEmptyCells(string strCells, Cell[,] radar)
        {
            var cells = new List<Cell>();
            for (var i = 0; i < strCells.Length; i++)
                if (char.IsLetter(strCells[i]))
                {
                    var cell = Cell.TranslateToCell(new[] { strCells[i], strCells[i + 1] });
                    if (Attack.CheckAttack(radar, cell))
                        cells.Add(cell);
                }

            return cells;
        }

        /// <summary>
        /// Поиск клетки по весу
        /// </summary>
        private Cell CellSearchByWeight(Cell[,] radar)
        {
            var moveAi = radar[0, 0];
            for (var x = 0; x < SizeMap; x++)
            for (var y = 0; y < SizeMap; y++)
                if (moveAi.Weight < radar[x, y].Weight)
                    moveAi = radar[x, y];

            return moveAi;
        }

        /// <summary>
        /// Смена игроков
        /// </summary>
        private void Uno() => (CurrentPlayer, NextPlayer) = (NextPlayer, CurrentPlayer);

        public static void PaintDeadShip(Ship ship, Player current, Player next)
        {
            // Для однушки
            if (ship.Cells.Length == 1)
            {
                var c = ship.Cells[0];
                PaintingPoint(c.X - 1, c.Y - 1, current, next);
                PaintingPoint(c.X, c.Y - 1, current, next);
                PaintingPoint(c.X + 1, c.Y - 1, current, next);

                PaintingPoint(c.X + 1, c.Y, current, next);

                PaintingPoint(c.X + 1, c.Y + 1, current, next);
                PaintingPoint(c.X, c.Y + 1, current, next);
                PaintingPoint(c.X - 1, c.Y + 1, current, next);

                PaintingPoint(c.X - 1, c.Y, current, next);
                return;
            }

            for (var j = 0; j < ship.Cells.Length; j++)
            {
                var c = ship.Cells[j];
                switch (ship.Height > ship.Width)
                {
                    case true:
                        if (j == 0)
                        {
                            PaintingPoint(c.X, c.Y - 1, current, next);
                            PaintingPoint(c.X - 1, c.Y - 1, current, next);
                            PaintingPoint(c.X + 1, c.Y - 1, current, next);
                        }
                        else if (j == ship.Cells.Length - 1)
                        {
                            PaintingPoint(c.X, c.Y + 1, current, next);
                            PaintingPoint(c.X - 1, c.Y + 1, current, next);
                            PaintingPoint(c.X + 1, c.Y + 1, current, next);
                        }

                        PaintingPoint(c.X - 1, c.Y, current, next);
                        PaintingPoint(c.X + 1, c.Y, current, next);
                        break;
                    case false:
                        if (j == 0)
                        {
                            PaintingPoint(c.X - 1, c.Y, current, next);
                            PaintingPoint(c.X - 1, c.Y - 1, current, next);
                            PaintingPoint(c.X - 1, c.Y + 1, current, next);
                        }
                        else if (j == ship.Cells.Length - 1)
                        {
                            PaintingPoint(c.X + 1, c.Y, current, next);
                            PaintingPoint(c.X + 1, c.Y - 1, current, next);
                            PaintingPoint(c.X + 1, c.Y + 1, current, next);
                        }

                        PaintingPoint(c.X, c.Y - 1, current, next);
                        PaintingPoint(c.X, c.Y + 1, current, next);
                        break;
                }
            }
        }

        /// <summary>
        /// Высчитывание веса координат
        /// </summary>
        /// <param name="radar">Поле</param>
        private void CalculationWeight(Cell[,] radar)
        {
            foreach (var cell in radar)
                cell.Weight = 1;

            for (var x = 0; x < SizeMap; x++)
            for (var y = 0; y < SizeMap; y++)
            {
                radar[x, y].Weight = radar[x, y].CheckShipAndPoint() ? 0 : radar[x, y].Weight;
                if (radar[x, y].Weight > 1) continue;

                if (radar[x, y].Ship != null)
                    switch (radar[x, y].Ship.IsDead)
                    {
                        case false:
                        {
                            if (Math.Max(radar[x, y].Ship.Height, radar[x, y].Ship.Width) > radar[x, y].Ship.Hp)
                            {
                                if (radar[x, y].Ship.Height > radar[x, y].Ship.Width)
                                {
                                    AddWeightAndCheckBorders(radar, x, y - 1);
                                    AddWeightAndCheckBorders(radar, x, y + 1);
                                }
                                else
                                {
                                    AddWeightAndCheckBorders(radar, x - 1, y);
                                    AddWeightAndCheckBorders(radar, x + 1, y);
                                }
                            }
                            else
                            {
                                AddWeightAndCheckBorders(radar, x - 1, y);
                                AddWeightAndCheckBorders(radar, x + 1, y);
                                AddWeightAndCheckBorders(radar, x, y - 1);
                                AddWeightAndCheckBorders(radar, x, y + 1);
                            }
                        }
                            break;
                        case true:
                            radar[x, y].Weight = 0;
                            break;
                    }
            }
        }

        /// <summary>
        /// Добавление веса определенной клетке, если такое возможно
        /// </summary>
        private static void AddWeightAndCheckBorders(Cell[,] radar, int x, int y)
        {
            if (Map.CheckBorders(x, y))
                radar[x, y].Weight *= 30;
        }

        /// <summary>
        /// Закрашивание клетки.
        /// </summary>
        /// <param name="x">Координата х.</param>
        /// <param name="y">Координата у.</param>
        /// <param name="current">Игрок, который ходит сейчас.</param>
        /// <param name="next">Игрок, который ходил следущим.</param>
        private static void PaintingPoint(int x, int y, Player current, Player next)
        {
            if (Map.CheckBorders(x, y) && next.Map.Cells[x, y].Ship == null)
            {
                current.Map.Radar[x, y].Color = MColor.Point;
                next.Map.Cells[x, y].Color = MColor.Point;
            }
        }

        /// <summary>
        /// Проверка хода, можно ли стрелять в данную клетку.
        /// </summary>
        /// <param name="cell">Проверяемая клетка.</param>
        /// <returns>true - если стрелять можно.</returns>
        public bool CheckMove(Cell cell) => CurrentPlayer.Map.Radar[cell.X, cell.Y].Color == MColor.Cell;
    }
}