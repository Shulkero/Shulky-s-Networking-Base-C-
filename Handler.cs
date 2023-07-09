using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
using System.Globalization;
using System.Net;
using UnityEngine;

namespace shared_handler
{
    public class Handler
    {

        /*===============================
                    HANDLERS

           handler_FindCInstance(string address, int port) 
                --> tries to find a client that matches. 
                returns null or the client instance.

           handler_CompileList(char separator, params string[] strings)
                --> Joins all the strings in strings and adds a separator in between
                each of the values.

        ===============================*/

        public string StringVector(float[,] toString)
        {
            return ("(" + toString[0, 0].ToString() + "o" + toString[0, 1].ToString() + ")").Replace(",", ".");
        }

        public string StringVector(Vector2 toString)
        {
            float[,] converted = new float[1, 2] { { toString.x, toString.y } };
            return ("(" + converted[0, 0].ToString() + "o" + converted[0, 1].ToString() + ")").Replace(",", ".");
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
            result = result.Replace("\"", "");
            string[] two = result.Split("o");
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

    /*public class Client_JsonFormat
    {
        public string Tag;
    }

    public class Player_JsonFormat
    {
        public string Tag;
    }

    public class Client_UpdatePack
    {
        public string Tag;
       * public string Pos;
    }*/

    public class Client_Instance
    {
        public string Tag;
        public Vector2 Pos;

        public GameObject Instance;

        public void Setup_Client(string tag, GameObject Prefab)
        {
            Tag = tag;
            Pos = Vector2.zero;
            Instance = Prefab;
        }

        public void SetPos(float[,] Position, float[,] Direction)
        {
            Pos = new Vector2(Position[0, 0], Position[0, 1]);
        }
    }
}