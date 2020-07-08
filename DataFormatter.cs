using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DataFormatter
{
    class Program
    {
        static void Main(string[] args)
        {
            //string directory = @"C:\Meine Items\Coding Ambitions\10th Semester\Exported Data\Solo Test Data\PC\";
            string directory = @"C:\Meine Items\Coding Ambitions\10th Semester\Exported Data\Solo Test Data\VR\";
            var filePaths = Directory.GetFiles(directory);
            var dataSets = new List<DataSet>();

            ImportAllDataSets();
            ReformatAndWrite();


            void ReformatAndWrite()
            {
                using StreamWriter writer = new StreamWriter(directory + "reformat.csv");
                string line;
                int vectorCount = dataSets[0].vectors.Count;

                for (int vectorIndex = 0; vectorIndex < vectorCount; ++vectorIndex)
                    for (int vectorValue = 0; vectorValue < 6; ++vectorValue)
                    {
                        line = (vectorIndex + 1) + ((ValueNames)vectorValue).ToString();
                        foreach (DataSet dataSet in dataSets)
                            line += ";" + dataSet.vectors[vectorIndex].values[vectorValue].ToString(CultureInfo.InvariantCulture);
                        writer.WriteLine(line);
                    }
            }



            void ImportAllDataSets()
            {
                foreach (string path in filePaths)
                {
                    DataSet dataSet = new DataSet();
                    dataSets.Add(dataSet);

                    int lineCount = File.ReadLines(path).Count();
                    using (StreamReader reader = new StreamReader(path))
                    {
                        reader.ReadLine(); // Two first lines are always redundant.
                        reader.ReadLine();
                        for (int i = 2; i < lineCount; ++i)
                        {
                            string[] values = reader.ReadLine().Split(',');
                            if (values.Length != 9) break; // Cuts out unnecessary lines.

                            var v = new Vector();
                            for (int j = 0; j < 6; ++j)
                                v.values[j] = ParseScientificNotation(values[j + 3]);
                            dataSet.vectors.Add(v);

                        }
                    };

                }

                static float ParseScientificNotation(string input) { return float.Parse(input, CultureInfo.GetCultureInfo("en-GB")); }
            }
        }


        private class DataSet { public List<Vector> vectors = new List<Vector>(); }
        private class Vector { public float[] values = new float[6]; }
        private enum ValueNames { X, Y, Z, NX, NY, NZ }
    }
}
