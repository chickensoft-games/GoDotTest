namespace GoDotTest {
  /// <summary>
  /// Test environment used by the test system.
  /// </summary>
  public interface ITestEnvironment {
    /// <summary>
    /// OS command line arguments.
    /// </summary>
    string[] CommandLineArgs { get; }
    /// <summary>
    /// Set to true to instruct the application to run tests instead of playing
    /// the game.
    /// </summary>
    bool ShouldRunTests { get; }
    /// <summary>
    /// Set to true if the the test runner should quit godot after the
    /// tests have finished running.
    /// </summary>
    bool QuitOnFinish { get; }
    /// <summary>
    /// Set to true if the test runner should stop executing tests after
    /// encountering an exception thrown from a test suite method. If this
    /// is false, the test runner will continue executing subsequent test
    /// methods and test suites. Setting this to true can be useful in a CI
    /// setting where you don't need to run all of the tests if one fails.
    /// </summary>
    bool StopOnError { get; }
    /// <summary>
    /// Set to true if the test runner should skip subsequent test methods in
    /// an individual test suite after encountering an error in one of the
    /// suite's test methods. This can be useful when each of the test methods
    /// in a suite depend on the success of the previous method.
    /// <br />
    /// Note that the test runner always invokes suites sequentially and
    /// synchronously. The sequential flag simply indicates whether or not the
    /// remaining methods in a suite should be skipped if an error occurs in one
    /// of the methods.
    /// </summary>
    bool Sequential { get; }

    /// <summary>
    /// Name or glob pattern of test suite to run.
    /// </summary>
    string? TestPatternToRun { get; }
  }

  /// <summary>
  /// Default <see cref="ITestEnvironment"/> implementation used by the test
  /// system.
  /// </summary>
  public class TestEnvironment : ITestEnvironment {
    /// <inheritdoc/>
    public string[] CommandLineArgs { get; }
    /// <inheritdoc/>
    public bool ShouldRunTests { get; }
    /// <inheritdoc/>
    public bool QuitOnFinish { get; }
    /// <inheritdoc/>
    public bool StopOnError { get; }
    /// <inheritdoc/>
    public bool Sequential { get; }
    /// <inheritdoc/>
    public string? TestPatternToRun { get; }

    /// <summary>Flag which indicates tests should be run.</summary>
    public const string TEST_FLAG = "--run-tests";
    /// <summary>Flag which indicates the program should exit on finish.
    /// </summary>
    public const string QUIT_ON_FINISH_FLAG = "--quit-on-finish";
    /// <summary>Flag which indicates it should stop on the first error rather
    /// than continuing to run the remaining tests.</summary>
    public const string STOP_ON_ERROR_FLAG = "--stop-on-error";
    /// <summary>Flag which indicates subsequent methods in a test suite
    /// should be skipped if an error is encountered.</summary>
    public const string SEQUENTIAL_FLAG = "--sequential";
    /// <summary>Default value for quit on finish.</summary>
    public const bool DEFAULT_QUIT_ON_FINISH = false;
    /// <summary>Default value for stop on error.</summary>
    public const bool DEFAULT_STOP_ON_ERROR = false;
    /// <summary>Default value sequential.</summary>
    public const bool DEFAULT_SEQUENTIAL = false;

    /// <summary>
    /// Creates a new test environment with the specified configuration
    /// settings.
    /// </summary>
    public TestEnvironment(
      bool shouldRunTests,
      bool quitOnFinish,
      bool stopOnError,
      bool sequential,
      string? testPatternToRun
    ) {
      ShouldRunTests = shouldRunTests;
      QuitOnFinish = quitOnFinish;
      StopOnError = stopOnError;
      Sequential = sequential;
      TestPatternToRun = testPatternToRun;
      CommandLineArgs = new string[] { };
    }

    /// <summary>
    /// Creates a new test environment from the specified command line
    /// arguments.
    /// </summary>
    /// <param name="commandLineArgs">Command line args.</param>
    /// <returns>A new test environment.</returns>
    public static TestEnvironment From(string[] commandLineArgs) {
      var quitOnFinish = DEFAULT_QUIT_ON_FINISH;
      var stopOnError = DEFAULT_STOP_ON_ERROR;
      var sequential = DEFAULT_SEQUENTIAL;
      var shouldRunTests = false;
      string? testPatternToRun = null;
      foreach (var arg in commandLineArgs) {
        var clean = arg.Trim().Replace(" ", "");
        var flag = clean.ToLower();
        var value = !flag.EndsWith("=false");
        if (flag.StartsWith(TEST_FLAG)) {
          shouldRunTests = true;
          if (flag.StartsWith(TEST_FLAG + "=")) {
            testPatternToRun = clean[(TEST_FLAG.Length + 1)..];
          }
        }
        else if (flag.StartsWith(QUIT_ON_FINISH_FLAG)) {
          quitOnFinish = value;
        }
        else if (flag.StartsWith(STOP_ON_ERROR_FLAG)) {
          stopOnError = value;
        }
        else if (flag.StartsWith(SEQUENTIAL_FLAG)) {
          sequential = value;
        }
      }
      return new TestEnvironment(
        shouldRunTests, quitOnFinish, stopOnError, sequential, testPatternToRun
      );
    }
  }
}
