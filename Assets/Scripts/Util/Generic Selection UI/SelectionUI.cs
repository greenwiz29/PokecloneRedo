using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDEUtils.UI
{
    public class SelectionUI<T> : MonoBehaviour where T : ISelectableItem
    {
        [SerializeField] float selectionSpeed = 5;
        float selectionTimer;
        List<T> items;
        int selection = 0;

        public event Action<int> OnSelected;
        public event Action OnBack;


        public void SetItems(List<T> items)
        {
            this.items = items;
            selectionTimer = 1 / selectionSpeed;
            UpdateSelectionUI();
        }

        public virtual void HandleUpdate()
        {
            int prev = selection;
            HandleListSelection();

            if (selection != prev)
            {
                UpdateSelectionUI();
            }

            UpdateSelectionTimer();

            if (Input.GetButtonDown("Action"))
            {
                OnSelected?.Invoke(selection);
            }
            else if (Input.GetButtonDown("Cancel"))
            {
                OnBack();
            }
        }

        private void UpdateSelectionUI()
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].OnSelectionChanged(i == selection);
            }
        }

        void HandleListSelection()
        {
            MenuSelectionMethods.HandleListSelection(ref selection, items.Count);
        }

        void UpdateSelectionTimer()
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
}