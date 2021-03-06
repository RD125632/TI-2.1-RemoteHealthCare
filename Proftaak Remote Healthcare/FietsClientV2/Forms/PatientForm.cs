﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FietsClient
{
    public partial class PatientForm : Form
    {
        private TcpConnection _connection;
        private PatientModel patienModel;
        
        public PatientForm(TcpConnection connection)
        {
            this._connection = connection;
            InitializeComponent();
            patienModel = PatientModel.patientModel;
            patienModel.patientform = this;
            DataHandler.IncomingErrorEvent += HandleError; //initialize event

            _connection.IncomingChatmessageEvent += new TcpConnection.ChatmassegeDelegate(printMessage);

            //TIJDELIJK STUK CODE OM MESSAGE TE TESTEN
            _connection.SendString("6|TOM|TOM|Je bent een homo");
            Console.WriteLine("Bericht versturen");
            //EINDE TESTCODE
        }

        private void HandleError(string error)
        {
            switch (error)
            {
                case "WrongComPort":
                    toolStripComboBox1.Text = "";
                    MessageBox.Show("ERROR: Comport not initialized... trying to close the comport", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "NotConnectedToBike":
                    MessageBox.Show("ERROR: Not connected to bike.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                default:
                    break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            toolStripComboBox1.Items.AddRange(ports);
        }

        private void requestDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            patienModel.startAskingData();
        }

        private void closePortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            patienModel.closeComPort();
        }

        private void openPortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            patienModel.startComPort(toolStripComboBox1.SelectedItem.ToString());
            requestDataToolStripMenuItem.Enabled = true;
            closePortToolStripMenuItem.Enabled = true;
        }

        private void confirmDistanceBox_Click(object sender, EventArgs e)
        {
            int n;
            if (int.TryParse(distanceBox.Text, out n))
            {
                patienModel.setDistanceMode(distanceBox.Text);
            }
            else
            {
                MessageBox.Show("Distance is not a valid number.");
            }
        }

        private void confirmTimeBox_Click(object sender, EventArgs e)
        {
            int minutes, seconds;
            bool isNumericS = int.TryParse(minuteBox.Text, out minutes);
            bool isNumericM = int.TryParse(secondBox.Text, out seconds);

            if (isNumericM)
            {
                if (isNumericS)
                    patienModel.setTimeMode(minutes + ":" + seconds);
                else MessageBox.Show("Minutes is not a valid number.");
            }
            else MessageBox.Show("Seconds is not a valid number.");
        }

        private void stopTrainingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            patienModel.reset();
        }

        private void setPower_Click(object sender, EventArgs e)
        {
            int n;
            if (int.TryParse(powerBox.Text, out n))
                patienModel.setPower(powerBox.Text);
            else
                MessageBox.Show("Power is not a valid number.");
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if(messageBox.Text != null)
            {
                String[] data = new String[2];
                data[0] = messageBox.Text;
                data[1] = _connection.currentData.GetUserID();
                messageBox.Clear();

                _connection.SendChatMessage(data);
            }
        }

        private void messageBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                sendButton_Click(sender, e);
            }
            
        }

        private void printMessage(string[] data)
        {

            string finalMessage = data[1] + ":\t\t" + data[3] + "\r\n";
            chatBox.AppendText(finalMessage);
        }

        
    }
}
