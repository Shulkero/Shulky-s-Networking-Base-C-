using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using System.Net;
using System;
using shared_handler;
using ClientLib;

public class UDP_Client : MonoBehaviour
{
    [SerializeField] private int sv_Port;
    [SerializeField] private string serverIP;
    private IPAddress sv_IP;


    /*========================
        CLIENT VARIABLES
    ========================*/
    private UDP_Networking _connection;
    [SerializeField] private int client_Port;
    public string cl_Tag;
    public int TimeID;

    //SEPARATOR CHARACTERS
    public char separator_parts = '#';
    public char separator_identifier = ';';
    public char separator_message = ';';

    //AwaitingRequests Part
    public List<ConfirmationRequest> AwaitingResponses;

    //Handler
    private Handler _handler;

    /*========================
       SERVER VARIABLES
   ========================*/

    public List<int> LastIDs;

    //TIMER VARIABLES
    private int TickDown = 50;

    /*========================
       OTHER CLIENT VARIABLES
   ========================*/
    public List<Client_Instance> server_Clients;

    /*=======================?
        SETUP SETUP SETUP
    ========================*/
    private void Awake()
    {
        //SET FRAMERATE TO 60fps SO THAT GAME DOESNT BURN MY PC.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        _handler = new Handler();
        sv_IP = IPAddress.Parse(serverIP);
        server_Clients = new List<Client_Instance>();

        //Setup LastIDs of Server.
        LastIDs = new List<int>();
        for(int i = 0; i < 5; i++)
        {
            LastIDs.Add(-1);
        }

        //Setup Awaiting Responses
        AwaitingResponses = new List<ConfirmationRequest>();
    }

    private void Start()
    {
        connection_create();
    }

    private void connection_create()
    {
        _connection = new UDP_Networking(); //Creates new connection
        _connection.Main(client_Port); //Starts listening in client_Port port

        Send("0", true, "Join");
    }

    /*=======================
     READING READING READING
    =======================*/

    private void FixedUpdate()
    {
        read();
        TickAwaiting();
    }

    private void read()
    {
        try
        {
            if (_connection.read_msg() > 0) //Checks if there are new MSGs
            {
                for (int i = 0; i < _connection.read_msg(); i++) //Loops trough all the messages
                {
                    string msg = _connection.lee_diccionario();
                    Client_Read newRead = new Client_Read();
                    newRead.Setup(msg, _connection, _handler, this);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Server couldn't read message:" + e);
        }
    }

    /*=======================
     SEND SEND SEND SEND SEND
    =======================*/

    public void Send(string header, bool ACK = false, params string[] parts)
    {
        TimeID++;

        // LAYERID; TIMEID; HEADER-- > Identifiers
        string Identifier = _handler.CompileList(separator_identifier, cl_Tag, TimeID.ToString(), header);

        // MSG1|MSG2|MSG3|MSG4...  --> Message 
        string Message = _handler.CompileList(separator_message, parts);

        //Identifier = _handler.CompileList(separator_identifier, main.cl_Tag, main.TimeID.ToString(), header);

        //Joins parts together.
        string ToSend = Identifier + separator_parts + Message;

        if (ACK)
        {
            ConfirmationRequest newRequest = new ConfirmationRequest();

            string Confirmation = _handler.CompileList(separator_identifier, "1");
            ToSend = ToSend + separator_parts + Confirmation;

            newRequest.SetupRequest(ToSend, TimeID);
            AwaitingResponses.Add(newRequest);
        }

        print(ToSend);
        _connection.Send(ToSend, sv_IP, sv_Port);
    }

    public void ResendMessage(string ToSend)
    {
        print(ToSend + " RESENT");
        _connection.Send(ToSend, sv_IP, sv_Port);
    }

    /*==============================
     REFRESH AWAITING CONFIRMATIONS
    ==============================*/

    private void TickAwaiting()
    {
        TickDown--;
        if(TickDown <= 0)
        {
            TickDown = 50;

            if(AwaitingResponses.Count == 0)
            {
                return;
            }

            List<ConfirmationRequest> ToRemove = new List<ConfirmationRequest>();
            foreach(ConfirmationRequest Request in AwaitingResponses)
            {
                Request.AmountsTicked++;

                if(Request.AmountsTicked > 10)
                {
                    ToRemove.Add(Request);
                }
                else
                {
                    ResendMessage(Request.Message);
                }
            }

            while(ToRemove.Count > 0)
            {
                AwaitingResponses.Remove(ToRemove[0]);
                ToRemove.Remove(ToRemove[0]);
            }
        }
    }
}
