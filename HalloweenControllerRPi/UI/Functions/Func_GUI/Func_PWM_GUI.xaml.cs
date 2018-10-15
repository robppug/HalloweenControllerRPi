using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.Controls;
using HalloweenControllerRPi.UI.Functions.Func_GUI;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi.Function_GUI
{
    public sealed partial class Func_PWM_GUI : UserControl, IXmlFunction, IFunctionGUI
    {
        private Func_PWM _Func;
        private DrawCanvas customLevelDraw;
        private bool _boInitialised = false;

        public event EventHandler OnRemove;

        public string DurationText
        {
            get { return (string)GetValue(DurationTextProperty); }
            set { SetValue(DurationTextProperty, value); }
        }

        public string LevelText
        {
            get { return (string)GetValue(LevelTextProperty); }
            set { SetValue(LevelTextProperty, value); }
        }

        public string StartDelayText
        {
            get { return (string)GetValue(StartDelayTextProperty); }
            set { SetValue(StartDelayTextProperty, value); }
        }

        public string RampRateText
        {
            get { return (string)GetValue(RampRateTextProperty); }
            set { SetValue(RampRateTextProperty, value); }
        }

        public string UpdateRateText
        {
            get { return (string)GetValue(UpdateRateTextProperty); }
            set { SetValue(UpdateRateTextProperty, value); }
        }

        public static readonly DependencyProperty DurationTextProperty = DependencyProperty.Register("DurationText", typeof(string), typeof(UserControl), new PropertyMetadata("Duration: xxx (ms)"));
        public static readonly DependencyProperty LevelTextProperty = DependencyProperty.Register("LevelText", typeof(string), typeof(UserControl), new PropertyMetadata("Level: xxx %"));
        public static readonly DependencyProperty StartDelayTextProperty = DependencyProperty.Register("StartDelayText", typeof(string), typeof(UserControl), new PropertyMetadata("Start Delay: xxx (ms)"));
        public static readonly DependencyProperty RampRateTextProperty = DependencyProperty.Register("RampRateText", typeof(string), typeof(UserControl), new PropertyMetadata("Ramp Rate: xxx"));
        public static readonly DependencyProperty UpdateRateTextProperty = DependencyProperty.Register("UpdateRateText", typeof(string), typeof(UserControl), new PropertyMetadata("Update Rate: xxx"));

        public uint MaxDuration
        {
            get { return _Func.MaxDuration_ms; }
            set { _Func.MaxDuration_ms = value; DurationText = "Duration: " + _Func.MinDuration_ms.ToString() + " to " + _Func.MaxDuration_ms.ToString() + " (ms)"; }
        }
        public uint MinDuration
        {
            get { return _Func.MinDuration_ms; }
            set { _Func.MinDuration_ms = value; DurationText = "Duration: " + _Func.MinDuration_ms.ToString() + " to " + _Func.MaxDuration_ms.ToString() + " (ms)"; }
        }

        public uint MaxLevel
        {
            get { return _Func.MaxLevel; }
            set { _Func.MaxLevel = value; LevelText = "Level: " + _Func.MinLevel.ToString() + " to " + _Func.MaxLevel.ToString() + " %"; }
        }
        public uint MinLevel
        {
            get {  return _Func.MinLevel; }
            set { _Func.MinLevel = value; LevelText = "Level: " + _Func.MinLevel.ToString() + " to " + _Func.MaxLevel.ToString() + " %"; }
        }

        public uint MaxStartDelay
        {
            get { return _Func.MaxDelay_ms; }
            set { _Func.MaxDelay_ms = value; StartDelayText = "Start Delay: " + _Func.MinDelay_ms.ToString() + " to " + _Func.MaxDelay_ms.ToString() + " (ms)"; }
        }
        public uint MinStartDelay
        {
            get { return _Func.MinDelay_ms; }
            set { _Func.MinDelay_ms = value; StartDelayText = "Start Delay: " + _Func.MinDelay_ms.ToString() + " to " + _Func.MaxDelay_ms.ToString() + " (ms)"; }
        }

        public uint MaxUpdateRate
        {
            get { return _Func.MaxUpdateRate; }
            set { _Func.MaxUpdateRate = value; UpdateRateText = "Update Rate: " + _Func.MinUpdateRate.ToString() + " to " + _Func.MaxUpdateRate.ToString(); }
        }
        public uint MinUpdateRate
        {
            get { return _Func.MinUpdateRate; }
            set { _Func.MinUpdateRate = value; UpdateRateText = "Update Rate: " + _Func.MinUpdateRate.ToString() + " to " + _Func.MaxUpdateRate.ToString(); }
        }

        public uint MaxRampRate
        {
            get { return _Func.MaxRampRate; }
            set { _Func.MaxRampRate = value; RampRateText = "Ramp Rate: " + _Func.MinRampRate.ToString() + " to " + _Func.MaxRampRate.ToString(); }
        }
        public uint MinRampRate
        {
            get { return _Func.MinRampRate; }
            set { _Func.MinRampRate = value; RampRateText = "Ramp Rate: " + _Func.MinRampRate.ToString() + " to " + _Func.MaxRampRate.ToString(); }
        }
        public Function Func
        {
            get { return _Func; }
            set { _Func = (value as Func_PWM); }
        }

        public Func_PWM_GUI()
        {
            InitializeComponent();

            _boInitialised = true;
        }

        public Func_PWM_GUI(IHostApp host, uint index, Function.tenTYPE entype) : this()
        {
            _Func = new Func_PWM(host, entype);

            MaxLevel = 100;
            MinLevel = 0;

            customLevelDraw = new DrawCanvas();
            customLevelDraw.PrimaryButtonClick += MouseDraw_PrimaryButtonClick;

            _Func.Index = index;
            _Func.evOnTrigger += (sender, e) => { Grid.Background = new SolidColorBrush(Colors.LightYellow); };
            _Func.evOnDelayEnd += (sender, e) => { Grid.Background = new SolidColorBrush(Colors.LightGreen); };
            _Func.evOnDurationEnd += (sender, e) => { Grid.Background = new SolidColorBrush(Colors.LightGray); };

            textTitle.Text = "PWM #" + index;
            textTitle.DoubleTapped += TextTitle_DoubleTapped;

            /* Populate the comboBox list with available PWM Functions */
            foreach (PWMFunctions f in Enum.GetValues(typeof(PWMFunctions)))
            {
                if (f != PWMFunctions.FUNC_NO_OF_FUNCTIONS)
                {
                    if ((f.IsConstant() && (entype == Function.tenTYPE.TYPE_CONSTANT))
                       || (f.IsTriggered() && (entype == Function.tenTYPE.TYPE_TRIGGER)))
                    {
                        comboBox_Functions.Items.Add(f.ToString());
                    }
                }
            }

            comboBox_Functions.SelectedValue = PWMFunctions.FUNC_ON.ToString();
            comboBox_Functions_DropDownClosed(comboBox_Functions, EventArgs.Empty);

            if (entype == Function.tenTYPE.TYPE_CONSTANT)
            {
                slider_Duration.IsEnabled = false;
                slider_StartDelay.IsEnabled = false;
                textBlock_Duration.Visibility = Visibility.Collapsed;
                textBlock_StartDelay.Visibility = Visibility.Collapsed;
            }

            _Func.FuncButtonType = typeof(Function_Button_PWM);

            RemoveButton.Click += RemoveButton_Click;
        }


        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            OnRemove?.Invoke(this, EventArgs.Empty);
        }

        private void TextTitle_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                textTitle.Text = FuncGUIHelper.SetCustomName(textTitle.Text).Result;
            }
        }

        private void comboBox_Functions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void MouseDraw_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DrawCanvas customDraw = (sender as DrawCanvas);
            double[] yPoints = new double[customDraw.CapturedPoints.Count];

            _Func.CustomLevels.Clear();

            for (int i = 0; i < customDraw.CapturedPoints.Count; i++)
            {
                Line l = customDraw.CapturedPoints[i];

                yPoints[i] = ((100 / customDraw.YMax) * (customDraw.YMax - (l.Y2 > l.Y1 ? l.Y2 : l.Y1)));

                _Func.CustomLevels.Add((uint)yPoints[i]);
            }
        }

        #region XML Handling
        public void ReadXml(XmlReader reader)
        {
            throw new Exception("Deprecated");
        }

        public void ReadXML(XElement element)
        {
            _Func.ReadXML(element);

            _boInitialised = false;
            _Func.Function = FuncGUIHelper.GetFunctionEnum(element.Attribute("Function").Value);
            _boInitialised = true;

            textTitle.Text = element.Attribute("CustomName").Value;
            DurationText = "Duration: " + _Func.MinDuration_ms.ToString() + " to " + _Func.MaxDuration_ms.ToString() + " (ms)";
            LevelText = "Level: " + _Func.MinLevel.ToString() + " to " + _Func.MaxLevel.ToString() + " %";
            StartDelayText = "Start Delay: " + _Func.MinDelay_ms.ToString() + " to " + _Func.MaxDelay_ms.ToString() + " (ms)";
            UpdateRateText = "Update Rate: " + _Func.MinUpdateRate.ToString() + " to " + _Func.MaxUpdateRate.ToString();
            RampRateText = "Ramp Rate: " + _Func.MinRampRate.ToString() + " to " + _Func.MaxRampRate.ToString();

            if (_Func.Function == PWMFunctions.FUNC_CUSTOM)
            {
                customLevelDraw.ReadXML(element);
            }

            /* Ignore MIN/MAX limits. */
            try
            {
                slider_Duration.RangeMin = (int)_Func.MinDuration_ms;
                slider_Duration.RangeMax = (int)_Func.MaxDuration_ms;
                slider_StartDelay.RangeMin = (int)_Func.MinDelay_ms;
                slider_StartDelay.RangeMax = (int)_Func.MaxDelay_ms;
                slider_MaxLevel.RangeMin = (int)_Func.MinLevel;
                slider_MaxLevel.RangeMax = (int)_Func.MaxLevel;
                slider_UpdateRate.RangeMin = (int)_Func.MinUpdateRate;
                slider_UpdateRate.RangeMax = (int)_Func.MaxUpdateRate;
                slider_RampRate.RangeMin = (int)_Func.MinRampRate;
                slider_RampRate.RangeMax = (int)_Func.MaxRampRate;
                comboBox_Functions.SelectedIndex = comboBox_Functions.Items.IndexOf(_Func.Function.ToString());

                CheckSelectedFunction(PWMFunctions.FUNC_OFF, _Func.Function);
            }
            catch { }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Type", GetType().ToString());
            writer.WriteAttributeString("CustomName", textTitle.Text);

            _Func.WriteXml(writer);

            customLevelDraw.WriteXml(writer);
        }
        #endregion

        public void Initialise()
        {
        }

        private void comboBox_Functions_DropDownClosed(object sender, object e)
        {
            if (_boInitialised == true)
            {
                PWMFunctions prevFunc = _Func.Function;

                _Func.Function = FuncGUIHelper.GetFunctionEnum((sender as ComboBox).SelectedValue.ToString());

                CheckSelectedFunction(_Func.Function, prevFunc);
            }
        }

        private void CheckSelectedFunction(PWMFunctions newFunc, PWMFunctions prevFunc)
        {
            bool boCanRamp = true;
            bool boCanUpdateTick = true;

            switch (newFunc)
            {
                case PWMFunctions.FUNC_OFF:
                    boCanUpdateTick = false;
                    boCanRamp = false;
                    break;
                case PWMFunctions.FUNC_ON:
                    boCanUpdateTick = false;
                    boCanRamp = false;
                    break;
                case PWMFunctions.FUNC_FLICKER_OFF:
                    break;
                case PWMFunctions.FUNC_FLICKER_ON:
                    break;
                case PWMFunctions.FUNC_RANDOM:
                    boCanRamp = false;
                    break;
                case PWMFunctions.FUNC_SIGNWAVE:
                    break;
                case PWMFunctions.FUNC_STROBE:
                    boCanRamp = false;
                    break;
                case PWMFunctions.FUNC_SWEEP_DOWN:
                    break;
                case PWMFunctions.FUNC_SWEEP_UP:
                    break;
                case PWMFunctions.FUNC_CUSTOM:
                    customLevelDraw.SecondaryButtonClick += (s, args) => { _Func.Function = prevFunc; newFunc = prevFunc; };
                    customLevelDraw.ShowAsync();
                    boCanRamp = false;
                    break;
                case PWMFunctions.FUNC_RAMP_ON:
                    break;
                case PWMFunctions.FUNC_RAMP_OFF:
                    break;
                case PWMFunctions.FUNC_RAMP_BOTH:
                    break;
                default:
                    break;
            }

            DurationText = "Duration: " + _Func.MinDuration_ms.ToString() + " to " + _Func.MaxDuration_ms.ToString() + " (ms)";
            LevelText = "Level: " + _Func.MinLevel.ToString() + " to " + _Func.MaxLevel.ToString() + " %";
            StartDelayText = "Start Delay: " + _Func.MinDelay_ms.ToString() + " to " + _Func.MaxDelay_ms.ToString() + " (ms)";
            UpdateRateText = "Update Rate: " + _Func.MinUpdateRate.ToString() + " to " + _Func.MaxUpdateRate.ToString();
            RampRateText = "Ramp Rate: " + _Func.MinRampRate.ToString() + " to " + _Func.MaxRampRate.ToString();

            slider_RampRate.IsEnabled = boCanRamp;
            textBlock_RampRate.Visibility = (boCanRamp ? Visibility.Visible : Visibility.Collapsed);
            slider_UpdateRate.IsEnabled = boCanUpdateTick;
            textBlock_UpdateRate.Visibility = (boCanUpdateTick ? Visibility.Visible : Visibility.Collapsed);
        }
    }
}
