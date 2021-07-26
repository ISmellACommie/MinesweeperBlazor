using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace MinesweeperBlazor
{
    public class GameBoard
    {
        public int Width { get; set; } = 16;
        public int Height { get; set; } = 16;
        public int MineCount { get; set; } = 40;
        public List<Panel> Panels { get; set; }
        public GameStatus Status { get; set; }
        public int MinesRemaining
        {
            get
            {
                return MineCount - Panels.Where(x => x.IsFlagged).Count();
            }
        }
        public Stopwatch Stopwatch { get; set; }

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
            Stopwatch = new Stopwatch();
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
            Stopwatch.Start();
        }

        public void MakeMove(int x, int y)
        {
            if (Status == GameStatus.AwaitingFirstMove)
            {
                FirstMove(x, y);
            }
            RevealPanel(x, y);
        }

        public void RevealPanel(int x, int y)
        {
            //Step 1: Find and reveal the clicked panel.
            var selectedPanel = Panels.First(panel => panel.X == x && panel.Y == y);
            selectedPanel.Reveal();

            //Step 2: If the panel is a mine, show all mines. Game over.
            if (selectedPanel.IsMine)
            {
                Status = GameStatus.Failed;
                RevealAllMines();
                return;
            }

            //Step 3: If the panel is a zero, cascade reveal neighbours.
            if (selectedPanel.AdjacentMines == 0)
            {
                RevealZeros(x, y);
            }

            //Step 4: If this move cause the game to be complete, mark it as such.
            CompletionCheck();
        }

        private void RevealAllMines()
        {
            Panels.Where(x => x.IsMine).ToList().ForEach(x => x.IsRevealed = true);
        }

        public void RevealZeros(int x, int y)
        {
            //Get all neighbour panels.
            var neighbourPanels = GetNeighbours(x, y).Where(panel => !panel.IsRevealed);

            foreach (var neighbour in neighbourPanels)
            {
                //For each neighbour panel, reveal that panel.
                neighbour.IsRevealed = true;

                //If the neighbour is also 0, reveal all of its neighbours too.
                if (neighbour.AdjacentMines == 0)
                {
                    RevealZeros(neighbour.X, neighbour.Y);
                }
            }
        }

        private void CompletionCheck()
        {
            var hiddenPanels = Panels.Where(x => !x.IsRevealed).Select(x => x.ID);
            var minePanels = Panels.Where(x => x.IsMine).Select(x => x.ID);
            if (!hiddenPanels.Except(minePanels).Any())
            {
                Status = GameStatus.Completed;
                Stopwatch.Stop();
            }
        }

        public void FlagPanel(int x, int y)
        {
            if (MinesRemaining > 0)
            {
                var panel = Panels.Where(z => z.X == x && z.Y == y).First();
                panel.Flag();
            }
        }
    }
}
