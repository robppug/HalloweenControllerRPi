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
      public enum OutputLevel
      {
         tLow = 0,
         tHigh = 1
      };

      public Func_RELAY() { }

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

         lData.Add(Index.ToString());
         lData.Add("1");

         this.SendCommand("SET", lData.ToArray());
      }

      private void OnDurationEnd(object sender, EventArgs e)
      {
         List<string> lData = new List<string>();

         lData.Add(Index.ToString());
         lData.Add("0");

         this.SendCommand("SET", lData.ToArray());
      }

      public override void WriteXml(System.Xml.XmlWriter writer)
      {
         base.WriteXml(writer);

         writer.WriteAttributeString("Duration", Duration_ms.ToString());
         writer.WriteAttributeString("Delay", Delay_ms.ToString());
      }

      public override List<char> SerializeSequence()
      {

         /* Create the serialised data:   
          *    "R (type) (index) (duration) (delay)" */
         this.Data.AddRange("R" + ' ' + (char)((int)this.Type + 0x30) + ' ' + (char)(this.Index + 0x30) + ' ' + Duration_ms.ToString() + ' ' + Delay_ms.ToString());

         return this.Data;
      }
   }
}
