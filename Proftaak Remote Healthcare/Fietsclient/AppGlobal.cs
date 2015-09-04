﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fietsclient
{
    public class AppGlobal
    {
        private static AppGlobal _instance;
        public static AppGlobal Instance
        {
            get { return _instance ?? (_instance = new AppGlobal()); }
        }

        private KettlerBikeComm _bikeComm;

        private AppGlobal()
        {
            
        }

        public void startComPort()
        {
            startComPort("COM5");
        }

        public void startComPort(string portname)
        {
            _bikeComm = new KettlerBikeComm(portname);
            _bikeComm.initComm();
        }

        
    }
}
