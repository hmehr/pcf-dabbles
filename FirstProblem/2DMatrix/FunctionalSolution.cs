
namespace FunctionalSolution
{
    public static class FunctionalClass
    {
        public static string TraverseMatrix(string[,] matrix)
        {
            int column = 0, row = 0;
            string direction = "right";
            var elements = new List<string>();
            List<(int col, int row)> visited = new List<(int col, int row)>();
            while (true)
            {
                if (column >= matrix.GetLength(0) || column < 0)
                {
                    break;
                }

                if (row >= matrix.GetLength(1) || row < 0)
                {
                    break;
                }

                if (visited.Any(v => v.row == row && v.col == column))
                {
                    elements.Add("LOOP");
                    break;
                }

                visited.Add((column, row));

                string element = matrix[column, row];
                string rawElement = StripControlCharacter(element);
                elements.Add(rawElement);
                direction = ExtractDirection(element, direction);
                (column, row) = SwitchDirection(direction, (column, row));
            }
            return string.Join(", ", elements);
        }

        static string StripControlCharacter(string element)
        {
            string output = element;
            if (element.StartsWith(">") || element.StartsWith("<") || element.StartsWith("^") || element.StartsWith("v"))
            {
                output = element.Substring(1);
            }
            return output;
        }

        static (int column, int row) SwitchDirection(string direction, (int column, int row) value)
        {
            switch (direction)
            {
                case "left":
                    { 
                        return (value.column, --value.row);
                    }
                case "right":
                    {
                        return (value.column, ++value.row);
                    }
                case "up":
                    {
                        return (--value.column, value.row);
                    }
                case "down":
                    {
                        return (++value.column, value.row);
                    }
            }

            return (value.row, value.column);
        }

        static string ExtractDirection(string element, string direction)
        {
            switch (element[0])
            {
                case ('>'):
                    return "right";
                case ('<'):
                    return "left";
                case ('^'):
                    return "up";
                case ('v'):
                    return "down";
            }

            return direction;
        }

    }
}
