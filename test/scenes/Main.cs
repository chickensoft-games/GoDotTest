using Godot;
using GoDotTest;

public class Main : Node2D {
  public override void _Ready() {
    var testEnv = TestEnvironment.From(OS.GetCmdlineArgs());
    if (testEnv.ShouldRunTests) {
      GetTree().ChangeScene("res://test/Tests.tscn");
    }
    else {
      GetTree().ChangeScene("res://scenes/Game.tscn");
    }
  }
}
