namespace Chickensoft.GoDotTest;

using System.Threading.Tasks;

/// <summary>
/// Test method executor for running a test method on a given test suite
/// instance.
/// </summary>
public interface ITestMethodExecutor
{
  /// <summary>
  /// Runs a test method on a given test suite instance with the given
  /// timeout.
  /// </summary>
  /// <param name="method">Test method to run.</param>
  /// <param name="instance">Test suite instance.</param>
  /// <param name="timeoutMilliseconds">Timeout in milliseconds.</param>
  /// <returns>
  /// A task which completes when the test method completes.
  /// </returns>
  Task Run(ITestMethod method, TestClass instance, int timeoutMilliseconds);
}

/// <summary>
/// Default test method executor.
/// </summary>
public class TestMethodExecutor : ITestMethodExecutor
{
  /// <inheritdoc/>
  public async Task Run(
    ITestMethod method, TestClass instance, int timeoutMilliseconds
  )
  {
    if (timeoutMilliseconds is 0)
    {
      await method.Invoke(instance);
    }
    else
    {
      await method.Invoke(instance, timeoutMilliseconds);
    }
  }
}
