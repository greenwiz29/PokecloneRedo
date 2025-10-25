using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDEUtils.UI
{
    public enum SelectionMode { LIST, GRID }

    public class SelectionUI<T> : MonoBehaviour where T : ISelectableItem
    {
        [SerializeField] float selectionSpeed = 5;
        float selectionTimer;
        List<T> items;
        protected int selection = 0;

        public event Action<int> OnSelected;
        public event Action OnBack;

        SelectionMode mode;
        int gridWidth;

        public void SetSelectionSettings(SelectionMode selectionMode, int gridWidth = 2)
        {
            mode = selectionMode;
            this.gridWidth = gridWidth;
        }

        public void SetItems(List<T> items)
        {
            this.items = items;
            selectionTimer = 1 / selectionSpeed;
            items.ForEach(i => i.Init());
            UpdateSelectionUI();
        }
        
        public void ClearItems()
        {
            items.ForEach(i => i.Clear());
            items = null;
        }

        public virtual void HandleUpdate()
        {
            int prev = selection;
            switch (mode)
            {
                case SelectionMode.LIST:
                    HandleListSelection();
                    break;
                case SelectionMode.GRID:
                    HandleGridSelection(gridWidth);
                    break;
            }

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

		public virtual void UpdateSelectionUI()
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

		void HandleGridSelection(int cols)
		{
			MenuSelectionMethods.HandleGridSelection(ref selection, items.Count, cols);
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