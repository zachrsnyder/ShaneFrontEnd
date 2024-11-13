using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using ServerCommunication;
using UnityEngine;
using Random = System.Random;

public class MoonrakerClient : MonoBehaviour
{
    MqttFactory factory = new MqttFactory();
    private ManagedMqttClientOptions client_options;
    private IManagedMqttClient client;
    public PrinterStatus printer;

    public delegate void pos_handler(float[] new_pos);
    public event pos_handler toolhead_moved;
    
    private Random rnd = new Random();
    private Regex rx = new Regex(@"(?<=\[)(.*?)(?=\\])",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    [SerializeField] private const string PrinterName = "Printer1";
    [SerializeField] private string mqtt_username;
    [SerializeField] private string mqtt_password;
    
    public async Task Connect_Client()
    {
        var ops = new MqttClientOptionsBuilder().WithTcpServer("152.70.157.193")
            .WithCredentials(mqtt_username, mqtt_password).Build();
        
        client_options = new ManagedMqttClientOptionsBuilder().WithClientOptions(ops).Build();
        client = factory.CreateManagedMqttClient();
        var mqttSubscribeOptions = factory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic(PrinterName + "/klipper/state/toolhead/#");
                })
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic(PrinterName + "/klipper/state/heater_bed/#");
                })
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic(PrinterName + "/klipper/state/extruder/#");
                })
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic(PrinterName + "/klipper/state/gcode_move/#");
                })
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic(PrinterName + "/klipper/state/fan/#");
                }).Build();
        
        client.ApplicationMessageReceivedAsync += HandleIncomingMessage;
        await client.StartAsync(client_options);
        await client.SubscribeAsync(mqttSubscribeOptions.TopicFilters);

    }

    private Task HandleIncomingMessage(MqttApplicationMessageReceivedEventArgs arg)
    {
        switch (arg.ApplicationMessage.Topic)
        {
            case (PrinterName + "/klipper/state/extruder/temperature"):
                JsonConvert.DeserializeObject<Dictionary<string, float>>(
                    Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment.Array, 0,
                        arg.ApplicationMessage.PayloadSegment.Array.Length))
                    .TryGetValue("value", out printer.extruder_temp);
                break;
            case (PrinterName + "/klipper/state/extruder/target"):
                JsonConvert.DeserializeObject<Dictionary<string, float>>(
                    Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment.Array, 0,
                        arg.ApplicationMessage.PayloadSegment.Array.Length))
                    .TryGetValue("value", out printer.extruder_target_temp);
                break;
            case (PrinterName + "/klipper/state/extruder/power"):
                JsonConvert.DeserializeObject<Dictionary<string, float>>(
                    Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment.Array, 0,
                        arg.ApplicationMessage.PayloadSegment.Array.Length))
                    .TryGetValue("value", out printer.extruder_power);
                break;
            case (PrinterName + "/klipper/state/heater_bed/temperature"):
                JsonConvert.DeserializeObject<Dictionary<string, float>>(
                    Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment.Array, 0,
                        arg.ApplicationMessage.PayloadSegment.Array.Length))
                    .TryGetValue("value", out printer.bed_temp);
                break;
            case (PrinterName + "/klipper/state/heater_bed/target"):
                JsonConvert.DeserializeObject<Dictionary<string, float>>(
                        Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment.Array, 0,
                            arg.ApplicationMessage.PayloadSegment.Array.Length))
                    .TryGetValue("value", out printer.bed_target_temp);
                break;
            case (PrinterName + "/klipper/state/heater_bed/power"):
                JsonConvert.DeserializeObject<Dictionary<string, float>>(
                        Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment.Array, 0,
                            arg.ApplicationMessage.PayloadSegment.Array.Length))
                    .TryGetValue("value", out printer.bed_power);
                break;
            case (PrinterName + "/klipper/state/fan/speed"):
                JsonConvert.DeserializeObject<Dictionary<string, float>>(
                        Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment.Array, 0,
                            arg.ApplicationMessage.PayloadSegment.Array.Length))
                    .TryGetValue("value", out printer.fan_speed);
                break;
            case (PrinterName + "/klipper/state/gcode_move/position"):
                printer.toolhead_position = Array.ConvertAll(rx.Match(Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment.Array, 0,
                    arg.ApplicationMessage.PayloadSegment.Array.Length)).Value.Split(", "), float.Parse);
                toolhead_moved.Invoke(printer.toolhead_position);
                break;
        }
        // Debug.Log("Tag: " + arg.Tag);
        // Debug.Log(Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment.Array, 0, arg.ApplicationMessage.PayloadSegment.Array.Length));
        // Debug.Log("To String: " + arg.ToString());
        Debug.Log("Toolhead Pos: " + printer.toolhead_position.ToString());
        Debug.Log("Heater_Bed Temp: " + printer.bed_temp + " Target: " + printer.bed_target_temp + " Power: " + printer.bed_power);
        Debug.Log("Extruder Temp: " + printer.extruder_temp + " Target: " + printer.extruder_target_temp + " Power: " + printer.extruder_power);
        Debug.Log("Fan speed: " + printer.fan_speed);
        return arg.AcknowledgeAsync(CancellationToken.None);
        
    }

    public void Run_Script(string script)
    {
        Task.Run(async () => await Run_GCode(script));
    }

    private async Task Run_GCode(string code)
    {
        await client.EnqueueAsync(PrinterName + "/moonraker/api/request",
            "{\"jsonrpc\":\"2.0\",\"method\": \"printer.gcode.script\",\"params\": { \"script\": \"" +
            code + "\"},\"id\": " + rnd.Next() + "}");
    }

    public async Task PausePrint()
    {
        await client.EnqueueAsync(PrinterName + "/moonraker/api/request",
            "{\"jsonrpc\":\"2.0\",\"method\": \"printer.print.pause\",\"id\": " + rnd.Next() + "}");
    }
    
    public async Task ResumePrint()
    {
        await client.EnqueueAsync(PrinterName + "/moonraker/api/request",
            "{\"jsonrpc\":\"2.0\",\"method\": \"printer.print.resume\",\"id\": " + rnd.Next() + "}");
    }
    
    public async Task CancelPrint()
    {
        await client.EnqueueAsync(PrinterName + "/moonraker/api/request",
            "{\"jsonrpc\":\"2.0\",\"method\": \"printer.print.cancel\",\"id\": " + rnd.Next() + "}");
        printer.currentGCodeName = null;
        printer.currentPreview = null;
    }
    
    public async Task EmergencyStop()
    {
        await client.EnqueueAsync(PrinterName + "/moonraker/api/request",
            "{\"jsonrpc\":\"2.0\",\"method\": \"printer.emergency_stop\",\"id\": " + rnd.Next() + "}");
    }

    public async Task StartPrint(string filename)
    {
        await client.EnqueueAsync(PrinterName + "/moonraker/api/request",
            "{\"jsonrpc\":\"2.0\",\"method\": \"printer.print.start\",\"params\": {\"filename\": \"" + filename + "\"},\"id\": " + rnd.Next() + "}");
    }
    
    public async Task Disconnect_Client()
    {
        if (client.IsConnected)
        {
            await client.StopAsync();
        }
        else
        {
            Debug.LogError("Client cannot be disconnected since it is not connected yet!");
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Task.Run(Connect_Client);
//        Debug.Log("Connected? " + client.IsConnected);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        Task.Run(Disconnect_Client);
    }
}
