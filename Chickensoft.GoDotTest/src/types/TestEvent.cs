namespace Chickensoft.GoDotTest;

using System;

/// <summary>Represents an event in the entire test system.
/// </summary>
public enum TestEvent {
  /// <summary>Test execution has started.</summary>
  Started,
  /// <summary>Test execution has finished.</summary>
  Finished,
}

/// <summary>Represents an event in a test suite.</summary>
public enum TestSuiteEvent {
  /// <summary>Test suite execution has started.</summary>
  Started,
  /// <summary>Test suite execution has finished successfully.</summary>
  Finished,
  /// <summary>Test suite execution finished with an error.</summary>
  ErrorEncountered
}

/// <summary>Represents a test method execution result.</summary>
public abstract class TestMethodEvent {
  private static readonly TestMethodPassedEvent _passed = new();
  private static readonly TestMethodSkippedEvent _skipped = new();
  private static readonly TestMethodStartedEvent _started = new();

  /// <summary>Test method passed event.</summary>
  /// <returns>The test method passed event.</returns>
  public static TestMethodPassedEvent Passed() => _passed;
  /// <summary>Test method skipped event.</summary>
  /// <returns>The test method skipped event.</returns>
  public static TestMethodSkippedEvent Skipped() => _skipped;
  /// <summary>Test method started event.</summary>
  /// <returns>The test method started event.</returns>
  public static TestMethodStartedEvent Started() => _started;
  /// <summary>Test method failed event.</summary>
  /// <param name="e"></param>
  /// <returns>A test method failed event.</returns>
  public static TestMethodFailedEvent Failed(Exception e) => new(e);
}

/// <summary>Represents a test method started event.</summary>
public class TestMethodStartedEvent : TestMethodEvent { }

/// <summary>Represents a successfully completed test.</summary>
public class TestMethodPassedEvent : TestMethodEvent { }

/// <summary>Represents a test which failed during execution.</summary>
public class TestMethodFailedEvent : TestMethodEvent {
  /// <summary>
  /// Exception that occurred while running the test method.
  /// </summary>
  public readonly Exception FailureException;

  /// <summary>
  /// Create a new TestMethodFailedEvent with the specified exception.
  /// </summary>
  /// <param name="failureException">Exception that occurred while running the
  /// test method.</param>
  public TestMethodFailedEvent(Exception failureException) {
    FailureException = failureException;
  }
}

/// <summary>Represents a test which was skipped.</summary>
public class TestMethodSkippedEvent : TestMethodEvent { }
