using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DecisionTree
{
    class Methods
    {
        public static int counter;
        //wyznaczanie separatora w csv
        public static char FindDelimiter(string[] tab)
        {
            List<char> delimiters = new List<char> {',', ';', '|', '.'};
            Dictionary<char, int> counts = delimiters.ToDictionary(key => key, value => 0);
            foreach (string line in tab)
            foreach (char c in delimiters)
                counts[c] += line.Count(t => t == c);
            return counts.FirstOrDefault(x => x.Value == counts.Values.Max()).Key;
        }

        public static int[][] TrimArray(int columnToRemove, int[][] originalArray)
        {
            int[][] result = new int[originalArray.Length][];
            for (int i = 0; i < result.GetLength(0); i++)
            {
                result[i] = new int[originalArray[0].Length - 1];
            }

            for (int i = 0, j = 0; i < originalArray.Length; i++)
            {
                for (int k = 0, u = 0; k < originalArray[0].Length ; k++)
                {
                    if (k == columnToRemove)
                        continue;

                    result[j][u] = originalArray[i][k];
                    u++;
                }
                j++;
            }

            return result;
        }

        public static void ShowData(int[][] data)
        {
            foreach (var line in data)
            {
                foreach (var value in line)
                {
                    Console.Write(value+"  ");
                }
                Console.WriteLine();
            }
        }

        //ładowanie danych z wyświetlaniem ich na bieżąco
        public static int[][] LoadData()
        {
            string fileName = "\\zoo.txt";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + fileName;
            string[] tabAllLines = File.ReadAllLines(path);
            char delimiter = FindDelimiter(tabAllLines);
            int[][] data = new int[tabAllLines.Length][];
            int counter = 0;
            foreach (var line in tabAllLines)
            {
                var values = line.Split(delimiter);
                List<int> lineList = new List<int>();

                foreach (var value in values)
                {
                    if (int.TryParse(value, out int temp))
                    {
                        lineList.Add(int.Parse(value));
                        Console.Write(value + " ");
                    }
                }
                data[counter] = lineList.ToArray();
                counter++;
                Console.WriteLine();
            }
            Console.ReadKey();
            Console.Clear();
            return data;
        }

        public static int[] FindClasses(int[][] data)
        {
            return ValuesOfColumn(data, data[0].Length-1);
        }

        public static int[] ValuesOfColumn(int[][] data, int column)
        {
            int[] result = new int[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = data[i][column];
            return result;
        }

        public static double[] ObtainProbabilities(int[] values)
        {
            List<int> classList = values.OfType<int>().ToList();
            int setSize = classList.Count;
            List<double> resultList = new List<double>();
            while (classList.Count > 0)
            {
                var first = classList.First();
                int count = classList.Count(x => x == first);
                resultList.Add(Convert.ToDouble(count) / Convert.ToDouble(setSize));
                classList.RemoveAll(x => x == first);
            }
            return resultList.ToArray();
        }

        public static double CalculateEntropy(double[] P)
        {
            double result = 0;
            foreach (var p in P)
                result += p * Math.Log(p, 2);
            return -result;
        }
        //Console.WriteLine("Drzewo: \n\n");
        //    int counter = 0;

        
        public static void Iterate(int[][] data)
        {
            counter++;
            

            if (data[0].Length != 0)
            {
                Dictionary<int, double> gainForColumn = new Dictionary<int, double>();

                for (int i = 0; i < data[0].Length - 1; i++)
                    gainForColumn.Add(i, GainRatio(data, i));

                var max = gainForColumn.FirstOrDefault(x => x.Value == gainForColumn.Values.Max());

                var values = ValuesOfColumn(data, max.Key);
                HashSet<int> valuesSet = new HashSet<int>(values);
                foreach (var value in valuesSet)
                {
                    for (int i = 0; i <= counter; i++)
                    {
                        Console.Write("  ");
                    }
                    Console.WriteLine("column: " + max.Key + "   value: " + value);
                    
                    int[][] tmpData = SplitData(data, value, max.Key);
                    Iterate(tmpData);
                }
            }
            counter--;
        }

        public static int[][] SplitData(int[][] data, int value, int column)
        {
            List<int[]> result = new List<int[]>();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i][column] == value)
                {
                    int[] tmp = new int[data[0].Length];
                    for (int j = 0; j < data[0].Length; j++)
                        tmp[j] = data[i][j];
                    result.Add(tmp);
                }
            }

            return TrimArray(column, result.ToArray());
        }

        public static void BuildTree(int[][] data)
        {
            counter = 0;
            Iterate(data);
        }

        public static double Info(int[] values, int[] classes)
        {
            int setSize = values.Length;
            double result = 0;

            Dictionary<int, List<int>> dict = new Dictionary<int,List<int>>();

            for (int i = 0; i < values.Length; i++)
            {
                if (!dict.ContainsKey(values[i]))
                    dict.Add(values[i], new List<int>());
                dict[values[i]].Add(classes[i]);
            }

            foreach (var entity in dict)
            {
                double[] prob = ObtainProbabilities(entity.Value.ToArray());
                int count = 0;
                foreach (var value in values)
                    if (entity.Key == value)
                        count++;
                result += (count / (double)setSize) * CalculateEntropy(ObtainProbabilities(entity.Value.ToArray()));
            }

            return result;
        }

        public static double GainRatio(int[][] data, int column)
        {
            return Gain(data, column) / SplitInfo(data, column);
        }

        public static double SplitInfo(int[][] data, int column)
        {
            int[] values = ValuesOfColumn(data, column);
            return CalculateEntropy(ObtainProbabilities(values));
        }

        public static double Gain(int[][] data, int column)
        {
            double result = 0;

            int[] classes = FindClasses(data);
            double entOfClasses = CalculateEntropy(ObtainProbabilities(classes));
            double info = Info(ValuesOfColumn(data, column), classes);

            return entOfClasses - info;
        }
    }
}
