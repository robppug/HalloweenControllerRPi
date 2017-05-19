namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Channels
{
   /// <summary>
   /// UART Channel BaudRates
   /// </summary>
   public enum BaudRates : uint
   {
      Baud_1200bps = 1200,
      Baud_2400bps = 2400,
      Baud_4800bps = 4800,
      Baud_9600bps = 9600,
      Baud_19200bps = 19200,
      Baud_38400bps = 38400,
      Baud_56000bps = 56000
   }

   /// <summary>
   /// UART Channel Parity
   /// </summary>
   public enum Parity : uint
   {
      Parity_EVEN,
      Parity_MARK,
      Parity_NONE,
      Parity_ODD,
      Parity_SPACE
   }

   /// <summary>
   /// UART Channel StopBits
   /// </summary>
   public enum StopBits : uint
   {
      StopBits_NONE,
      StopBits_ONE,
      StopBits_ONEPOINTFIVE,
      StopBits_TWO,
   }

   public interface IUartChannelProvider
   {
      BaudRates BaudRate { get; set; }
      uint DataBits { get; set; }
      StopBits StopBits { get; set; }
      Parity Parity { get; set; }
   }
}