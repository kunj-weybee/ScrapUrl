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
        string sourceFolderPath = "C:\\Users\\DELL\\Downloads\\Source";

        // at this file data gets print into text document
        string notepadFilePath = "C:\\Users\\DELL\\Desktop\\URL_List.txt";

        // Get all subdirectories in the source folder
        string[] subFolders = Directory.GetDirectories(sourceFolderPath);

        foreach (string subFolder in subFolders)
        {
            // Get all JSON files in the current subFolder
            string[] jsonFiles = Directory.GetFiles(subFolder, "*.json");

            foreach (string jsonFilePath in jsonFiles)
            {
                ProcessJsonFile(jsonFilePath, notepadFilePath, subFolder);
            }
        }


        Console.WriteLine("Processing completed. Press Enter to exit.");
        Console.ReadLine();
    }

    static void ProcessJsonFile(string jsonFilePath, string notepadFilePath, string subFolder)
    {
        string fileName = Path.GetFileName(jsonFilePath);

        string folderPath = Path.GetDirectoryName(jsonFilePath);

        string folderName = Path.GetFileName(folderPath);

        string jsonFileContent = File.ReadAllText(jsonFilePath);

        JObject jsonObject = JObject.Parse(jsonFileContent);

        JArray subSectionsArray = jsonObject["SubSections"] as JArray;

        if (subSectionsArray != null)
        {
            var urls = ExtractUrlsFromSubSections(subSectionsArray).ToList();

            SaveUrlsToFile(notepadFilePath, urls);

            Console.WriteLine($"Count: {count}\t Processed folder: {folderName}\t Processed file: {fileName}\t {urls.Count} URLs extracted.");
            count++;
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
                urls.AddRange(ExtractUrlsFromLinesWithRowsUpdatedZero(dataLines));
            }

            // Recursively process nested SubSections
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
        using (StreamWriter writer = new StreamWriter(outputPath, true))
        {
            foreach (var url in urls)
            {
                writer.WriteLine(url);
            }
        }
    }
}