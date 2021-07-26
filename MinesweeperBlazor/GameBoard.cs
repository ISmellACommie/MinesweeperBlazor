using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinesweeperBlazor
{
    public class GameBoard
    {
        public int Width { get; set; } = 16;
        public int Height { get; set; } = 16;
        public int MineCount { get; set; } = 40;
        public List<Panel> Panels { get; set; }
        public GameStatus Status { get; set; }

        public void Initialise(int width, int height, int mines)
        {
            Width = width;
            Height = height;
            MineCount = mines;
            Panels = new List<Panel>();

            int id = 1;
            for (int y = 1; y <= height; y++)
            {
                for (int x = 1; x <= width; x++)
                {
                    Panels.Add(new Panel(id, x, y));
                    id++;
                }
            }

            Status = GameStatus.AwaitingFirstMove;
        }

        public void Reset()
        {
            Initialise(Width, Height, MineCount);
        }

        public List<Panel> GetNeighbours(int x, int y)
        {
            var nearbyPanels = Panels.Where(panel => panel.X >= (x - 1)
            && panel.X <= (x + 1)
            && panel.Y >= (y - 1)
            && panel.Y >= (y + 1));
            var currentPanel = Panels.Where(panel => panel.X == x && panel.Y == y);

            return nearbyPanels.Except(currentPanel).ToList();
        }

        public void FirstMove(int x, int y)
        {
            Random rand = new Random();
            /* For any board, take the user's first revealed panel
             * and any neighbours of that panel, and mark them
             * as unavailable for mine placement.
             */
            var neighbours = GetNeighbours(x, y); //Get all neighbours

            //Add the clicked panle to "unavailable for mines" group
            neighbours.Add(Panels.Find(z => z.X == x && z.Y == y));

            //Select all panels from set which are available for mine placement.
            //Order them randomly.
            var mineList = Panels.Except(neighbours).OrderBy(user => rand.Next());

            //Select the first Z random panels.
            var mineSlots = mineList.Take(MineCount).ToList().Select(z => new { z.X, z.Y });

            //Place the mines in the randomly selected panels.
            foreach (var mineCoord in mineSlots)
            {
                Panels.Single(panel => panel.X == mineCoord.X && panel.Y == mineCoord.Y).IsMine = true;
            }

            /*For every panel which is not a mine.
             * including the unavailable ones from earlier,
             * determine and save the adjacent mines.
             */
            foreach (var openPanel in Panels.Where(panel => !panel.IsMine))
            {
                var nearbyPanels = GetNeighbours(openPanel.X, openPanel.Y);
                openPanel.AdjacentMines = nearbyPanels.Count(z => z.IsMine);
            }

            //Mark the game as started.
            Status = GameStatus.InProgress;
        }
    }
}
