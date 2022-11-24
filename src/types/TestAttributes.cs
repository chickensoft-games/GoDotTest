namespace GoDotTest;
using System;
using System.Runtime.CompilerServices;

/// <summary>Base class for test method attributes.</summary>
public abstract class TestRunnerMethodAttribute : Attribute {
  /// <summary>
  /// Line the attribute was defined on.
  /// </summary>
  public readonly int Line;
  /// <summary>
  /// Creates a new MethodAttribute with the specified line number.
  /// </summary>
  /// <param name="line">Line number.</param>
  public TestRunnerMethodAttribute([CallerLineNumber] int line = 0)
    => Line = line;
}

/// <summary>
/// Attribute used to mark a method as a test.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestAttribute : TestRunnerMethodAttribute {
  /// <summary>
  /// Creates a new TestAttribute with the specified line number.
  /// </summary>
  /// <param name="line">Line number.</param>
  public TestAttribute([CallerLineNumber] int line = 0) : base(line) { }
}

/// <summary>
/// Attribute used to mark a setup method to be called once before all the
/// tests run.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SetupAllAttribute : TestRunnerMethodAttribute {
  /// <summary>
  /// Creates a new SetupAllAttribute with the specified line number.
  /// </summary>
  /// <param name="line">Line number.</param>
  public SetupAllAttribute([CallerLineNumber] int line = 0) : base(line) { }
}

/// <summary>
/// Attribute used to mark a cleanup method to be called once after all the
/// tests run.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CleanupAllAttribute : TestRunnerMethodAttribute {
  /// <summary>
  /// Creates a new CleanupAllAttribute with the specified line number.
  /// </summary>
  /// <param name="line">Line number.</param>
  public CleanupAllAttribute([CallerLineNumber] int line = 0) : base(line) { }
}

/// <summary>
/// Attribute used to mark a setup method to be called before each test runs.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SetupAttribute : TestRunnerMethodAttribute {
  /// <summary>
  /// Creates a new SetupAttribute with the specified line number.
  /// </summary>
  /// <param name="line">Line number.</param>
  public SetupAttribute([CallerLineNumber] int line = 0) : base(line) { }
}

/// <summary>
/// Attribute used to mark a cleanup method to be called after each test runs.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CleanupAttribute : TestRunnerMethodAttribute {
  /// <summary>
  /// Creates a new CleanupAttribute with the specified line number.
  /// </summary>
  /// <param name="line">Line number.</param>
  public CleanupAttribute([CallerLineNumber] int line = 0) : base(line) { }
}

/// <summary>
/// Test method attribute used to customize the timeout duration of a test
/// method. This overrides any global timeout settings.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TimeoutAttribute : Attribute {
  /// <summary>Test method timeout, in milliseconds.</summary>
  public int TimeoutMilliseconds { get; }
  /// <summary>
  /// Create a new timeout attribute with the specified timeout.
  /// </summary>
  /// <param name="timeoutMilliseconds">Number of milliseconds to wait
  /// before timing out.</param>
  public TimeoutAttribute(int timeoutMilliseconds)
    => TimeoutMilliseconds = timeoutMilliseconds;
}

/// <summary>
/// Test suite class attribute which indicates the test methods in the class
/// depend on the success of the previous method. If this attribute is
/// applied, all of the test and utility methods in the test suite must
/// succeed. If a method fails, the remaining methods in the suite will not
/// be run.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SequentialAttribute : Attribute {
  /// <summary>Create a new sequential attribute.</summary>
  public SequentialAttribute() { }
}

