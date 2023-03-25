using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Networking;
using shared_handler;

namespace ClientLib
{
    public class Client_Read
    {
        /*========================

           MESSAGE READING CLASS

        So... Format of messages:

        IP:PORT#PLAYERID;TIMEID;HEADER;MD5 --> ALL OF THIS IS MANDATORY APART FROM THE MD5 WHICH IS ONLY FOR IMPORTANT MESSAGES
        Then we introduce a # to split into 3.

        #MSG1|MSG2|MSG3|MSG4... --> NON MANDATORY PART. ACTUAL CONTENT OF MESSAGE


        So the -t-h-r-e-e- fourt parts now because of ACKs and MD5

        ===============================================
            IP:PORT                 --> Address
            PLAYERID;TIMEID;HEADER;  --> Identifiers
            MSG1|MSG2|MSG3|MSG4...  --> Message 
            ACK_ID|MD5              --> Confirmations
        ===============================================



        =========================*/

        //RECEIVED INFO VARIABLES
        int rc_Port;
        string rc_Tag;
        int rc_TimeID;
        IPAddress rc_IP;

        //CLIENT INFO VARIABLES
        UDP_Networking _connection;
        UDP_Client main;
        Handler _handler;

        //MESSAGE INFO
        string content;
        private string[] Message;
        bool send_Confirm = false;


        /*=======================
         SETUP SETUP SETUP SETUP
        =======================*/
        public void Setup(string msg, UDP_Networking networking, Handler handler, UDP_Client master)
        {
            main = master;
            content = msg;
            _connection = networking;
            _handler = handler;
            Parse();
        }

        /*=======================
         PARSE PARSE PARSE PARSE
        =======================*/

        private void Parse()
        {
            try
            {
                // Parse --> IP:PORT, Identifier and Message
                string[] Parts = content.Split(main.separator_parts);

                // Parse --> IP:PORT
                string[] Address = Parts[0].Split(":");
                rc_IP = IPAddress.Parse(Address[0]);
                rc_Port = int.Parse(Address[1]);

                // Parse --> Identifier PLAYERID;TIMEID;HEADER
                string[] Identifier = Parts[1].Split(main.separator_identifier);
                rc_Tag = Identifier[0];
                rc_TimeID = int.Parse(Identifier[1]);
                int header = int.Parse(Identifier[2]);

                if (Parts.Length > 3)
                {
                    string[] Confirmations = Parts[3].Split(main.separator_identifier);
                    if (Confirmations[0] == "1")
                    {
                        send_Confirm = true;
                    }
                }

                if (send_Confirm)
                {
                    SendACK();
                }

                //CHECK TIME ID
                if (main.LastIDs[header] > rc_TimeID)
                {
                    if (header != 1)
                    {
                        return;
                    }
                }
                main.LastIDs[header] = rc_TimeID;

                // Parse --> Message
                Message = Parts[2].Split(main.separator_message);

                // Check header
                switch (header)
                {
                    case 0: //Client joining
                        ClientInfoReceived();
                        break;
                    case 1: //Confirmation received
                        ConfirmationReceived();
                        break;
                }

            }
            catch (Exception e)
            {
                Debug.Log("Couldn't parse message:" + e);
            }
        }


        /*==============================================
        
                MESSAGE SENDER

        ===============================================*/

        public void Reply(string header, bool ACK = false, params string[] parts)
        {
            main.TimeID++;

            // LAYERID; TIMEID; HEADER-- > Identifiers
            string Identifier = _handler.CompileList(main.separator_identifier, main.cl_Tag, main.TimeID.ToString(), header);

            // MSG1|MSG2|MSG3|MSG4...  --> Message 
            string Message = _handler.CompileList(main.separator_message, parts);

            //Identifier = _handler.CompileList(separator_identifier, main.cl_Tag, main.TimeID.ToString(), header);
            
            //Joins parts together.
            string ToSend = Identifier + main.separator_parts + Message;

            if(ACK)
            {
                string Confirmations = _handler.CompileList(main.separator_identifier, "1");
                ToSend = ToSend + main.separator_parts + Confirmations;

                ConfirmationRequest newRequest = new ConfirmationRequest();
                newRequest.SetupRequest(ToSend, main.TimeID);
                main.AwaitingResponses.Add(newRequest);
            }

            Debug.Log(ToSend);
            _connection.Send(ToSend, rc_IP, rc_Port);
        }

        public void SendACK()
        {
            string Message = _handler.CompileList(main.separator_message, "ACK");
            string Identifier = _handler.CompileList(main.separator_identifier, main.cl_Tag, rc_TimeID.ToString(), "1");

            string ToSend = Identifier + main.separator_parts + Message;

            Debug.Log(ToSend);
            _connection.Send(ToSend, rc_IP, rc_Port);
        }


        /*=======================
         CASES CASES CASES CASES
        =======================*/

        /*-------------------------
          JOIN INTRO JOIN INTRO
        -------------------------*/

        private void ClientInfoReceived()
        {
            Client_JsonFormat receivedJSON = JsonUtility.FromJson<Client_JsonFormat>(Message[0]); //Received JSON is turned into a normal instance.

            foreach(Client_Instance Instance in main.server_Clients)
            {
                if(Instance.Tag == receivedJSON.Tag) //No doubles please :>
                {
                    return;
                }
                Client_Instance newInstance = new Client_Instance();
                newInstance.Setup_Client(receivedJSON.Tag);
                main.server_Clients.Add(newInstance);
            }
        }

        /*-------------------------
          CONFIRMATION RECEIVED
        -------------------------*/
        
        private void ConfirmationReceived()
        {
            List<ConfirmationRequest> ToRemove = new List<ConfirmationRequest>();
            foreach (ConfirmationRequest Request in main.AwaitingResponses)
            {
                if (Request.TimeID == rc_TimeID)
                {
                    ToRemove.Add(Request);
                }
            }

            while (ToRemove.Count > 0)
            {
                main.AwaitingResponses.Remove(ToRemove[0]);
                ToRemove.Remove(ToRemove[0]);
            }
        }
    }

    public class Client_Instance
    {
        public string Tag;
        
        public void Setup_Client(string tag)
        {
            Tag = tag;
        }
    }
}

