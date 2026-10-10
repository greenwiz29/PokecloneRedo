using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class PokemonSpeciesStatsCsvImporter
{
    private class ImportRow
    {
        public int DexNumber;
        public string Name;
        public string Description;

        public PokemonType Type1;
        public PokemonType Type2;

        public int HP;
        public int Attack;
        public int Defense;
        public int SpAttack;
        public int SpDefense;
        public int Speed;

        public AbilityID Ability;
        public AbilityID HiddenAbility;
        public int NormalAbilityPercent;

        public int ExpYield;
        public int CatchRate;
        public PokemonGenderRatio GenderRatio;
    }

    public static PokemonSpeciesImportReport Import(string csvText, string speciesFolder)
    {
        PokemonSpeciesImportReport report = new PokemonSpeciesImportReport();

        List<List<string>> rows = ParseCsv(csvText);

        if (rows.Count == 0)
        {
            report.Errors.Add("CSV contains no rows.");
            return report;
        }

        Dictionary<string, int> columns;

        try
        {
            columns = BuildColumnMap(rows[0]);
        }
        catch (Exception exception)
        {
            report.Errors.Add(exception.Message);
            return report;
        }

        List<ImportRow> importRows = new();

        for (int i = 1; i < rows.Count; i++)
        {
            if (IsEmptyRow(rows[i]))
                continue;

            try
            {
                ImportRow row = ParseRow(rows[i], columns, i + 1);

                importRows.Add(row);
            }
            catch (Exception exception)
            {
                report.Errors.Add(exception.Message);
            }
        }

        // Abort before modifying any assets if the CSV has parsing errors.
        if (report.Errors.Count > 0)
            return report;

        // Prevent two CSV rows from targeting the same species.
        var duplicateDexNumbers = importRows
            .GroupBy(row => row.DexNumber)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        foreach (int dexNumber in duplicateDexNumbers)
        {
            report.Errors.Add(
                $"CSV contains multiple rows for Dex #{dexNumber}.");
        }

        if (report.Errors.Count > 0)
            return report;

        Dictionary<int, PokemonSpecies> existingSpecies = FindExistingSpecies(speciesFolder, report);

        if (report.Errors.Count > 0)
            return report;

        foreach (ImportRow row in importRows)
        {
            PokemonSpecies species;

            if (existingSpecies.TryGetValue(row.DexNumber, out species))
            {
                UpdateSpecies(species, row);
                report.Updated++;
            }
            else
            {
                species = CreateSpecies(row, speciesFolder);
                existingSpecies.Add(row.DexNumber, species);
                report.Created++;
            }

            PokemonSpeciesSpriteAssigner.AssignSprites(species);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return report;
    }

    private static Dictionary<int, PokemonSpecies> FindExistingSpecies(string speciesFolder, PokemonSpeciesImportReport report)
    {
        Dictionary<int, PokemonSpecies> result = new();

        string[] guids = AssetDatabase.FindAssets("t:PokemonSpecies", new[] { speciesFolder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PokemonSpecies species = AssetDatabase.LoadAssetAtPath<PokemonSpecies>(path);

            if (species == null)
                continue;

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

    private static PokemonSpecies CreateSpecies(ImportRow row, string speciesFolder)
    {
        PokemonSpecies species = ScriptableObject.CreateInstance<PokemonSpecies>();

        string fileName = $"{SanitizeFileName(row.Name)}.asset";

        string assetPath = $"{speciesFolder.TrimEnd('/')}/{fileName}";

        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        AssetDatabase.CreateAsset(species, assetPath);

        Undo.RegisterCreatedObjectUndo(species, "Create Pokemon Species");

        UpdateSpecies(species, row);

        EditorUtility.SetDirty(species);

        return species;
    }

    private static void UpdateSpecies(PokemonSpecies species, ImportRow row)
    {
        Undo.RecordObject(species, $"Import Pokemon Species #{row.DexNumber}");

        SerializedObject so = new SerializedObject(species);

        // Identity
        so.FindProperty("identity.name").stringValue = row.Name;

        so.FindProperty("identity.dexNumber").intValue = row.DexNumber;

        so.FindProperty("identity.description").stringValue = row.Description;

        // Battle
        so.FindProperty("battle.type1").enumValueIndex = (int)row.Type1;

        so.FindProperty("battle.type2").enumValueIndex = (int)row.Type2;

        so.FindProperty("battle.baseStats.maxHP").intValue = row.HP;

        so.FindProperty("battle.baseStats.attack").intValue = row.Attack;

        so.FindProperty("battle.baseStats.defense").intValue = row.Defense;

        so.FindProperty("battle.baseStats.spAttack").intValue = row.SpAttack;

        so.FindProperty("battle.baseStats.spDefense").intValue = row.SpDefense;

        so.FindProperty("battle.baseStats.speed").intValue = row.Speed;

        so.FindProperty("battle.ability").enumValueIndex = (int)row.Ability;

        so.FindProperty("battle.hiddenAbility").enumValueIndex = (int)row.HiddenAbility;

        so.FindProperty("battle.normalAbilityPercent").intValue = row.NormalAbilityPercent;

        so.FindProperty("battle.expYield").intValue = row.ExpYield;

        so.FindProperty("battle.catchRate").intValue = row.CatchRate;

        so.FindProperty("battle.genderRatio").enumValueIndex = (int)row.GenderRatio;

        so.ApplyModifiedProperties();

        // PokemonDB currently uses the Unity Object name as its key.
        species.name = row.Name;

        EditorUtility.SetDirty(species);
    }

    private static ImportRow ParseRow(List<string> cells, Dictionary<string, int> columns, int rowNumber)
    {
        string GetRequired(string column)
        {
            if (!columns.TryGetValue(column, out int index))
                throw new Exception($"CSV row {rowNumber}: missing required column '{column}'.");

            if (index >= cells.Count)
                throw new Exception($"CSV row {rowNumber}: column '{column}' has no value.");

            string value = cells[index].Trim();

            if (string.IsNullOrEmpty(value))
                throw new Exception($"CSV row {rowNumber}: required column '{column}' is empty.");

            return value;
        }

        string GetOptional(string column)
        {
            if (!columns.TryGetValue(column, out int index))
                return string.Empty;

            if (index >= cells.Count)
                return string.Empty;

            return cells[index].Trim();
        }

        ImportRow row = new();

        row.DexNumber = ParseInt(GetRequired("dexNumber"), "dexNumber", rowNumber);

        row.Name = GetRequired("name");

        row.Description = GetOptional("description");

        row.Type1 = ParseEnum<PokemonType>(GetRequired("type1"), "type1", rowNumber);

        string type2 = GetOptional("type2");
        row.Type2 = string.IsNullOrEmpty(type2)
            ? PokemonType.None : ParseEnum<PokemonType>(type2, "type2", rowNumber);

        row.HP = ParseInt(GetRequired("hp"), "hp", rowNumber);

        row.Attack = ParseInt(GetRequired("attack"), "attack", rowNumber);

        row.Defense = ParseInt(GetRequired("defense"), "defense", rowNumber);

        row.SpAttack = ParseInt(GetRequired("spAttack"), "spAttack", rowNumber);

        row.SpDefense = ParseInt(GetRequired("spDefense"), "spDefense", rowNumber);

        row.Speed = ParseInt(GetRequired("speed"), "speed", rowNumber);

        string ability = GetOptional("ability");
        row.Ability = string.IsNullOrEmpty(ability)
            ? AbilityID.none : ParseEnum<AbilityID>(ability, "ability", rowNumber);

        string hiddenAbility = GetOptional("hiddenAbility");
        row.HiddenAbility = string.IsNullOrEmpty(hiddenAbility)
            ? AbilityID.none : ParseEnum<AbilityID>(hiddenAbility, "hiddenAbility", rowNumber);

        string abilityPercent = GetOptional("normalAbilityPercent");

        row.NormalAbilityPercent = string.IsNullOrEmpty(abilityPercent)
                ? 70 : ParseInt(abilityPercent, "normalAbilityPercent", rowNumber);

        string expYield = GetOptional("expYield");

        row.ExpYield = string.IsNullOrEmpty(expYield)
                ? 0 : ParseInt(expYield, "expYield", rowNumber);

        string catchRate = GetOptional("catchRate");

        row.CatchRate = string.IsNullOrEmpty(catchRate)
                ? 255 : ParseInt(catchRate, "catchRate", rowNumber);

        row.GenderRatio =
            ParseEnum<PokemonGenderRatio>(GetRequired("genderRatio"), "genderRatio", rowNumber);

        return row;
    }

    private static int ParseInt(string value, string fieldName, int rowNumber)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
        {
            throw new Exception(
                $"CSV row {rowNumber}: '{value}' is not a valid integer " +
                $"for '{fieldName}'.");
        }

        return result;
    }

    private static T ParseEnum<T>(string value, string fieldName, int rowNumber) where T : struct, Enum
    {
        if (!Enum.TryParse<T>(value, true, out T result))
        {
            throw new Exception(
                $"CSV row {rowNumber}: '{value}' is not a valid " +
                $"{typeof(T).Name} value for '{fieldName}'.");
        }

        return result;
    }

    private static Dictionary<string, int> BuildColumnMap(List<string> header)
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

        string[] requiredColumns =
        {
            "dexNumber",
            "name",
            "type1",
            "hp",
            "attack",
            "defense",
            "spAttack",
            "spDefense",
            "speed",
            "genderRatio"
        };

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

    private static bool IsEmptyRow(List<string> row)
    {
        return row.All(string.IsNullOrWhiteSpace);
    }

    private static string SanitizeFileName(string name)
    {
        StringBuilder builder = new StringBuilder(name);

        foreach (char invalid in System.IO.Path.GetInvalidFileNameChars())
            builder.Replace(invalid, '_');

        return builder.ToString();
    }

    private static List<List<string>> ParseCsv(string text)
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
                    // Ignore CR; LF handles the row boundary.
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

        // Capture final field/row.
        currentRow.Add(currentField.ToString());

        if (!IsEmptyRow(currentRow))
            rows.Add(currentRow);

        return rows;
    }
}

public class PokemonSpeciesImportReport
{
    public int Created { get; set; }
    public int Updated { get; set; }

    public List<string> Errors { get; } = new();
}