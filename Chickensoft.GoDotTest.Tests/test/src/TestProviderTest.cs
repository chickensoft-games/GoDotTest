namespace Chickensoft.GoDotTest.Tests;

using System.Reflection;
using System.Threading.Tasks;
using Godot;
using GoDotTest;
using Shouldly;

public class TestProviderTest : TestClass {
  public TestProviderTest(Node testScene) : base(testScene) { }
  [Test]
  public void GetTestOpByNameGetsSuiteWithName() {
    var provider = new TestProvider();
    var assembly = Assembly.GetExecutingAssembly();
    var op = provider.GetTestOpByName(assembly, nameof(TestTestIgnored));
    op.ShouldNotBeNull();
    op.Suite.Name.ShouldBe(nameof(TestTestIgnored));
  }

  [Test]
  public void GetTestOpByNameReturnsNullForNonexistentSuite() {
    var provider = new TestProvider();
    var assembly = Assembly.GetExecutingAssembly();
    var op = provider.GetTestOpByName(assembly, "NonexistentSuite");
    op.ShouldBeNull();
  }

  [Test]
  public void GetTestOpByNameReturnsNullWhenNothingAfterDot() {
    var provider = new TestProvider();
    var assembly = Assembly.GetExecutingAssembly();
    var op = provider.GetTestOpByName(assembly, nameof(TestTestIgnored) + ".");
    op.ShouldBeNull();
  }

  [Test]
  public void GetTestOpByNameReturnsIndividualTestOpWhenMethodSpecified() {
    var provider = new TestProvider();
    var assembly = Assembly.GetExecutingAssembly();
    var op = provider.GetTestOpByName(
      assembly,
      nameof(TestTestIgnored3) + "." + nameof(TestTestIgnored3.Test1)
    );
    op.ShouldNotBeNull();
    var individualOp = op.ShouldBeAssignableTo<IndividualTestOp>();
    individualOp.Suite.Name.ShouldBe(nameof(TestTestIgnored3));
    individualOp.Method.Name.ShouldBe(nameof(TestTestIgnored3.Test1));
  }

  [Test]
  public void GetTestOpsByPatternGetsOps() {
    var provider = new TestProvider();
    var assembly = Assembly.GetExecutingAssembly();
    var suites = provider.GetTestOpsByPattern(assembly, "TestTest*");
    suites.ShouldNotBeNull();
    suites.ShouldNotBeEmpty();
    suites.ShouldContain(op => op.Suite.Name == nameof(TestTestIgnored));
    suites.ShouldContain(op => op.Suite.Name == nameof(TestTestIgnored2));
    suites.ShouldContain(op => op.Suite.Name == nameof(TestTestIgnored3));
  }

  [Test]
  public void GetTestOpsByPatternGetsTestOpsByNameIfDotInPattern() {
    var provider = new TestProvider();
    var assembly = Assembly.GetExecutingAssembly();
    var suites = provider.GetTestOpsByPattern(
      assembly,
      nameof(TestTestIgnored3) + "." + nameof(TestTestIgnored3.Test1)
    );
    suites.ShouldNotBeNull();
    suites.ShouldHaveSingleItem();
    var individualOp = suites[0].ShouldBeAssignableTo<IndividualTestOp>();
    individualOp.Suite.Name.ShouldBe(nameof(TestTestIgnored3));
    individualOp.Method.Name.ShouldBe(nameof(TestTestIgnored3.Test1));
  }

  [Test]
  public void GetIndividualTestOpRespectsSuiteAndMethodName() {
    var provider = new TestProvider();
    var assembly = Assembly.GetExecutingAssembly();

    var op = provider.GetIndividualTestOp(
      assembly,
      nameof(TestTestIgnored3),
      nameof(TestTestIgnored3.Test1)
    );
    op.ShouldNotBeNull();

    var individualOp = op.ShouldBeAssignableTo<IndividualTestOp>();

    individualOp.Suite.Name.ShouldBe(nameof(TestTestIgnored3));
    individualOp.Method.Name.ShouldBe(nameof(TestTestIgnored3.Test1));
  }

  [Test]
  public void GetIndividualTestOpReturnsNullIfSuiteNameNotFound() {
    var provider = new TestProvider();
    var assembly = Assembly.GetExecutingAssembly();
    var op = provider.GetIndividualTestOp(
      assembly,
      "NonexistentSuite",
      nameof(TestTestIgnored3.Test1)
    );
  }

  [Test]
  public void GetIndividualTestOpReturnsNullIfMethodNameNotFound() {
    var provider = new TestProvider();
    var assembly = Assembly.GetExecutingAssembly();
    var op = provider.GetIndividualTestOp(
      assembly,
      nameof(TestTestIgnored3),
      "NonexistentMethod"
    );
    op.ShouldBeNull();
  }

  [Test]
  public void IsAsyncRecognizesAsyncMethod() => TestProvider.IsAsynchronous(
    typeof(TestProviderTest).GetMethod(nameof(AsyncMethod))!
  ).ShouldBeTrue();

  public async Task AsyncMethod() => await Task.CompletedTask;
}
