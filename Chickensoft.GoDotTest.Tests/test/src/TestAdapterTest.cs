namespace Chickensoft.GoDotTest.Tests;

using Chickensoft.Log;
using Godot;
using GoDotTest;
using LightMock.Generator;
using Shouldly;

public class TestAdapterTest : TestClass {
  public TestAdapterTest(Node testScene) : base(testScene) { }

  [Test]
  public void CreateLogUsesLog() {
    var adapter = new TestAdapter();
    var log = new Mock<ILog>();
    var logObject = log.Object;
    adapter.CreateLog(logObject).ShouldBe(log.Object);
  }

  [Test]
  public void CreateLogCreatesLog() {
    var adapter = new TestAdapter();
    adapter.CreateLog(null).ShouldBeAssignableTo<ILog>();
  }

  [Test]
  public void CreateTestEnvironmentUsesEnvironment() {
    var adapter = new TestAdapter();
    var env = new Mock<ITestEnvironment>();
    adapter.CreateTestEnvironment(env.Object).ShouldBe(env.Object);
  }

  [Test]
  public void CreateTestEnvironmentCreatesEnvironment() {
    var adapter = new TestAdapter();
    adapter.CreateTestEnvironment(null)
      .ShouldBeAssignableTo<ITestEnvironment>();
  }

  [Test]
  public void CreateProviderCreatesProvider() {
    var adapter = new TestAdapter();
    adapter.CreateProvider().ShouldBeAssignableTo<ITestProvider>();
  }

  [Test]
  public void CreateReporterCreatesReporter() {
    var adapter = new TestAdapter();
    var log = new Mock<ILog>();
    adapter.CreateReporter(log.Object).ShouldBeAssignableTo<ITestReporter>();
  }

  [Test]
  public void CreateMethodExecutorCreatesMethodExecutor() {
    var adapter = new TestAdapter();
    adapter.CreateMethodExecutor().ShouldBeAssignableTo<ITestMethodExecutor>();
  }

  [Test]
  public void CreateExecutorCreatesExecutor() {
    var adapter = new TestAdapter();
    var methodExecutor = new Mock<ITestMethodExecutor>();
    var log = new Mock<ILog>();
    adapter.CreateExecutor(methodExecutor.Object, false, false, 0)
      .ShouldBeAssignableTo<ITestExecutor>();
  }
}
