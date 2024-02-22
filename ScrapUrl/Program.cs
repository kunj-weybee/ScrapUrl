using System;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Linq;

class Program
{
    static int count = 1;

    static void Main()
    {
        // path where the JSON files are located
        string directoryPath = "C:\\Users\\DELL\\Downloads\\Source\\2024_02_07";

        // at this file data gets print into text document
        string outputPath = "C:\\Users\\DELL\\Desktop\\URL_List.txt";

        // json files no array store karva
        string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");

        foreach (string filePath in jsonFiles)
        {
            ProcessJsonFile(filePath, outputPath);
        }

        Console.WriteLine("Processing completed. Press Enter to exit.");
        Console.ReadLine();
    }

    static void ProcessJsonFile(string filePath, string outputPath)
    {
        string fileName = Path.GetFileName(filePath);

        string jsonContent = File.ReadAllText(filePath);

        JObject jsonObject = JObject.Parse(jsonContent);

        JArray subSectionsArray = jsonObject["SubSections"] as JArray;

        if (subSectionsArray != null)
        {
            var urls = ExtractUrlsFromSubSections(subSectionsArray).ToList();

            SaveUrlsToFile(outputPath, urls);

            Console.WriteLine($"Processed file: {fileName}. {urls.Count} URLs extracted.");
        }
        else
        {
            Console.WriteLine($"Invalid JSON structure in file: {fileName}. Check your JSON file format.");
        }
    }

    static IEnumerable<string> ExtractUrlsFromSubSections(JArray subSectionsArray)
    {
        var urls = new List<string>();

        foreach (var subSection in subSectionsArray)
        {
            var dataLines = subSection["Data"] as JArray;

            if (dataLines != null)
            {
                // Extract URLs from lines with RowsUpdated: 0
                urls.AddRange(ExtractUrlsFromLinesWithRowsUpdatedZero(dataLines));
            }

            // Recursively process nested SubSections . because there are nested objects in subsection .
            var nestedSubSections = subSection["SubSections"] as JArray;
            if (nestedSubSections != null)
            {
                urls.AddRange(ExtractUrlsFromSubSections(nestedSubSections));
            }
        }

        return urls;
    }

    static IEnumerable<string> ExtractUrlsFromLinesWithRowsUpdatedZero(JArray dataLines)
    {
        var urls = new List<string>();

        foreach (var line in dataLines)
        {
            if (line.ToString().Contains("RowsUpdated: 0"))
            {
                // to match URLs
                string pattern = @"https:\/\/[^\s]+";
                Match match = Regex.Match(line.ToString(), pattern);

                if (match.Success)
                {
                    urls.Add(match.Value);
                }
            }
        }

        return urls;
    }

    static void SaveUrlsToFile(string outputPath, IEnumerable<string> urls)
    {
        // Append to the existing file or create a new file if it doesn't exist
        using (StreamWriter writer = new StreamWriter(outputPath, true))
        {
            foreach (var url in urls)
            {
                writer.WriteLine(url);
            }
        }
    }
}