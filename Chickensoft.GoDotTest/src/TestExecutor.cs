namespace Chickensoft.GoDotTest;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Component which runs individual test methods on a test suite, in the
/// correct order.
/// </summary>
public interface ITestExecutor
{
  /// <summary>
  /// Whether the test executor should stop at the first exception it
  /// encounters.
  /// </summary>
  bool StopOnError { get; }

  /// <summary>
  /// Whether the remaining test methods in a suite should be skipped if
  /// an error occurs in one of the methods.
  /// </summary>
  bool Sequential { get; }

  /// <summary>
  /// Timeout for test methods in the test suite, if any, in milliseconds.
  /// </summary>
  int TimeoutMilliseconds { get; }

  /// <summary>
  /// Runs a test suite using the specified node as the scene root.
  /// </summary>
  /// <param name="sceneRoot">Test scene root node.</param>
  /// <param name="ops">Test operations to execute.</param>
  /// <param name="reporter">Test reporter.</param>
  /// <returns>A future which completes when the test suite finishes
  /// execution.</returns>
  Task Run(
    Node sceneRoot, List<TestOp> ops, ITestReporter reporter
  );
}

/// <summary>
/// Default <see cref="ITestExecutor"/> implementation. Runs test suite
/// methods in order, synchronously. SetupAll methods are called once before
/// the test. CleanupAll methods are called once after the test. Setup and
/// Cleanup methods are called before and after each test method,
/// respectively.
/// </summary>
public class TestExecutor : ITestExecutor
{
  /// <summary>
  /// If true, the test executor will stop executing test methods after the
  /// first error occurs.
  /// </summary>
  public bool StopOnError { get; }

  /// <summary>
  /// Amount of time to wait for a test method to finish before timing out.
  /// </summary>
  public int TimeoutMilliseconds { get; }

  /// <summary>
  /// True if the test executor should skip subsequent test methods in an
  /// individual test suite after encountering an error in one of the suite's
  /// test methods.
  /// </summary>
  public bool Sequential { get; }

  private readonly ITestMethodExecutor _methodExecutor;

  /// <summary>
  /// Creates a new test executor.
  /// </summary>
  /// <param name="methodExecutor">Test method executor.</param>
  /// <param name="stopOnError">True if the executor should stop at the first
  /// exception encountered.</param>
  /// <param name="sequential"></param>
  /// <param name="timeoutMilliseconds">Timeout for each test method, if any,
  /// in milliseconds.</param>
  public TestExecutor(
    ITestMethodExecutor methodExecutor,
    bool stopOnError,
    bool sequential,
    int timeoutMilliseconds = 0
  )
  {
    _methodExecutor = methodExecutor;
    StopOnError = stopOnError;
    Sequential = sequential;
    TimeoutMilliseconds = timeoutMilliseconds;
  }

  /// <inheritdoc/>
  public async Task Run(
    Node sceneRoot, List<TestOp> ops, ITestReporter reporter
  )
  {
    reporter.Update(TestEvent.Started);
    try
    {
      foreach (var op in ops)
      {
        await Run(sceneRoot, op, reporter);
      }
    }
    catch (StoppedException)
    {
      // The only exception thrown by `Run` is a StoppedException.
      // We don't need to do anything here — test execution is already stopped
      // by the time we get here.
    }
    finally
    {
      reporter.Update(TestEvent.Finished);
      reporter.OutputFinalReport();
    }
  }

  /// <summary>
  /// Executes a single test operation.
  /// </summary>
  /// <param name="sceneRoot">Test scene root node.</param>
  /// <param name="op">A test operation to execute.</param>
  /// <param name="reporter">Test reporter to receive test events.</param>
  /// <returns>Asynchronous task that completes when the test operation has
  /// finished running.</returns>
  /// <exception cref="StoppedException"></exception>
  protected async Task Run(
    Node sceneRoot, TestOp op, ITestReporter reporter
  )
  {
    var suite = op.Suite;

    var instance = (TestClass)Activator.CreateInstance(
      suite.TestClassType, sceneRoot
    )!;
    // Chain the test methods together in the order they should be executed.
    // ---------------------------------------------------------------------
    // All methods tagged with [SetupAll] attributes, followed by a series
    // of [Test] methods, each preceded by the list of [Setup] methods and
    // followed by the list of [Cleanup] methods.
    // Finally, all methods tagged with [CleanupAll].
    // ---------------------------------------------------------------------
    var allMethods = GetMethodExecutionSequence(op);
    var skip = false;
    var errorEncountered = false;
    reporter.SuiteUpdate(suite, TestSuiteEvent.Started);
    foreach (var method in allMethods)
    {
      try
      {
        var isCleanupMethod =
          method.Type is TestMethodType.Cleanup or TestMethodType.CleanupAll;
        var skipCurrentMethod = skip && !isCleanupMethod;
        if (skipCurrentMethod)
        {
          reporter.MethodUpdate(suite, method, TestMethodEvent.Skipped());
        }
        else
        {
          reporter.MethodUpdate(suite, method, TestMethodEvent.Started());
          await _methodExecutor.Run(method, instance, TimeoutMilliseconds);
          reporter.MethodUpdate(suite, method, TestMethodEvent.Passed());
        }
      }
      catch (Exception e)
      {
        errorEncountered = true;
        var exception = e.InnerException ?? e;
        reporter.MethodUpdate(
          suite, method, TestMethodEvent.Failed(exception)
        );
        if (Sequential || suite.Sequential)
        { skip = true; }

        // Run every error handling method on the suite when an error is
        // encountered.
        foreach (var failureMethod in suite.FailureMethods)
        {
          try
          {
            reporter.MethodUpdate(suite, method, TestMethodEvent.Started());
            await _methodExecutor.Run(
              failureMethod, instance, TimeoutMilliseconds
            );
            reporter.MethodUpdate(suite, method, TestMethodEvent.Passed());
          }
          catch (Exception failureException)
          {
            failureException =
              failureException.InnerException ?? failureException;
            reporter.MethodUpdate(
              suite, method, TestMethodEvent.Failed(failureException)
            );
          }
        }

        if (StopOnError)
        {
          throw new StoppedException(exception);
        }
      }
    }
    reporter.SuiteUpdate(
      suite,
      errorEncountered
        ? TestSuiteEvent.ErrorEncountered
        : TestSuiteEvent.Finished
      );
  }

  public static IEnumerable<ITestMethod> GetMethodExecutionSequence(TestOp op)
  {
    var methods =
        // Differentiate between the different types of test operations.
        // For test suite operations, we want to run all the test methods in
        // the suite. Otherwise, if it's an individual test operation, we just
        // need to run the single test method (along with any setup/cleanup
        // methods).
        op switch
        {
          IndividualTestOp individualOp => [individualOp.Method],
          TestSuiteOp suiteOp => suiteOp.Suite.TestMethods,
          _ => [] // never happens
        };

    if (methods.Count == 0)
    {
      return [];
    }

    return op.Suite.SetupAllMethods.Concat(methods.SelectMany(
        testMethod =>
          op.Suite
            .SetupMethods.Append(testMethod)
            .Concat(op.Suite.CleanupMethods)
      )
    ).Concat(op.Suite.CleanupAllMethods);
  }
}
