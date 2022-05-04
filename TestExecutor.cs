namespace GoDotTest {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Godot;

  /// <summary>
  /// Component which runs individual test methods on a test suite, in the
  /// correct order.
  /// </summary>
  public interface ITestExecutor {
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
    /// <param name="suite">Test suite to run.</param>
    /// <param name="sceneRoot">Test scene root node.</param>
    /// <param name="reporter">Test reporter.</param>
    /// <returns>A future which completes when the test suite finishes
    /// execution.</returns>
    Task Run(Node sceneRoot, ITestSuite suite, ITestReporter reporter);
  }

  /// <summary>
  /// Default <see cref="ITestExecutor"/> implementation. Runs test suite
  /// methods in order, synchronously. SetupAll methods are called once before
  /// the test. CleanupAll methods are called once after the test. Setup and
  /// Cleanup methods are called before and after each test method,
  /// respectively.
  /// </summary>
  public class TestExecutor : ITestExecutor {
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
    /// <value></value>
    public bool Sequential { get; }

    /// <summary>
    /// Creates a new test executor.
    /// </summary>
    /// <param name="stopOnError">True if the executor should stop at the first
    /// exception encountered.</param>
    /// <param name="sequential"></param>
    /// <param name="timeoutMilliseconds">Timeout for each test method, if any,
    /// in milliseconds.</param>
    public TestExecutor(
      bool stopOnError,
      bool sequential,
      int timeoutMilliseconds = 0
    ) {
      StopOnError = stopOnError;
      Sequential = sequential;
      TimeoutMilliseconds = timeoutMilliseconds;
    }

    /// <summary>
    /// Runs a list of test suites.
    /// </summary>
    /// <param name="sceneRoot">Test scene root node.</param>
    /// <param name="suites">List of test suites to run.</param>
    /// <param name="reporter">Test reporter to receive test events.</param>
    /// <returns>Asynchronous task that completes when the test suites have
    /// finished running.</returns>
    public async Task Run(
      Node sceneRoot, List<ITestSuite> suites, ITestReporter reporter
    ) {
      try {
        foreach (var suite in suites) {
          await Run(sceneRoot, suite, reporter);
        }
      }
      catch (StoppedException) {
        // The only exception thrown by `Run` is a StoppedOnException.
      }
      finally {
        reporter.OutputFinalReport();
      }
    }

    /// <summary>
    /// Runs a single test suite.
    /// </summary>
    /// <param name="sceneRoot">Test scene root node.</param>
    /// <param name="suite">A test suites to run.</param>
    /// <param name="reporter">Test reporter to receive test events.</param>
    /// <returns>Asynchronous task that completes when the test suite has
    /// finished running.</returns>
    public async Task Run(
      Node sceneRoot, ITestSuite suite, ITestReporter reporter
    ) {
      var instance = (TestClass)Activator.CreateInstance(
        suite.TestClassType, sceneRoot
      );
      // Chain the test methods together in the order they should be executed.
      // ---------------------------------------------------------------------
      // All methods tagged with [SetupAll] attributes, followed by a series
      // of [Test] methods, each preceded by the list of [Setup] methods and
      // followed by the list of [Cleanup] methods.
      // Finally, all methods tagged with [CleanupAll].
      // ---------------------------------------------------------------------
      var allMethods = suite.SetupAllMethods.Concat(
        suite.TestMethods.SelectMany(
          testMethod =>
            suite.SetupMethods.Append(testMethod).Concat(suite.CleanupMethods)
        )
      ).Concat(suite.CleanupAllMethods);
      var skip = false;
      var errorEncountered = false;
      reporter.SuiteUpdate(suite, TestSuiteEvent.Started);
      foreach (var method in allMethods) {
        try {
          if (!skip) {
            reporter.MethodUpdate(suite, method, TestMethodEvent.Started());
            await Run(method, instance);
            reporter.MethodUpdate(suite, method, TestMethodEvent.Passed());
          }
          else {
            reporter.MethodUpdate(suite, method, TestMethodEvent.Skipped());
          }
        }
        catch (Exception e) {
          errorEncountered = true;
          var innerException = e.InnerException;
          reporter.MethodUpdate(suite, method, TestMethodEvent.Failed(innerException ?? e));
          if (StopOnError) {
            throw new StoppedException(innerException ?? e);
          }
          if (Sequential || suite.Sequential) { skip = true; }
        }
      }
      reporter.SuiteUpdate(
        suite,
        errorEncountered
          ? TestSuiteEvent.ErrorEncountered
          : TestSuiteEvent.Finished
        );
    }

    /// <summary>
    /// Runs a single test method.
    /// </summary>
    /// <param name="method">The test method to run.</param>
    /// <param name="instance">An instance of the test method's test
    /// suite.</param>
    /// <returns>An asynchronous task that completes when the method has
    /// finished running.</returns>
    protected async Task Run(ITestMethod method, TestClass instance) {
      if (TimeoutMilliseconds is 0) {
        await method.Invoke(instance);
      }
      else {
        await method.Invoke(instance, TimeoutMilliseconds);
      }
    }
  }
}
