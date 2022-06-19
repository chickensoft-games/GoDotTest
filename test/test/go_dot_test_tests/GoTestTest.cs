using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using GoDotLog;
using GoDotTest;
using Moq;
using Shouldly;

public class TestTestAdapter : TestAdapter {
  public Action<Mock<ITestExecutor>> OnExecutorCreated;

  public TestTestAdapter(
    Action<Mock<ITestExecutor>> onExecutorCreated
  ) => OnExecutorCreated = onExecutorCreated;

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
    var adapter = new Mock<TestAdapter>(MockBehavior.Strict);
    GoTest.Adapter = adapter.Object;
    var testEnv = TestEnvironment.From(new string[] { });
    var log = new Mock<ILog>(MockBehavior.Strict);
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
    provider.Setup(provider => provider.GetTestSuites(It.IsAny<Assembly>())
      ).Returns(new List<ITestSuite>());

    var reporter = new Mock<ITestReporter>();
    reporter.Setup(reporter => reporter.HadError).Returns(true);

    var executor = new Mock<ITestExecutor>();
    executor.Setup(
      executor => executor.Run(
        TestScene, It.IsAny<List<ITestSuite>>(), reporter.Object
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
      It.IsAny<ITestMethodExecutor>(),
      It.IsAny<bool>(),
      It.IsAny<bool>(),
      It.IsAny<int>()
    )).Returns(executor.Object);

    int? testExitCode = null;
    GoTest.OnExit = (node, exitCode) => testExitCode = exitCode;

    GoTest.Adapter = adapter.Object;
    await GoTest.RunTests(Assembly.GetExecutingAssembly(), TestScene, testEnv, log.Object);
    testExitCode.ShouldBe(1);
  }

  // Put the default adapter back once we're done testing the test system.
  [CleanupAll]
  public void CleanupAll() {
    GoTest.Adapter = GoTest.DefaultAdapter;
    GoTest.OnExit = GoTest.DefaultOnExit;
  }
}
