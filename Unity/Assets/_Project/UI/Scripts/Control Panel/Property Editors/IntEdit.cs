using System;
using _Project.UI.Scripts.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Control_Panel.Property_Editors
{
    /// <summary>
    /// A UI class that allows for editing an integer value via a slider and an input field. Presents a
    /// <see cref="OnValueChanged"/> event that is invoked whenever the value is changed. This change can come from the
    /// slider, input field or a direct assignment to the <see cref="Value"/> field.
    /// </summary>
    public class IntEdit : MonoBehaviour
    {
        public UnityEvent<int> OnValueChanged;

        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Slider slider;
        [SerializeField] private InputField input;
        [SerializeField] private string tooltip;

        /// <summary>
        /// The displayed title of this <see cref="IntEdit"/>.
        /// </summary>
        public string Title
        {
            get => title.text;
            set => title.text = value;
        }

        private int _value;

        /// <summary>
        /// The value of this <see cref="IntEdit"/>. When set, the slider and input field UI elements will be updated.
        /// <see cref="OnValueChanged"/> will be invoked when it is set.
        /// </summary>
        public int Value
        {
            get => _value;
            set
            {
                if (_value == value) return;

                // Set the value.
                _value = CorrectValue(value);

                // Only change the slider if it is not up-to-date.
                if ((int)slider.value != _value)
                    slider.value = _value;

                // Try to change the input field if it is not up-to-date.
                try
                {
                    if (CorrectValue(int.Parse(input.text)) != _value)
                        input.text = _value.ToString();
                }
                // An improperly formated input field is interpreted as a 0. Update it if the value is not actually 0.
                catch (FormatException)
                {
                    if (_value != CorrectValue(0))
                        input.text = _value.ToString();
                }

                // Notify listeners of the change.
                OnValueChanged?.Invoke(_value);
            }
        }

        [SerializeField] private int minValue;

        /// <summary>
        /// The minimum value this <see cref="IntEdit"/> can take on. When set, <see cref="Value"/> will be recalculated
        /// and if it is below the new minimum it will be clamped.
        /// </summary>
        public int MinValue
        {
            get => minValue;
            set
            {
                minValue = value;
                if (slider != null) slider.minValue = minValue;
                Value = Value < minValue ? minValue : Value; // Update the value if it is below the new minimum.
            }
        }

        [SerializeField] private int maxValue;

        /// <summary>
        /// The maximum value this <see cref="IntEdit"/> can take on. When set, <see cref="Value"/> will be recalculated
        /// and if it is above the new maximum it will be clamped.
        /// </summary>
        public int MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = value;
                if (slider != null) slider.maxValue = maxValue;
                Value = Value > maxValue ? maxValue : Value; // Update the value if it is above the new maximum.
            }
        }

        [SerializeField] private bool interactable;

        /// <summary>
        /// Whether this <see cref="IntEdit"/>'s UI is interactable.
        /// </summary>
        public bool Interactable
        {
            get => interactable;
            set
            {
                interactable = value;
                slider.interactable = interactable;
                input.interactable = interactable;

                Text inputText = input.transform.Find("Text").GetComponent<Text>();

                if (interactable)
                {
                    title.color = new Color(title.color.r, title.color.g, title.color.b, 1.0f);
                    inputText.color = new Color(inputText.color.r, inputText.color.g, inputText.color.b, 1.0f);
                }
                else
                {
                    title.color = new Color(title.color.r, title.color.g, title.color.b, 0.4f);
                    inputText.color = new Color(inputText.color.r, inputText.color.g, inputText.color.b, 0.4f);
                }
            }
        }

        /// <summary>
        /// Correct <paramref name="value"/> to fit within the restrictions of this <see cref="IntEdit"/>. This involves
        /// clamping it between <see cref="MinValue"/> and <see cref="MaxValue"/>.
        /// </summary>
        /// <param name="value"> The integer value to correct. </param>
        /// <returns> <paramref name="value"/> clamped. </returns>
        private int CorrectValue(int value)
        {
            return Mathf.Clamp(value, MinValue, MaxValue);
        }

        private int GetInputValue()
        {
            // Try to get the value from the input field.
            try
            {
                return int.Parse(input.text);
            }
            // If the input field is not formatted properly we default to 0.
            catch (FormatException)
            {
                return 0;
            }
        }

        private void CheckSliderValueChanged()
        {
            Value = (int)slider.value;
        }

        /// <summary>
        /// Callback for <see cref="InputField.onValueChanged"/>. We update <see cref="Value"/>, but we don't set the input
        /// field's text. This means that, while changing, an input field can be in an improperly formatted state (e.g.
        /// empty). <see cref="Value"/> will then be set to <see cref="CorrectValue(int)"/> applied to 0.
        /// </summary>
        private void CheckInputValueChanged()
        {
            Value = GetInputValue();
        }

        /// <summary>
        /// Callback for <see cref="InputField.onEndEdit"/>. We update <see cref="Value"/> and set the input field's text.
        /// This means that if an input field was left in an improperly formatted state its text will be set to 
        /// <see cref="CorrectValue(int)"/> applied to 0.
        /// </summary>
        private void CheckInputEndEdit()
        {
            Value = GetInputValue();
            input.text = Value.ToString();
        }

        private void Awake()
        {
            slider.wholeNumbers = true;
            slider.value = MinValue; // Default to min value.
            slider.minValue = MinValue;
            slider.maxValue = MaxValue;
            slider.onValueChanged.AddListener(delegate { CheckSliderValueChanged(); });

            input.text = MinValue.ToString(); // Default to min value.
            input.onValueChanged.AddListener(delegate { CheckInputValueChanged(); });
            input.onEndEdit.AddListener(delegate { CheckInputEndEdit(); });

            // Update interactability based on serialized value in inspector.
            Interactable = interactable;

            TooltipTrigger tooltipTrigger = title.GetComponent<TooltipTrigger>();
            tooltipTrigger.Content = tooltip;
        }
    }
}