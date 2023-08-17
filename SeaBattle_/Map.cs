using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeaBattle_.Models;
using SeaBattle_.Supplementation;

namespace SeaBattle_
{
    /// <summary>
    /// Карта.
    /// </summary>
    public class Map
    {
        /// <summary>
        /// Размер поля.
        /// </summary>
        private static int SizeMap { get; set; }

        /// <summary>
        /// Размер ячейки.
        /// </summary>
        private int CellSize { get; }

        /// <summary>
        /// Поле игрока
        /// </summary>
        public Cell[,] Cells { get; }

        /// <summary>
        /// Вражеское поле
        /// </summary>
        public Cell[,] Radar { get; }

        /// <summary>
        /// Объект для отображения.
        /// </summary>
        private readonly Main _main;

        /// <summary>
        /// Конструктор с 3 параметрами.
        /// </summary>
        /// <param name="main">Отображатель.</param>
        /// <param name="sizeMap">Размер поля.</param>
        /// <param name="cellSize">Размер ячейки.</param>
        public Map(Main main, int sizeMap, int cellSize)
        {
            _main = main;
            SizeMap = sizeMap;
            CellSize = cellSize;
            Cells = new Cell[SizeMap, SizeMap];
            Radar = new Cell[SizeMap, SizeMap];
            Init(Cells);
            Init(Radar);
        }

        /// <summary>
        /// Инициализация карты.
        /// </summary>
        /// <param name="cells">Массив ячеек.</param>
        private static void Init(Cell[,] cells)
        {
            for (var x = 0; x < SizeMap; x++)
            for (var y = 0; y < SizeMap; y++)
                cells[x, y] = new Cell { X = x, Y = y };
        }

        /// <summary>
        /// Получение типа поля (свое/вражеское).
        /// </summary>
        /// <param name="fieldPart">Тип поля.</param>
        /// <returns>Массив клеток. Карта (либо своя, либо вражеская).</returns>
        public Cell[,] GetFieldPart(FieldPart fieldPart)
        {
            switch (fieldPart)
            {
                case FieldPart.Map:
                    return Cells;
                case FieldPart.Radar:
                    return Radar;
                default:
                    return new Cell[1, 1];
            }
        }

        /// <summary>
        /// Прорисовка поля/ рисование карты
        /// </summary>
        public void DrawField(FieldPart fieldPart)
        {
            var i = fieldPart == FieldPart.Radar ? 350 : 0;
            var field = GetFieldPart(fieldPart);
            var k = fieldPart == FieldPart.Radar ? 100 : 0;
            for (var x = 0; x < SizeMap; x++)
            {
                for (var y = 0; y < SizeMap; y++)
                {
                    var button = new Button();
                    button.Location = new Point(i + x * CellSize + CellSize, y * CellSize + CellSize);
                    button.Size = new Size(CellSize, CellSize);
                    button.BackColor = field[x, y].Color;

                    if (_main.Controls.Count < 202)
                        _main.Controls.Add(button);
                    else
                        _main.Controls[2 + k + x * 10 + y].BackColor = field[x, y].Color;
                }
            }
        }

        /// <summary>
        /// Рисование букв и цифр на вьюхе
        /// </summary>
        public void DrawFieldChar(FieldPart fieldPart)
        {
            var i = fieldPart == FieldPart.Radar ? 350 : 0;
            var ch = 'А';
            for (var x = 0; x < 10; x++)
            {
                ch = ch == 'Й' ? 'К' : ch;
                var l = new Label();
                l.Text = ch.ToString();
                l.Size = new Size(CellSize, CellSize);
                l.Location = new Point(i + x * CellSize + CellSize, 0);
                l.TextAlign = ContentAlignment.MiddleCenter;
                _main.Controls.Add(l);
                ch++;
            }

            for (var y = 0; y < 10; y++)
            {
                var l = new Label();
                l.Text = y == 9 ? 0.ToString() : (y + 1).ToString();
                l.Size = new Size(CellSize, CellSize);
                l.Location = new Point(i, y * CellSize + CellSize);
                l.TextAlign = ContentAlignment.MiddleCenter;
                _main.Controls.Add(l);
            }
        }

        /// <summary>
        /// Рандомная расстановка кораблей
        /// </summary>
        public async Task ShipsPlacementRandom(FieldPart fieldPart)
        {
            var cells = GetFieldPart(fieldPart);
            var shipsCount = new[]
            {
                4, 3, 3, 2, 2, 2, 1, 1, 1, 1
            };
            var rnd = new Random();

            foreach (var i in shipsCount)
            {
                var newX = rnd.Next(0, SizeMap);
                var newY = rnd.Next(0, SizeMap);
                var ship = new Ship();
                ship.UpdateRotationForInstallationToPlace(rnd.Next(1, 3), i);

                while (CheckShips(newX, newY, cells, ship))
                {
                    // изменение начального поля и разворота корабля
                    newX = rnd.Next(0, SizeMap);
                    newY = rnd.Next(0, SizeMap);
                    ship.UpdateRotationForInstallationToPlace(rnd.Next(1, 3), i);
                }

                // Установка корабля на поле
                await CreateShipOnMap(ship, cells, newX, newY);
            }

            // Костыль, без него, карты почему-то одинаковые у соперников
            await Task.Delay(30);
        }

