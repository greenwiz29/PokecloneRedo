using UnityEngine;

public class MenuSelectionMethods
{
    static float selectionTimer = 0.2f;
    public static void HandleListSelection(ref int selection, int maxOptions)
    {
        float v = Input.GetAxis("Vertical");

        if (selectionTimer == 0 && Mathf.Abs(v) > 0.2f)
        {
            selection += -(int)Mathf.Sign(v);

            if (selection < 0)
            {
                selection = maxOptions - 1;
            }
            else if (selection >= maxOptions)
            {
                selection = 0;
            }

            selection = Mathf.Clamp(selection, 0, maxOptions - 1);
            selectionTimer = 0.2f;
        }
        UpdateSelectionTimer();
    }

    public static void HandleCategorySelection(ref int selectedCategory, int maxOptions)
    {
        float h = Input.GetAxis("Horizontal");

        if (selectionTimer == 0 && Mathf.Abs(h) > 0.2f)
        {
            selectedCategory += (int)Mathf.Sign(h);

            if (selectedCategory < 0)
            {
                selectedCategory = maxOptions - 1;
            }
            else if (selectedCategory >= maxOptions)
            {
                selectedCategory = 0;
            }

            selectedCategory = Mathf.Clamp(selectedCategory, 0, maxOptions - 1);
            selectionTimer = 0.2f;
        }
        UpdateSelectionTimer();
    }

    public static void HandleGridSelection(ref int selection, int maxOptions, int columns = 2)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (selectionTimer == 0 && (Mathf.Abs(h) > 0.2f || Mathf.Abs(v) > 0.2f))
        {
            if (Mathf.Abs(h) > Mathf.Abs(v))
            {
                selection += (int)Mathf.Sign(h);
            }
            else
            {
                selection += -(int)Mathf.Sign(v) * columns;
            }            

            if (selection < 0)
            {
                selection = maxOptions - 1;
            }
            else if (selection >= maxOptions)
            {
                selection = 0;
            }

            selection = Mathf.Clamp(selection, 0, maxOptions - 1);
            selectionTimer = 0.2f;
        }
        UpdateSelectionTimer();        
    }

    static void UpdateSelectionTimer()
    {
        if (selectionTimer > 0)
        {
            selectionTimer -= Time.deltaTime;
        }
        if (selectionTimer < 0)
        {
            selectionTimer = 0;
        }
    }
}
