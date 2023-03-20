﻿using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using SeaBattle_.Supplementation;

namespace SeaBattle_.Models
{
    public class Cell
    {
        public Ship Ship { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        
        public int Weight { get; set; }
        
        public Color Color { get; set; }

        public Cell() => Color = MColor.Cell;

        public bool CheckShipAndPoint() => Ship != null || Color == MColor.Point;
        
        /// <summary>
        /// Перевод поля из строкового вида в тип данных Cell 
        /// </summary>
        /// <param name="cellStr">массив char</param>
        /// <returns>поля типа Cell</returns>
        public static Cell TranslateToCell(IEnumerable<char> cellStr)
        {
            var move = new Cell();
            foreach (var ch in cellStr)
                if (char.IsLetter(ch))
                {
                    var rch = ch == 'К' ? 'Й' : ch;
                    var c = 0;
                    for (var i = 'А'; i <= 'Й'; i++)
                    {
                        if (rch == i)
                        {
                            move.X = c;
                            break;
                        }

                        c++;
                    }
                }
                else if (char.IsDigit(ch))
                {
                    move.Y = ch - '0' - 1;
                    move.Y = move.Y == -1 ? 9 : move.Y;
                }

            return move;
        }

        public static Task<string> TranslateFromCellToString(Cell cell)
        {
            var ch = 'A';
            var str = "";
            for (var i = 0; i < 10; ch++, i++)
                if (cell.X == i)
                    str += ch;
            for (var i = 0; i < 10; i++)
                if (cell.Y == i)
                {
                    if (i == 9)
                    {
                        str += "0";
                        break;
                    }
                    str += i + 1;
                }
            
            return Task.FromResult(str);
        }
    }
}