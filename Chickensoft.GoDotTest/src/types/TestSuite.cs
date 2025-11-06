namespace Chickensoft.GoDotTest;

using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Represents a test suite (a class that extends <see cref="TestClass"/>.
/// </summary>
public interface ITestSuite
{
  /// <summary>
  /// Name of the test class (determined by the class name).
  /// </summary>
  string Name { get; }
  /// <summary>
  /// Type of the class containing the test.
  /// </summary>
  Type TestClassType { get; }
  /// <summary>List of setup methods defined in this test suite.</summary>
  List<ITestMethod> SetupMethods { get; }
  /// <summary>List of setup all methods defined in this test suite.</summary>
  List<ITestMethod> SetupAllMethods { get; }
  /// <summary>List of test methods defined in this test suite.</summary>
  List<ITestMethod> TestMethods { get; }
  /// <summary>List of cleanup methods defined in this test suite.</summary>
  List<ITestMethod> CleanupMethods { get; }
  /// <summary>
  /// List of cleanup all methods defined in this test suite.
  /// </summary>
  List<ITestMethod> CleanupAllMethods { get; }
  /// <summary>
  /// Whether the remaining test methods in the suite should be skipped if
  /// an error occurs in one of the methods.
  /// </summary>
  bool Sequential { get; }
  /// <summary>
  /// List of methods that are called when the test suite encounters an
  /// exception.
  /// </summary>
  List<ITestMethod> FailureMethods { get; }
}

/// <summary>
/// Represents a suite of test methods (a test class).
/// </summary>
public class TestSuite : ITestSuite
{
  /// <inheritdoc/>
  public Type TestClassType { get; }
  /// <inheritdoc/>
  public string Name { get; }
  /// <inheritdoc/>
  public List<ITestMethod> SetupMethods { get; }
  /// <inheritdoc/>
  public List<ITestMethod> SetupAllMethods { get; }
  /// <inheritdoc/>
  public List<ITestMethod> TestMethods { get; }
  /// <inheritdoc/>
  public List<ITestMethod> CleanupMethods { get; }
  /// <inheritdoc/>
  public List<ITestMethod> CleanupAllMethods { get; }
  /// <inheritdoc/>
  public List<ITestMethod> FailureMethods { get; }
  /// <inheritdoc/>
  public bool Sequential { get; }

  /// <summary>
  /// Creates a new test suite (test class) representation.
  /// </summary>
  /// <param name="info">TypeInfo for the test class.</param>
  /// <param name="testClassType">Type of the test class.</param>
  /// <param name="setupMethods">Setup methods.</param>
  /// <param name="setupAllMethods">Setup all methods.</param>
  /// <param name="testMethods">Test methods.</param>
  /// <param name="cleanupMethods">Clean up methods.</param>
  /// <param name="cleanupAllMethods">Cleanup all methods.</param>
  /// <param name="failureMethods">Failure methods.</param>
  public TestSuite(
    TypeInfo info,
    Type testClassType,
    List<ITestMethod> setupMethods,
    List<ITestMethod> setupAllMethods,
    List<ITestMethod> testMethods,
    List<ITestMethod> cleanupMethods,
    List<ITestMethod> cleanupAllMethods,
    List<ITestMethod> failureMethods
  )
  {
    Sequential = info.GetCustomAttribute<SequentialAttribute>(false) != null;
    TestClassType = testClassType;
    Name = testClassType.Name;
    SetupMethods = setupMethods;
    SetupAllMethods = setupAllMethods;
    TestMethods = testMethods;
    CleanupMethods = cleanupMethods;
    CleanupAllMethods = cleanupAllMethods;
    FailureMethods = failureMethods;
  }
}
