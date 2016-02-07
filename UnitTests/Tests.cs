using CanonicalFormEquation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class EquationTests
    {
        private Equation equation;
        private string test1 = "abc + bca = cab - bac";
        private string test2 = "2a + 2b - 2ab =  -4ab";
        private string test3 = "(a - b) - (b - a) = 0";
        private string test4 = "a^2 + b^2 + a^2 + b^2 = - 2abc";
        private string test5 = "(a + (b + c - (c + b + a))) = a";
        private string test6 = "(((((((((((((((a + b - c))))))))))))))) = a + b + c";
        private string test7 = "7c + 10.5b - 3.5c^2 = 0";


        [TestInitialize]
        public void Init()
        {
            equation = new Equation();
        }

        /// <summary>
        /// Tests order of variables in summands.
        /// Summands with the same set of variables have the same type.
        /// </summary>
        [TestMethod]
        public void TestVariablesOrder()
        {
            equation.Parse(test1);
            Assert.AreEqual(equation.CanonicalCount, 1);
        }


        /// <summary>
        /// Tests division of equation by number
        /// </summary>
        [TestMethod]
        public void TestDivision()
        {
            equation.Parse(test2);
            Assert.AreEqual(equation.CanonicalCount, 3);
        }


        /// <summary>
        /// Tests brackets parsing
        /// </summary>
        [TestMethod]
        public void TestBrackets()
        {
            equation.Parse(test3);
            Assert.AreEqual(equation.CanonicalCount, 2);

        }
        /// <summary>
        /// Tests powers parsing
        /// </summary>
        [TestMethod]
        public void TestPowers()
        {
            equation.Parse(test4);
            Assert.AreEqual(equation.CanonicalCount, 3);
        }

        /// <summary>
        /// Tests nested brackets parsing
        /// </summary>
        [TestMethod]
        public void TestNestedBrackets()
        {
            equation.Parse(test5);
            Assert.AreEqual(equation.CanonicalCount, 1);
        }


        /// <summary>
        /// Tests parsing of a big amount of brackets
        /// </summary>
        [TestMethod]
        public void TestManyBrackets()
        {
            equation.Parse(test6);
            Assert.AreEqual(equation.CanonicalCount, 1);
        }

        /// <summary>
        /// Tests fractional numbers parsing
        /// </summary>
        [TestMethod]
        public void TestFractions()
        {
            equation.Parse(test7);
            Assert.AreEqual(equation.CanonicalCount, 3);
        }

    }
}
