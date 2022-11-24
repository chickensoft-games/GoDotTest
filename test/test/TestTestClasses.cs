namespace GoDotTestTest;
using Godot;
using GoDotTest;

public class TestTest : TestClass {
  public TestTest(Node testScene) : base(testScene) { }

  [SetupAll]
  public void SetupAll() {
    // This test will also be run stand-alone by the framework since it extends
    // TestClass, so we need to prevent doubling up the record of invocations.
    TestExecutorTest.Called.Clear();
    TestExecutorTest.Called.Add("SetupAll");
  }

  [Setup]
  public void Setup() => TestExecutorTest.Called.Add("Setup");

  [Test]
  public void Test() => TestExecutorTest.Called.Add("Test");

  [Cleanup]
  public void Cleanup() => TestExecutorTest.Called.Add("Cleanup");

  [CleanupAll]
  public void CleanupAll() => TestExecutorTest.Called.Add("CleanupAll");
}

[Sequential]
public class TestTest2 : TestClass {
  public TestTest2(Node testScene) : base(testScene) { }

  [Test]
  public void Test1() { }

  [Test]
  public void Test2() { }

  [CleanupAll]
  public void CleanupAll() { }
}

public class TestTest3 : TestClass {
  public TestTest3(Node testScene) : base(testScene) { }

  [Test]
  public void Test1() { }

  [Test]
  public void Test2() { }

  [CleanupAll]
  public void CleanupAll() { }
}
