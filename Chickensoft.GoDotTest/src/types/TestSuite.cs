namespace Chickensoft.GoDotTest;

using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Represents a test suite (a class that extends <see cref="TestClass"/>.
/// </summary>
public interface ITestSuite {
  /// <summary>
  /// Name of the test class (determined by the class name).
  /// </summary>
  string Name { get; }
  /// <summary>
  /// Type of the class containing the test.
  /// </summary>
  Type TestClassType { get; }
  /// <summary>List of setup methods defined in this test suite.</summary>
  public List<ITestMethod> SetupMethods { get; }
  /// <summary>List of setup all methods defined in this test suite.</summary>
  public List<ITestMethod> SetupAllMethods { get; }
  /// <summary>List of test methods defined in this test suite.</summary>
  public List<ITestMethod> TestMethods { get; }
  /// <summary>List of cleanup methods defined in this test suite.</summary>
  public List<ITestMethod> CleanupMethods { get; }
  /// <summary>
  /// List of cleanup all methods defined in this test suite.
  /// </summary>
  public List<ITestMethod> CleanupAllMethods { get; }
  /// <summary>
  /// Whether the remaining test methods in the suite should be skipped if
  /// an error occurs in one of the methods.
  /// </summary>
  public bool Sequential { get; }
}

/// <summary>
/// Represents a suite of test methods (a test class).
/// </summary>
public class TestSuite : ITestSuite {
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
  public bool Sequential { get; }

  /// <summary>
  /// Creates a new test suite (test class) representation.
  /// </summary>
  /// <param name="info"></param>
  /// <param name="testClassType">Type of the test class.</param>
  /// <param name="setupMethods"></param>
  /// <param name="setupAllMethods"></param>
  /// <param name="testMethods">List of test methods belonging to this
  /// instance of the test class.</param>
  /// <param name="cleanupMethods"></param>
  /// <param name="cleanupAllMethods"></param>
  public TestSuite(
    TypeInfo info,
    Type testClassType,
    List<ITestMethod> setupMethods,
    List<ITestMethod> setupAllMethods,
    List<ITestMethod> testMethods,
    List<ITestMethod> cleanupMethods,
    List<ITestMethod> cleanupAllMethods
  ) {
    Sequential = info.GetCustomAttribute<SequentialAttribute>(false) != null;
    TestClassType = testClassType;
    Name = testClassType.Name;
    SetupMethods = setupMethods;
    SetupAllMethods = setupAllMethods;
    TestMethods = testMethods;
    CleanupMethods = cleanupMethods;
    CleanupAllMethods = cleanupAllMethods;
  }
}
