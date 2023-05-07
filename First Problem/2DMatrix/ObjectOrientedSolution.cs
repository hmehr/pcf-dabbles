namespace ObjectOrientedSolution
{
    public class Traverse
    {
        private readonly Cell _currentCell;
        private Direction _direction;
        private readonly List<string> _parsedCells;
        private readonly List<Cell> _visitedCells;
        private readonly Matrix _matrix;
        private string CurrentElement => _matrix.GetElemetAt(_currentCell);

        private string GetCurrentElementWithoutControlCharacters()
        {
            string output = CurrentElement;
            if (CurrentElement.StartsWith("<") || CurrentElement.StartsWith(">") || CurrentElement.StartsWith("^") || CurrentElement.StartsWith("v"))
            {
                output = CurrentElement.Substring(1);
            }          
            return output;
        }

        public Traverse(string[,] matrix)
        {
            _matrix = new Matrix(matrix);
            _direction = Direction.Right;
            _currentCell = new Cell(0, 0);
            _parsedCells = new List<string>();
            _visitedCells = new List<Cell>();
        }

        /// <summary>
        /// Parse method visits each cell of the matrix based on control characters and returns the parsed cell values
        /// </summary>
        /// <returns>The final result of parsed values separated by comma</returns>
        public string Parse()
        {
            while (!HasParsingEnded())
            {
                if (!TrySaveVisited())
                {
                    break;
                }
                SaveElement();
                SetDirection();
                MoveToTheNextCell();
            }

            return string.Join(", ", _parsedCells);
        }

        /// <summary>
        /// Returns true if parsing goes beyond the indexes of our matrix, otherwise false
        /// </summary>        
        private bool HasParsingEnded()
        {
            if (_currentCell.Column >= _matrix.Columns() || _currentCell.Column < 0)
            {
                return true;
            }
            if (_currentCell.Row >= _matrix.Rows() || _currentCell.Row < 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// We save a list of all cells that we visited. If we visit a previously visited cell, we detect a loop and exit.
        /// </summary>        
        private bool TrySaveVisited()
        {
            if (_visitedCells.Any(v => v == _currentCell))
            {
                _parsedCells.Add("LOOP");
                return false;
            }
            _visitedCells.Add(new Cell(_currentCell));
            return true;
        }

        /// <summary>
        /// Change the state of our direction based on the current element's control character
        /// </summary>
        private void SetDirection()
        {
            
            switch (CurrentElement.Trim()[0])//first element is the control character
            {
                case ('>'):
                    _direction = Direction.Right;
                    break;
                case ('<'):
                    _direction = Direction.Left;
                    break;
                case ('^'):
                    _direction = Direction.Up;
                    break;
                case ('v'):
                    _direction = Direction.Down;
                    break;
            }
        }

        private void SaveElement()
        {
            _parsedCells.Add(GetCurrentElementWithoutControlCharacters());
        }

        private void MoveToTheNextCell()
        {
            switch (_direction)
            {
                case Direction.Left:
                    {
                        _currentCell.Row--;
                        break;
                    }
                case Direction.Right:
                    {
                        _currentCell.Row++;
                        break;
                    }
                case Direction.Up:
                    {
                        _currentCell.Column--;
                        break;
                    }
                case Direction.Down:
                    {
                        _currentCell.Column++;
                        break;
                    }
                default:
                    break;
            }            
        }        
    }

    public enum Direction
    {
        Up, Down, Left, Right
    }

    /// <summary>
    /// Cell class wraps column and row properties of our traverse and matrix classes
    /// </summary>
    public class Cell : IEquatable<Cell>
    {
        public int Column { get; set; }
        public int Row { get; set; }

        public Cell(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public Cell(Cell cell)
        {
            Column = cell.Column;
            Row = cell.Row;
        }

        public static bool operator ==(Cell e1, Cell e2)
        {
            return e1.Equals(e2);
        }

        public static bool operator !=(Cell e1, Cell e2) => !(e1 == e2);

        public bool Equals(Cell? other)
        {
            return other?.Row == Row && other?.Column == Column;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Cell);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Column, Row);
        }
    }

    public class Matrix
    {
        private readonly string[,] _cells;

        public Matrix(string[,] matrix)
        {
            _cells = matrix;
        }
        public int Rows() => _cells.GetLength(1);
        public int Columns() => _cells.GetLength(0);
        public string GetElemetAt(Cell cell)
        {
            return _cells[cell.Column, cell.Row];
        }
    }
}