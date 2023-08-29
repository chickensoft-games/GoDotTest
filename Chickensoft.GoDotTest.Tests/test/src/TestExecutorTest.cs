namespace Chickensoft.GoDotTest.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using GoDotTest;
using LightMock;
using LightMock.Generator;
using LightMoq;
using Shouldly;

// Test the test system using itself to test itself.

public class TestExecutorTest : TestClass {
  public TestExecutorTest(Node testScene) : base(testScene) { }

  // Test data shared between TestTest and ourselves.

  public static readonly List<string> Called = new();

  [Test]
  public async Task RunsASingleTestSuiteForReal() {
    var methodExecutor = new TestMethodExecutor();
    var testExecutor = new TestExecutor(
      methodExecutor: methodExecutor,
      stopOnError: false,
      sequential: false,
      timeoutMilliseconds: 0
    );

    testExecutor.StopOnError.ShouldBe(false);
    testExecutor.Sequential.ShouldBe(false);
    testExecutor.TimeoutMilliseconds.ShouldBe(0);

    // Typically, you should mock all your dependencies. Testing the test
    // provider would be a real pain, and we know it works or this test
    // wouldn't even be running, so we'll just use one of its static methods.
    // Otherwise, this test would be much more involved.
    var suite = TestProvider.GetTestSuite(typeof(TestTestIgnored));
    suite.ShouldBeAssignableTo<ITestSuite>();

    var reporter = new Mock<ITestReporter>();

    await testExecutor.Run(
      TestScene, new List<ITestSuite>() { suite }, reporter.Object
    );

    Called.ShouldBe(new List<string>() {
      "SetupAll",
      "Setup",
      "Test",
      "Cleanup",
      "Setup",
      "FailingTest",
      "Failure",
      "FailingFailure",
      "Cleanup",
      "CleanupAll"
    });
  }

  [Test]
  public async Task RunStopsOnError() {
    var methodExecutor = new Mock<ITestMethodExecutor>();

    methodExecutor.Setup(
      exe => exe.Run(
        The<ITestMethod>.Is(
          method => method.Name == nameof(TestTestIgnored2.Test1)
        ),
        The<TestClass>.IsAnyValue,
        The<int>.IsAnyValue
      )
    ).Throws(() => new InvalidOperationException("Ahem"));

    var testExecutor = new TestExecutor(
      methodExecutor: methodExecutor.Object,
      stopOnError: true,
      sequential: false
    );

    var reporter = new Mock<ITestReporter>();

    var suite = TestProvider.GetTestSuite(typeof(TestTestIgnored2));

    await testExecutor.Run(
      sceneRoot: TestScene,
      suites: new List<ITestSuite>() {
        suite
      },
      reporter: reporter.Object
    );

    methodExecutor.VerifyAll();
  }

  [Test]
  public async Task RunSkipsSubsequentOnSequentialWhenErrorOccurs() {
    var methodExecutor = new Mock<ITestMethodExecutor>();

    methodExecutor.Setup(
      exe => exe.Run(
        The<ITestMethod>.Is(
          method => method.Name == nameof(TestTestIgnored3.Test1)
        ),
        The<TestClass>.IsAnyValue,
        The<int>.IsAnyValue
      )
    ).Throws(() => new InvalidOperationException("Ahem"));

    methodExecutor.Setup(
      exe => exe.Run(
        The<ITestMethod>.Is(
          method => method.Name == nameof(TestTestIgnored3.CleanupAll)
        ),
        The<TestClass>.IsAnyValue,
        The<int>.IsAnyValue
      )
    );

    var testExecutor = new TestExecutor(
      methodExecutor: methodExecutor.Object,
      stopOnError: false,
      sequential: true
    );

    var reporter = new Mock<ITestReporter>();

    var suite = TestProvider.GetTestSuite(typeof(TestTestIgnored3));

    await testExecutor.Run(
      sceneRoot: TestScene,
      suites: new List<ITestSuite>() {
        suite
      },
      reporter: reporter.Object
    );
  }
}
