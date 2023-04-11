using System;
using System.Collections.Generic;
using System.IO;
using SeaBattle_.Models;

namespace SeaBattle_.Supplementation
{
    public static class Attack
    {
        /// <summary>
        /// Выстрел
        /// </summary>
        /// <param name="current">текущий игрок</param>
        /// <param name="next">следующий игрок</param>
        /// <param name="move">ход (поле)</param>
        /// <param name="game">игра</param>
        /// <returns>в случае попадания - true</returns>
        public static bool DoAttack(Player current, Player next, Cell move, Game game)
        {
            var cell = next.Map.Cells[move.X, move.Y];
            if (cell.Ship != null && !cell.Ship.IsDead)
            {
                cell.Ship.Hp -= 1;
                cell.Color = MColor.DamageShip;
                current.Map.Radar[move.X, move.Y].Color = MColor.Ship;
                current.Map.Radar[move.X, move.Y].Ship = cell.Ship;
                if (cell.Ship.Hp <= 0)
                {
                    // Считаем кол-во уничтоженных кораблей + кладем в индекс по длине корабля
                    current.ShipsDead[cell.Ship.Cells.Length - 1]++;
                    cell.Ship.IsDead = true;
                    Game.PaintDeadShip(cell.Ship, current, next);
                }

                return true;
            }

            if (cell.Ship == null)
            {
                cell.Color = MColor.Point;
                current.Map.Radar[move.X, move.Y].Color = MColor.Point;
                return false;
            }

            return false;
        }

        /// <summary>
        /// Выбор атаки по файлу
        /// </summary>
        public static MoveMobFile AttackFromFile(List<MoveMobFile> list)
        {
            var fileNumber = new Random().Next(1, 9);
            var flag = list.Count != 0;
            while (flag)
            {
                // почему очень много стреляет
                foreach (var mob in list)
                {
                    if (mob.NameFile == fileNumber)
                    {
                        fileNumber = new Random().Next(1, 9);
                        flag = true;
                        break;
                    }

                    flag = false;
                }
            }

            var str = "FilesWithMove\\Move" + fileNumber + ".txt";
            var m = new MoveMobFile
            {
                Str = new StreamReader(str).ReadLine()
            };
            // ReSharper disable once PossibleNullReferenceException
            m.CountMove = m.Str.Length / 4;
            m.NameFile = fileNumber;
            return m;
        }

        // Много похожего! todo: прописать перегрузку или объединить все в один метод
        /// <summary>
        /// Проверка атаки
        /// </summary>
        /// <param name="map">карта</param>
        /// <param name="move">ход</param>
        /// <returns>В случае успеха - true</returns>
        public static bool CheckAttack(Cell[,] map, Cell move) =>
            map[move.X, move.Y].Color != MColor.Point &&
            map[move.X, move.Y].Color != MColor.DamageShip;
    }
}