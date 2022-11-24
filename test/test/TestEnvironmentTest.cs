namespace GoDotTestTest;
using Godot;
using GoDotTest;
using Shouldly;

public class TestEnvironmentTest : TestClass {
  public TestEnvironmentTest(Node testScene) : base(testScene) { }

  [Test]
  public void ConstructsTestEnvironmentWithPatternFlag() {
    var testEnvironment = TestEnvironment.From(new string[] {
      "--run-tests=SomeTest"
    });
    testEnvironment.TestPatternToRun.ShouldBe("SomeTest");
  }

  [Test]
  public void ConstructsTestEnvironmentWithSimpleFlags() {
    var args = new string[] {
      "--quit-on-finish",
       "--stop-on-error",
       "--sequential",
       "--coverage",
    };
    var testEnvironment = TestEnvironment.From(args);
    testEnvironment.QuitOnFinish.ShouldBeTrue();
    testEnvironment.StopOnError.ShouldBeTrue();
    testEnvironment.Sequential.ShouldBeTrue();
    testEnvironment.Coverage.ShouldBeTrue();
    testEnvironment.CommandLineArgs.ShouldBe(args);
  }
}
