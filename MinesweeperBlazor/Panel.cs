using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinesweeperBlazor
{
    public class Panel
    {
        public int ID { get; set; }
        //Horizontal position
        public int X { get; set; }
        //Vertical position
        public int Y { get; set; }
        public bool IsMine { get; set; }
        public int AdjacentMines { get; set; }
        public bool IsRevealed { get; set; }
        public bool IsFlagged { get; set; }

        public Panel(int id, int x, int y)
        {
            ID = id;
            X = x;
            Y = y;
        }

        public void Reveal()
        {
            IsRevealed = true;
            IsFlagged = false; //Revealed panels cannot be flagged
        }
    }
}
