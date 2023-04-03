namespace Chickensoft.GoDotTest;

using System;

/// <summary>
/// Base class for all exceptions thrown by GoDotTest.
/// </summary>
public abstract class TestRunnerException : Exception {
  /// <summary>
  /// Creates a new test runner exception with the specified message.
  /// </summary>
  /// <param name="message">Exception message.</param>
  protected TestRunnerException(string message) : base(message) { }

  /// <summary>
  /// Creates a new test runner exception with the specified message and
  /// inner exception.
  /// </summary>
  /// <param name="message">Exception message.</param>
  /// <param name="innerException">Exception which occurred.</param>
  protected TestRunnerException(string message, Exception? innerException) :
    base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a test class uses an async void test method.
/// </summary>
public class AsyncVoidException : TestRunnerException {
  private const string MESSAGE =
    "Async void methods are not supported in tests. For more background on " +
    "the challenges posed by async void methods, see " +
    "http://haacked.com/archive/2014/11/11/async-void-methods/";

  /// <summary>
  /// Creates a new async void exception.
  /// </summary>
  public AsyncVoidException() :
    base(MESSAGE) { }
}

/// <summary>
/// Exception thrown when a test method takes too long to run.
/// </summary>
public class TestTimeoutException : TestRunnerException {
  /// <summary>
  /// Creates a new test timeout exception.
  /// </summary>
  /// <param name="message">Timeout message.</param>
  public TestTimeoutException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when the first error is encountered while running tests
/// and the test environment indicates that no further tests should be run
/// after encountering the first error.
/// </summary>
public class StoppedException : TestRunnerException {
  private const string MESSAGE =
    "Test execution stopped because of an error.";

  /// <summary>
  /// Creates a new stopped exception.
  /// </summary>
  /// <param name="innerException">Exception that occurred.</param>
  public StoppedException(Exception innerException) :
    base(MESSAGE, innerException) { }
}
