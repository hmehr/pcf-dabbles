using ObjectOrientedSolution;
using FunctionalSolution;

namespace _2DMatrix
{    

    public class Program
    {
        static void Main(string[] args)
        {
            string[,] matrix1 = new string[,]
            {
                { "HI", "1", "2", "3", "v4" },
                { ">9", "10", "11", "v12", "13" },
                { "^14", "15", "16", "17", "<18" }
            };

            string[,] matrix2 = new string[,]
            {
                { "HI", "1", "v2", ">3", "v4" },
                { "v9", "<10", ">11", "^12", "13" },
                { ">14", "^15", "16", "17", "<18" }
            };

            string[,] matrix3 = new string[,]
            {
                { "HI", "1", "v2", ">3", "4" },
                { "v9", "<10", ">11", "^12", "13" },
                { "14", "^15", "16", "17", "<18" }
            };

            bool IsOoo = true;

            if (IsOoo)
            {

                Console.WriteLine(new Traverse(matrix1).Parse());
                Console.WriteLine(new Traverse(matrix2).Parse());
                Console.WriteLine(new Traverse(matrix3).Parse());
            }
            else
            {
                Console.WriteLine(FunctionalClass.TraverseMatrix(matrix1));
                Console.WriteLine(FunctionalClass.TraverseMatrix(matrix2));
                Console.WriteLine(FunctionalClass.TraverseMatrix(matrix3));
            }            
        }
    }
}