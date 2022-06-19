namespace GoDotTest {
  using GoDotLog;

  /// <summary>
  /// Represents an object which constructs classes needed for testing.
  /// </summary>
  public interface ITestAdapter {
    /// <summary>
    /// Creates a test executor.
    /// </summary>
    /// <param name="methodExecutor">Test method executor.</param>
    /// <param name="stopOnError">Stop on the first error (or not).</param>
    /// <param name="sequential">Whether to skip subsequent tests in a suite.
    /// </param>
    /// <param name="timeoutMilliseconds">Timeout for test methods, in
    /// milliseconds.</param>
    /// <returns>Test executor.</returns>
    ITestExecutor CreateExecutor(
      ITestMethodExecutor methodExecutor,
      bool stopOnError,
      bool sequential,
      int timeoutMilliseconds
    );
    /// <summary>
    /// Creates a test method executor.
    /// </summary>
    /// <returns>Test method executor.</returns>
    ITestMethodExecutor CreateMethodExecutor();
    /// <summary>
    /// Creates a test provider.
    /// </summary>
    /// <returns>A test provider.</returns>
    ITestProvider CreateProvider();
    /// <summary>
    /// Creates a test reporter.
    /// </summary>
    /// <param name="log">Log to use for outputting reports.</param>
    /// <returns>A test reporter.</returns>
    ITestReporter CreateReporter(ILog log);
  }

  /// <summary>
  /// Default test adapter implementation.
  /// </summary>
  public class TestAdapter : ITestAdapter {
    /// <inheritdoc/>
    public virtual ITestProvider CreateProvider() => new TestProvider();
    /// <inheritdoc/>
    public virtual ITestReporter CreateReporter(ILog log)
      => new TestReporter(log);
    /// <inheritdoc/>
    public virtual ITestMethodExecutor CreateMethodExecutor()
      => new TestMethodExecutor();
    /// <inheritdoc/>
    public virtual ITestExecutor CreateExecutor(
      ITestMethodExecutor methodExecutor,
      bool stopOnError,
      bool sequential,
      int timeoutMilliseconds
    ) => new TestExecutor(
      methodExecutor: methodExecutor,
      stopOnError: stopOnError,
      sequential: sequential,
      timeoutMilliseconds: timeoutMilliseconds
    );
  }
}
