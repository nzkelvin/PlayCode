using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    class Program
    {
        static void Main(string[] args)
        {
            // [Length(row number), Width(column number)]
            int[,] initialWorld = new int[15, 17] {
                                                    {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                                                    {0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0},
                                                    {0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0},
                                                    {0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0},
                                                    {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                                                    {0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0},
                                                    {0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0},
                                                    {0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0},
                                                    {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                                                    {0,0,0,0,1,1,1,0,0,0,1,1,1,0,0,0,0},
                                                    {0,0,0,0,0,0,1,0,1,0,1,0,0,0,0,0,0},
                                                    {0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0},
                                                    {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                                                    {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                                                    {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            };

            var world = new World(initialWorld);

            for (var i = 0; i < 500; i++)
            {
                world.Evolve();

                //var tmp = world.ToString();
                Console.SetCursorPosition(0, 0);
                Console.Write(world.ToString());
                System.Threading.Thread.Sleep(200);
            }

            Console.Read();
        }
    }

    public class World
    {
        public int Length { get; set; } // Y direction
        public int Width { get; set; } // X direction
        public Cell[,] CurrentGen { get; private set; }
        private Cell[,] _nextGen;

        public World(int[,] initWorld)
        {
            Length = initWorld.GetLength(0);
            Width = initWorld.GetLength(1);

            CurrentGen = new Cell[Length, Width]; 
            _nextGen = new Cell[Length, Width];

            for(var r = 0; r < Length; r++)
            {
                for(var c = 0; c < Width; c++)
                {
                    CurrentGen[r, c] = new Cell() { Row = r, Column = c, Status = (CellStatus)initWorld[r, c] };
                }
            }

            for (var r = 0; r < Length; r++)
            {
                for (var c = 0; c < Width; c++)
                {
                    _nextGen[r, c] = new Cell() { Row = r, Column = c, Status = CellStatus.Dead };
                }
            }
        }

        public void Evolve()
        {
            for (var r = 0; r < Length; r++)
            {
                for (var c = 0; c < Width; c++)
                {
                    _nextGen[r, c].UpdateStatus(CurrentGen);
                }
            }

            // Once evaluation completes, next Generation becomes this evaluation
            for (var r = 0; r < Length; r++)
            {
                for (var c = 0; c < Width; c++)
                {
                    CurrentGen[r, c].Status = _nextGen[r, c].Status;
                }
            }
        }

        public override string ToString()
        {
            var textBuilder = new StringBuilder();

            for (var r = 0; r < Length; r++)
            {
                for (var c = 0; c < Width; c++)
                {
                    textBuilder.Append((int)CurrentGen[r, c].Status);
                }

                textBuilder.AppendLine();
            }

            return textBuilder.ToString();
        }
    }

    public class Cell
    {
        public int Column { get; set; }
        public int Row { get; set; }
        public CellStatus Status { get; set; }

        public void UpdateStatus(Cell[,] currentWorld)
        {
            int liveNeighbourCount = 0;

            for (var r = Row - 1; r <= Row + 1; r++)
            {
                for (var c = Column - 1; c <= Column + 1; c++)
                {
                    if (r >= 0 && r < currentWorld.GetLength(0) 
                        && c >= 0 && c < currentWorld.GetLength(1) 
                        && currentWorld[r, c].Status == CellStatus.Alive)
                    {
                        if (r == Row && c == Column)
                            continue;

                        liveNeighbourCount++;
                    }
                }
            }

            //if (liveNeighbourCount < 2 || liveNeighbourCount > 3)
            //{
            //    Status = CellStatus.Dead;
            //}

            if (liveNeighbourCount == 3)
            {
                Status = CellStatus.Alive;
            }
            else if (liveNeighbourCount == 2)
            {
                Status = currentWorld[Row, Column].Status;
            }
            else
            {
                Status = CellStatus.Dead;
            }



            // if 2, nothing changes. Lived keeps alive
        }
    }

    public enum CellStatus
    {
        Dead=0,
        Alive=1
    }
}
