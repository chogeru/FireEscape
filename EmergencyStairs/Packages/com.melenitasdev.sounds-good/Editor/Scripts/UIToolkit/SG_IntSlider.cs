using UnityEngine.UIElements;

namespace MelenitasDev.SoundsGood.Editor
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
    public partial class SG_IntSlider : VisualElement
#else
    public class SG_IntSlider : VisualElement
#endif
    {
#if !UNITY_2023_2_OR_NEWER
        public new class UxmlFactory : UxmlFactory<SG_IntSlider, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Label = new() { name = "label", defaultValue = "Label" };
            UxmlIntAttributeDescription m_Min = new() { name = "min", defaultValue = 0 };
            UxmlIntAttributeDescription m_Max = new() { name = "max", defaultValue = 10 };
            UxmlIntAttributeDescription m_Value = new() { name = "value", defaultValue = 0 };
            UxmlStringAttributeDescription m_Suffix = new() { name = "suffix", defaultValue = "" };
            UxmlFloatAttributeDescription m_Space = new() { name = "space", defaultValue = 0f };
            UxmlFloatAttributeDescription m_RightSpace = new() { name = "right-space", defaultValue = 0f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var control = (SG_IntSlider)ve;

                control.LabelText = m_Label.GetValueFromBag(bag, cc);
                control.MinValue = m_Min.GetValueFromBag(bag, cc);
                control.MaxValue = m_Max.GetValueFromBag(bag, cc);
                control.Value = m_Value.GetValueFromBag(bag, cc);
                control.Suffix = m_Suffix.GetValueFromBag(bag, cc);
                control.Space = m_Space.GetValueFromBag(bag, cc);
                control.RightSpace = m_RightSpace.GetValueFromBag(bag, cc);
            }
        }
#endif

        private readonly Label titleLabel;
        private readonly SliderInt slider;
        private readonly Label valueLabel;

        private string suffix = "";

        public event System.Action<int> OnValueChanged;

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("label")]
#endif
        public string LabelText { get => titleLabel.text; set => titleLabel.text = value; }

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("suffix")]
#endif
        public string Suffix
        {
            get => suffix;
            set
            {
                suffix = value ?? "";
                UpdateValueLabel(Value);
            }
        }

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("min")]
#endif
        public int MinValue { get => slider.lowValue; set => slider.lowValue = value; }

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("max")]
#endif
        public int MaxValue { get => slider.highValue; set => slider.highValue = value; }

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("value")]
#endif
        public int Value
        {
            get => slider.value;
            set
            {
                slider.SetValueWithoutNotify(value);
                UpdateValueLabel(value);
            }
        }

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("space")]
#endif
        public float Space
        {
            get => titleLabel.style.marginRight.value.value;
            set => titleLabel.style.marginRight = value;
        }

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("right-space")]
#endif
        public float RightSpace
        {
            get => valueLabel.style.marginLeft.value.value;
            set => valueLabel.style.marginLeft = value;
        }

        public SG_IntSlider ()
        {
            AddToClassList("sg-setting-row");

            var row = new VisualElement();
            row.AddToClassList("sg-setting-row__content");
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            titleLabel = new Label("Label");
            titleLabel.AddToClassList("sg-setting-row__title");

            slider = new SliderInt();
            slider.AddToClassList("sg-setting-row__slider");
            slider.style.flexGrow = 1f;

            valueLabel = new Label();
            valueLabel.AddToClassList("sg-setting-row__value");

            row.Add(titleLabel);
            row.Add(slider);
            row.Add(valueLabel);

            Add(row);

            slider.RegisterValueChangedCallback(evt =>
            {
                UpdateValueLabel(evt.newValue);
                OnValueChanged?.Invoke(evt.newValue);
            });

            UpdateValueLabel(slider.value);
        }

        private void UpdateValueLabel (int val) { valueLabel.text = $"{val}{suffix}"; }
    }
}