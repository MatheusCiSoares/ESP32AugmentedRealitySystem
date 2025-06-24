using System.Threading.Tasks;
using Godot;

public partial class CentralPanel : Node3D
{
	private ScrollContainer scrollBox;
	private RichTextLabel loremIpsum;
	public override void _Ready()
	{
		scrollBox = GetNode<ScrollContainer>("Sprite3D/SubViewport/ScrollBox");
		loremIpsum = GetNode<RichTextLabel>("Sprite3D/SubViewport/ScrollBox/MarginContainer/LoremIpsum");

		SmoothScroll();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public void SmoothScroll()
	{
		Tween tween = CreateTween();

		float boxSize = loremIpsum.Size.Y;

		tween.TweenProperty(
			scrollBox,
			"scroll_vertical",
			boxSize,
			50
		);

		tween.Play();
	}

}
