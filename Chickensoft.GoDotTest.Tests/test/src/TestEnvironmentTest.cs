namespace Chickensoft.GoDotTest.Tests;

using Godot;
using GoDotTest;
using Shouldly;

public class TestEnvironmentTest : TestClass
{
  public TestEnvironmentTest(Node testScene) : base(testScene) { }

  [Test]
  public void ConstructsTestEnvironmentWithPatternFlag()
  {
    var testEnvironment = TestEnvironment.From([
      "--run-tests=SomeTest"
    ]);
    testEnvironment.TestPatternToRun.ShouldBe("SomeTest");
  }

  [Test]
  public void ConstructsTestEnvironmentWithSimpleFlags()
  {
    var args = new string[] {
      "--quit-on-finish",
      "--listen-trace",
      "--stop-on-error",
      "--sequential",
      "--coverage",
    };
    var testEnvironment = TestEnvironment.From(args);
    testEnvironment.QuitOnFinish.ShouldBeTrue();
    testEnvironment.ListenTrace.ShouldBeTrue();
    testEnvironment.StopOnError.ShouldBeTrue();
    testEnvironment.Sequential.ShouldBeTrue();
    testEnvironment.Coverage.ShouldBeTrue();
    testEnvironment.CommandLineArgs.ShouldBe(args);
  }
}
