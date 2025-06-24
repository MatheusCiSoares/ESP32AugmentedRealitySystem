using Godot;

public partial class DevicePanel : Node3D
{
	private Label deviceID;
	private Label accX;
	private Label accY;
	private Label accZ;
	private Label yaw;
	private Label pitch;
	private Label roll;
	public override void _Ready()
	{
		deviceID = GetNode<Label>("Sprite3D/SubViewport/PanelContainer/VBoxContainer/DeviceID");
		accX = GetNode<Label>("Sprite3D/SubViewport/PanelContainer/VBoxContainer/HBoxContainer/Col1/AccX");
		accY = GetNode<Label>("Sprite3D/SubViewport/PanelContainer/VBoxContainer/HBoxContainer/Col2/AccY");
		accZ = GetNode<Label>("Sprite3D/SubViewport/PanelContainer/VBoxContainer/HBoxContainer/Col3/AccZ");
		yaw = GetNode<Label>("Sprite3D/SubViewport/PanelContainer/VBoxContainer/HBoxContainer/Col1/Yaw");
		pitch = GetNode<Label>("Sprite3D/SubViewport/PanelContainer/VBoxContainer/HBoxContainer/Col2/Pitch");
		roll = GetNode<Label>("Sprite3D/SubViewport/PanelContainer/VBoxContainer/HBoxContainer/Col3/Roll");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void GetDeviceID(byte indexID)
	{
		deviceID.Text = "Device: " + indexID;
	}

	public void GetMPUValues(float[] mpuValues)
	{
		accX.Text = "AccX: " + mpuValues[0];
		accY.Text = "AccY: " + mpuValues[1];
		accZ.Text = "AccZ: " + mpuValues[2];
		yaw.Text = "Yaw: " + mpuValues[4];
		pitch.Text = "Pitch: " + mpuValues[5];
		roll.Text = "Roll: " + mpuValues[6];
	}
}
