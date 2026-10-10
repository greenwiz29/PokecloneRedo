using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PokemonSpeciesSpriteAssigner
{
    private const string BasePath = "Assets/Art/Pokemons/";

    public static bool AssignSprites(PokemonSpecies pokemon)
    {
        if (pokemon == null)
        {
            Debug.LogError("Cannot assign sprites: PokemonSpecies is null.");
            return false;
        }

        if (pokemon.DexNumber <= 0)
        {
            Debug.LogError(
                $"Cannot assign sprites for '{pokemon.name}': " +
                "Dex Number must be greater than 0.");
            return false;
        }

        Undo.RecordObject(pokemon, $"Auto Assign Pokemon Sprites #{pokemon.DexNumber}");

        SerializedObject so = new SerializedObject(pokemon);
        string dex = pokemon.DexNumber.ToString();

        // ===== FRONT / BACK =====
        AssignSprite(so, "front", $"{BasePath}Front/{dex}.png");
        AssignSprite(so, "back", $"{BasePath}Back/{dex}.png");

        AssignSprite(so, "frontFemale", $"{BasePath}Front/female/{dex}.png");
        AssignSprite(so, "backFemale", $"{BasePath}Back/female/{dex}.png");

        AssignSprite(so, "frontShiny", $"{BasePath}Front/shiny/{dex}.png");
        AssignSprite(so, "backShiny", $"{BasePath}Back/shiny/{dex}.png");

        AssignSprite(so, "frontFemaleShiny", $"{BasePath}Front/shiny/female/{dex}.png");
        AssignSprite(so, "backFemaleShiny", $"{BasePath}Back/shiny/female/{dex}.png");

        // ===== OVERWORLD NORMAL =====
        AssignOverworld(so, "walkDown", $"{BasePath}overworld/normal/down/", dex);
        AssignOverworld(so, "walkUp", $"{BasePath}overworld/normal/up/", dex);
        AssignOverworld(so, "walkLeft", $"{BasePath}overworld/normal/left/", dex);
        AssignOverworld(so, "walkRight", $"{BasePath}overworld/normal/right/", dex);

        // ===== OVERWORLD SHINY =====
        AssignOverworld(so, "walkDownShiny", $"{BasePath}overworld/shiny/down/", dex);
        AssignOverworld(so, "walkUpShiny", $"{BasePath}overworld/shiny/up/", dex);
        AssignOverworld(so, "walkLeftShiny", $"{BasePath}overworld/shiny/left/", dex);
        AssignOverworld(so, "walkRightShiny", $"{BasePath}overworld/shiny/right/", dex);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(pokemon);

        return true;
    }

    private static void AssignSprite(SerializedObject so, string propertyName, string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning(
                $"Could not find sprite '{path}' for property '{propertyName}'.");
            return;
        }

        SerializedProperty visuals = so.FindProperty("visuals");
        SerializedProperty battle = visuals.FindPropertyRelative("battle");
        SerializedProperty spriteProperty = battle.FindPropertyRelative(propertyName);

        if (spriteProperty == null)
        {
            Debug.LogError($"Could not find battle sprite property '{propertyName}'.");
            return;
        }

        spriteProperty.objectReferenceValue = sprite;
    }

    private static void AssignOverworld(SerializedObject so, string propertyName, string rootPath, string dex)
    {
        SerializedProperty visuals = so.FindProperty("visuals");
        SerializedProperty overworld = visuals.FindPropertyRelative("overworld");
        SerializedProperty list = overworld.FindPropertyRelative(propertyName);
        list.ClearArray();

        List<Sprite> frames = new List<Sprite>();

        // Base frame
        AddFrameIfExists(frames, $"{rootPath}{dex}.png");

        // frame2, frame3, etc.
        int frameIndex = 2;
        while (true)
        {
            string framePath = $"{rootPath}frame{frameIndex}/{dex}.png";
            Sprite frame = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);

            if (frame == null)
                break;

            frames.Add(frame);
            frameIndex++;
        }

        for (int i = 0; i < frames.Count; i++)
        {
            list.InsertArrayElementAtIndex(i);
            list.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        }
    }

    private static void AddFrameIfExists(List<Sprite> frames, string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (sprite != null)
            frames.Add(sprite);
    }
}