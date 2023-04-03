namespace Chickensoft.GoDotTest.Tests;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using GoDotLog;
using GoDotTest;
using LightMock;
using LightMock.Generator;
using LightMoq;
using Shouldly;

public class TestTestAdapter : TestAdapter {
  public Action<Mock<ITestExecutor>> OnExecutorCreated;

  public TestTestAdapter(
    Action<Mock<ITestExecutor>> onExecutorCreated
  ) {
    OnExecutorCreated = onExecutorCreated;
  }

  public override ITestExecutor CreateExecutor(
      ITestMethodExecutor methodExecutor,
      bool stopOnError,
      bool sequential,
      int timeoutMilliseconds
    ) {
    var executor = new Mock<ITestExecutor>();
    OnExecutorCreated?.Invoke(executor);
    return executor.Object;
  }
}

public class GoTestTest : TestClass {
  public GoTestTest(Node testScene) : base(testScene) { }

  [Test]
  public async Task DoesNothingIfNotRunningTests() {
    var adapter = new Mock<TestAdapter>();
    GoTest.Adapter = adapter.Object;
    var testEnv = TestEnvironment.From(Array.Empty<string>());
    var log = new Mock<ILog>();
    var assembly = Assembly.GetExecutingAssembly();
    await GoTest.RunTests(assembly, TestScene, testEnv, log.Object);
  }

  [Test]
  public async Task ExitsWithFailingExitCodeWhenTestsFail() {
    var testEnv = TestEnvironment.From(
      new string[] { "--run-tests=ahem", "--quit-on-finish" }
    );
    var log = new Mock<ILog>();

    var provider = new Mock<ITestProvider>();
    provider.Setup(provider => provider.GetTestSuites(The<Assembly>.IsAnyValue)
      ).Returns(new List<ITestSuite>());

    var reporter = new Mock<ITestReporter>();
    reporter.Setup(reporter => reporter.HadError).Returns(true);

    var executor = new Mock<ITestExecutor>();
    executor.Setup(
      executor => executor.Run(
        TestScene, The<List<ITestSuite>>.IsAnyValue, reporter.Object
      )
    ).Returns(Task.CompletedTask);

    var adapter = new Mock<ITestAdapter>();
    adapter.Setup(adapter => adapter.CreateTestEnvironment(testEnv))
      .Returns(testEnv);
    adapter.Setup(adapter => adapter.CreateLog(log.Object)).Returns(log.Object);
    adapter.Setup(
      adapter => adapter.CreateProvider()
    ).Returns(provider.Object);
    adapter.Setup(
      adapter => adapter.CreateReporter(log.Object)
    ).Returns(reporter.Object);
    adapter.Setup(adapter => adapter.CreateExecutor(
      The<ITestMethodExecutor>.IsAnyValue,
      The<bool>.IsAnyValue,
      The<bool>.IsAnyValue,
      The<int>.IsAnyValue
    )).Returns(executor.Object);

    int? testExitCode = null;
    GoTest.OnExit = (node, exitCode) => testExitCode = exitCode;

    GoTest.Adapter = adapter.Object;
    await GoTest.RunTests(
      Assembly.GetExecutingAssembly(), TestScene, testEnv, log.Object
    );
    testExitCode.ShouldBe(1);
  }

  [Test]
  public async Task ExitsWithFailingExitCodeWhenTestsFailOnCoverage() {
    var testEnv = TestEnvironment.From(
      new string[] { "--run-tests=ahem", "--coverage", "--quit-on-finish" }
    );
    var log = new Mock<ILog>();

    var provider = new Mock<ITestProvider>();
    provider.Setup(provider => provider.GetTestSuites(The<Assembly>.IsAnyValue)
      ).Returns(new List<ITestSuite>());

    var reporter = new Mock<ITestReporter>();
    reporter.Setup(reporter => reporter.HadError).Returns(true);

    var executor = new Mock<ITestExecutor>();
    executor.Setup(
      executor => executor.Run(
        TestScene, The<List<ITestSuite>>.IsAnyValue, reporter.Object
      )
    ).Returns(Task.CompletedTask);

    var adapter = new Mock<ITestAdapter>();
    adapter.Setup(adapter => adapter.CreateTestEnvironment(testEnv))
      .Returns(testEnv);
    adapter.Setup(adapter => adapter.CreateLog(log.Object)).Returns(log.Object);
    adapter.Setup(
      adapter => adapter.CreateProvider()
    ).Returns(provider.Object);
    adapter.Setup(
      adapter => adapter.CreateReporter(log.Object)
    ).Returns(reporter.Object);
    adapter.Setup(adapter => adapter.CreateExecutor(
      The<ITestMethodExecutor>.IsAnyValue,
      The<bool>.IsAnyValue,
      The<bool>.IsAnyValue,
      The<int>.IsAnyValue
    )).Returns(executor.Object);

    int? testExitCode = null;
    GoTest.OnForceExit = (node, exitCode) => testExitCode = exitCode;

    GoTest.Adapter = adapter.Object;
    await GoTest.RunTests(
      Assembly.GetExecutingAssembly(), TestScene, testEnv, log.Object
    );
    testExitCode.ShouldBe(1);
  }

  /// <summary>
  /// Put the default adapter back once we're done testing the test system.
  /// </summary>
  [CleanupAll]
  public void CleanupAll() {
    GoTest.Adapter = GoTest.DefaultAdapter;
    GoTest.OnExit = GoTest.DefaultOnExit;
    GoTest.OnForceExit = GoTest.DefaultOnForceExit;
  }
}
