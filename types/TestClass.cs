namespace GoDotTest {
  using Godot;

  /// <summary>
  /// Represents a class which contains test methods.
  /// </summary>
  public abstract class TestClass {
    /// <summary>
    /// Test scene node. Use as you wish — just remember to clean up
    /// after yourself!
    /// </summary>
    public readonly Node TestScene;

    /// <summary>
    /// Creates a new test using the specified test scene.
    /// </summary>
    public TestClass(Node testScene) => TestScene = testScene;
  }
}
