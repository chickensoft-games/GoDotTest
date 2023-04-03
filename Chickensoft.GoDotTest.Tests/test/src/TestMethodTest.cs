namespace Chickensoft.GoDotTest.Tests;

using System.Threading.Tasks;
using Godot;
using GoDotTest;
using Shouldly;

public class TestMethodTest : TestClass {
  public TestMethodTest(Node testScene) : base(testScene) { }

  [Test]
  public void RespectsTimeout() {
    var method = typeof(TestMethodTest).GetMethod(
      nameof(TestMethodTest.MethodWithTimeout)
    )!;
    var testMethod = new TestMethod(method, TestMethodType.Test);
    testMethod.TimeoutMilliseconds.ShouldBe(150);
  }

  [Test]
  public async Task WillNotInvokeAsyncVoidMethod() {
    var method = typeof(TestMethodTest).GetMethod(
      nameof(TestMethodTest.AsyncVoidMethod)
    )!;
    var testMethod = new TestMethod(method, TestMethodType.Test);
    await Should.ThrowAsync<AsyncVoidException>(() => testMethod.Invoke(this));
  }

  [Test]
  public async Task ThrowsTimeoutExceptionWhenTestTimesOut() {
    var method = typeof(TestMethodTest).GetMethod(
      nameof(TestMethodTest.MethodWithTimeout)
    )!;
    var testMethod = new TestMethod(method, TestMethodType.Test);
    await Should.ThrowAsync<TestTimeoutException>(
      () => testMethod.Invoke(this, 200)
    );
  }

  [Timeout(150)]
  public async Task MethodWithTimeout() => await Task.Delay(300);

  public async void AsyncVoidMethod() => await Task.CompletedTask;
}
