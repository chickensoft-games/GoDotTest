namespace Chickensoft.GoDotTest.Tests;

using System.Reflection;
using System.Threading.Tasks;
using Godot;
using GoDotTest;
using Shouldly;

public class TestProviderTest : TestClass {
  public TestProviderTest(Node testScene) : base(testScene) { }
  [Test]
  public void GetTestSuiteByNameGetsSuiteWithName() {
    var provider = new TestProvider();
    var assembly = Assembly.GetExecutingAssembly();
    var suite = provider.GetTestSuiteByName(assembly, nameof(TestTest));
    suite.ShouldNotBeNull();
    suite.Name.ShouldBe(nameof(TestTest));
  }

  [Test]
  public void GetTestSuitesByPatternGetsSuites() {
    var provider = new TestProvider();
    var assembly = Assembly.GetExecutingAssembly();
    var suites = provider.GetTestSuitesByPattern(assembly, "TestTest*");
    suites.ShouldNotBeNull();
    suites.ShouldNotBeEmpty();
    suites.ShouldContain(suite => suite.Name == nameof(TestTest));
    suites.ShouldContain(suite => suite.Name == nameof(TestTest2));
    suites.ShouldContain(suite => suite.Name == nameof(TestTest3));
  }

  [Test]
  public void IsAsyncRecognizesAsyncMethod() => TestProvider.IsAsynchronous(
    typeof(TestProviderTest).GetMethod(nameof(TestProviderTest.AsyncMethod))!
  ).ShouldBeTrue();

  public async Task AsyncMethod() => await Task.CompletedTask;
}
