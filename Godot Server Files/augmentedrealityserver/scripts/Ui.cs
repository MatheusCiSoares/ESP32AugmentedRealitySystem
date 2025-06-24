using Godot;
using System;
using System.Threading.Tasks;

// PARA SEXTA DIA 30
// 1. Remover clientes desconectados da lista
// 2. Spawnar objeto vinculado ao cliente

public partial class Ui : Control
{
	public Label serverStatus;
	public static VBoxContainer clientList;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		serverStatus = GetNode<Label>("ServerStatus");
		clientList = GetNode<VBoxContainer>("ClientList");

		if (MyServer.Instance != null)
		{
			serverStatus.Text = $"Servidor: {MyServer.Instance.getStatus().ToString()} | IP: {MyServer.Instance.getIpAddress().ToString()} | Hostname: {MyServer.Instance.getHostName()}";
		}
		else
		{
			serverStatus.Text = "Instancia não foi iniciada.";
		}

		Label clientId = new Label();
		clientId.Text = "0: Desconectado";
		for (int i = 1; i < 9; i++)
		{
			CreateClientList("0: Desconectado");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetServerClientList();
	}

	private void CreateClientList(string texto)
	{
		Label novaLabel = new Label();
		novaLabel.Text = texto;
		clientList.AddChild(novaLabel);
	}

	private static async Task GetServerClientList()
	{
		byte[] clientsCheck = MyServer.Instance.GetClientIDs();
		//GD.Print($"TESTE DE PUXAR: {clientsCheck}");
		for (byte pos = 0; pos < clientList.GetChildCount(); pos++)
		{
			if (clientList.GetChild(pos) is Label label)
			{
				if (clientsCheck[pos].ToString().StartsWith("0"))
				{
					label.Text = clientsCheck[pos].ToString() + ": Desconectado";
				}
				else
				{
					label.Text = clientsCheck[pos].ToString() + ": Conectado";
				}
			}
		}
	}
}
