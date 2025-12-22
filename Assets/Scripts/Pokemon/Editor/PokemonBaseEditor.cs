using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(PokemonBase))]
public class PokemonBaseEditor : Editor
{
    enum PreviewDirection { Down, Up, Left, Right }

    bool showPreview;
    PreviewDirection previewDirection = PreviewDirection.Down;
    bool previewShiny;

    double lastFrameTime;
    int previewFrame;
    const float frameDuration = 0.2f;
    bool previewFemale;
    bool isPlaying = true;


    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);

        if (GUILayout.Button("Auto-Assign Sprites from Dex Number"))
        {
            AutoAssignSprites((PokemonBase)target);
        }
        GUILayout.Space(10);
        DrawOverworldPreview((PokemonBase)target);

        DrawDefaultInspector();
    }

    void AutoAssignSprites(PokemonBase pokemon)
    {
        Undo.RecordObject(pokemon, "Auto Assign Pokemon Sprites");

        SerializedObject so = new SerializedObject(pokemon);
        int dexNumber = so.FindProperty("dexNumber").intValue;
        string dex = dexNumber.ToString();

        string basePath = "Assets/Art/Pokemons/";

        // ===== FRONT / BACK =====
        AssignSprite(so, "frontSprite", $"{basePath}Front/{dex}.png");
        AssignSprite(so, "backSprite", $"{basePath}Back/{dex}.png");

        AssignSprite(so, "frontSpriteFemale", $"{basePath}Front/female/{dex}.png");
        AssignSprite(so, "backSpriteFemale", $"{basePath}Back/female/{dex}.png");

        AssignSprite(so, "frontSpriteShiny", $"{basePath}Front/shiny/{dex}.png");
        AssignSprite(so, "backSpriteShiny", $"{basePath}Back/shiny/{dex}.png");

        AssignSprite(so, "frontSpriteFemaleShiny", $"{basePath}Front/shiny/female/{dex}.png");
        AssignSprite(so, "backSpriteFemaleShiny", $"{basePath}Back/shiny/female/{dex}.png");

        // ===== OVERWORLD NORMAL =====
        AssignOverworld(so, "walkDownSprites", $"{basePath}overworld/normal/down/", dex);
        AssignOverworld(so, "walkUpSprites", $"{basePath}overworld/normal/up/", dex);
        AssignOverworld(so, "walkLeftSprites", $"{basePath}overworld/normal/left/", dex);
        AssignOverworld(so, "walkRightSprites", $"{basePath}overworld/normal/right/", dex);

        // ===== OVERWORLD SHINY =====
        AssignOverworld(so, "walkDownSpritesShiny", $"{basePath}overworld/shiny/down/", dex);
        AssignOverworld(so, "walkUpSpritesShiny", $"{basePath}overworld/shiny/up/", dex);
        AssignOverworld(so, "walkLeftSpritesShiny", $"{basePath}overworld/shiny/left/", dex);
        AssignOverworld(so, "walkRightSpritesShiny", $"{basePath}overworld/shiny/right/", dex);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(pokemon);

        Debug.Log($"[PokemonBaseEditor] Auto-assigned sprites for Dex #{dex}");
    }

    // ---------- Helpers ----------
    void ResetPreviewFrame()
    {
        previewFrame = 0;
        lastFrameTime = EditorApplication.timeSinceStartup;
    }

    void AssignSprite(SerializedObject so, string propertyName, string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            return;

        so.FindProperty(propertyName).objectReferenceValue = sprite;
    }

    void AssignOverworld(SerializedObject so, string propertyName, string rootPath, string dex)
    {
        SerializedProperty list = so.FindProperty(propertyName);
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

    void AddFrameIfExists(List<Sprite> frames, string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
            frames.Add(sprite);
    }
    void DrawOverworldPreview(PokemonBase pokemon)
    {
        showPreview = EditorGUILayout.Foldout(showPreview, "Overworld Preview", true);
        if (!showPreview)
            return;

        EditorGUILayout.BeginVertical("box");

        EditorGUI.BeginChangeCheck();

        previewDirection = (PreviewDirection)EditorGUILayout.EnumPopup(
            "Direction", previewDirection);
        previewShiny = EditorGUILayout.Toggle("Shiny", previewShiny);
        previewFemale = EditorGUILayout.Toggle("Female", previewFemale);

        if (EditorGUI.EndChangeCheck())
        {
            ResetPreviewFrame();
        }

        GUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(isPlaying ? "Pause" : "Play", GUILayout.Width(80)))
        {
            isPlaying = !isPlaying;
            lastFrameTime = EditorApplication.timeSinceStartup;
        }

        EditorGUILayout.EndHorizontal();

        List<Sprite> frames = GetPreviewFrames(pokemon);
        if (frames == null || frames.Count == 0)
        {
            EditorGUILayout.HelpBox("No overworld sprites found for this variant.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        if (isPlaying)
            AnimatePreview(frames);

        Sprite sprite = frames[previewFrame];
        if (sprite != null)
        {
            Rect rect = GUILayoutUtility.GetRect(64, 64, GUILayout.ExpandWidth(false));
            DrawSprite(rect, sprite);
        }

        EditorGUILayout.EndVertical();
    }
    List<Sprite> GetPreviewFrames(PokemonBase pokemon)
    {
        // If you later add female overworld sprites,
        // this is where you would branch to them.
        // For now, female uses the same overworld sprites.

        return previewDirection switch
        {
            PreviewDirection.Down => previewShiny
                ? pokemon.WalkDownAnimShiny
                : pokemon.WalkDownAnim,

            PreviewDirection.Up => previewShiny
                ? pokemon.WalkUpAnimShiny
                : pokemon.WalkUpAnim,

            PreviewDirection.Left => previewShiny
                ? pokemon.WalkLeftAnimShiny
                : pokemon.WalkLeftAnim,

            PreviewDirection.Right => previewShiny
                ? pokemon.WalkRightAnimShiny
                : pokemon.WalkRightAnim,

            _ => null
        };
    }

    void AnimatePreview(List<Sprite> frames)
    {
        double time = EditorApplication.timeSinceStartup;
        if (time - lastFrameTime > frameDuration)
        {
            previewFrame = (previewFrame + 1) % frames.Count;
            lastFrameTime = time;
            Repaint();
        }
    }

    void DrawSprite(Rect rect, Sprite sprite)
    {
        Rect texRect = sprite.textureRect;
        texRect.x /= sprite.texture.width;
        texRect.width /= sprite.texture.width;
        texRect.y /= sprite.texture.height;
        texRect.height /= sprite.texture.height;

        GUI.DrawTextureWithTexCoords(rect, sprite.texture, texRect);
    }

}
