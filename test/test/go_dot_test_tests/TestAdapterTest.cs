using Godot;
using GoDotLog;
using GoDotTest;
using Moq;
using Shouldly;

public class TestAdapterTest : TestClass {
  public TestAdapterTest(Node testScene) : base(testScene) { }

  [Test]
  public void CreateLogUsesLog() {
    var adapter = new TestAdapter();
    var log = new Mock<ILog>();
    adapter.CreateLog(log.Object).ShouldBe(log.Object);
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
