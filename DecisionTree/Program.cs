namespace DecisionTree
{
    class Program
    {
        static void Main(string[] args)
        {
            int[][] dane = Methods.LoadData();
            Methods.BuildTree(dane);
            System.Console.ReadKey();
        }
    }
}