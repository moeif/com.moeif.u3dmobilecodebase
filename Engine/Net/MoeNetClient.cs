using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetCoreServer;
using UnityEngine;
using System.Collections.Generic;

public class MoeNetClient : MoeSingleton<MoeNetClient> 
{

	public enum Type
	{
		Ssl,
		Tcp,
		Udp
	}

	//[SerializeField]
	//private ExampleEchoClientUi ui = null;

	#region SettingFields

	[SerializeField]
	private string serverIp = "127.0.0.1";

	[SerializeField]
	private int serverPort = 3333;

	[Tooltip("Number of times the message is repeated to simulate more requests.")]
	[SerializeField]
	private int repeatMessage = 0;

	[Tooltip("Non-blocking async sending. Not recommended for UDP (those are already non-blocking)")]
	[SerializeField]
	private bool async = true;

	[SerializeField]
	private Type type = Type.Tcp;

	[SerializeField, Tooltip("Only needed for SSL Sockets")]
	private SslCertificateAsset sslCertificateAsset = null;

	[Tooltip("Try to reconnect if connection could not be established or was lost")]
	[SerializeField]
	private bool reconnect = true;

	[SerializeField]
	private float reconnectionDelay = 1.0f;

	#endregion

	private const int MAX_PACKAGE_SIZE = 512 * 1024;
	private const byte PACKAGE_SYMBOL = 0x68;
	private const int PACKAGE_LENGTH = 10;       // [0x68, 32bit protoId, 32bit package length, 0x68]

	/// <summary>
	/// 处理接收到的数据
	/// </summary>
	private List<byte> bufferList = new List<byte>(MAX_PACKAGE_SIZE);

	/// <summary>
	/// 打包发送的数据
	/// </summary>
	private List<byte> packBuffer = new List<byte>(8192);

	public bool Async => async;

	// Used implementation as interface to allow easy switching
	private IUnitySocketClient socketClient;

	// Buffer will be used for sending and receiving to avoid creating garbage
	private byte[] buffer;
	private bool disconnecting;
	private bool applicationQuitting;

	

	private void Start()
	{
		disconnecting = false;
	}

	private void OnDestroy()
	{
		Disconnect();
	}


