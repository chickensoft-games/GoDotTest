namespace Chickensoft.GoDotTest.Tests;

using Godot;
using GoDotTest;
using Shouldly;

public class TestEnvironmentTest : TestClass {
  public TestEnvironmentTest(Node testScene) : base(testScene) { }

  private static readonly string[] _commandLineArgs = new string[] {
      "--run-tests=SomeTest"
    };

  [Test]
  public void ConstructsTestEnvironmentWithPatternFlag() {
    var testEnvironment = TestEnvironment.From(_commandLineArgs);
    testEnvironment.TestPatternToRun.ShouldBe("SomeTest");
  }

  [Test]
  public void ConstructsTestEnvironmentWithSimpleFlags() {
    var args = new string[] {
      "--quit-on-finish",
      "--suppress-trace",
      "--stop-on-error",
      "--sequential",
      "--coverage",
    };
    var testEnvironment = TestEnvironment.From(args);
    testEnvironment.QuitOnFinish.ShouldBeTrue();
    testEnvironment.SuppressTrace.ShouldBeTrue();
    testEnvironment.StopOnError.ShouldBeTrue();
    testEnvironment.Sequential.ShouldBeTrue();
    testEnvironment.Coverage.ShouldBeTrue();
    testEnvironment.CommandLineArgs.ShouldBe(args);
  }
}
