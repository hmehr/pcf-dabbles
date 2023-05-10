## 2D Array Problem


This solution is written using .net6.0 and C# 10. I chose C# because of the readbility and prior expertise.


### Two solutions

1. **Functional solution** which is the easy one with a few static functions without changing any state. This solution works but it's rather messy. While I was doing that, I thought that I can do a cleaner solution with some classes and a nice design following OOO principles.
2. **Object Oriented solution** which uses a few classes to save the state of the traveresed matrix.
   This solution's main class is [`Traverse`](2DMatrix/ObjectOrientedSolution.cs#L3). This class has a main public method called [`parse`](2DMatrix/ObjectOrientedSolution.cs#L35) that visits each cell of the matrix and constructs the response string based on the steps. There's a [`Matrix`](2DMatrix/ObjectOrientedSolution.cs#L188) class that encapsulates a 2D array and a few utility methods to get the number of rows, columns and the string value of a cell given its indexes. To encapsulate the indexes of a 2D array, we use the class [`Cell`](2DMatrix/ObjectOrientedSolution.cs#L148) that has `col` and `row` fields, initialized in the constructor. The default equality operator is overriden, consequently GetHashCode has to be overriden as well.

There are unit tests that test some of the corner cases that I could think of. The tests are located in [`2DMatrix.Test`](2DMatrix.Test/UnitTests.cs) folder.

The main project compiles as an executable that runs the code with a few sample cases. There's a switch to choose which solution to use. By default it runs the OOO solution.
