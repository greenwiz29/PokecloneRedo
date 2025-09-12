using UnityEngine;

public class MenuSelectionMethods
{
    public static void HandleListSelection(ref int selection, int maxOptions)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selection++;
            if (selection > maxOptions)
                selection = 0;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selection--;
            if (selection < 0)
                selection = maxOptions;
        }
        selection = Mathf.Clamp(selection, 0, maxOptions);
    }

    public static void HandleCategorySelection(ref int selectedCategory, int maxOptions)
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedCategory++;
            if (selectedCategory > maxOptions)
                selectedCategory = 0;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedCategory--;
            if (selectedCategory < 0)
                selectedCategory = maxOptions;
        }
        selectedCategory = Mathf.Clamp(selectedCategory, 0, maxOptions);
    }

    public static void HandleGridSelection(ref int selection, int maxOptions)
    {
        // TODO: Update keybindings here to allow control customization
        // Might also involve switching to Unity's new input system.
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++selection;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --selection;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selection += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selection -= 2;
        }

        selection = Mathf.Clamp(selection, 0, maxOptions - 1);
    }

}
