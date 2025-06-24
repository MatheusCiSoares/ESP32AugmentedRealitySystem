using Godot;
using System;
using System.Collections.Generic;

public partial class Ambient3D : Node3D
{
	[Export]
	private PackedScene virtualDevice;
	private Dictionary<byte, VirtualDevice> virtualDevices = new Dictionary<byte, VirtualDevice>();
	private SubViewport tester;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MyServer.Instance.ClientConnected += OnClientConnected;
		MyServer.Instance.Connect(MyServer.SignalName.ClientDisconnected, Callable.From<byte>(OnClientDisconnected));
		MyServer.Instance.MPUData += OnMpuData;
		//MyServer.Instance.TFTDataL += OnTFTDataL;
		//MyServer.Instance.TFTDataR += OnTFTDataR;
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnClientConnected(byte clientID)
	{
		GD.Print($"Instanciando dispositivo virtual: {clientID}");

		VirtualDevice newVirtualDevice = (VirtualDevice)virtualDevice.Instantiate();
		newVirtualDevice.ID = clientID;
		newVirtualDevice.myServer = MyServer.Instance;
		newVirtualDevice.Position = new Vector3(0, 0, 0);
		AddChild(newVirtualDevice);

		virtualDevices[clientID] = newVirtualDevice;
	}

	public void OnClientDisconnected(byte clientID)
	{
		if (virtualDevices.TryGetValue(clientID, out var device))
		{
			device.QueueFree();
			virtualDevices.Remove(clientID);
		}
	}
	public void OnMpuData(byte clientID, float[] mpuData)
	{
		if (virtualDevices.TryGetValue(clientID, out var device))
		{
			device.MoveDevice(mpuData);
		}
		else
		{
			GD.PrintErr($"Dispositivo {clientID} não encontrado.");
		}
	}
}