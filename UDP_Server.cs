using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using System.Net;
using System;
using ServerLib;
using shared_handler;

public class UDP_Server : MonoBehaviour
{
    /*=======================
        SERVER VARIABLES
    =======================*/
    private UDP_Networking _server;
    [SerializeField] private int server_Port;

    //TIME IDS
    public int TimeID;
    private int TickDown = 50;

    //CLIENT LIST
    public List<Server_Client> server_Clients;

    //HANDLER
    private Handler _handler;



    private void Awake()
    {
        //SET FRAMERATE TO 60fps SO THAT GAME DOESNT BURN MY PC.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        //INIT CLIENT LIST
        server_Clients = new List<Server_Client>();

        //START SERVER
        _handler = new Handler();
        _handler.SetupHandler(this);

        server_create();
    }

    private void server_create()
    {
        _server = new UDP_Networking(); //Creates new connection
        _server.Main(server_Port); //Starts listening in client_Port port
    }

    private void FixedUpdate()
    {
        read();
        TickAwaiting();
        //CInstances_SendPos();
    }


    /*=============================
    
       GENERIC SERVER FUNCTIONS

    =============================*/
    private void read()
    {
        try
        {
            if (_server.read_msg() > 0) //Checks if there are new MSGs
            {
                for (int i = 0; i < _server.read_msg(); i++) //Loops trough all the messages
                {
                    string msg = _server.lee_diccionario();
                    Server_Read newRead = new Server_Read();
                    newRead.Setup(msg, _server, _handler, this, server_Clients);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Server couldn't read message:" + e);
        }
    }

    /*==============================
      RESEND RESEND RESEND RESEND
    ==============================*/

    public void ResendMessage(string ToSend, Server_Client client)
    {
        print(ToSend);
        _server.Send(ToSend, client.IP, client.Port);
    }


    /*==============================
     REFRESH AWAITING CONFIRMATIONS
    ==============================*/

    private void TickAwaiting()
    {
        TickDown--;
        if (TickDown <= 0)
        {
            TickDown = 50;

            if(server_Clients.Count != 0)
            {
                foreach (Server_Client cl in server_Clients)
                {
                    List<ConfirmationRequest> ToRemove = new List<ConfirmationRequest>();
                    if (cl.AwaitingResponses.Count != 0)
                    {
                        foreach (ConfirmationRequest Request in cl.AwaitingResponses)
                        {
                            Request.AmountsTicked++;

                            if (Request.AmountsTicked > 10)
                            {
                                ToRemove.Add(Request);
                            }
                            else
                            {
                                ResendMessage(Request.Message, cl);
                            }
                        }
                    }

                    while (ToRemove.Count > 0)
                    {
                        cl.AwaitingResponses.Remove(ToRemove[0]);
                        ToRemove.Remove(ToRemove[0]);
                    }
                }
            } 
        }
    }
}