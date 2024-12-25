using Godot;
using System;

public partial class birdmodel : Node3D
{
	private Skeleton3D skeleton;
	private int busIndex;
	private int boneIndex;
	private double lastSpikedMS = DateTimeOffset.Now.ToUnixTimeMilliseconds();
	private int maxX;
	private int maxY;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		skeleton = GetNode<Skeleton3D>("Armature/Skeleton3D");
		boneIndex = skeleton.FindBone("Bone");
		busIndex = AudioServer.GetBusIndex("birdmic");
		// capture = (AudioEffectCapture)AudioServer.GetBusEffect(busIndex, 0);
		DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Transparent, true);
		GetTree().Root.Transparent = true;
		GetTree().Root.TransparentBg = true;

		Vector2 window = GetWindow().Size;
		// GD.Print(window);
		// Define the corners of the texture in local space
		Vector2[] textureCorners = new Vector2[]
		{
			new Vector2(0, 0), // Top left corner
            new Vector2(window.X, 0),  // Top right corner
            new Vector2(window.X, window.Y),   // Bottom right corner
            new Vector2(0,window.Y)   // Bottom left corner
        };

		// Set the mouse passthrough for the window
		// DisplayServer.WindowSetMousePassthrough(textureCorners);
		maxX = DisplayServer.ScreenGetSize().X;
		maxY = DisplayServer.ScreenGetSize().Y;
	}



	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float rawSample = AudioServer.GetBusPeakVolumeLeftDb(busIndex, 0) + 200;
		if (rawSample < 10)
		{
			lastSpikedMS = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}
		// GD.Print(Math.Round(rawSample));
		rawSample /= 100;
		// GD.Print(rawSample);
		moveJaw(rawSample);
		pointAtCursor(DisplayServer.MouseGetPosition());
	}


	private void moveJaw(float db)
	{
		double tick = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		if (tick - lastSpikedMS > 250)
		{
			skeleton.SetBonePosePosition(boneIndex, new Vector3((float)Math.Abs(Mathf.Sin(tick / 500 * Math.PI * 2) * Math.Pow(db, 2)), 0, 0));
			return;
		}
		skeleton.SetBonePosePosition(boneIndex, new Vector3(0, 0, 0));
	}

	private void pointAtCursor(Vector2 mousePos)
	{
		float mouseX = mousePos.X;
		float mouseY = mousePos.Y;
		float howLeft = mouseX * ((float)90/maxX)-90;
		float howUp = mouseY * -((float)45/maxY);
		
		
		GlobalRotationDegrees = new Vector3(0, howLeft, howUp);
	}

	private float map(float value, float fromMax, float toMax)
	{
		return value/toMax*fromMax;
	}
}
