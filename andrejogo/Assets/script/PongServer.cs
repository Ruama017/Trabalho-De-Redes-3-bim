using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class PongServer : MonoBehaviour
{
    public Transform paddleServer; // Raquete do servidor (Player 1)
    public Transform paddleClient; // Raquete do cliente (Player 2)
    public Transform ball;

    private Vector2 ballVelocity = new Vector2(4f, 4f);

    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private Thread serverThread;
    private bool running = false;

    void Start()
    {
        // Inicia o servidor
        serverThread = new Thread(ServerLoop);
        serverThread.Start();
    }

    void Update()
    {
        // Movimento da raquete do servidor (Player 1)
        float move = Input.GetAxis("Vertical");
        paddleServer.Translate(Vector3.up * move * 8f * Time.deltaTime);

        // Movimento da bola (somente o servidor controla)
        ball.Translate(ballVelocity * Time.deltaTime);

        // Rebater nas bordas
        if (ball.position.y > 4.5f || ball.position.y < -4.5f)
        {
            ballVelocity.y *= -1;
        }

        // Rebater nas raquetes
        if (Vector3.Distance(ball.position, paddleServer.position) < 0.6f && ballVelocity.x < 0)
        {
            ballVelocity.x *= -1;
        }

        if (Vector3.Distance(ball.position, paddleClient.position) < 0.6f && ballVelocity.x > 0)
        {
            ballVelocity.x *= -1;
        }

        // Enviar estado do jogo ao cliente
        SendGameState();
    }

    void ServerLoop()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Debug.Log("Servidor aguardando cliente...");

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
                    string message = Encoding.UTF8.GetString(buffer, 0, bytes);

                    // O cliente envia posição Y da raquete dele
                    float clientY;
                    if (float.TryParse(message, out clientY))
                    {
                        paddleClient.position = new Vector3(paddleClient.position.x, clientY, 0);
                    }
                }
            }
        }
        catch (SocketException e)
        {
            Debug.Log("Erro no servidor: " + e.Message);
        }
    }

    void SendGameState()
    {
        if (stream == null) return;

        // Envia posição da bola e raquete do servidor
        string data = ball.position.x + ";" + ball.position.y + ";" + paddleServer.position.y;
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        stream.Write(bytes, 0, bytes.Length);
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
