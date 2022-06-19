using System.Reflection;
using Godot;
using GoDotTest;

public class Tests : Node2D {
  public override async void _Ready()
    => await GoTest.RunTests(Assembly.GetExecutingAssembly(), this);
}
