using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

public class CsvImporter
{
    
    public static Dictionary<string, int> BuildColumnMap(List<string> header, List<string> requiredColumns)
    {
        Dictionary<string, int> result = new(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < header.Count; i++)
        {
            string name = header[i].Trim();

            if (string.IsNullOrEmpty(name))
                continue;

            if (result.ContainsKey(name))
            {
                throw new Exception($"CSV header contains duplicate column '{name}'.");
            }

            result.Add(name, i);
        }

        foreach (string required in requiredColumns)
        {
            if (!result.ContainsKey(required))
            {
                throw new Exception(
                    $"CSV is missing required column '{required}'.");
            }
        }

        return result;
    }

    public static int ParseInt(string value, string fieldName, int rowNumber)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
        {
            throw new Exception(
                $"CSV row {rowNumber}: '{value}' is not a valid " +
                $"integer for '{fieldName}'.");
        }

        return result;
    }

    public static bool IsEmptyRow(List<string> row)
    {
        return row.All(string.IsNullOrWhiteSpace);
    }

    public static List<List<string>> ParseCsv(string text)
    {
        List<List<string>> rows = new();
        List<string> currentRow = new();
        StringBuilder currentField = new();

        bool insideQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (insideQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        insideQuotes = false;
                    }
                }
                else
                {
                    currentField.Append(c);
                }

                continue;
            }

            switch (c)
            {
                case '"':
                    insideQuotes = true;
                    break;

                case ',':
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                    break;

                case '\r':
                    break;

                case '\n':
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();

                    rows.Add(currentRow);
                    currentRow = new List<string>();
                    break;

                default:
                    currentField.Append(c);
                    break;
            }
        }

        currentRow.Add(currentField.ToString());

        if (!IsEmptyRow(currentRow))
            rows.Add(currentRow);

        return rows;
    }
}
