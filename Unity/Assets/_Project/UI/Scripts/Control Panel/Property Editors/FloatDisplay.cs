using System;
using System.Globalization;
using _Project.UI.Scripts.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Control_Panel.Property_Editors
{
    /// <summary>
    /// A UI class that allows for displaying a floating point value using an input field.
    /// </summary>
    public class FloatDisplay : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI title;
        [SerializeField]
        private InputField input;
        [SerializeField]
        private string tooltip;

        /// <summary>
        /// The displayed title of this display.
        /// </summary>
        public string Title
        {
            get => title.text;
            set => title.text = value;
        }

        private float _value;
        /// <summary>
        /// The value of this display.
        /// </summary>
        public float Value
        {
            get => _value;
            set
            {
                // Set the value.
                _value = (float)Math.Round(value, Digits);
                input.text = value.ToString(CultureInfo.InvariantCulture);
            }
        }
        
        [SerializeField]
        private int digits;
        /// <summary>
        /// The number of digits the display's value will be rounded to.
        /// </summary>
        public int Digits => digits;

        private void Awake()
        {
            TooltipTrigger tooltipTrigger = title.GetComponent<TooltipTrigger>();
            tooltipTrigger.Content = tooltip;
        }
    }
}
