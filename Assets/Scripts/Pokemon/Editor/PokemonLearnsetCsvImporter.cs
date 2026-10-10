using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class PokemonLearnsetCsvImporter
{
    private const string moveFolder = "Assets/Game/Resources/Moves/";
    private class ImportRowLevelUp
    {
        public int DexNumber;
        public int Level;
        public string MoveName;
    }

    private class ImportRowItem
    {
        public int DexNumber;
        public string MoveName;
    }

    private class ResolvedRowLevelUp
    {
        public int DexNumber;
        public int Level;
        public MoveBase Move;
    }

    private class ResolvedRowItem
    {
        public int DexNumber;
        public MoveBase Move;
    }

    public static PokemonLevelUpImportReport ImportLevelUpMoves(string csvText, string speciesFolder)
    {
        PokemonLevelUpImportReport report = new();

        List<List<string>> rows = CsvImporter.ParseCsv(csvText);

        if (rows.Count == 0)
        {
            report.Errors.Add("CSV contains no rows.");
            return report;
        }

        Dictionary<string, int> columns;

        try
        {
            columns = CsvImporter.BuildColumnMap(rows[0], new List<string>() { "dexNumber", "level", "move" });
        }
        catch (Exception exception)
        {
            report.Errors.Add(exception.Message);
            return report;
        }

        List<ImportRowLevelUp> importRows = new();

        for (int i = 1; i < rows.Count; i++)
        {
            if (CsvImporter.IsEmptyRow(rows[i]))
                continue;

            try
            {
                ImportRowLevelUp row = ParseRowLevelUp(rows[i], columns, i + 1);
                importRows.Add(row);
            }
            catch (Exception exception)
            {
                report.Errors.Add(exception.Message);
            }
        }

        // Abort before modifying any assets if CSV parsing failed.
        if (report.Errors.Count > 0)
            return report;

        // Prevent duplicate learnset entries in the CSV.
        var duplicateRows = importRows
            .GroupBy(row => new
            {
                row.DexNumber,
                row.Level,
                MoveName = row.MoveName.Trim()
            })
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        foreach (var duplicate in duplicateRows)
        {
            report.Errors.Add(
                $"CSV contains duplicate level-up entry: " +
                $"Dex #{duplicate.DexNumber}, Level {duplicate.Level}, " +
                $"Move '{duplicate.MoveName}'.");
        }

        if (report.Errors.Count > 0)
            return report;

        Dictionary<int, PokemonSpecies> speciesByDex = FindExistingSpecies(speciesFolder, report);

        if (report.Errors.Count > 0)
            return report;

        Dictionary<string, MoveBase> movesByName = FindExistingMoves(report);

        if (report.Errors.Count > 0)
            return report;

        // Resolve every CSV row before modifying anything.
        List<ResolvedRowLevelUp> resolvedRows = new();

        foreach (ImportRowLevelUp row in importRows)
        {
            if (!speciesByDex.TryGetValue(row.DexNumber, out PokemonSpecies species))
            {
                report.Errors.Add(
                    $"CSV row for Dex #{row.DexNumber}: " +
                    $"no PokemonSpecies asset exists for that Dex number.");

                continue;
            }

            string moveName = row.MoveName.Trim();

            if (!movesByName.TryGetValue(moveName, out MoveBase move))
            {
                report.Errors.Add(
                    $"CSV row for Dex #{row.DexNumber}, Level {row.Level}: " +
                    $"no MoveBase asset named '{moveName}' was found.");

                continue;
            }

            resolvedRows.Add(new ResolvedRowLevelUp
            {
                DexNumber = row.DexNumber,
                Level = row.Level,
                Move = move
            });
        }

        // Abort before modifying any assets if reference resolution failed.
        if (report.Errors.Count > 0)
            return report;

        // Group the fully-resolved rows by species.
        Dictionary<int, List<LearnableMove>> learnsets = new Dictionary<int, List<LearnableMove>>();

        foreach (ResolvedRowLevelUp row in resolvedRows)
        {
            if (!learnsets.TryGetValue(row.DexNumber, out List<LearnableMove> moves))
            {
                moves = new List<LearnableMove>();
                learnsets.Add(row.DexNumber, moves);
            }

            moves.Add(CreateLearnableMove(row.Move, row.Level));
        }

        // Only now modify assets.
        foreach (var pair in learnsets)
        {
            PokemonSpecies species = speciesByDex[pair.Key];

            Undo.RecordObject(species, $"Import Level-Up Learnset #{pair.Key}");

            species.SetLevelUpMoves(pair.Value);

            EditorUtility.SetDirty(species);

            report.Updated++;
        }

        AssetDatabase.SaveAssets();

        return report;
    }

    public static PokemonLevelUpImportReport ImportItemMoves(string csvText, string speciesFolder)
    {
        PokemonLevelUpImportReport report = new();

        List<List<string>> rows = CsvImporter.ParseCsv(csvText);

        if (rows.Count == 0)
        {
            report.Errors.Add("CSV contains no rows.");
            return report;
        }

        Dictionary<string, int> columns;

        try
        {
            columns = CsvImporter.BuildColumnMap(rows[0], new List<string>() { "dexNumber", "move" });
        }
        catch (Exception exception)
        {
            report.Errors.Add(exception.Message);
            return report;
        }

        List<ImportRowItem> importRows = new();

        for (int i = 1; i < rows.Count; i++)
        {
            if (CsvImporter.IsEmptyRow(rows[i]))
                continue;

            try
            {
                ImportRowItem row = ParseRowItems(rows[i], columns, i + 1);
                importRows.Add(row);
            }
            catch (Exception exception)
            {
                report.Errors.Add(exception.Message);
            }
        }

        // Abort before modifying any assets if CSV parsing failed.
        if (report.Errors.Count > 0)
            return report;

        // Prevent duplicate learnset entries in the CSV.
        var duplicateRows = importRows
            .GroupBy(row => new
            {
                row.DexNumber,
                MoveName = row.MoveName.Trim()
            })
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        foreach (var duplicate in duplicateRows)
        {
            report.Errors.Add(
                $"Items CSV contains duplicate move entry: " +
                $"Dex #{duplicate.DexNumber}, " +
                $"Move '{duplicate.MoveName}'.");
        }

        if (report.Errors.Count > 0)
            return report;

        Dictionary<int, PokemonSpecies> speciesByDex = FindExistingSpecies(speciesFolder, report);

        if (report.Errors.Count > 0)
            return report;

        Dictionary<string, MoveBase> movesByName = FindExistingMoves(report);

        if (report.Errors.Count > 0)
            return report;

        // Resolve every CSV row before modifying anything.
        List<ResolvedRowItem> resolvedRows = new();

        foreach (ImportRowItem row in importRows)
        {
            if (!speciesByDex.TryGetValue(row.DexNumber, out PokemonSpecies species))
            {
                report.Errors.Add(
                    $"CSV row for Dex #{row.DexNumber}: " +
                    $"no PokemonSpecies asset exists for that Dex number.");

                continue;
            }

            string moveName = row.MoveName.Trim();

            if (!movesByName.TryGetValue(moveName, out MoveBase move))
            {
                report.Errors.Add(
                    $"CSV row for Dex #{row.DexNumber}: " +
                    $"no MoveBase asset named '{moveName}' was found.");

                continue;
            }

            resolvedRows.Add(new ResolvedRowItem
            {
                DexNumber = row.DexNumber,
                Move = move
            });
        }

        // Abort before modifying any assets if reference resolution failed.
        if (report.Errors.Count > 0)
            return report;

        // Group the fully-resolved rows by species.
        Dictionary<int, List<MoveBase>> learnsets = new Dictionary<int, List<MoveBase>>();

        foreach (ResolvedRowItem row in resolvedRows)
        {
            if (!learnsets.TryGetValue(row.DexNumber, out List<MoveBase> moves))
            {
                moves = new List<MoveBase>();
                learnsets.Add(row.DexNumber, moves);
            }

            moves.Add(row.Move);
        }

        // Only now modify assets.
        foreach (var pair in learnsets)
        {
            PokemonSpecies species = speciesByDex[pair.Key];

            Undo.RecordObject(species, $"Import Item Learnset #{pair.Key}");

            species.SetItemMoves(pair.Value);

            EditorUtility.SetDirty(species);

            report.Updated++;
        }

        AssetDatabase.SaveAssets();

        return report;
    }

    private static LearnableMove CreateLearnableMove(MoveBase move, int level)
    {
        return new LearnableMove(move, level);
    }

    private static Dictionary<int, PokemonSpecies> FindExistingSpecies(string speciesFolder, PokemonLevelUpImportReport report)
    {
        Dictionary<int, PokemonSpecies> result = new();

        string[] guids = AssetDatabase.FindAssets("t:PokemonSpecies", new[] { speciesFolder });

        Debug.Log(
            $"Level-Up importer searching for PokemonSpecies in " +
            $"'{speciesFolder}'. Found {guids.Length} asset(s).");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            PokemonSpecies species = AssetDatabase.LoadAssetAtPath<PokemonSpecies>(path);

            if (species == null)
            {
                Debug.LogWarning(
                    $"Found PokemonSpecies GUID '{guid}' at '{path}', " +
                    $"but LoadAssetAtPath returned null.");
                continue;
            }

            Debug.Log(
                $"Found PokemonSpecies '{species.name}' at '{path}' " +
                $"with Dex #{species.DexNumber}.");

            int dexNumber = species.DexNumber;

            if (result.ContainsKey(dexNumber))
            {
                report.Errors.Add(
                    $"Multiple PokemonSpecies assets already exist " +
                    $"for Dex #{dexNumber}: '{result[dexNumber].name}' " +
                    $"and '{species.name}'.");
                continue;
            }

            result.Add(dexNumber, species);
        }

        return result;
    }

    private static Dictionary<string, MoveBase> FindExistingMoves(PokemonLevelUpImportReport report)
    {
        Dictionary<string, MoveBase> result = new(StringComparer.OrdinalIgnoreCase);

        string[] guids = AssetDatabase.FindAssets("t:MoveBase", new[] { moveFolder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            MoveBase move = AssetDatabase.LoadAssetAtPath<MoveBase>(path);

            if (move == null)
                continue;

            string moveName = move.Name.Trim();

            if (string.IsNullOrEmpty(moveName))
            {
                report.Errors.Add(
                    $"MoveBase asset '{path}' has an empty name.");
                continue;
            }

            if (result.ContainsKey(moveName))
            {
                report.Errors.Add(
                    $"Multiple MoveBase assets have the name " +
                    $"'{moveName}'.");
                continue;
            }

            result.Add(moveName, move);
        }

        return result;
    }

    private static ImportRowLevelUp ParseRowLevelUp(List<string> cells, Dictionary<string, int> columns, int rowNumber)
    {
        string GetRequired(string column)
        {
            if (!columns.TryGetValue(column, out int index))
            {
                throw new Exception(
                    $"CSV row {rowNumber}: missing required " +
                    $"column '{column}'.");
            }

            if (index >= cells.Count)
            {
                throw new Exception(
                    $"CSV row {rowNumber}: column '{column}' " +
                    $"has no value.");
            }

            string value = cells[index].Trim();

            if (string.IsNullOrEmpty(value))
            {
                throw new Exception(
                    $"CSV row {rowNumber}: required column " +
                    $"'{column}' is empty.");
            }

            return value;
        }

        ImportRowLevelUp row = new();

        row.DexNumber = CsvImporter.ParseInt(GetRequired("dexNumber"), "dexNumber", rowNumber);

        row.Level = CsvImporter.ParseInt(GetRequired("level"), "level", rowNumber);

        row.MoveName = GetRequired("move");

        if (row.DexNumber <= 0)
        {
            throw new Exception($"CSV row {rowNumber}: Dex number must be greater than 0.");
        }

        if (row.Level < 1)
        {
            throw new Exception($"CSV row {rowNumber}: level must be at least 1.");
        }

        return row;
    }

    private static ImportRowItem ParseRowItems(List<string> cells, Dictionary<string, int> columns, int rowNumber)
    {
        string GetRequired(string column)
        {
            if (!columns.TryGetValue(column, out int index))
            {
                throw new Exception(
                    $"CSV row {rowNumber}: missing required " +
                    $"column '{column}'.");
            }

            if (index >= cells.Count)
            {
                throw new Exception(
                    $"CSV row {rowNumber}: column '{column}' " +
                    $"has no value.");
            }

            string value = cells[index].Trim();

            if (string.IsNullOrEmpty(value))
            {
                throw new Exception(
                    $"CSV row {rowNumber}: required column " +
                    $"'{column}' is empty.");
            }

            return value;
        }

        ImportRowItem row = new();

        row.DexNumber = CsvImporter.ParseInt(GetRequired("dexNumber"), "dexNumber", rowNumber);

        row.MoveName = GetRequired("move");

        if (row.DexNumber <= 0)
        {
            throw new Exception($"CSV row {rowNumber}: Dex number must be greater than 0.");
        }

        return row;
    }

}

public class PokemonLevelUpImportReport
{
    public int Updated { get; set; }

    public List<string> Errors { get; } = new();
}