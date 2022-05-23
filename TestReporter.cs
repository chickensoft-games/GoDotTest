namespace GoDotTest {
  using System;
  using System.Collections.Generic;
  using GoDotNet;

  /// <summary>
  /// Class which stores results for each test suite method.
  /// </summary>
  public interface ITestReporter {
    /// <summary>True if an error was encountered in any test suite.</summary>
    bool HadError { get; }
    /// <summary>
    /// Called when an event concerning a test suite method has occurred.
    /// </summary>
    /// <param name="suite">Test suite which is running.</param>
    /// <param name="method">Test suite method which was executed.</param>
    /// <param name="methodEvent">Test event.</param>
    void MethodUpdate(
      ITestSuite suite, ITestMethod method, TestMethodEvent methodEvent
    );

    /// <summary>
    /// Called when an event concerning a test suite has occurred.
    /// </summary>
    /// <param name="suite">Test suite.</param>
    /// <param name="suiteEvent">Test suite event.</param>
    void SuiteUpdate(ITestSuite suite, TestSuiteEvent suiteEvent);

    /// <summary>
    /// Called when an event concerning the entire test system has occurred.
    /// </summary>
    /// <param name="testEvent">Test event which occurred.</param>
    void Update(TestEvent testEvent);

    /// <summary>
    /// Called after tests have run to report the results.
    /// </summary>
    void OutputFinalReport();
  }

  /// <summary>
  /// Default <see cref="ITestReporter"/> implementation for storing test suite
  /// method results.
  /// </summary>
  public class TestReporter : ITestReporter {
    /// <summary>Status prefix used for good messages.</summary>
    protected const string GOOD = "> OK >>";
    /// <summary>Status prefix used for bad messages.</summary>
    protected const string BAD = "> !! >>";
    /// <summary>Status prefix used for neutral messages.</summary>
    protected const string BLANK = "> ^^ >>";

    /// <summary>
    /// Dictionary of test suite method failures. Each key is a test suite which
    /// provides a dictionary of test methods and their corresponding exception.
    /// </summary>
    protected Dictionary<
      ITestSuite, Map<ITestMethod, Exception>
    > _failures { get; } = new();

    /// <inheritdoc/>
    public bool HadError => _failures.Count > 0;

    /// <summary>Log used to output test results.</summary>
    protected ILog _log { get; }

    /// <summary>
    /// Create a test reporter.
    /// </summary>
    /// <param name="log">Log used to output test results.</param>
    public TestReporter(ILog log) => _log = log;

    /// <inheritdoc/>
    public void MethodUpdate(
      ITestSuite suite, ITestMethod method, TestMethodEvent testEvent
    ) {
      // Avoid cluttering logs by silencing successful utility method runs.
      if (
        (testEvent is TestMethodStartedEvent or TestMethodPassedEvent) &&
        method.Type != TestMethodType.Test
      ) { return; }

      if (testEvent is TestMethodPassedEvent) {
        _log.Print(Prefix(suite, method, GOOD) + "Test passed! :)");
      }
      else if (testEvent is TestMethodFailedEvent failure) {
        _log.Print(Prefix(suite, method, BAD) + "Test failed! :(");
        AddFailure(suite, method, failure.FailureException);
      }
      else if (testEvent is TestMethodSkippedEvent) {
        _log.Print(Prefix(suite, method, BLANK) + "Test skipped! :|");
      }
      else if (testEvent is TestMethodStartedEvent) {
        _log.Print(Prefix(suite, method, BLANK) + "Test started! :3");
      }
    }

    /// <inheritdoc/>
    public void SuiteUpdate(ITestSuite suite, TestSuiteEvent suiteEvent) {
      if (suiteEvent is TestSuiteEvent.Started) {
        _log.Print(Prefix(suite, BLANK) + "Test suite started! :3");
      }
      else if (suiteEvent is TestSuiteEvent.Finished) {
        _log.Print(Prefix(suite, GOOD) + "Test suite finished! :D");
      }
      else if (suiteEvent is TestSuiteEvent.ErrorEncountered) {
        // Only is sent when we are supposed to exit on the first error.
        _log.Print(Prefix(suite, BAD) + "Test suite error. Aborting! :(");
      }
    }

    /// <inheritdoc/>
    public void Update(TestEvent testEvent) {
      if (testEvent is TestEvent.Started) {
        _log.Print(Prefix(BLANK) + "Started testing! :3");
      }
      else if (testEvent is TestEvent.Finished) {
        var smiley = HadError ? ":(" : ":D";
        var status = HadError ? BAD : GOOD;
        _log.Print(Prefix(status) + $"Finished testing! {smiley}");
      }
    }

    /// <inheritdoc/>
    public void OutputFinalReport() {
      if (HadError) {
        foreach (var (suite, methods) in _failures) {
          foreach (var method in methods.Keys) {
            var e = methods[method];
            _log.Print(
              Prefix(suite, method, BAD) + $"Error occurred: {e.Message}"
            );
            _log.Print(e);
          }
        }
      }
    }

    /// <summary>
    /// Adds a failure to the dictionary of test suite method failures.
    /// </summary>
    /// <param name="suite">Test suite where the error occurred.</param>
    /// <param name="method">Test method where the error occurred.</param>
    /// <param name="e">Exception that occurred.</param>
    protected void AddFailure(
      ITestSuite suite, ITestMethod method, Exception e
    ) {
      if (!_failures.ContainsKey(suite)) { _failures[suite] = new(); }
      _failures[suite][method] = e;
    }

    /// <summary>
    /// Create a log prefix for the given test suite method.
    /// </summary>
    /// <param name="suite">Test suite.</param>
    /// <param name="method">Test method.</param>
    /// <param name="status">Test method status.</param>
    /// <returns>A nicely formatted log prefix string.</returns>
    protected string Prefix(ITestSuite suite, ITestMethod method, string status)
      => $"{status} {suite.Name}::{method.Name} [{method.Type}] > ";

    /// <summary>
    /// Create a log prefix for the given test suite suite.
    /// </summary>
    /// <param name="suite">Test suite.</param>
    /// <param name="status">Test suite status.</param>
    /// <returns>A nicely formatted log prefix string.</returns>
    protected string Prefix(ITestSuite suite, string status)
      => $"{status} {suite.Name} > ";

    /// <summary>
    /// Create a log prefix based on the given status.
    /// </summary>
    /// <param name="status">Test status.</param>
    /// <returns>A nicely formatted log prefix string.</returns>
    protected string Prefix(string status) => $"{status} > ";
  }
}
