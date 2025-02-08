namespace Chickensoft.GoDotTest;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.Log;
using GoDotCollections;

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
  > Failures { get; } = [];

  /// <inheritdoc/>
  public bool HadError => Failures.Count > 0;

  /// <summary>Log used to output test results.</summary>
  protected ILog Log { get; }

  /// <summary>
  /// The number of test methods that were skipped.
  /// </summary>
  public int NumSkippedMethods { get; private set; }

  /// <summary>
  /// The number of test methods which have successfully passed.
  /// </summary>
  public int NumPassingMethods { get; private set; }

  /// <summary>
  /// The number of test methods which have failed to pass.
  /// </summary>
  public int NumFailingMethods
    => Failures.Values.Sum(static failingMethods => failingMethods.Count);

  /// <summary>
  /// Create a test reporter.
  /// </summary>
  /// <param name="log">Log used to output test results.</param>
  public TestReporter(ILog log) {
    Log = log;
  }

  /// <inheritdoc/>
  public void MethodUpdate(
    ITestSuite suite, ITestMethod method, TestMethodEvent methodEvent
  ) {
    // Avoid cluttering logs by silencing successful utility method runs.
    if (
      (methodEvent is TestMethodStartedEvent or TestMethodPassedEvent) &&
      method.Type != TestMethodType.Test
    ) { return; }

    if (methodEvent is TestMethodPassedEvent) {
      Log.Print(Prefix(suite, method, GOOD) + "Test passed! :)");
      NumPassingMethods++;
    }
    else if (methodEvent is TestMethodFailedEvent failure) {
      Log.Print(Prefix(suite, method, BAD) + "Test failed! :(");
      AddFailure(suite, method, failure.FailureException);
    }
    else if (methodEvent is TestMethodSkippedEvent) {
      Log.Print(Prefix(suite, method, BLANK) + "Test skipped! :|");
      NumSkippedMethods++;
    }
    else if (methodEvent is TestMethodStartedEvent) {
      Log.Print(Prefix(suite, method, BLANK) + "Test started! :3");
    }
  }

  /// <inheritdoc/>
  public void SuiteUpdate(ITestSuite suite, TestSuiteEvent suiteEvent) {
    if (suiteEvent is TestSuiteEvent.Started) {
      Log.Print(Prefix(suite, BLANK) + "Test suite started! :3");
    }
    else if (suiteEvent is TestSuiteEvent.Finished) {
      Log.Print(Prefix(suite, GOOD) + "Test suite finished! :D");
    }
    else if (suiteEvent is TestSuiteEvent.ErrorEncountered) {
      // Only is sent when we are supposed to exit on the first error.
      Log.Print(Prefix(suite, BAD) + "Test suite error. Aborting! :(");
    }
  }

  /// <inheritdoc/>
  public void Update(TestEvent testEvent) {
    if (testEvent is TestEvent.Started) {
      Log.Print(Prefix(BLANK) + "Started testing! :3");
    }
    else if (testEvent is TestEvent.Finished) {
      var smiley = HadError ? ":(" : ":D";
      var status = HadError ? BAD : GOOD;
      Log.Print(Prefix(status) + $"Finished testing! {smiley}");
    }
  }

  /// <inheritdoc/>
  public void OutputFinalReport() {
    var numFailingMethods = Failures.Values.Sum(
      static failingMethods => failingMethods.Count
    );

    if (HadError) {
      foreach (var (suite, methods) in Failures) {
        foreach (var method in methods.Keys) {
          var e = methods[method]!;
          Log.Print(
            Prefix(suite, method, BAD) + $"Error occurred: {e.Message}"
          );
          Log.Print(e);
        }
      }
    }

    Log.Print(
      Prefix(HadError ? BAD : GOOD) +
      "Test results: " +
      $"Passed: {NumPassingMethods} | " +
      $"Failed: {NumFailingMethods} | " +
      $"Skipped: {NumSkippedMethods}"
    );
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
    if (!Failures.ContainsKey(suite)) { Failures[suite] = new(); }
    Failures[suite][method] = e;
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
  /// Create a log prefix for the given test suite.
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
  protected string Prefix(string status) => $"{status} ";
}
