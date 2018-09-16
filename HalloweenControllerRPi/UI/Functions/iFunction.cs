using HalloweenControllerRPi.Device;
using System;

namespace HalloweenControllerRPi.Functions
{
    /// <summary>
    /// Controller FUNCTION interface definition
    /// </summary>
    interface IFunction
    {
        Command FunctionKeyCommand { get; set; }

        uint Duration_ms { get; }
        uint MinDuration_ms { get; set; }
        uint MaxDuration_ms { get; set; }
        uint MinDelay_ms { get; set; }
        uint Index { get; set; }

        EventHandler evOnTrigger { get; set; }
        EventHandler evOnDelayEnd { get; set; }
        EventHandler evOnDurationEnd { get; set; }
        EventHandler evOnFunctionUpdated { get; set; }

        bool ProcessRequest(char cFunc, char subFunc, char cFuncIndex, uint u32FuncValue);
        bool CheckTriggerConditions(uint u32FuncValue);
    }
}