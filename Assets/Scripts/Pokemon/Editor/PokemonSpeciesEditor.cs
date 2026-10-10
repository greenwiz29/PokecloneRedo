using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(PokemonSpecies))]
public class PokemonSpeciesEditor : Editor
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
            PokemonSpecies pokemon = (PokemonSpecies)target;

            if (PokemonSpeciesSpriteAssigner.AssignSprites(pokemon))
            {
                Debug.Log(
                    $"[PokemonSpeciesEditor] Auto-assigned sprites " +
                    $"for Dex #{pokemon.DexNumber}");
            }
        }
        GUILayout.Space(10);
        DrawOverworldPreview((PokemonSpecies)target);

        DrawDefaultInspector();
    }

    // ---------- Helpers ----------
    void ResetPreviewFrame()
    {
        previewFrame = 0;
        lastFrameTime = EditorApplication.timeSinceStartup;
    }

    void DrawOverworldPreview(PokemonSpecies pokemon)
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

    List<Sprite> GetPreviewFrames(PokemonSpecies pokemon)
    {
        // If you later add female overworld sprites,
        // this is where you would branch to them.
        // For now, female uses the same overworld sprites.

        return previewDirection switch
        {
            PreviewDirection.Down => previewShiny
                ? pokemon.OverworldSprites.WalkDownShiny
                : pokemon.OverworldSprites.WalkDown,

            PreviewDirection.Up => previewShiny
                ? pokemon.OverworldSprites.WalkUpShiny
                : pokemon.OverworldSprites.WalkUp,

            PreviewDirection.Left => previewShiny
                ? pokemon.OverworldSprites.WalkLeftShiny
                : pokemon.OverworldSprites.WalkLeft,

            PreviewDirection.Right => previewShiny
                ? pokemon.OverworldSprites.WalkRightShiny
                : pokemon.OverworldSprites.WalkRight,

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
