using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using System;
using System.Net;



public class server_CInstance
{ 
    public void Setup(string Addr, int Port, string Username, GameObject CInstance_Object)
    {
        CInstance_Username = Username;
        CInstance_Addr = Addr;
        CInstance_Port = Port;
        CInstance = CInstance_Object;
        CInstance.name = Username;
        CInstance.transform.position = new Vector3(0, 1, 0);
        CInstance.transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public GameObject CInstance;
    public string CInstance_Username;
    public string CInstance_Addr;
    public int CInstance_Port;
}

public class CInstance_JsonFormat
{
    public string CInstance_Username;
    public string CInstance_Pos;
    public string CInstance_Rot;
}

public class Server : MonoBehaviour
{
    /*=======================
        SERVER VARIABLES
    =======================*/
    private UDP_Networking _server;
    private int server_Port;

    [SerializeField] private int MovementSpeed;
    [SerializeField] private int JumpPower;

    [SerializeField] private bool debug_host;
    [SerializeField] GameObject CInstance_Prefab;

    private List<server_CInstance> server_CInstances;
    


    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        /*if (!Menu.RunHost)
        {
            this.enabled = false;
        }
        server_Port = Menu.Server_Port;*/

        if(debug_host)
        {
            server_create();
        }

        server_CInstances = new List<server_CInstance>();
    }

    private void FixedUpdate()
    {
        connect_read();
        CInstances_SendPos();
    }

    private void server_create()
    {
        _server = new UDP_Networking(); //Creates new connection
        _server.Main(server_Port); //Starts listening in client_Port port
    }

    private void OnApplicationQuit()
    {
        _server.Stop();
    }

    private void SendMsg(string header, List<string> msg_Parts, string client_IP, int client_Port)
    {
        string msg = "";

        for (int i = 0; i < msg_Parts.Count; i++)
        {
            if (i != 0)
            {
                msg = msg + ";";
            }
            msg = msg + msg_Parts[i];
        }
        msg = header + ";" + msg;
        _server.Send(msg, IPAddress.Parse(client_IP), client_Port);
    }

    private void SendAll(string header, List<string> msg_Parts, server_CInstance Except)
    {
        string msg = "";

        for (int i = 0; i < msg_Parts.Count; i++)
        {
            if (i != 0)
            {
                msg = msg + ";";
            }
            msg = msg + msg_Parts[i];
        }
        msg = header + ";" + msg;

        for(int i = 0; i < server_CInstances.Count; i ++)
        {
            if(server_CInstances[i] != Except)
            {
                _server.Send(msg, IPAddress.Parse(server_CInstances[i].CInstance_Addr), server_CInstances[i].CInstance_Port);
            }
        }
    }

