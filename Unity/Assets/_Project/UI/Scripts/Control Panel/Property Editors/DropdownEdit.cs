using _Project.UI.Scripts.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Control_Panel.Property_Editors
{
    public class DropdownEdit : MonoBehaviour
    {
        public UnityEvent<int> onValueChanged = new();

        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private string tooltip;

        /// <summary>
        /// The displayed title of this <see cref="DropdownEdit"/>.
        /// </summary>
        public string Title
        {
            get => title.text;
            set => title.text = value;
        }

        private int _value;
        public int Value
        {
            get => _value;
            set
            {
                Debug.Log($"Value.set: {value}");
                if (_value == value) return;
                _value = value;
                Debug.Log($"{Title}: invoking onValueChanged ({onValueChanged}) with value {value}");
                onValueChanged.Invoke(_value);
            }
        }

        [SerializeField] private bool interactable;

        /// <summary>
        /// Whether this <see cref="DropdownEdit"/>'s UI is interactable.
        /// </summary>
        public bool Interactable
        {
            get => interactable;
            set
            {
                interactable = value;
                dropdown.interactable = interactable;

                Graphic graphic = dropdown.targetGraphic;

                if (interactable)
                {
                    title.color = new Color(title.color.r, title.color.g, title.color.b, 1.0f);
                    graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 1.0f);
                }
                else
                {
                    title.color = new Color(title.color.r, title.color.g, title.color.b, 0.4f);
                    graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0.4f);
                }
            }
        }

        private void Awake()
        {
            dropdown.onValueChanged.AddListener(value => Value = value);
            
            // Update interactability based on serialized value in inspector.
            Interactable = interactable;

            TooltipTrigger tooltipTrigger = title.GetComponent<TooltipTrigger>();
            tooltipTrigger.Content = tooltip;
        }
    }
}