	[ContextMenu("Connect")]
	private void Connect()
	{
		if (socketClient != null && (socketClient.IsConnected || socketClient.IsConnecting))
		{
			Disconnect();
			socketClient = null;
		}

		switch (type)
		{
			case Type.Ssl:
				var sslContext = sslCertificateAsset.GetContext();
				socketClient = new UnitySslClient(sslContext, serverIp, serverPort);
				break;
			case Type.Tcp:
				socketClient = new UnityTcpClient(serverIp, serverPort);
				break;
			case Type.Udp:
				socketClient = new UnityUdpClient(serverIp, serverPort);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		//buffer = new byte[socketClient.OptionReceiveBufferSize];
		buffer = new byte[MAX_PACKAGE_SIZE];

		socketClient.OnConnectedEvent += OnConnected;
		socketClient.OnDisconnectedEvent += OnDisconnected;
		socketClient.OnErrorEvent += OnError;
		socketClient.ConnectAsync();
	}

	[ContextMenu("Disconnect")]
	public void Disconnect()
	{
		if (socketClient == null)
		{
			return;
		}

		disconnecting = true;
		socketClient.Disconnect();

		while (socketClient.IsConnected)
		{
			Thread.Yield();
		}

		socketClient.OnConnectedEvent -= OnConnected;
		socketClient.OnDisconnectedEvent -= OnDisconnected;
		socketClient.OnErrorEvent -= OnError;
		disconnecting = false;
	}

	private void Update()
	{
		//ui.UpdateState(socketClient);
		// TODO Update State
		bool connected = socketClient != null && socketClient.IsConnected;

		if (!connected || !socketClient.HasEnqueuedPackages())
		{
			return;
		}

		string messages = $"Messages received at frame {Time.frameCount}:\n";
		while (socketClient.HasEnqueuedPackages())
		{
			int length = socketClient.GetNextPackage(ref buffer);
			byte[] receivedData = new byte[length];
			Array.Copy(buffer, receivedData, length);
			bufferList.AddRange(receivedData);

			//Debug.LogFormat("Get Package Length: {0}, BufLength: {1}", length, buffer.Length);
			//var message = Encoding.UTF8.GetString(buffer, 0, length);
			//messages += message + "\n";
			//Debug.LogFormat("After Get Package BufLength: {0}", buffer.Length);
		}

		while(bufferList.Count > PACKAGE_LENGTH)
        {
			byte[] headerBytes = bufferList.GetRange(0, PACKAGE_LENGTH).ToArray();
			byte packageSymbol1 = headerBytes[0];
			int protoId = BitConverter.ToInt32(headerBytes, 1);
			int dataLength = BitConverter.ToInt32(headerBytes, 5);
			byte packageSymbol2 = headerBytes[9];
			if(packageSymbol1 != packageSymbol2)
            {
				Debug.LogErrorFormat("数据包包头错误，Symbol不相同: {0} {1}", packageSymbol1, packageSymbol2);
				bufferList.Clear();
				break;
            }

			if(bufferList.Count - PACKAGE_LENGTH >= dataLength)
            {
				byte[] packageData = bufferList.GetRange(PACKAGE_LENGTH, dataLength).ToArray();
				bufferList.RemoveRange(0, PACKAGE_LENGTH + dataLength);
				PackageHandleMap.Inst.HandlePackage(protoId, packageData);
            }
        }

		//ui.AddResponseText(messages);
		// TODO: Response Message
	}

	private void OnConnected()
	{
		Debug.Log($"{socketClient.GetType()} connected a new session with Id {socketClient.Id}");
	}

	private void OnDisconnected()
	{
		Debug.Log($"{socketClient.GetType()} disconnected a session with Id {socketClient.Id}");
		if (applicationQuitting)
		{
			return;
		}

		if (reconnect && !disconnecting)
		{
			ReconnectDelayedAsync();
		}
	}

	private async void ReconnectDelayedAsync()
	{
		await Task.Delay((int)(reconnectionDelay * 1000));

		if (socketClient.IsConnected || socketClient.IsConnecting)
		{
			return;
		}

		Debug.Log("Trying to reconnect");
		socketClient.ConnectAsync();
	}

	private void OnError(SocketError error)
	{
		Debug.LogError($"{socketClient.GetType()} caught an error with code {error}");
	}

	private void TriggerDisconnect()
	{
		Disconnect();
	}

	public void ApplyInputAndConnect(string serverIpInput, int serverPortInput, Type typeInput)
	{
		serverIp = serverIpInput;
		serverPort = serverPortInput;
		type = typeInput;
		ValidateSettings();
		Connect();
	}

	public void SetAsync(bool async)
	{
		this.async = async;
		ValidateSettings();
	}

	private void ValidateSettings()
	{
		if (async && type == Type.Udp)
		{
			Debug.LogWarning("Using UDP with async is not recommended and might lead to unwanted behavior.");
		}
	}

	private void Send(byte[] message)
	{
		if (async)
		{
			socketClient.SendAsync(message);
		}
		else
		{
			socketClient.Send(message);
		}
	}

	public void SendPackage(int protoId, byte[] data)
    {
		packBuffer.Clear();
		int dataLength = data.Length;
		packBuffer.Add(PACKAGE_SYMBOL);
		packBuffer.AddRange(BitConverter.GetBytes(protoId));
		packBuffer.AddRange(BitConverter.GetBytes(dataLength));
		packBuffer.Add(PACKAGE_SYMBOL);
		packBuffer.AddRange(data);
		byte[] bytes = packBuffer.ToArray();
		Debug.LogFormat("Send Package, protoId: {0}, packageLength: {1}", protoId, bytes.Length);
		Send(bytes);
    }

	
	private void OnApplicationQuit()
	{
		applicationQuitting = true;
	}

   // private void OnGUI()
   // {
   //     if (GUILayout.Button("Connect"))
   //     {
			//Connect();
   //     }

   //     if (GUILayout.Button("Disconnect"))
   //     {
			//Disconnect();
   //     }
   // }
}