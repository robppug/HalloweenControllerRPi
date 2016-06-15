using System;

namespace HalloweenControllerRPi.Functions
{
   /// <summary>
   /// Controller FUNCTION interface definition
   /// </summary>
   internal interface iFunction
   {
      //Command FunctionKeyCommand { get; set; }

      uint Duration_ms { get; set; }
      uint Delay_ms { get; set; }
      uint Index { get; set; }

      EventHandler evOnTrigger { get; set; }
      EventHandler evOnDelayEnd { get; set; }
      EventHandler evOnDurationEnd { get; set; }

      bool boProcessRequest(char cFunc, char cFuncIndex, uint u32FuncValue);

      bool boCheckTriggerConditions(uint u32FuncValue);
   }
}