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

public class TestExecutorTest : TestClass
{
  public TestExecutorTest(Node testScene) : base(testScene) { }

  // Test data shared between TestTest and ourselves.

  public static readonly List<string> Called = [];

  [Test]
  public async Task RunsASingleTestSuiteForReal()
  {
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
    var op = TestProvider.GetTestSuiteOp(typeof(TestTestIgnored));
    op.ShouldBeAssignableTo<TestSuiteOp>();

    var reporter = new Mock<ITestReporter>();

    await testExecutor.Run(
      TestScene, [op], reporter.Object
    );

    Called.ShouldBe([
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
    ]);
  }

  [Test]
  public async Task RunStopsOnError()
  {
    var methodExecutor = new Mock<ITestMethodExecutor>();

    methodExecutor.Setup(
      static exe => exe.Run(
        The<ITestMethod>.Is(
          static method => method.Name == nameof(TestTestIgnored2.Test1)
        ),
        The<TestClass>.IsAnyValue,
        The<int>.IsAnyValue
      )
    ).Throws(static () => new InvalidOperationException("Ahem"));

    var testExecutor = new TestExecutor(
      methodExecutor: methodExecutor.Object,
      stopOnError: true,
      sequential: false
    );

    var reporter = new Mock<ITestReporter>();

    var ops = TestProvider.GetTestSuiteOp(typeof(TestTestIgnored2));

    await testExecutor.Run(
      sceneRoot: TestScene,
      ops: [ops],
      reporter: reporter.Object
    );

    methodExecutor.VerifyAll();
  }

  [Test]
  public async Task RunSkipsSubsequentOnSequentialWhenErrorOccurs()
  {
    var methodExecutor = new Mock<ITestMethodExecutor>();

    methodExecutor.Setup(
      static exe => exe.Run(
        The<ITestMethod>.Is(
          static method => method.Name == nameof(TestTestIgnored3.Test1)
        ),
        The<TestClass>.IsAnyValue,
        The<int>.IsAnyValue
      )
    ).Throws(static () => new InvalidOperationException("Ahem"));

    methodExecutor.Setup(
      static exe => exe.Run(
        The<ITestMethod>.Is(
          static method => method.Name == nameof(TestTestIgnored3.CleanupAll)
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

    var suite = TestProvider.GetTestSuiteOp(typeof(TestTestIgnored3));

    await testExecutor.Run(
      sceneRoot: TestScene,
      ops: [
        suite
      ],
      reporter: reporter.Object
    );
  }

  [Test]
  public void GetMethodExecutionSequenceReturnsIndividualMethodSequence()
  {
    var suite = new Mock<ITestSuite>();
    var method = new Mock<ITestMethod>();

    List<ITestMethod> setupAllMethods = [new Mock<ITestMethod>().Object];
    List<ITestMethod> setupMethods = [new Mock<ITestMethod>().Object];
    List<ITestMethod> cleanupMethods = [new Mock<ITestMethod>().Object];
    List<ITestMethod> cleanupAllMethods = [new Mock<ITestMethod>().Object];

    suite.Setup(suite => suite.SetupAllMethods).Returns(setupAllMethods);
    suite.Setup(suite => suite.SetupMethods).Returns(setupMethods);
    suite.Setup(suite => suite.CleanupMethods).Returns(cleanupMethods);
    suite.Setup(suite => suite.CleanupAllMethods).Returns(cleanupAllMethods);

    var op = new IndividualTestOp(
      Suite: suite.Object,
      Method: method.Object
    );

    TestExecutor.GetMethodExecutionSequence(op)
      .ShouldBe([
        setupAllMethods[0],
        setupMethods[0],
        method.Object,
        cleanupMethods[0],
        cleanupAllMethods[0]
      ]);
  }

  private sealed record FakeTestOp(ITestSuite Suite) : TestOp(Suite)
  {
    public override List<ITestMethod> TestMethods => Suite.TestMethods;
  }

  [Test]
  public void GetMethodExecutionSequenceReturnsEmptySequence()
  {
    var suite = new Mock<ITestSuite>();
    suite.Setup(s => s.TestMethods).Returns([]);
    var op = new FakeTestOp(Suite: suite.Object);

    TestExecutor.GetMethodExecutionSequence(op).ShouldBe([]);
  }
}
