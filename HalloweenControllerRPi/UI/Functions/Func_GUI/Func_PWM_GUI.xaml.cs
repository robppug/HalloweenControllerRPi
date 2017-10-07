using HalloweenControllerRPi.Controls;
using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.Controls;
using HalloweenControllerRPi.UI.Functions.Func_GUI;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using System;
using System.Xml.Serialization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi.Function_GUI
{
   public sealed partial class Func_PWM_GUI : UserControl, IXmlSerializable, IFunctionGUI
   {
      private Func_PWM _Func;
      private DrawCanvas customLevelDraw;
      private bool _boInitialised = false;

      public event EventHandler OnRemove;

      public uint MaxLevel
      {
         get { textBlock_MaxLevel.Text = "Max Level: " + _Func.MaxLevel.ToString() + " %"; return _Func.MaxLevel; }
         set { _Func.MaxLevel = value; textBlock_MaxLevel.Text = "Max Level: " + value.ToString() + " %"; }
      }
      public uint MinLevel
      {
         get { textBlock_MinLevel.Text = "Min Level: " + _Func.MinLevel.ToString() + " %"; return _Func.MinLevel; }
         set { _Func.MinLevel = value; textBlock_MinLevel.Text = "Min Level: " + value.ToString() + " %"; }
      }

      public Function Func
      {
         get { return this._Func; }
         set { this._Func = (value as Func_PWM); }
      }

      public Func_PWM_GUI()
      {
         this.InitializeComponent();

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
         
         textTitle.Text = "PWM #" + index;
         textTitle.DoubleTapped += TextTitle_DoubleTapped;
         textBlock_MaxLevel.DataContext = this;
         _Func.Duration_ms = (uint)slider_Duration.Value;
         _Func.Delay_ms = (uint)slider_MaxLevel.RangeMax;

         /* Populate the comboBox list with available PWM Functions */
         foreach (PWMFunctions f in Enum.GetValues(typeof(PWMFunctions)))
         {
            if (f != PWMFunctions.FUNC_NO_OF_FUNCTIONS)
            {
               if (  (f.IsConstant() && (entype == Function.tenTYPE.TYPE_CONSTANT))
                  || (f.IsTriggered() && (entype == Function.tenTYPE.TYPE_TRIGGER)))
               {
                  comboBox_Functions.Items.Add(f.ToString());
               }
            }
         }

         comboBox_Functions.SelectedValue = PWMFunctions.FUNC_ON.ToString();
         comboBox_Functions_DropDownClosed(comboBox_Functions, EventArgs.Empty);

         if (entype == Func_PWM.tenTYPE.TYPE_CONSTANT)
         {
            slider_Duration.IsEnabled = false;
            textBlock_Duration.Visibility = Visibility.Collapsed;
            slider_StartDelay.IsEnabled = false;
            textBlock_StartDelay.Visibility = Visibility.Collapsed;
         }

         _Func.FuncButtonType = typeof(Function_Button_PWM);

         this.RemoveButton.Click += RemoveButton_Click;
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

      private void slider_Duration_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            this._Func.Duration_ms = (uint)(sender as Slider).Value;
            this.textBlock_Duration.Text = "Duration: " + this._Func.Duration_ms.ToString() + " (ms)";
         }
      }

      private void slider_StartDelay_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            this._Func.Delay_ms = (uint)(sender as Slider).Value;
            this.textBlock_StartDelay.Text = "Start Delay: " + this._Func.Delay_ms.ToString() + " (ms)";
         }
      }

      private void slider_UpdateRate_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            this._Func.UpdateRate = (uint)(sender as Slider).Value;
            this.textBlock_UpdateRate.Text = "Update Rate: " + this._Func.UpdateRate.ToString();
         }
      }

      private void slider_RampRate_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            this._Func.RampRate = (uint)(sender as Slider).Value;
            this.textBlock_RampRate.Text = "Ramp Rate: " + this._Func.RampRate.ToString();
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
      public void ReadXml(System.Xml.XmlReader reader)
      {
         _Func.ReadXml(reader);
         
         _Func.Function = FuncGUIHelper.GetFunctionEnum(reader.GetAttribute("Function").ToString());

         MinLevel = Convert.ToUInt16(reader.GetAttribute("MinLevel"));
         MaxLevel = Convert.ToUInt16(reader.GetAttribute("MaxLevel"));

         textTitle.Text = reader.GetAttribute("CustomName");
         textBlock_Duration.Text = "Duration: " + _Func.Duration_ms.ToString() + " (ms)";
         textBlock_UpdateRate.Text = "Update Rate: " + _Func.UpdateRate.ToString();
         textBlock_RampRate.Text = "Ramp Rate: " + _Func.RampRate.ToString();
         textBlock_StartDelay.Text = "Start Delay: " + _Func.Delay_ms.ToString() + " (ms)";

         if (_Func.Function == PWMFunctions.FUNC_CUSTOM)
         {
            this.customLevelDraw.ReadXml(reader);
         }

         /* Ignore MIN/MAX limits. */
         try
         {
            slider_Duration.Value = (int)_Func.Duration_ms;
            slider_StartDelay.Value = (int)_Func.Delay_ms;
            slider_MaxLevel.RangeMin = (int)_Func.MinLevel;
            slider_MaxLevel.RangeMax = (int)_Func.MaxLevel;
            slider_UpdateRate.Value = (int)_Func.UpdateRate;
            comboBox_Functions.SelectedIndex = comboBox_Functions.Items.IndexOf(_Func.Function.ToString());
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
         bool boCanRamp = true;
         bool boCanUpdateTick = true;

         if (_boInitialised == true)
         {
            PWMFunctions prevFunc = _Func.Function;

            _Func.Function = FuncGUIHelper.GetFunctionEnum((sender as ComboBox).SelectedValue.ToString());

            switch(_Func.Function)
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
                  customLevelDraw.SecondaryButtonClick += (s, args) => { _Func.Function = prevFunc; (sender as ComboBox).SelectedValue = prevFunc; };
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

            slider_RampRate.IsEnabled = boCanRamp;
            textBlock_RampRate.Visibility = (boCanRamp ? Visibility.Visible : Visibility.Collapsed);
            slider_UpdateRate.IsEnabled = boCanUpdateTick;
            textBlock_UpdateRate.Visibility = (boCanUpdateTick ? Visibility.Visible : Visibility.Collapsed);
         }
      }

      private void slider_Duration_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
      {
         if(slider_Duration.Maximum > 60000)
         {
            slider_Duration.Maximum = 10000;
         }
         else
         {
            slider_Duration.Maximum += 10000;
         }
      }
   }
}
