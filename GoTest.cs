namespace GoDotTest {
  using System.Threading.Tasks;
  using Godot;
  using GoDotNet;

  /// <summary>
  /// GoDotTest is a simple, opinionated test runner for Godot. It finds test
  /// suites (classes which extend <see cref="TestClass"/>) and runs the
  /// ones that match the test environment name glob pattern (or all of them
  /// if there's no pattern and the environment indicates that tests should
  /// be run).
  ///
  /// Test methods are indicated by the [Test] attribute and are run one at a
  /// time on each test suite in the order they are declared. If they are
  /// asynchronous, each one is awaited before moving on to the next.
  ///
  /// GoDotTest encourages you to use the mocking and assertion frameworks of
  /// your choice. Certain asynchronous utilities are provided to simulate input
  /// and to allow frame execution to occur for specific periods of time to make
  /// testing easier, but everything else is up to you.
  ///
  /// For information on how to debug tests from the editor and collect code
  /// coverage, please see the accompanying documentation.
  /// </summary>
  public class GoTest {
    /// <summary>
    /// Runs tests indicated to be run by the test environment. If the test
    /// environment does not indicate that tests should be run, nothing will
    /// happen.
    ///
    /// Tests are run in the specified scene root node, allowing them to attach
    /// other nodes and scenes as needed.
    /// </summary>
    /// <param name="sceneRoot">Scene root node hosting the tests.</param>
    /// <param name="env">The test environment containing test configuration
    /// settings.</param>
    /// <param name="log">A log for outputting messages.</param>
    /// <returns>An asynchronous task that completes when the tests have
    /// finished running.</returns>
    public static async Task RunTests(
      Node sceneRoot, ITestEnvironment env, ILog log
    ) {
      if (!env.ShouldRunTests) { return; }
      var testProvider = new TestProvider();
      var testPattern = env.TestPatternToRun;
      var testSuites = (testPattern == null)
        ? testProvider.GetTestSuites()
        : testProvider.GetTestSuiteByPattern(testPattern);
      var testReporter = new TestReporter(log);
      var testRunner = new TestExecutor(
        env.StopOnError,
        env.Sequential,
        10000
      );
      await testRunner.Run(sceneRoot, testSuites, testReporter);
    }
  }
}
