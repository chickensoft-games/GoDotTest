namespace GoDotTest {
  using System.Threading.Tasks;

  public interface ITestMethodExecutor {
    Task Run(ITestMethod method, TestClass instance, int timeoutMilliseconds);
  }

  public class TestMethodExecutor : ITestMethodExecutor {
    /// <summary>
    /// Runs a single test method.
    /// </summary>
    /// <param name="method">The test method to run.</param>
    /// <param name="instance">An instance of the test method's test
    /// suite.</param>
    /// <returns>An asynchronous task that completes when the method has
    /// finished running.</returns>
    public async Task Run(
      ITestMethod method, TestClass instance, int timeoutMilliseconds
    ) {
      if (timeoutMilliseconds is 0) {
        await method.Invoke(instance);
      }
      else {
        await method.Invoke(instance, timeoutMilliseconds);
      }
    }
  }
}
