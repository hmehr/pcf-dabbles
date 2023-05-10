using ObjectOrientedSolution;
using FunctionalSolution;

namespace _2DMatrix.Test
{
    public class UnitTests
    {
        [Fact]
        public void Should_Visit_All_Elements_And_Loop_v1()
        {
            //arrange
            string[,] matrix = new string[,]
            {
                { "HI", "1", "2", "3", "v4" },
                { ">9", "10", "11", "v12", "13" },
                { "^14", "15", "16", "17", "<18" }
            };

            //act
            var traverse = new Traverse(matrix);
            string result = traverse.Parse();

            string result2 = FunctionalClass.TraverseMatrix(matrix);

            //assert
            string expectedResult = "HI, 1, 2, 3, 4, 13, 18, 17, 16, 15, 14, 9, 10, 11, 12, LOOP";
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedResult, result2);


        }

        [Fact]
        public void Should_Visit_All_Elements_And_Loop_V2()
        {
            //arrange
            string[,] matrix = new string[,]
            {
                { "HI", "1", "v2", ">3", "v4" },
                { "v9", "<10", ">11", "^12", "13" },
                { ">14", "^15", "16", "17", "<18" }
            };

            //act
            var traverse = new Traverse(matrix);
            string result = traverse.Parse();
            string result2 = FunctionalClass.TraverseMatrix(matrix);
            //assert
            string expectedResult = "HI, 1, 2, 11, 12, 3, 4, 13, 18, 17, 16, 15, 10, 9, 14, LOOP";
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedResult, result2);
        }

        [Fact]
        public void Should_Visit_Select_Elements_And_Exit_At_4()
        {
            //arrenge
            string[,] matrix = new string[,]
            {
                { "HI", "1", "v2", ">3", "4" },
                { "v9", "<10", ">11", "^12", "13" },
                { "14", "^15", "16", "17", "<18" }
            };

            //act
            var traverse = new Traverse(matrix);
            string result = traverse.Parse();
            string result2 = FunctionalClass.TraverseMatrix(matrix);

            //assert
            string expectedResult = "HI, 1, 2, 11, 12, 3, 4";
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedResult, result2);

        }

        [Fact]
        public void Should_Visit_Single_Element_And_Exit()
        {
            //arrenge
            string[,] matrix = new string[,]
            {
                { "HI"}              
            };

            //act
            var traverse = new Traverse(matrix);
            string result = traverse.Parse();
            string result2 = FunctionalClass.TraverseMatrix(matrix);

            //assert
            string expectedResult = "HI";
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedResult, result2);

        }

        [Fact]
        public void Should_Visit_Arbitrary_Strings()
        {
            //arrenge
            string[,] matrix = new string[,]
            {
                { "HI", "$", "vv%", ">3", "4" },
                { "v9", "<10", ">#$T", "&*JJ", "t" },
                { "14", "^15", "16", "17", "<18" }
            };

            //act
            var traverse = new Traverse(matrix);
            string result = traverse.Parse();
            string result2 = FunctionalClass.TraverseMatrix(matrix);

            //assert
            string expectedResult = "HI, $, v%, #$T, &*JJ, t";
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedResult, result2);

        }
    }
}