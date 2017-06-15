using HalloweenControllerRPi.Controls;
using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.Controls;
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
         foreach (Func_PWM.tenFUNCTION f in Enum.GetValues(typeof(Func_PWM.tenFUNCTION)))
         {
            if (f != Func_PWM.tenFUNCTION.FUNC_NO_OF_FUNCTIONS)
               comboBox_Functions.Items.Add(f.ToString());
         }
         comboBox_Functions.SelectedIndex = 0;

         if (entype == Func_PWM.tenTYPE.TYPE_CONSTANT)
         {
            slider_Duration.IsEnabled = false;
            slider_StartDelay.IsEnabled = false;
         }

         _Func.FuncButtonType = typeof(Function_Button_PWM);
      }

      private void TextTitle_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
      {
         if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
         {
            this.SetCustomName();
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

      private void slider_MaxLevel_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            
            this.textBlock_MaxLevel.Text = "Max Level: " + this._Func.MaxLevel.ToString() + " (%)";
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
            this.textBlock_UpdateRate.Text = "Update Rate: " + this._Func.UpdateRate.ToString() + " (ms)";
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

         for (int i = 0; i < xPoints.Length; i++)
         {
            Line l = customDraw.CapturedPoints[i];

            yPoints[i] = ((100 / customDraw.YMax) * (customDraw.YMax - (l.Y2 > l.Y1 ? l.Y2 : l.Y1)));

            _Func.CustomLevels.Add((uint)yPoints[i]);
         }
      }

      #region XML Handling
      public void ReadXml(System.Xml.XmlReader reader)
      {
         this._Func.Delay_ms = Convert.ToUInt16(reader.GetAttribute("Delay"));
         this._Func.Duration_ms = Convert.ToUInt16(reader.GetAttribute("Duration"));
         this._Func.MaxLevel = Convert.ToUInt16(reader.GetAttribute("MaxLevel"));
         this._Func.UpdateRate = Convert.ToUInt16(reader.GetAttribute("UpdateRate"));
         this._Func.Function = (Func_PWM.tenFUNCTION)Convert.ToUInt16(reader.GetAttribute("Function"));
         //this.gb_FunctionName.Text = reader.GetAttribute("CustomName");

         this.textBlock_Duration.Text = "Duration: " + this._Func.Duration_ms.ToString() + " (ms)";
         this.textBlock_MaxLevel.Text = "Max Level: " + this._Func.MaxLevel.ToString() + " (%)";
         this.textBlock_UpdateRate.Text = "Update Rate: " + this._Func.UpdateRate.ToString() + " (ms)";
         this.textBlock_StartDelay.Text = "Start Delay: " + this._Func.Delay_ms.ToString() + " (ms)";

         /* Ignore MIN/MAX limits. */
         try
         {
            this.slider_Duration.Value = (int)this._Func.Duration_ms;
            this.slider_StartDelay.Value = (int)this._Func.Delay_ms;
            this.slider_MaxLevel.RangeMax = (int)this._Func.MaxLevel;
            this.slider_UpdateRate.Value = (int)this._Func.UpdateRate;
            this.comboBox_Functions.SelectedIndex = (int)this._Func.Function;
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
         writer.WriteAttributeString("CustomName", this.textTitle.Text);

         this._Func.WriteXml(writer);
      }
      #endregion

      public async void SetCustomName()
      {
         ContentDialog cd = new ContentDialog();
         StackPanel panel = new StackPanel();
         TextBox tb = new TextBox() { Text = this.textTitle.Text };
         panel.Orientation = Orientation.Vertical;
         panel.Children.Add(tb);

         cd.Title = "Enter Custom Name";
         cd.PrimaryButtonText = "OK";
         cd.PrimaryButtonClick += (sender, e) =>
         {
            this.textTitle.Text = tb.Text;
         };
         cd.Content = panel;
         await cd.ShowAsync();
      }

      public void Initialise()
      {
         
      }

      private void comboBox_Functions_DropDownClosed(object sender, object e)
      {
         if (_boInitialised == true)
         {
            Func_PWM.tenFUNCTION prevFunc = this._Func.Function;

            this._Func.Function = (Func_PWM.tenFUNCTION)(sender as ComboBox).SelectedIndex;

            if (this._Func.Function == Func_PWM.tenFUNCTION.FUNC_CUSTOM)
            {
               customLevelDraw.SecondaryButtonClick += (s, args) => { this._Func.Function = prevFunc; (sender as ComboBox).SelectedIndex = (int)prevFunc; };
               customLevelDraw.ShowAsync();
            }
         }
      }
   }
}
