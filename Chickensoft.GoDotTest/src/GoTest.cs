namespace Chickensoft.GoDotTest;

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Chickensoft.Log;
using Godot;

/// <summary>
/// <para>
/// GoDotTest is a simple, opinionated test runner for Godot. It finds test
/// suites (classes which extend <see cref="TestClass"/>) and runs the
/// ones that match the test environment name glob pattern (or all of them
/// if there's no pattern and the environment indicates that tests should
/// be run).
/// </para>
/// <para>
/// Test methods are indicated by the [Test] attribute and are run one at a
/// time on each test suite in the order they are declared. If they are
/// asynchronous, each one is awaited before moving on to the next.
/// </para>
/// <para>
/// GoDotTest encourages you to use the mocking and assertion frameworks of
/// your choice. Certain asynchronous utilities are provided to simulate input
/// and to allow frame execution to occur for specific periods of time to make
/// testing easier, but everything else is up to you.
/// </para>
/// <para>
/// For information on how to debug tests from the editor and collect code
/// coverage, please see the accompanying documentation.
/// </para>
/// </summary>
public class GoTest {
  /// <summary>
  /// The default test adapter used to construct test system objects.
  /// </summary>
  public static readonly ITestAdapter DefaultAdapter = new TestAdapter();
  /// <summary>
  /// Test adapter to use when constructing objects needed for testing.
  /// A useless abstraction, but allows for better unit testing of the test
  /// system itself.
  /// </summary>
  public static ITestAdapter Adapter { get; set; } = DefaultAdapter;
  /// <summary>
  /// Timeout for each Test, Setup and SetupAll
  /// </summary>
  public static int TimeoutMilliseconds { get; set; } = 10000;

  /// <summary>Default action to perform for exiting.</summary>
  public static Action<Node, int> DefaultOnExit { get; }
    = static (node, exitCode) => node.GetTree().Quit(exitCode);

  /// <summary>
  /// Force exit (for use when running coverage to work around Godot 4's exit
  /// behavior).
  /// <br />
  /// Godot 4's exit behavior doesn't seem to exit gracefully as far as
  /// coverlet is concerned, so we have to force exit through C# to get
  /// coverlet to realize we've finished. If we don't, coverlet cannot generate
  /// the coverage report. In Godot 3, this was not an issue.
  /// <br />
  /// See coverlet docs about expected exit behavior: https://t.ly/A51Q
  /// </summary>
  public static Action<Node, int> DefaultOnForceExit { get; } = static (node, exitCode)
    => System.Environment.Exit(exitCode);

  /// <summary>Action to perform for exiting.</summary>
  public static Action<Node, int> OnExit { get; set; } = DefaultOnExit;

  /// <summary>Action to perform for force-exiting.</summary>
  public static Action<Node, int> OnForceExit { get; set; }
    = DefaultOnForceExit;

  /// <summary>
  /// <para>
  /// Runs tests indicated to be run by the test environment. If the test
  /// environment does not indicate that tests should be run, nothing will
  /// happen.
  /// </para>
  /// <para>
  /// Tests are run in the specified scene root node, allowing them to attach
  /// other nodes and scenes as needed.
  /// </para>
  /// </summary>
  /// <param name="assembly">Assembly to load test files from.</param>
  /// <param name="sceneRoot">Scene root node hosting the tests.</param>
  /// <param name="env">The test environment containing test configuration
  /// settings.</param>
  /// <param name="log">A log for outputting messages.</param>
  /// <param name="predicate">Function which receives a test suite and returns
  /// whether the suite should be executed.</param>
  /// <returns>An asynchronous task that completes when the tests have
  /// finished running.</returns>
  public static async Task RunTests(
    Assembly assembly,
    Node sceneRoot,
    ITestEnvironment? env = null,
    ILog? log = null,
    Func<TestOp, bool>? predicate = null
  ) {
    var suiteFilter = predicate ?? (static suite => true);
    env = Adapter.CreateTestEnvironment(env);
    log = Adapter.CreateLog(log);
    if (!env.ShouldRunTests) { return; }
    using var listenerManager = new TraceListenerManager(env);
    var provider = Adapter.CreateProvider();
    var pattern = env.TestPatternToRun;
    var ops = (pattern == null)
      ? provider.GetTestOps(assembly)
      : provider.GetTestOpsByPattern(assembly, pattern);
    ops = [.. ops.Where(suiteFilter)];
    var reporter = Adapter.CreateReporter(log);
    var methodExecutor = Adapter.CreateMethodExecutor();
    var executor = Adapter.CreateExecutor(
      methodExecutor: methodExecutor,
      stopOnError: env.StopOnError,
      sequential: env.Sequential,
      timeoutMilliseconds: TimeoutMilliseconds
    );
    await executor.Run(sceneRoot, ops, reporter);
    if (env.QuitOnFinish) {
      var exitCode = reporter.HadError ? 1 : 0;
      var exitFn = env.Coverage ? OnForceExit : OnExit;
      exitFn(sceneRoot, exitCode);
    }
  }
}
