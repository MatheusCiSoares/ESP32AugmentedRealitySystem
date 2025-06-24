using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public partial class MyServer : Node
{
    //================= GERENCIAMENTO DO SERVIDOR =================//
    private static TcpListener? myTcpServer;
    public static MyServer Instance { get; private set; }
    private const ushort PORT = 8080;

    [Signal]
    public delegate void SendStatusSignalEventHandler(string status);

    //================= GERENCIAMENTO DE CLIENTES =================//
    private static byte maxIDs = 9; // 8 Livres e 1 para notificar que a lista está cheia.
    private static byte[] clientIDs = new byte[maxIDs]; // Lista de IDs disponíveis
    private static Dictionary<byte, TcpClient> clientMap = new();
    [Signal]
    public delegate void ClientConnectedEventHandler(byte clientID);
    [Signal]
    public delegate void ClientDisconnectedEventHandler(byte clientID);
    [Signal]
    public delegate void MPUDataEventHandler(byte clientID, float[] mpuData);

    public override void _EnterTree()
    {
        Instance = this;
    }
    public override void _Ready()
    {
        myTcpServer = new TcpListener(IPAddress.Any, PORT);
        myTcpServer.Start();
        GD.Print($"Servidor funcionando no IP: {getIpAddress().ToString()}:{PORT}.");
        GD.Print($"Hostname: {Dns.GetHostName()}");

        // LOOP
        ThreadTask1();
    }
    
    public partial class ResponseHolder : GodotObject
    {
        public byte[] Stream { get; set; }
    }

    public bool getStatus()
    {
        return myTcpServer.Server.IsBound;
    }
    public IPAddress getIpAddress()
    {
        string hostName = Dns.GetHostName();
        return Dns.GetHostByName(hostName).AddressList[1];
    }
    public string getHostName()
    {
        return Dns.GetHostName();
    }

    public byte[] GetClientIDs()
    {
        return clientIDs;
    }

    private async void ThreadTask1()
    {
        while (true)
        {
            var client = await myTcpServer.AcceptTcpClientAsync();
            GD.Print("Iniciando conexão com cliente.");

            _ = ReadDataAsync(client);
            //HandleDisplayAsync(client);
        }

    }

    private static async Task ReadDataAsync(TcpClient clientIndex)
    {
        NetworkStream clientStream = clientIndex.GetStream();
        string? message;
        byte[]? response;
        byte position = 0; // Salvar referência da posição do cliente na lista.
        byte msgID = 0;
        try
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int byteRead = await clientStream.ReadAsync(buffer, 0, buffer.Length);

                if (byteRead == 0) break;

                message = Encoding.UTF8.GetString(buffer, 0, byteRead);
                //GD.Print($"Mensagem: {message}");
                if (message.StartsWith("ID: "))
                {
                    msgID = (byte)char.GetNumericValue(message.Last()); // Deve ser trocado caso a lista aceite 10 ou mais clientes.
                    // Se o ESP32 enviar o ID 0, significa que ele ainda não se conectou ao servidor
                    if (msgID == 0)
                    {
                        GD.Print("Novo dispositivo tentando se comunicar. Procurando vaga.");
                        for (position = 0; position < clientIDs.Length; position++)
                        {
                            if (clientIDs[position] == 0 && position < clientIDs.Length - 1)
                            {
                                clientIDs[position] = (byte)(position + 1);
                                clientMap[clientIDs[position]] = clientIndex;
                                GD.Print($"Dispositivo conectado na posição {position} com o ID {clientIDs[position]}");
                                response = Encoding.UTF8.GetBytes($"Seu ID: {position + 1}");
                                clientStream.Write(response, 0, response.Length);
                                GD.Print($"Posição atual: position {position} e length {clientIDs.Length - 1}");

                                await Task.Delay(2000);
                                Instance.EmitSignal(SignalName.ClientConnected, clientIDs[position]);
                                break;
                            }
                            else if (position == clientIDs.Length - 1)
                            {
                                GD.Print("Lista cheia! Desconectando dispositivo.");
                                clientStream.Write(Encoding.UTF8.GetBytes("Lista cheia."));
                                clientStream.Close();
                            }
                        }
                    }
                    // Se o ESP32 enviar outro ID, significa que ele já estava conectado, porém perdeu a conexão
                    else
                    {
                        // Verificar se a posição está disponível para sincronizar novamente.
                        GD.Print("Cliente previamente conectado. Tentando reconectar.");

                        if (clientIDs[msgID - 1] == 0)
                        {
                            // Como o ID sempre é igual à posição +1, não é necessário fazer uma varredura pela lista.
                            clientIDs[msgID - 1] = msgID;
                        }
                        else
                        {
                            // Quando o ESP32 desconectou, mas o servidor não computou. Ou outro ESP32 se conectou no lugar.
                            GD.Print("Erro: Posição já ocupada. Verifique se outro dispositivo está conectado na posição.");
                            clientStream.Close();
                        }
                    }
                }
                else if (message.StartsWith("MPU: "))
                {
                    string cleanMpuData = message.Replace("MPU: ", "");
                    cleanMpuData = cleanMpuData.Replace("\n", "");
                    try
                    {
                        float[] mpuData = cleanMpuData.Split('|', StringSplitOptions.RemoveEmptyEntries).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToArray();
                        Instance.EmitSignal(SignalName.MPUData, clientIDs[position], mpuData);
                        
                    }
                    catch
                    {

                    }
                }
            }
            await Task.Delay(1);
        }
        catch (Exception ex)
        {
            GD.Print($"Erro: {ex.Message}");
            GD.Print(position);
        }
        finally
        {
            clientStream.Close();
            clientIDs[position] = 0;
            clientMap.Remove((byte)(position + 1));
            GD.Print($"POSITION: {position}");
            GD.Print("Cliente desconectado.");
            Instance.EmitSignal(SignalName.ClientDisconnected, (byte)(position + 1));
        }
    }

    public async Task OnDisplaySignal(byte ID, byte[] displayStream)
    {
        TcpClient clientIndex;
        if (clientMap.TryGetValue(ID, out clientIndex))
        {

            try
            {
                NetworkStream clientStream = clientIndex.GetStream();

                // Header fixo
                byte[] header = Encoding.ASCII.GetBytes("TFT");

                byte[] streamSize = BitConverter.GetBytes((uint)displayStream.Length);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(streamSize);
                }
                await clientStream.WriteAsync(header, 0, header.Length);
                await clientStream.WriteAsync(streamSize, 0, streamSize.Length);
                await clientStream.WriteAsync(displayStream, 0, displayStream.Length);

                //GD.Print($"Imagem enviada");
                //GD.Print($"Imagem enviada: {streamSize.Length} bytes e stream {displayStream.Length}");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Erro no envio da imagem: {ex}");
            }
            finally
            {

            }
        }
        else
        {
            GD.PrintErr("Cliente não encontrado para enviar imagem");
        }
        
    }
    
}