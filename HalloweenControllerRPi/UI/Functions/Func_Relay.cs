using HalloweenControllerRPi.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Functions
{
   public class Func_RELAY : Function
   {
      public enum tenOutputLevel
      {
         tLow = 0,
         tHigh = 1
      };

      public Func_RELAY()
      {
      }

      public Func_RELAY(IHostApp host, tenTYPE entype)
         : base(host, entype)
      {
         FunctionKeyCommand = new Command("RELAY", 'R');

         evOnDelayEnd += OnTrigger;
         evOnDurationEnd += OnDurationEnd;
      }

      /// <summary>
      /// This does the actual processing of the onTrigger EVENT.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnTrigger(object sender, EventArgs e)
      {
         List<string> lData = new List<string>();

         lData.Add(Index.ToString("00"));
         lData.Add(((uint)tenOutputLevel.tHigh).ToString());

         this.SendCommand("SET", lData.ToArray());
      }

      private void OnDurationEnd(object sender, EventArgs e)
      {
         List<string> lData = new List<string>();

         lData.Add(Index.ToString("00"));
         lData.Add(((uint)tenOutputLevel.tLow).ToString());

         this.SendCommand("SET", lData.ToArray());
      }

      public override void WriteXml(System.Xml.XmlWriter writer)
      {
         base.WriteXml(writer);

         writer.WriteAttributeString("Duration", Duration_ms.ToString());
         writer.WriteAttributeString("Delay", Delay_ms.ToString());
      }
   }
}
