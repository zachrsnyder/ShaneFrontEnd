using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UX;
using MQTTnet;
using MQTTnet.Client;
using UnityEngine;
using Random = System.Random;


public enum Axis
{
    X,
    Y,
    Z,
    HOME
};
public class ToolheadPositionControl : MonoBehaviour
{
    MqttFactory factory = new MqttFactory();
    private IMqttClient client;
    [SerializeField] private string printerInstance = "Printer1";
    private Random rnd = new Random();

    [SerializeField] private PressableButton Homebtn;

    [SerializeField] private MRTKTMPInputField setx;
    [SerializeField] private MRTKTMPInputField sety;
    [SerializeField] private MRTKTMPInputField setz;
    
    public async Task Connect_Client()
    {
        client = factory.CreateMqttClient();
        var options = new MqttClientOptionsBuilder().WithTcpServer("152.70.157.193")
            .WithCredentials("Truman", "GoTigers").Build();

        var response = await client.ConnectAsync(options, CancellationToken.None);
        
        Debug.Log(response.ToString());


    }
    // Start is called before the first frame update
    void Start()
    {
        //TODO: Find a way to keep track of toolhead position (either on client or by subbing to printer info events) so we can update the position text in realtime
        
        Task.Run(Connect_Client);
        Homebtn.OnClicked.AddListener(delegate{MoveToolhead(Axis.HOME);});
        setx.keyboardType = TouchScreenKeyboardType.DecimalPad;
        sety.keyboardType = TouchScreenKeyboardType.DecimalPad;
        setz.keyboardType = TouchScreenKeyboardType.DecimalPad;
        setx.onEndEdit.AddListener(delegate(string arg0) { MoveToolhead(Axis.X, float.Parse(arg0), false); });
        sety.onEndEdit.AddListener(delegate(string arg0) { MoveToolhead(Axis.Y, float.Parse(arg0), false); });
        setz.onEndEdit.AddListener(delegate(string arg0) { MoveToolhead(Axis.Z, float.Parse(arg0), false); });
        
    }

    public void MoveToolhead(Axis axis, float position=0, bool isRelative = true)
    {
        switch (axis)
        {
            case Axis.X:
                Task.Run(async () => await Move_X(position, isRelative));
                setx.text = String.Empty;
                break;
            case Axis.Y:
                Task.Run(async () => await Move_Y(position, isRelative));
                sety.text = String.Empty;
                break;
            case Axis.Z:
                Task.Run(async () => await Move_Z(position, isRelative));
                setz.text = String.Empty;
                break;
            case Axis.HOME:
                Task.Run(async () => await Home_Axis());
                break;
        }
        
    }
    

    private async Task Home_Axis()
    {
        //If we aren't connected, try to connect again until we are
        while (!client.IsConnected)
        {
            try
            {
                await Connect_Client();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        string command = "G28";
        var newmsg = new MqttApplicationMessageBuilder().WithTopic(printerInstance + "/moonraker/api/request")
            .WithPayload("{\"jsonrpc\":\"2.0\",\"method\": \"printer.gcode.script\",\"params\": { \"script\": \"" +
                         command + "\"},\"id\": " + rnd.Next() + "}").Build();
        
        await client.PublishAsync(newmsg, CancellationToken.None);
    }

    public async Task Move_X(float x, bool isRelative = false, uint speed=6000)
    {
        //If we aren't connected, try to connect again until we are
        while (!client.IsConnected)
        {
            try
            {
                await Connect_Client();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        string command;
        if (isRelative)
        {
            command = "G91\nG1 X" + x + "F" + speed + "\nG90";
        }
        else
        {
            command = "G1 X" + x + " F" + speed;
        }

        var newmsg = new MqttApplicationMessageBuilder().WithTopic(printerInstance + "/moonraker/api/request")
            .WithPayload("{\"jsonrpc\":\"2.0\",\"method\": \"printer.gcode.script\",\"params\": { \"script\": \"" +
                         command + "\"},\"id\": " + rnd.Next() + "}").Build();
        
        await client.PublishAsync(newmsg, CancellationToken.None);
    }

    public void Run_Script(string script)
    {
        Task.Run(async () => await Run_GCode(script));
    }

    private async Task Run_GCode(string code)
    {
        //If we aren't connected, try to connect again until we are
        while (!client.IsConnected)
        {
            try
            {
                await Connect_Client();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        var newmsg = new MqttApplicationMessageBuilder().WithTopic(printerInstance + "/moonraker/api/request")
            .WithPayload("{\"jsonrpc\":\"2.0\",\"method\": \"printer.gcode.script\",\"params\": { \"script\": \"" +
                         code + "\"},\"id\": " + rnd.Next() + "}").Build();
        
        await client.PublishAsync(newmsg, CancellationToken.None);
    }
    public async Task Move_Y(float y, bool isRelative = false, uint speed=6000)
    {
        //If we aren't connected, try to connect again until we are
        while (!client.IsConnected)
        {
            try
            {
                await Connect_Client();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        string command;
        if (isRelative)
        {
            command = "G91\nG1 Y" + y + "F" + speed + "\nG90";
        }
        else
        {
            command = "G1 Y" + y + " F" + speed;
        }

        var newmsg = new MqttApplicationMessageBuilder().WithTopic(printerInstance + "/moonraker/api/request")
            .WithPayload("{\"jsonrpc\":\"2.0\",\"method\": \"printer.gcode.script\",\"params\": { \"script\": \"" +
                         command + "\"},\"id\": " + rnd.Next() + "}").Build();
        
        await client.PublishAsync(newmsg, CancellationToken.None);
    }
    
    public async Task Move_Z(float z, bool isRelative = false, uint speed=1500)
    {
        //If we aren't connected, try to connect again until we are
        while (!client.IsConnected)
        {
            try
            {
                await Connect_Client();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        string command;
        if (isRelative)
        {
            command = "G91\nG1 Z" + z + "F" + speed + "\nG90";
        }
        else
        {
            command = "G1 Z" + z + " F" + speed;
        }

        var newmsg = new MqttApplicationMessageBuilder().WithTopic(printerInstance + "/moonraker/api/request")
            .WithPayload("{\"jsonrpc\":\"2.0\",\"method\": \"printer.gcode.script\",\"params\": { \"script\": \"" +
                         command + "\"},\"id\": " + rnd.Next() + "}").Build();

        await client.PublishAsync(newmsg, CancellationToken.None);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public void quitBtn()
    {
        Application.Quit();
    }
    
    private async Task DisconnectClient()
    {
        //Disconnect Cleanly
        var mqttDisconOptions = factory.CreateClientDisconnectOptionsBuilder().Build();
        await client.DisconnectAsync(mqttDisconOptions, CancellationToken.None);
    }
    private void OnDestroy()
    {

        Task.Run(DisconnectClient);
    }
}
