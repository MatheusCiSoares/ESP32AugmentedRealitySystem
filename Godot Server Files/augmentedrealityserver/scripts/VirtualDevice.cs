// Este objeto é dependente do ambiente 3D, não se comunicando diretamente com o servidor.
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Godot;

public partial class VirtualDevice : Node3D
{
    public byte ID { get; set; }

    public MyServer myServer { get; set; }
    float imageQuality = 0.5f;

    Vector3 moveVelocity = Vector3.Zero;
    private CharacterBody3D dynamicObject;
    private MeshInstance3D debugMesh;
    private DevicePanel devicePanel;
    private MeshInstance3D eyeRigL;
    private MeshInstance3D eyeRigR;
    private SubViewport viewsCombined;
    private SubViewport viewportL;
    private SubViewport viewportR;
    private Camera3D eyeCamL;
    private Camera3D eyeCamR;

    [Signal]
    public delegate void DisplayStreamEventHandler(byte ID, byte[] displayStream);
    public override void _Ready()
    {
        GD.Print($"Objeto virtual {ID} pronto!");
        dynamicObject = GetNode<CharacterBody3D>("DynamicObject");
        debugMesh = GetNode<MeshInstance3D>("DynamicObject/DebugMesh");
        devicePanel = GetNode<DevicePanel>("DevicePanel");

        debugMesh.Visible = false;

        devicePanel.GetDeviceID(ID);       

        eyeRigL = GetNode<MeshInstance3D>("DynamicObject/EyeRigL");
        eyeRigR = GetNode<MeshInstance3D>("DynamicObject/EyeRigR");
        viewsCombined = GetNode<SubViewport>("DynamicObject/ViewsCombined/SubViewport");
        //viewportL = GetNode<SubViewport>("DynamicObject/ViewsCombined/SubViewport/Sprite2DL/ViewportL");
        //viewportR = GetNode<SubViewport>("DynamicObject/ViewsCombined/SubViewport/Sprite2DR/ViewportR");
        eyeCamL = GetNode<Camera3D>("DynamicObject/ViewsCombined/SubViewport/Sprite2DL/ViewportL/EyeCamL");
        eyeCamR = GetNode<Camera3D>("DynamicObject/ViewsCombined/SubViewport/Sprite2DR/ViewportR/EyeCamR");

        //_ = SendDisplayLoopL();
        //_ = SendDisplayLoopR();
        _ = SendDisplayStream();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public void MoveDevice(float[] mpuData)
    {
        eyeCamL.GlobalTransform = eyeRigL.GlobalTransform;
        eyeCamR.GlobalTransform = eyeRigR.GlobalTransform;

        //RotateObjectLocal(new Vector3(1, 0, 0), mpuData[0] / 50);
        //RotateObjectLocal(new Vector3(0, 1, 0), mpuData[1] / 50);
        //RotateObjectLocal(new Vector3(0, 0, 1), mpuData[2] / 50);
        dynamicObject.Rotation += new Vector3(-mpuData[5] / 10, mpuData[6] / 10, mpuData[4] / 10);

        
        Vector3 localMovement = new Vector3(-mpuData[5] / 10, -mpuData[6] / 10, mpuData[4] / 10);
        GD.Print($"{mpuData[0]} | {mpuData[1]} | {mpuData[2]}");
        Vector3 globalMovement = GlobalTransform.Basis * localMovement;

        //dynamicObject.GlobalTranslate(globalMovement);

        devicePanel.GetMPUValues(mpuData);
    }

    public async Task SendDisplayStream()
    {
        while (true)
        {

            byte[] viewStream = viewsCombined.GetTexture().GetImage().SaveJpgToBuffer(imageQuality);
            //EmitSignal(SignalName.DisplayStream, ID, viewStream);
            await myServer.OnDisplaySignal(ID, viewStream);
            await Task.Delay(200);
        }
    }
}