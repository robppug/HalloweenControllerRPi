using HalloweenControllerRPi.Attributes;
using HalloweenControllerRPi.Device.Controllers.BusDevices;
using HalloweenControllerRPi.Device.Controllers.Providers;
using HalloweenControllerRPi.Extentions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.Drivers
{

    public enum Commands
    {
        [AccessType("W"), RxLength(10)]
        NEXT_SONG = 0X01,  // Play next song.
        [AccessType("W"), RxLength(10)]
        PREV_SONG = 0X02,  // Play previous song.
        [AccessType("W"), RxLength(10)]
        PLAY_W_INDEX = 0X03,
        [AccessType("W"), RxLength(10)]
        VOLUME_UP = 0X04,
        [AccessType("W"), RxLength(10)]
        VOLUME_DOWN = 0X05,
        [AccessType("W"), RxLength(10)]
        SET_VOLUME = 0X06,
        [AccessType("W"), RxLength(10)]
        SNG_CYCL_PLAY = 0X08,  // Single Cycle Play.
        [AccessType("W"), RxLength(10)]
        SEL_DEV = 0X09,
        [AccessType("W"), RxLength(10)]
        SLEEP_MODE = 0X0A,
        [AccessType("W"), RxLength(10)]
        WAKE_UP = 0X0B,
        [AccessType("W"), RxLength(10)]
        RESET = 0X0C,
        [AccessType("W"), RxLength(10)]
        PLAY = 0X0D,
        [AccessType("W"), RxLength(10)]
        PAUSE = 0X0E,
        [AccessType("W"), RxLength(10)]
        PLAY_FOLDER_FILE = 0X0F,
        [AccessType("W"), RxLength(10)]
        STOP_PLAY = 0X16,
        [AccessType("W"), RxLength(10)]
        FOLDER_CYCLE = 0X17,
        [AccessType("W"), RxLength(10)]
        SHUFFLE_PLAY = 0x18,
        [AccessType("W"), RxLength(10)]
        SET_SNGL_CYCL = 0X19, // Set single cycle.
        [AccessType("W"), RxLength(10)]
        SET_DAC = 0X1A,
        [AccessType("W"), RxLength(10)]
        PLAY_W_VOL = 0X22,
        [AccessType("R"), RxLength(10)]
        TRACK_FINISH_TF = 0X3D,
        [AccessType("R"), RxLength(10)]
        QUERY_CURRENT_STATUS = 0X42,
        [AccessType("R"), RxLength(10)]
        QUERY_CURRENT_VOLUME = 0X43,
        [AccessType("R"), RxLength(10)]
        QUERY_CURRENT_EQ = 0X44,
        [AccessType("R"), RxLength(10)]
        QUERY_TOTAL_TRACKS_TF = 0X48,
        [AccessType("R"), RxLength(10)]
        QUERY_CURRENT_TRACKS_TF = 0X4C,
        [AccessType("R"), RxLength(1)]
        COMMAND_UNKNOWN = 0X62,

    }

    /// <summary>
    /// Register ENUM extension class
    /// </summary>
    internal static class CommandEnumExtensions
    {
        public static bool CanRead(this Commands cmd)
        {
            if (cmd < Commands.COMMAND_UNKNOWN)
            {
                AccessTypeAttribute mde = EnumExtension<AccessTypeAttribute, Commands>.GetModeAttribute(cmd);
                if (mde != null)
                    return mde.Read;
                else
                    return true;
            }

            return false;
        }

        public static bool CanWrite(this Commands cmd)
        {
            if (cmd < Commands.COMMAND_UNKNOWN)
            {
                AccessTypeAttribute mde = EnumExtension<AccessTypeAttribute, Commands>.GetModeAttribute(cmd);
                if (mde != null)
                    return mde.Write;
                else
                    return true;
            }

            return false;
        }

        public static int GetExpectedRxLength(this Commands cmd)
        {
            if (cmd < Commands.COMMAND_UNKNOWN)
            {
                RxLengthAttribute mde = EnumExtension<RxLengthAttribute, Commands>.GetModeAttribute(cmd);

                if (mde != null)
                    return mde.ExpectedLength;
                else
                    return 0;
            }

            return 0;
        }
    }

    internal class CataLexCommand
    {
        public Commands Command;
        public ushort Data;
    }

    public class Catalex_YX5300<T> : ISoundProvider, IDeviceCommsProvider<T> where T : IDeviceComms
    {
        private bool _initialised = false;
        private List<byte> _rxBuffer = new List<byte>();
        private const byte MAX_VOLUME = 30;
        private const byte COMMAND_START_BYTE = 0x7E;
        private const byte COMMAND_END_BYTE = 0xEF;

        public event EventHandler<SoundProviderEventArgs> StateChanged;
        public event EventHandler<SoundProviderStatusEventArgs> StatusChanged;

        public T BusDeviceComms { get; private set; }

        private static void BuildCommand(ref List<byte> buffer, Commands command, bool feedback, ushort dat = 0)
        {
            if (buffer != null)
            {
                buffer.Add(COMMAND_START_BYTE);
                buffer.Add(0xFF);
                buffer.Add(0x06);   // Len
                buffer.Add((byte)command);
                buffer.Add((byte)(feedback ? 0x01 : 0x00));   // 0x00 NO, 0x01 feedback
                buffer.Add((byte)(dat >> 8));  //datah
                buffer.Add((byte)(dat));       //datal
                buffer.Add(COMMAND_END_BYTE);
            }
        }

        private static List<CataLexCommand> DecodeCommand(byte[] buffer)
        {
            bool boValidCommand = false;
            List<byte> detectedCommand = new List<byte>();
            List<CataLexCommand> decodedCommands = new List<CataLexCommand>();

            for (int b = 0; b < buffer.Length; b++)
            {
                if (boValidCommand)
                {
                    detectedCommand.Add(buffer[b]);

                    if (buffer[b] == COMMAND_END_BYTE)
                    {
                        //Send event to inform a full command has been received.
                        boValidCommand = false;

                        CataLexCommand result = new CataLexCommand()
                        {
                            Command = (Commands)detectedCommand[3],
                            Data = (ushort)((detectedCommand[5] << 8) | detectedCommand[6])
                        };

                        if (result.Command.GetExpectedRxLength() != 0)
                        {
                            decodedCommands.Add(result);
                        }
                    }
                }
                else
                {
                    if (buffer[b] == COMMAND_START_BYTE)
                    {
                        detectedCommand.Add(buffer[b]);
                        boValidCommand = true;
                    }
                }
            }


            return decodedCommands;
        }

        private static List<CataLexCommand> DecodeCommand(List<byte> buffer)
        {
            return DecodeCommand(buffer.ToArray());
        }

        public Catalex_YX5300()
        {
            _initialised = false;
        }

        public void InitialiseDriver(bool proceedOnFail = false)
        {
            List<byte> _dataBuffer = new List<byte>();

            if (_initialised == true)
            {
                BusDeviceComms.DataReceived += _stream_DataReceived;
                BuildCommand(ref _dataBuffer, Commands.SEL_DEV, false, 0x02);

                BusDeviceComms.WriteRead(_dataBuffer.ToArray());

                Task.Delay(200).Wait();
            }
            else
            {
                throw new Exception("Driver needs to be Opened first.");
            }
        }

        private void _stream_DataReceived(object sender, DeviceCommsEventArgs e)
        {
            _rxBuffer.AddRange(e.Data);

            List<CataLexCommand> commands = DecodeCommand(_rxBuffer);

            foreach (CataLexCommand cmd in commands)
            {
                switch (cmd.Command)
                {
                    case Commands.TRACK_FINISH_TF:
                        StateChanged?.Invoke(this, new SoundProviderEventArgs(SoundProviderEventArgs.State.SoundFinished));
                        break;

                    case Commands.QUERY_CURRENT_STATUS:
                        StatusChanged?.Invoke(this, new SoundProviderStatusEventArgs(cmd.Data));
                        break;

                    default:
                        break;
                }

                //Remove the processed command from the RX BUFFER
                _rxBuffer.RemoveRange(0, cmd.Command.GetExpectedRxLength());
            }
        }

        public void Open(T stream)
        {
            if (_initialised == false)
            {
                BusDeviceComms = stream;
                _initialised = true;
            }
            else
            {
                throw new Exception("Driver (" + this + ") is already Open.");
            }
        }

        public void Close()
        {
            _initialised = false;
            BusDeviceComms = default(T);
        }

        public void Play(byte track, bool loop)
        {
            List<byte> _dataBuffer = new List<byte>();

            BuildCommand(ref _dataBuffer, Commands.PLAY_W_INDEX, false, (ushort)track);

            BusDeviceComms.WriteRead(_dataBuffer.ToArray());
        }

        public void Stop()
        {
            List<byte> _dataBuffer = new List<byte>();

            BuildCommand(ref _dataBuffer, Commands.STOP_PLAY, false);

            BusDeviceComms.Write(_dataBuffer.ToArray());
        }

        public void Status()
        {
            List<byte> _dataBuffer = new List<byte>();

            BuildCommand(ref _dataBuffer, Commands.QUERY_CURRENT_STATUS, false);

            BusDeviceComms.Write(_dataBuffer.ToArray());
        }

        public ushort GetNumberOfTracks()
        {
            List<byte> _dataBuffer = new List<byte>();
            List<byte> readData = new List<byte>();
            byte[] rxData;

            BuildCommand(ref _dataBuffer, Commands.QUERY_TOTAL_TRACKS_TF, false);

            rxData = BusDeviceComms.WriteRead(_dataBuffer.ToArray());

            List<CataLexCommand> commands = DecodeCommand(rxData);

            foreach (CataLexCommand cmd in commands)
            {
                if (cmd.Command == Commands.QUERY_TOTAL_TRACKS_TF)
                {
                    return cmd.Data;
                }
            }

            return 0;
        }

        public void Volume(byte vol)
        {
            List<byte> _dataBuffer = new List<byte>();

            vol /= (100 / MAX_VOLUME);

            if (vol < 0)
                vol = 0;
            else if (vol > MAX_VOLUME)
                vol = MAX_VOLUME;

            BuildCommand(ref _dataBuffer, Commands.SET_VOLUME, false, vol);

            BusDeviceComms.WriteRead(_dataBuffer.ToArray());
        }

        public void Next()
        {
            throw new NotImplementedException();
        }

        public void Previous()
        {
            throw new NotImplementedException();
        }
    }
}
