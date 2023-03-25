using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServerLib;
using System.Net;

namespace shared_handler
{
    public class Handler
    {

        UDP_Server main;

        public void SetupHandler(UDP_Server Server)
        {
            main = Server;
        }

        /*===============================
                    HANDLERS

           handler_FindCInstance(string address, int port) 
                --> tries to find a client that matches. 
                returns null or the client instance.

           handler_CompileList(char separator, params string[] strings)
                --> Joins all the strings in strings and adds a separator in between
                each of the values.

        ===============================*/

        /*private server_CInstance handler_FindCInstance(string msg_addr, int msg_port, string username = null)
        {
            try
            {
                for (int i = 0; i < server_CInstances.Count; i++)
                {
                    if (server_CInstances[i].CInstance_Addr == msg_addr && server_CInstances[i].CInstance_Port == msg_port)
                    {
                        if (username != null)
                        {
                            if (server_CInstances[i].CInstance_Username == username)
                            {
                                return server_CInstances[i];
                            }
                        }
                        else
                        {
                            return server_CInstances[i];
                        }
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Debug.Log("Error in handler_FindCInstance" + e);
                return null;
            }
        }*/


        public Server_Client ClientByTag(string search_Tag)
        {
            foreach(Server_Client cl in main.server_Clients) //Loop trough all connected clients
            {
                if (cl.Tag == search_Tag) //Tag match
                {
                    
                    return cl;
                }
            }
            return null;
        }

        public Server_Client ClientByAddr(int search_Port, IPAddress search_IP)
        {
            foreach (Server_Client cl in main.server_Clients) //Loop trough all connected clients
            {
                if (cl.IP == search_IP && cl.Port == search_Port) //Address matches.
                {
                    Debug.Log(cl.IP + "-" + search_IP + " " + cl.Port + "-" + search_Port);
                    return cl;
                }
            }
            return null;
        }

        public string CompileList(char separator, params string[] strings)
        {
            try
            {
                string toReturn = "";
                for (int i = 0; i < strings.Length; i++)
                {
                    if(i == strings.Length-1)
                    {
                        toReturn = toReturn + strings[i];
                    }
                    else
                    {
                        toReturn = toReturn + strings[i] + separator;
                    }
                }
                return toReturn;
            }
            catch (Exception e)
            {
                Debug.Log("Error at handler_CompileList " + e);
                return null;
            }
        }

        public string CompileClient(Server_Client client)
        {
            Client_JsonFormat newJson = new Client_JsonFormat();
            newJson.Tag = client.Tag;
            string CompiledClient = JsonUtility.ToJson(newJson);
            return CompiledClient;
        }


    }

    public class ConfirmationRequest
    {
        public string Message;
        public int TimeID;
        public int AmountsTicked;

        public void SetupRequest(string msg, int id)
        {
            Message = msg;
            TimeID = id;
        }
    }

    public class Client_JsonFormat
    {
        public string Tag;
    }
}