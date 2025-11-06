namespace Chickensoft.GoDotTest;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents a test method type.
/// </summary>
public enum TestMethodType
{
  /// <summary>
  /// Setup all test method, runs once before all test methods in the suite.
  /// </summary>
  SetupAll,
  /// <summary>Setup test method, runs before each test method.</summary>
  Setup,
  /// <summary>Test method.</summary>
  Test,
  /// <summary>Cleanup test method, runs after each test.</summary>
  Cleanup,
  /// <summary>Cleanup all test method, runs once after all the test
  /// methods in the suite.</summary>
  CleanupAll,
  /// <summary>Failure method that is run whenever any test in the suite fails.
  /// </summary>
  Failure,
}

/// <summary>
/// Represents test method types.
/// </summary>
public static class TestMethodTypes
{
  private static readonly Dictionary<Type, TestMethodType> _testMethodTypes
    = new()
    {
      [typeof(SetupAllAttribute)] = TestMethodType.SetupAll,
      [typeof(SetupAttribute)] = TestMethodType.Setup,
      [typeof(TestAttribute)] = TestMethodType.Test,
      [typeof(CleanupAttribute)] = TestMethodType.Cleanup,
      [typeof(CleanupAllAttribute)] = TestMethodType.CleanupAll,
      [typeof(FailureAttribute)] = TestMethodType.Failure,
    };

  /// <summary>
  /// Determines the <see cref="TestMethodType"/> based on the given
  /// <see cref="TestRunnerMethodAttribute"/> type.
  /// </summary>
  /// <param name="type">Type of a <see cref="TestRunnerMethodAttribute"/>
  /// subclass.
  /// </param>
  /// <returns>The type of test method the attribute type represents.
  /// </returns>
  public static TestMethodType GetTestMethodType(this Type type)
    => _testMethodTypes[type];
}

/// <summary>
/// Represents a test or utility (Setup, Cleanup, SetupAll, CleanupAll)
/// method in a <see cref="TestClass"/>.
/// </summary>
public interface ITestMethod
{
  /// <summary>Name of the test method.</summary>
  string Name { get; }

  /// <summary>Whether the method is asynchronous (or not).</summary>
  bool IsAsync { get; }

  /// <summary>
  /// Type of the test method (setup, cleanup, test, setup all, cleanup all).
  /// </summary>
  TestMethodType Type { get; }

  /// <summary>
  /// Custom timeout for the method, if indicated by annotation on the test
  /// method.
  /// </summary>
  int? TimeoutMilliseconds { get; }

  /// <summary>Invokes the test method.</summary>
  /// <param name="testInstance"></param>
  /// <returns>A future that completes when the method finishes.</returns>
  Task Invoke(TestClass testInstance);

  /// <summary>Invokes the test method with a timeout.</summary>
  /// <param name="testInstance">Test suite instance.</param>
  /// <param name="timeoutMilliseconds">The timeout, in milliseconds.</param>
  /// <returns>A future that completes when the method finishes.</returns>
  Task Invoke(TestClass testInstance, int timeoutMilliseconds);
}

/// <summary>
/// Default implementation of a test class method.
/// </summary>
public class TestMethod : ITestMethod
{
  private readonly MethodInfo _testMethod;
  /// <inheritdoc/>
  public string Name { get; }
  /// <inheritdoc/>
  public bool IsAsync { get; }
  /// <inheritdoc/>
  public TestMethodType Type { get; }
  /// <inheritdoc/>
  public int? TimeoutMilliseconds { get; }

  /// <summary>
  /// Creates a new test method representation.
  /// </summary>
  /// <param name="testMethod">Method info of the test method.</param>
  /// <param name="type"></param>
  public TestMethod(MethodInfo testMethod, TestMethodType type)
  {
    _testMethod = testMethod;
    Name = testMethod.Name;
    IsAsync
      = testMethod.GetCustomAttribute<AsyncStateMachineAttribute>(false)
        != null;
    var customTimeout =
      testMethod.GetCustomAttribute<TimeoutAttribute>(false);
    if (customTimeout != null)
    {
      TimeoutMilliseconds = customTimeout.TimeoutMilliseconds;
    }
    Type = type;
  }

  /// <inheritdoc/>
  public async Task Invoke(TestClass testInstance)
  {
    if (IsAsync)
    {
      if (_testMethod.ReturnType == typeof(void))
      {
        throw new AsyncVoidException();
      }
      await (Task)_testMethod.Invoke(testInstance, null)!;
    }
    else
    {
      // Invoke test method synchronously when possible.
      _testMethod.Invoke(testInstance, null);
    }
  }

  /// <inheritdoc/>
  public async Task Invoke(TestClass testInstance, int timeoutMilliseconds)
  {
    // Implementation credit: https://stackoverflow.com/a/22078975
    using var timeoutCancellationTokenSource = new CancellationTokenSource();
    var task = Invoke(testInstance);
    // Prefer the timeout duration specified by the test method, if any.
    var completedTask = await Task.WhenAny(
      task,
      Task.Delay(
        TimeoutMilliseconds ?? timeoutMilliseconds,
        timeoutCancellationTokenSource.Token
      )
    );
    if (completedTask == task)
    {
      timeoutCancellationTokenSource.Cancel();
      // Re-await task to propagate exceptions correctly.
      await task;
    }
    else
    {
      throw new TestTimeoutException($"Test method [{Name}] timed out.");
    }
  }
}
