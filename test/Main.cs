namespace GoDotTestTest;
using System.Reflection;
using Godot;
using GoDotTest;

public partial class Main : Node2D {
  public override async void _Ready()
    => await GoTest.RunTests(Assembly.GetExecutingAssembly(), this);
}