        /// <summary>
        /// Установка корабля на карте
        /// </summary>
        /// <param name="ship">Корабль</param>
        /// <param name="cells">Карта</param>
        /// <param name="newX">Поле по Х</param>
        /// <param name="newY">Поле по У</param>
        private static Task CreateShipOnMap(Ship ship, Cell[,] cells, int newX, int newY)
        {
            var count = 0;
            var end = ship.Height > ship.Width ? ship.Height : ship.Width;
            ship.Cells = new Cell[end];
            ship.Hp = end;
            while (count < end)
            {
                if (ship.Rotation == 1)
                {
                    cells[newX, newY + count].Color = MColor.Ship;
                    cells[newX, newY + count].Ship = ship;
                    cells[newX, newY + count].Ship.Cells[count] = new Cell
                    {
                        X = newX,
                        Y = newY + count
                    };
                }
                else
                {
                    cells[newX + count, newY].Color = MColor.Ship;
                    cells[newX + count, newY].Ship = ship;
                    cells[newX + count, newY].Ship.Cells[count] = new Cell
                    {
                        X = newX + count,
                        Y = newY
                    };
                }

                count++;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Имеется приближение к бортам
        /// </summary>
        public async Task ShipsPlacementNotRandom(FieldPart fieldPart)
        {
            var cells = GetFieldPart(fieldPart);
            var rnd = new Random();
            var zone = rnd.Next(5);
            var shipsCount = new[]
            {
                4, 3, 3, 2, 2, 2
            };

            foreach (var i in shipsCount)
            {
                int newX, newY;
                Ship ship;
                switch (zone)
                {
                    // Сверху
                    case 1:
                        ship = new Ship
                        {
                            Height = 1,
                            Width = i,
                            Rotation = 2
                        };
                        newY = rnd.Next(0, 3);
                        // Берем одну из {0, 2} горизонтали
                        while ((double)newY % 2 != 0)
                            newY = rnd.Next(0, 3);
                        newX = rnd.Next(0, SizeMap);
                        var count1 = 0;
                        while (CheckShips(newX, newY, cells, ship))
                        {
                            newY = rnd.Next(0, 3);
                            while ((double)newY % 2 != 0)
                                newY = rnd.Next(0, count1 > 150 ? 5 : 3);
                            newX = rnd.Next(0, SizeMap);
                            count1++;
                        }

                        break;
                    // Справа
                    case 2:
                        ship = new Ship
                        {
                            Height = i,
                            Width = 1,
                            Rotation = 1
                        };
                        newX = rnd.Next(7, 10);
                        // Берем одну из {7, 9} вертикалей
                        while ((double)newX % 2 == 0)
                            newX = rnd.Next(7, 10);
                        newY = rnd.Next(0, SizeMap);
                        var count2 = 0;

                        while (CheckShips(newX, newY, cells, ship))
                        {
                            newX = rnd.Next(7, 10);
                            while ((double)newX % 2 == 0)
                                newX = rnd.Next(count2 > 150 ? 5 : 7, 10);
                            newY = rnd.Next(0, SizeMap);
                            count2++;
                        }

                        break;
                    // Снизу
                    case 3:
                        ship = new Ship
                        {
                            Height = 1,
                            Width = i,
                            Rotation = 2
                        };
                        newY = rnd.Next(7, 10);
                        // Берем одну из {7, 9} горизонтали
                        while ((double)newY % 2 == 0)
                            newY = rnd.Next(7, 10);
                        newX = rnd.Next(0, SizeMap);
                        var count3 = 0;
                        while (CheckShips(newX, newY, cells, ship))
                        {
                            newY = rnd.Next(7, 10);
                            while ((double)newY % 2 == 0)
                                newY = rnd.Next(count3 > 150 ? 5 : 7, 10);
                            newX = rnd.Next(0, SizeMap);
                            count3++;
                        }

                        break;
                    // Слева
                    case 4:
                        ship = new Ship
                        {
                            Height = i,
                            Width = 1,
                            Rotation = 1
                        };
                        newX = rnd.Next(0, 3);
                        // Берем одну из {0, 2} вертикаль
                        while ((double)newX % 2 != 0)
                            newX = rnd.Next(0, 3);
                        newY = rnd.Next(0, SizeMap);
                        var count4 = 0;
                        while (CheckShips(newX, newY, cells, ship))
                        {
                            newX = rnd.Next(0, 3);
                            while ((double)newX % 2 != 0)
                                newX = rnd.Next(0, count4 > 150 ? 5 : 3);
                            newY = rnd.Next(0, SizeMap);
                            count4++;
                        }

                        break;
                    default:
                        newX = newY = 0;
                        ship = new Ship();
                        break;
                }

                await CreateShipOnMap(ship, cells, newX, newY);
            }

            // Однушки
            shipsCount = new[]
            {
                1, 1, 1, 1
            };
            foreach (var i in shipsCount)
            {
                var newX = rnd.Next(0, SizeMap);
                var newY = rnd.Next(0, SizeMap);
                var ship = new Ship
                {
                    Rotation = rnd.Next(1, 3)
                };

                while (CheckShips(newX, newY, cells, ship))
                {
                    newX = rnd.Next(0, SizeMap);
                    newY = rnd.Next(0, SizeMap);
                }

                // Установка корабля на поле
                await CreateShipOnMap(ship, cells, newX, newY);
            }

            await Task.Delay(30);
        }

        /// <summary>
        /// Вынес проверку полей в отдельный метод
        /// </summary>
        /// <param name="newX">Координата Х</param>
        /// <param name="newY">Координата У</param>
        /// <param name="cells">Карта</param>
        /// <param name="ship">Корабль</param>
        /// <returns>true - если можно поставить</returns>
        private static bool CheckShips(int newX, int newY, Cell[,] cells, Ship ship) =>
            !(CheckBorders(newX, newY) &&
              CheckAngle(cells, newX, newY) &&
              // Последнее поле
              CheckBorders(newX + ship.Width - 1, newY + ship.Height - 1) &&
              CheckAngle(cells, newX + ship.Width - 1, newY + ship.Height - 1) &&
              // Проверка полей от первого до последнего и рядом лежащих
              ShipPlacement(cells, newX, newY, ship, ship.Rotation));

        /// <summary>
        /// Проверка координаты на существование в ней корабля и точки
        /// </summary>
        public static bool CheckShipAndPoint(Cell[,] cells, int x, int y)
            => cells[x, y].Ship == null && cells[x, y].Color != MColor.Point;

        /// <summary>
        /// Проверка координаты на ее существование в поле
        /// </summary>
        public static bool CheckBorders(int x, int y) => x >= 0 && x < SizeMap && y >= 0 && y < SizeMap;

        /// <summary>
        /// Проверка угловых координат
        /// </summary>
        private static bool CheckAngle(Cell[,] cells, int x, int y)
        {
            var minX = CheckMin(x);
            var maxX = CheckMax(x);
            var minY = CheckMin(y);
            var maxY = CheckMax(y);

            return
                CheckShipAndPoint(cells, x - minX, y - minY) && CheckShipAndPoint(cells, x - minX, y + maxY) &&
                CheckShipAndPoint(cells, x + maxX, y - minY) && CheckShipAndPoint(cells, x + maxX, y + maxY) &&
                CheckShipAndPoint(cells, x, y - minY) && CheckShipAndPoint(cells, x, y + maxY) &&
                CheckShipAndPoint(cells, x - minX, y) && CheckShipAndPoint(cells, x + maxX, y);
        }

        /// <summary>
        /// Проверка корабля на существование с передаваемой координатой
        /// </summary>
        /// <param name="cells">Поле</param>
        /// <param name="newX">Координата по х</param>
        /// <param name="newY">Координата по y</param>
        /// <param name="ship">Корабль, который размещается</param>
        /// <param name="r">Разворот, r = 1 - по вертикали, иначе по горизонтали</param>
        private static bool ShipPlacement(Cell[,] cells, int newX, int newY, Ship ship, int r)
        {
            var end = r == 1 ? ship.Height : ship.Width;
            var s = r == 1 ? newY : newX;
            end += s;

            while (s < end)
            {
                if (s == SizeMap) return false;

                var downX = CheckMin(newX);
                var upX = CheckMax(newX);
                var downY = CheckMin(newY);
                var upY = CheckMax(newY);

                var c1 = CheckShipAndPoint(cells, newX, newY);
                bool c2, c3;
                if (r == 1)
                {
                    // Вертикаль
                    c2 = CheckShipAndPoint(cells, newX + upX, s);
                    c3 = CheckShipAndPoint(cells, newX - downX, s);
                }
                else
                {
                    // Горизонталь
                    c2 = CheckShipAndPoint(cells, s, newY + upY);
                    c3 = CheckShipAndPoint(cells, s, newY - downY);
                }

                if (!c1 || !c2 || !c3)
                    return false;

                s++;
            }

            return true;
        }

        /// <summary>
        /// Проврка на минимальный клетки.
        /// </summary>
        /// <param name="number">Проверяемая клетка.</param>
        /// <returns>1 - в случае успеха, иначе - 0.</returns>
        private static int CheckMin(int number) => number <= 0 ? 0 : 1;

        /// <summary>
        /// Проврка на максимальную клетки.
        /// </summary>
        /// <param name="number">Проверяемая клетка.</param>
        /// <returns>1 - в случае успеха, иначе - 0.</returns>
        private static int CheckMax(int number) => number >= SizeMap - 1 ? 0 : 1;
    }
}