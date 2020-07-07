using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using System.Linq;


public static class DataHandler
{
    public static List<Vector3> Positions, Normals;
    private static List<string[]> CsvData;

    private static string GetCsvDataPoint(int x, int y) { return CsvData[y][x]; }
    //private static void SetCsvDataPoint(int x, int y, string value) { CsvData[y][x] = value; }

    /// <summary>
    /// Reads a .csv file and imports positions and normals.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="landmarks"></param>
    public static void Import(string path, int[] landmarks = null)
    {
        ReadCsv();
        ImportVectors();


        void ReadCsv()
        {
            int LineCount = File.ReadLines(path).Count();
            CsvData = new List<string[]>();
            using (StreamReader reader = new StreamReader(path))
            {
                for (int y = 0; y < LineCount; ++y)
                {
                    string[] values = reader.ReadLine().Split(',');
                    if (values.Length == 1) break; // Hard code: Cuts out unnecessary lines.
                    CsvData.Add(values);
                }
            };
        }

        void ImportVectors()
        {
            const int offset = 2; // The first two lines contain no vectors.
            Positions = new List<Vector3>();
            Normals = new List<Vector3>();

            for (int y = offset; y < CsvData.Count; ++y)
                if (!SkipPoint(y))
                {
                    Positions.Add(ImportVector(3, y));
                    Normals.Add(ImportVector(6, y).normalized);
                }

            Vector3 ImportVector(int column, int row)
            {
                // X axis is mirrored in data, hence the minus.
                return new Vector3(
                    -float.Parse(GetCsvDataPoint(column + 0, row), CultureInfo.GetCultureInfo("en-GB")),
                    float.Parse(GetCsvDataPoint(column + 1, row), CultureInfo.GetCultureInfo("en-GB")),
                    float.Parse(GetCsvDataPoint(column + 2, row), CultureInfo.GetCultureInfo("en-GB"))
                );



            }

            bool SkipPoint(int col)
            {
                if (landmarks == null || landmarks.Length == 0) return false;

                // Numbering of landmarks are 1-indexed, hence y + 1.
                return !landmarks.Contains(col + 1);
            }
        }
    }

    /// <summary>
    /// Exports the annotation points.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <param name="dots">Annotation point objects.</param>
    public static void Export(string path, List<GameObject> dots)
    {
        var positions = new List<Vector3>();
        var normals = new List<Vector3>();
        GenerateVectors();
        // From gameobjects to lists of vectors.



        void GenerateVectors()
        {
            foreach (GameObject dot in dots)
            {
                Vector3 pos = dot.transform.position;
                pos.x *= -1; // Mirror x axis
                positions.Add(pos);
                normals.Add(dot.transform.rotation * Vector3.forward);
            }
        }

        // From list of vectors to csv.

        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("Landmarks," + positions.Count + ",v1.0,(cm)");
            writer.WriteLine("#,Name,Type,X,Y,Z,NX,NY,NZ");

            for (int i = 0; i < positions.Count; ++i)
                writer.WriteLine((i + 1) + "," + (i + 1) + ",normal" + FormatVector(positions[i]) + FormatVector(normals[i]));
        }

        string FormatVector(Vector3 v)
        {
            return FormatToScientificNotation(v.x)
                + FormatToScientificNotation(v.y)
                + FormatToScientificNotation(v.z);

            string FormatToScientificNotation(float value) { return "," + value.ToString("e6", CultureInfo.InvariantCulture).Remove(11, 1); }
        }
    }

}
