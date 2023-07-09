using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
using ServerLib;
using System.Globalization;
using System.Net;
using Defective;

namespace shared_handler
{
    public class ServerHandler
    {

        UDP_Server main;
        BackgroundRun back;





        public void SetupHandler(UDP_Server Server, BackgroundRun Background)
        {
            main = Server;
            back = Background;
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
            foreach(Server_Client cl in back.server_Clients) //Loop trough all connected clients
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
            foreach (Server_Client cl in back.server_Clients) //Loop trough all connected clients
            {
                if (cl.IP == search_IP && cl.Port == search_Port) //Address matches.
                {
                    Console.Write(cl.IP + "-" + search_IP + " " + cl.Port + "-" + search_Port);
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
                Console.Write("Error at handler_CompileList " + e);
                return null;
            }
        }

        public string CompileClient(Server_Client client, bool Player = false)
        {
            if(Player) //THE PLAYER REPLYING TO
            {
                Defective.JSON.JSONObject PlayerJson = new Defective.JSON.JSONObject();
                PlayerJson.AddField("tag", client.Tag);
                return PlayerJson.ToString();
            }
            else //OTHER PLAYER
            { 
                Defective.JSON.JSONObject clientJSON = new Defective.JSON.JSONObject();
                clientJSON.AddField("Tag", client.Tag);
                return clientJSON.ToString();
            }
        }

        public string CompileUpdatePack(Server_Client client)
        {
            Defective.JSON.JSONObject UpdateJson = new Defective.JSON.JSONObject();
            UpdateJson.AddField("tag", client.Tag);
            UpdateJson.AddField("pos", StringVector(client.Pos));

            return UpdateJson.ToString();
        }

        public string StringVector(float[,] toString)
        {
            return ("(" + toString[0, 0].ToString() + "o" + toString[0, 1].ToString() + ")").Replace(",", ".");
        }

        public List<T> ListFromArray<T>(T[] array)
        {
            List<T> ToReturn = new List<T>();
            for(int i = 0; i < array.Length; i++)
            {
                ToReturn.Add(array[i]);
            }
            return ToReturn;
        }

        public string StringFromArray<T>(T[] list, Type type)
        {
            string result = "";

            for(int i = 0; i < list.Length; i++)
            {
                result = result + "," + list[i];
            }

            return result;
        }

        public float[,] ExtractVector(string input)
        {
            string result = input.Replace("(", "");
            result = result.Replace(")", "");
            string[] two = result.Split('o');
            float[,] Final = new float[1, 2] { { float.Parse(two[0], CultureInfo.InvariantCulture.NumberFormat), float.Parse(two[1], CultureInfo.InvariantCulture.NumberFormat) } };
            return Final;
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
}