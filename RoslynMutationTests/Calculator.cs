namespace RoslynMutationTests
{
    public class Calculator
    {
        public static int MultiplyByTwo(int number)
        {
            const int two = 2;
            return number * two;
        }
    }
}