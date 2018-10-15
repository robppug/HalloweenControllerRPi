using HalloweenControllerRPi.Device;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace HalloweenControllerRPi.Functions
{
    public class Func_SOUND : Function, IXmlFunction
    {
        private uint _volume;

        private DispatcherTimer _pollTimer;

        #region Parameters
        public uint AvailableTracks { get; set; } = 0;

        public uint Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                SendCommand("VOLUME", Volume);
            }
        }

        public uint Track { get; set; } = 0;

        public List<int> RandomTracks { get; set; } = new List<int>();

        public bool Loop { get; set; } = false;

        public bool Randomise { get; set; } = false;

        public int Status { get; private set; } = 0;

        #endregion

        public Func_SOUND() { }

        public Func_SOUND(IHostApp host, tenTYPE entype) : base(host, entype)
        {
            FunctionKeyCommand = new Command("SOUND", 'S');

            evOnDelayEnd += OnTrigger;
            evOnDurationEnd += OnDurationEnd;
        }

        internal void Initialise()
        {
            SendCommand("AVAILABLE TRACKS");

            _pollTimer = new DispatcherTimer();
            _pollTimer.Interval = TimeSpan.FromSeconds(10);
            _pollTimer.Tick += PollTimer_Tick;
            _pollTimer.Start();
        }


        private void PollTimer_Tick(object sender, object e)
        {
            SendCommand("GETSTATUS");

            System.Diagnostics.Debug.WriteLine("Sound Status: " + Status.ToString());
        }

        /// <summary>
        /// This does the actual processing of the onTrigger EVENT.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrigger(object sender, EventArgs e)
        {
            uint track = Track;

            GetRandomTrack(ref track);

            SendCommand("TRACK", track);
            SendCommand("VOLUME", Volume);
            SendCommand("LOOP", (Loop ? 1 : 0));
            SendCommand("PLAY");
        }

        private void GetRandomTrack(ref uint track)
        {
            if ((Randomise == true) && (RandomTracks.Count > 0))
            {
                track = (uint)RandomTracks[new Random().Next(0, RandomTracks.Count - 1)];
            }
        }

        private void OnDurationEnd(object sender, EventArgs e)
        {

            if (Type == tenTYPE.TYPE_TRIGGER)
            {
                //If duration is 0, just play the sound to end.
                if (Duration_ms > 0)
                {
                    SendCommand("STOP");
                }
            }
            else
            {
                SendCommand("STOP");
            }
        }

        public override void ReadXML(XElement element)
        {
            base.ReadXML(element);

            Volume = Convert.ToUInt32(element.Attribute("Volume").Value);
            Track = Convert.ToUInt32(element.Attribute("Track").Value);
            Loop = Convert.ToBoolean(element.Attribute("Loop").Value);
            Randomise = Convert.ToBoolean(element.Attribute("Randomise").Value);

            if (Randomise == true)
            {
                RandomTracks.Clear();

                foreach (var elem in element.Descendants())
                {
                    if (elem.Name.LocalName == "RandomTracks")
                    {
                        RandomTracks.AddRange(elem.Attributes().Select((x => Convert.ToInt32(x.Value))).ToList());
                    }
                }
            }

            evOnFunctionUpdated?.Invoke(this, EventArgs.Empty);
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteAttributeString("Volume", Volume.ToString());
            writer.WriteAttributeString("Track", Track.ToString());
            writer.WriteAttributeString("Loop", Loop.ToString());
            writer.WriteAttributeString("Randomise", Randomise.ToString());
            writer.WriteStartElement("RandomTracks");
            foreach (int t in RandomTracks)
            {
                writer.WriteAttributeString("Track" + t.ToString(), t.ToString());
            }
            writer.WriteEndElement();
        }

        public override bool ProcessRequest(char cFunc, char subFunc, char cFuncIndex, uint u32FuncValue)
        {
            if (cFunc == (char)0)
                return base.ProcessRequest(cFunc, subFunc, cFuncIndex, u32FuncValue);
            else
            {
                if (cFuncIndex == Index)
                {
                    switch (subFunc)
                    {
                        case 'I':
                            return base.ProcessRequest(cFunc, subFunc, cFuncIndex, u32FuncValue);

                        case 'A':
                            AvailableTracks = u32FuncValue;
                            evOnFunctionUpdated?.Invoke(this, EventArgs.Empty);
                            return true;

                        case 'F':
                            if (Loop == true)
                            {
                                OnTrigger(this, EventArgs.Empty);
                                return true;
                            }
                            break;

                        case 'C':
                            int lastStatus = Status;

                            Status = (int)(u32FuncValue & 0xFF);

                            // Was playing but now it's stopped
                            if ( (lastStatus == 1) && (Status == 0) )
                            {
                                if (Loop == true)
                                { 
                                    OnTrigger(this, EventArgs.Empty);
                                    return true;
                                }
                            }
                            
                            break;

                        default:
                            break;
                    }
                }
            }

            return false;
        }
    }
}
