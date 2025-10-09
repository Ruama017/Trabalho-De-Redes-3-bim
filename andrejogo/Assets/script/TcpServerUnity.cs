using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TcpServerUnity : MonoBehaviour
{
    private TcpListener server;
    private Thread serverThread;
    private TcpClient client;
    private NetworkStream stream;
    private bool running = false;

    public string receivedMessage;

    void Start()
    {
        // Inicia o servidor na porta 5000
        serverThread = new Thread(StartServer);
        serverThread.Start();
    }

    void StartServer()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Debug.Log("Servidor TCP iniciado na porta 5000. Aguardando cliente...");

            client = server.AcceptTcpClient();
            stream = client.GetStream();
            running = true;
            Debug.Log("Cliente conectado!");

            byte[] buffer = new byte[1024];

            while (running)
            {
                if (stream.DataAvailable)
                {
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytes);
                    Debug.Log("Mensagem recebida do cliente: " + receivedMessage);

                    // Exemplo de resposta
                    string response = "Servidor recebeu: " + receivedMessage;
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                }
            }
        }
        catch (SocketException e)
        {
            Debug.Log("Erro no servidor: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        running = false;
        stream?.Close();
        client?.Close();
        server?.Stop();
        serverThread?.Abort();
    }
}