    private void connect_read()
    {
        try
        {
            if (_server.read_msg() > 0) //Checks if there are new MSGs
            {
                for (int i = 0; i < _server.read_msg(); i++) //Loops trough all the messages
                {
                    string msg = _server.lee_diccionario();
                    handler_Parse(msg);
                    //Receiver newMsg = new Receiver(); //Creates a new instance to process the message.
                    //newMsg.InitializeReceiver(msg, this, sender); //Initializes said instance.
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Server couldn't read message:" + e);
        }
    }


    /*===============================
                HANDLERS

       handler_Parse --> Main handler.

       handler_FindCInstance(string address, int port) 
            --> tries to find a client that matches. 
            returns null or the client instance.

       handler_ParseVector3(string x,y,z)

    ===============================*/
    private void handler_Parse(string content)
    {
        try
        {
            // Parse --> IP:PORT and MSG
            string[] msg_Split = content.Split("#");

            // Parse --> ;
            string[] msg_Parts = msg_Split[1].Split(";");

            // Parse --> IP:PORT
            string[] connect_dir = msg_Split[0].Split(":");

            // Check header
            switch (msg_Parts[0])
            {
                case "0": //Client joining
                    server_login(connect_dir[0], int.Parse(connect_dir[1]), msg_Parts);
                    break;
                case "1": //Position update from client
                    Debug.Log("RECEIVING UPDATE FROM CLIENT");
                    CInstance_PosUpdate(connect_dir[0], int.Parse(connect_dir[1]), msg_Parts);
                    break;
                case "2": //CInstance Info.
                    CInstance_SendInfo(connect_dir[0], int.Parse(connect_dir[1]), msg_Parts);
                    break;
                case "3": //Client left server.
                    CInstance_Disconnect(connect_dir[0], int.Parse(connect_dir[1]), msg_Parts);
                    break;


            }
        }
        catch (Exception e)
        {
            Debug.Log("Couldn't parse message:" + e);
        }
    }

    private server_CInstance handler_FindCInstance(string msg_addr, int msg_port, string username = null)
    {
        try
        {
            for (int i = 0; i < server_CInstances.Count; i++)
            {
                if (server_CInstances[i].CInstance_Addr == msg_addr && server_CInstances[i].CInstance_Port == msg_port)
                {
                    if(username != null)
                    {
                        if(server_CInstances[i].CInstance_Username == username)
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
    }

    private server_CInstance handler_FindCInstanceByName(string username)
    {
        try
        {
            for(int i = 0; i < server_CInstances.Count; i++)
            {
                if(server_CInstances[i].CInstance_Username == username)
                {
                    return server_CInstances[i];
                }
            }
            return null;
        }
        catch (Exception e)
        {
            Debug.Log("Error in handler_FindCInstanceByName" + e);
            return null;
        }
    }

    private Vector3 handler_ParseVector3(string vector_String)
    {
        try
        {
            string[] vector_Components = vector_String.Split("_");
            Vector3 vector_Result = new Vector3();
            vector_Result.x = float.Parse(vector_Components[0]);
            vector_Result.y = float.Parse(vector_Components[1]);
            vector_Result.z = float.Parse(vector_Components[2]);

            return vector_Result;
        }
        catch (Exception e)
        {
            Debug.Log("Error in handler_ParseVector3" + e);
            return new Vector3(0, 0, 0);
        }
    }


    /*===============================
        CLIENT JOINS: HEADER "0"
    ===============================*/
    private void server_login(string msg_addr, int msg_port, string[] msg_Parts)
    {
        try
        {
            server_CInstance CInstance_Found = handler_FindCInstance(msg_addr, msg_port);
            if (CInstance_Found != null) //Client doesn't exist.
            {
                server_CInstances.Remove(CInstance_Found);
            }

            server_CInstance new_CInstance = new server_CInstance();

            new_CInstance.Setup(msg_addr, msg_port, msg_Parts[1], Instantiate(CInstance_Prefab, new Vector3(0, 1, 0), Quaternion.identity));
            server_CInstances.Add(new_CInstance);

            List<string> login_answer = new List<string>();
            login_answer.Add("1");

            SendMsg("0", login_answer, msg_addr, msg_port);
            Debug.Log("VerifiedClient");
        }
        catch (Exception e)
        {
            Debug.Log("Error in server_login" + e);
        }
    }

    /*===============================
       POSITION UPDATE: HEADER "1"
    ===============================*/
    private void CInstance_PosUpdate(string msg_addr, int msg_port, string[] msg_Parts)
    {
        try
        {
            server_CInstance CInstance_Found = handler_FindCInstance(msg_addr, msg_port);

            if (CInstance_Found != null)
            {
                GameObject CInstance_Object = CInstance_Found.CInstance;
                Rigidbody CInstance_Physics = CInstance_Object.GetComponent<Rigidbody>();

                if (msg_Parts[1] == "Jump")
                {
                    int layerMask = 1 << 6;
                    RaycastHit hit;
                    if (Physics.Raycast(CInstance_Object.transform.position, transform.TransformDirection(Vector3.down), out hit, 2f, layerMask))
                    {
                        CInstance_Physics.AddForce(new Vector3(0, 1, 0) * JumpPower);
                    }
                    return;
                }

                Vector3 PositionBools = handler_ParseVector3(msg_Parts[1]);
                //CInstance_Found.CInstance_Rot = handler_ParseVector3(msg_Parts[2]);

                List<string> PositionUpdate = new List<string>();

                float i;
                float k;
                float j;

                CInstance_Object.transform.eulerAngles = handler_ParseVector3(msg_Parts[2]);
                Debug.Log(PositionBools);
                float angle = CInstance_Object.transform.eulerAngles.y;
                i = ((Mathf.Cos(-angle / Mathf.Rad2Deg)) * (PositionBools.x)) + ((Mathf.Sin(angle / Mathf.Rad2Deg)) * (PositionBools.z));
                j = CInstance_Physics.velocity.y;
                k = ((Mathf.Sin(-angle / Mathf.Rad2Deg)) * (PositionBools.x)) + ((Mathf.Cos(angle / Mathf.Rad2Deg)) * (PositionBools.z));


                CInstance_Physics.velocity = new Vector3(i * Time.fixedDeltaTime * MovementSpeed, j, k * Time.fixedDeltaTime * MovementSpeed);

                //PositionUpdate.Add("1");
                /*PositionUpdate.Add(CInstance_Found.CInstance_Username);
                PositionUpdate.Add(CInstance_Found.CInstance_Pos.x + "_" + CInstance_Found.CInstance_Pos.y + "_" + CInstance_Found.CInstance_Pos.z);
                PositionUpdate.Add(CInstance_Found.CInstance_Rot.x + "_" + CInstance_Found.CInstance_Rot.y + "_" + CInstance_Found.CInstance_Rot.z);

                SendAll("1", PositionUpdate, CInstance_Found);*/
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error in CInstance_PosUpdate" + e);
        }
    }

    private void CInstances_SendPos()
    {
        try
        {
            foreach(server_CInstance CInstance in server_CInstances)
            {
                List<string> PositionUpdate = new List<string>();
                GameObject CInstance_Object = CInstance.CInstance;
                PositionUpdate.Add(CInstance_Object.transform.position.x + "_" + CInstance_Object.transform.position.y + "_"  + CInstance_Object.transform.position.z);


                foreach(server_CInstance CInstance_Other in server_CInstances)
                {
                    if(CInstance_Other != CInstance)
                    {
                        GameObject Other_Object = CInstance_Other.CInstance;
                        CInstance_JsonFormat JsonInfo = new CInstance_JsonFormat();
                        JsonInfo.CInstance_Pos = (Other_Object.transform.position.x + "_" + Other_Object.transform.position.y + "_" + Other_Object.transform.position.z);
                        JsonInfo.CInstance_Rot = (Other_Object.transform.eulerAngles.x + "_" + Other_Object.transform.eulerAngles.y + "_" + Other_Object.transform.eulerAngles.z);
                        JsonInfo.CInstance_Username = CInstance_Other.CInstance_Username;

                        string Json = JsonUtility.ToJson(JsonInfo);
                        PositionUpdate.Add(Json);
                    }
                }

                SendMsg("1", PositionUpdate, CInstance.CInstance_Addr, CInstance.CInstance_Port);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error in CInstance_SendPos error provocado por Alpaka" + e);
        }
    }

    /*===============================
       REQUEST OF CLIENT INFO: HEADER "2"
    ===============================*/
    private void CInstance_SendInfo(string msg_addr, int msg_port, string[] msg_Parts)
    {
        try
        {
            server_CInstance CInstance_Found = handler_FindCInstanceByName(msg_Parts[1]);

            if(CInstance_Found != null)
            {
                List<string> ClientInfo = new List<string>();
                ClientInfo.Add(CInstance_Found.CInstance_Username);

                SendMsg("2", ClientInfo, msg_addr, msg_port);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error in CInstance_SendInfo" + e);
        }
    }

    /*===============================
       CLIENT DISCONNECTED: HEADER "3"
    ===============================*/
    private void CInstance_Disconnect(string msg_addr, int msg_port, string[] msg_Parts)
    {
        try
        {
            server_CInstance CInstance_Found = handler_FindCInstance(msg_addr, msg_port, msg_Parts[1]);

            if(CInstance_Found != null)
            {
                List<string> LeftNotification = new List<string>();
                LeftNotification.Add(CInstance_Found.CInstance_Username);
                SendAll("3", LeftNotification, CInstance_Found);

                server_CInstances.Remove(CInstance_Found);
                Destroy(CInstance_Found.CInstance);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error in CInstance_Disconnect" + e);
        }
    }
}
