using System;
using UnityEditor;
using UnityEngine;

public class PokemonSpeciesImporterWindow : EditorWindow
{
    private TextAsset speciesStatsSourceCsv;

    private TextAsset levelUpMovesSourceCsv;

    private TextAsset itemMovesSourceCsv;

    private const string SpeciesFolder = "Assets/Game/Resources/Pokemon/";
    private const string MovesFolder = "Assets/Game/Resources/Moves/";

    [MenuItem("Tools/Pokemon/Import Species Data")]
    public static void ShowWindow()
    {
        GetWindow<PokemonSpeciesImporterWindow>("Pokemon Species Importer");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.LabelField("Pokemon Species Stats CSV Importer", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Imports Identity and Battle data from a CSV. " +
            "Existing species are matched by Dex Number; missing species are created.",
            MessageType.Info);

        GUILayout.Space(10);

        speciesStatsSourceCsv = (TextAsset)EditorGUILayout.ObjectField("Source CSV", speciesStatsSourceCsv, typeof(TextAsset), false);

        GUILayout.Space(10);

        using (new EditorGUI.DisabledScope(speciesStatsSourceCsv == null))
        {
            if (GUILayout.Button("Import Species Data", GUILayout.Height(30)))
            {
                ImportSpecies();
            }
        }

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Pokemon Learnset Importer", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Imports level-up learnsets from a batch CSV. " +
            "Pokemon are matched by Dex Number and moves are matched by name. " +
            "The CSV completely replaces the level-up learnset for each " +
            "species it contains.",
            MessageType.Info);

        GUILayout.Space(10);

        levelUpMovesSourceCsv = (TextAsset)EditorGUILayout.ObjectField("Level Up Source CSV", levelUpMovesSourceCsv, typeof(TextAsset), false);

        GUILayout.Space(10);

        using (new EditorGUI.DisabledScope(levelUpMovesSourceCsv == null))
        {
            if (GUILayout.Button("Import Level-Up Learnsets", GUILayout.Height(30)))
            {
                ImportLevelUpMoves();
            }
        }

        EditorGUILayout.HelpBox(
            "Imports item-based learnsets from a batch CSV. " +
            "Pokemon are matched by Dex Number and moves are matched by name. " +
            "The CSV completely replaces the items learnset for each " +
            "species it contains.",
            MessageType.Info);

        GUILayout.Space(10);

        itemMovesSourceCsv = (TextAsset)EditorGUILayout.ObjectField("Item Learnset Source CSV", itemMovesSourceCsv, typeof(TextAsset), false);

        GUILayout.Space(10);

        using (new EditorGUI.DisabledScope(itemMovesSourceCsv == null))
        {
            if (GUILayout.Button("Import Item Learnsets", GUILayout.Height(30)))
            {
                ImportItemMoves();
            }
        }
    }

    private void ImportSpecies()
    {
        try
        {
            PokemonSpeciesImportReport report = PokemonSpeciesStatsCsvImporter.Import(speciesStatsSourceCsv.text, SpeciesFolder);

            if (report.Errors.Count > 0)
            {
                Debug.LogError(
                    $"Pokemon species import failed with {report.Errors.Count} error(s).");

                foreach (string error in report.Errors)
                    Debug.LogError(error);

                EditorUtility.DisplayDialog("Pokemon Species Import",
                    $"Import failed with {report.Errors.Count} error(s).\n\nNo assets were modified.",
                    "OK");

                return;
            }

            Debug.Log(
                $"Pokemon species import complete. " +
                $"Created: {report.Created}, Updated: {report.Updated}");

            EditorUtility.DisplayDialog("Pokemon Species Import", $"Import complete.\n\n" +
                $"Created: {report.Created}\nUpdated: {report.Updated}", "OK");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            EditorUtility.DisplayDialog("Pokemon Species Import",
                "An unexpected error occurred. See the Console for details.", "OK");
        }
    }

    private void ImportLevelUpMoves()
    {
        try
        {
            PokemonLevelUpImportReport report = PokemonLearnsetCsvImporter.ImportLevelUpMoves(levelUpMovesSourceCsv.text, SpeciesFolder);

            if (report.Errors.Count > 0)
            {
                Debug.LogError(
                    $"Pokemon level-up import failed with " +
                    $"{report.Errors.Count} error(s).");

                foreach (string error in report.Errors)
                    Debug.LogError(error);

                EditorUtility.DisplayDialog(
                    "Pokemon Level-Up Import",
                    $"Import failed with " +
                    $"{report.Errors.Count} error(s).\n\n" +
                    "No assets were modified.",
                    "OK");

                return;
            }

            Debug.Log(
                $"Pokemon level-up import complete. " +
                $"Updated: {report.Updated}");

            EditorUtility.DisplayDialog(
                "Pokemon Level-Up Import",
                $"Import complete.\n\n" +
                $"Species updated: {report.Updated}",
                "OK");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            EditorUtility.DisplayDialog(
                "Pokemon Level-Up Import",
                "An unexpected error occurred. " +
                "See the Console for details.",
                "OK");
        }
    }

    private void ImportItemMoves()
    {
        try
        {
            PokemonLevelUpImportReport report = PokemonLearnsetCsvImporter.ImportItemMoves(itemMovesSourceCsv.text, SpeciesFolder);

            if (report.Errors.Count > 0)
            {
                Debug.LogError(
                    $"Pokemon items learnset import failed with " +
                    $"{report.Errors.Count} error(s).");

                foreach (string error in report.Errors)
                    Debug.LogError(error);

                EditorUtility.DisplayDialog(
                    "Pokemon Item Learnset Import",
                    $"Import failed with " +
                    $"{report.Errors.Count} error(s).\n\n" +
                    "No assets were modified.",
                    "OK");

                return;
            }

            Debug.Log(
                $"Pokemon Item Learnset import complete. " +
                $"Updated: {report.Updated}");

            EditorUtility.DisplayDialog(
                "Pokemon Item Learnset Import",
                $"Import complete.\n\n" +
                $"Species updated: {report.Updated}",
                "OK");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            EditorUtility.DisplayDialog(
                "Pokemon Item Learnset Import",
                "An unexpected error occurred. " +
                "See the Console for details.",
                "OK");
        }
    }

}