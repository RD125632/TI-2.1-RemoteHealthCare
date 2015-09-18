﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System.Diagnostics;

namespace FietsSimulator
{
    class FietsSimulator
    {
        private SerialPort comport;
        private Mode curmode; 
        private int _power, _heartbeat, rpm, speed, distance, energy;
        private Stopwatch stopwatch = new Stopwatch();
        private Random r = new Random();

        public int Power
        {
            get { return _power; }
            set
            {
                if (value >= 25 && value <= 400)
                    _power = value;
                if (value < 25)
                    _power = 25;
                if (value > 400)
                    _power = 400;
            }
        }

        public int Heartbeat
        {
           get { return r.Next(60, 160); }
        }


        private enum Mode
        {
            NONE,
            CONSOLE,
            DISTANCE,
            TIME
        }

        public FietsSimulator(String addr)
        {
            this.comport = new SerialPort(addr, 9600);
            comport.DataReceived += new SerialDataReceivedEventHandler(ReceiveData);
            comport.Open();
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;
            aTimer.Enabled = true;
        }

        private void ReceiveData(object sender, SerialDataReceivedEventArgs e)
        {
            string message = comport.ReadLine().Trim();
            Console.WriteLine(message);
            if (message == "RS")
            {
                curmode = Mode.NONE;

                Power = 25;
                SendData("ACK");
            }
            else if (message == "CU")
            {
                curmode = Mode.CONSOLE;
                SendData("ACK");
            }else if (message == "PD")
            {
                if (curmode == Mode.CONSOLE && message.Split().Length == 2)
                {
                    distance = Int32.Parse(message.Split(' ')[1]);
                    curmode = Mode.DISTANCE;
                    stopwatch.Start();
                    rpm = 100;
                    speed = 10;
                }
                else
                {
                    SendData("ERROR");
                }
            }
            else if (message.Contains("PW"))
            {
                if (curmode != Mode.NONE && message.Split().Length == 2)
                {
                    this.Power = Int32.Parse(message.Split(' ')[1]);
                }
                else
                {
                    SendData("ERROR");
                }
            }
            else if (message == "ST" && curmode != Mode.NONE)
            {
                SendStatus();
            }
            else
            {
                SendData("ERROR");
            }
        }

        private void SendData(string message)
        {
            this.comport.WriteLine(message);
        }
        private void SendStatus()
        {
            SendData(Heartbeat.ToString() + "\t"+rpm +"\t"+speed*10+"\t"+distance+"\t" + Power.ToString() + "\t600\t"+getTimeElapsed()+"\t200\r");
        }

        private string getTimeElapsed()
        {
            if(curmode == Mode.DISTANCE)
            {
                long seconds = (stopwatch.ElapsedMilliseconds / 1000) % 60;
                long minutes = ((stopwatch.ElapsedMilliseconds - seconds) / 1000) / 60;
                return minutes + ":" + seconds;
            }
            else
            {
                return "00:00";
            }
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
           if(curmode == Mode.DISTANCE)
            {
                distance -= 1;
            }
        }
    }
}
