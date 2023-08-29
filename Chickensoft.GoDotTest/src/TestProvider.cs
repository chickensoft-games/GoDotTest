namespace Chickensoft.GoDotTest;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

/// <summary>
/// Test provider interface responsible for finding test suites in the
/// current assembly.
/// </summary>
public interface ITestProvider {
  /// <summary>
  /// Loads all test suites from the given assembly. A test suite is a
  /// subclass of <see cref="TestClass"/> containing methods annotated with
  /// <see cref="TestAttribute"/>, <see cref="SetupAttribute"/>, etc.
  /// </summary>
  /// <param name="assembly">Assembly to search for test suites in.</param>
  /// <returns>List of test suites.</returns>
  List<ITestSuite> GetTestSuites(Assembly assembly);
  /// <summary>
  /// Searches through each <see cref="TestSuite"/> in the assembly for
  /// the first test suite type that matches the specified name exactly.
  /// </summary>
  /// <param name="assembly"></param>
  /// <param name="name">Test Suite type name.</param>
  /// <returns>The first test suite with an exact name match, if any.
  /// </returns>
  ITestSuite? GetTestSuiteByName(Assembly assembly, string name);
  /// <summary>
  /// Searches through each <see cref="TestSuite"/> in the assembly for
  /// each test suite type that matches the specified name glob (not case
  /// sensitive).
  /// </summary>
  /// <param name="assembly"></param>
  /// <param name="nameGlob">Name glob pattern to match.</param>
  /// <returns>A list of matching test suites.</returns>
  List<ITestSuite> GetTestSuitesByPattern(Assembly assembly, string nameGlob);
}

/// <summary>
/// Test provider implementation responsible for finding test suites in the
/// current assembly.
/// </summary>
public class TestProvider : ITestProvider {
  /// <summary>
  /// Mapping of test method attribute types to their corresponding test
  /// method type enum.
  /// </summary>
  protected readonly Dictionary<Type, TestMethodType> _testMethodTypes =
    new() {
      [typeof(SetupAllAttribute)] = TestMethodType.SetupAll,
      [typeof(SetupAttribute)] = TestMethodType.Setup,
      [typeof(TestAttribute)] = TestMethodType.Test,
      [typeof(CleanupAttribute)] = TestMethodType.Cleanup,
      [typeof(CleanupAllAttribute)] = TestMethodType.CleanupAll,
    };

  /// <inheritdoc/>
  public List<ITestSuite> GetTestSuites(Assembly assembly) => assembly.GetTypes().Where(
      type =>
        type.IsSubclassOf(typeof(TestClass)) &&
        !type.IsAbstract
        && type.IsClass
    ).Select(GetTestSuite).ToList();

  /// <inheritdoc/>
  public ITestSuite? GetTestSuiteByName(Assembly assembly, string name) =>
    GetTestSuites(assembly).Find(suite => suite.Name == name);

  /// <inheritdoc/>
  public List<ITestSuite> GetTestSuitesByPattern(
    Assembly assembly, string nameGlob
  ) => GetTestSuites(assembly).Where(
    suite => MatchesGlob(suite.Name, nameGlob)
  ).ToList();

  /// <summary>
  /// Fetches a test suite from the given type.
  /// </summary>
  /// <param name="classType">Subclass type of a test suite.</param>
  /// <returns>The test suite.</returns>
  public static ITestSuite GetTestSuite(Type classType) {
    var setupMethods = GetMethods(classType, typeof(SetupAttribute));
    var setupAllMethods = GetMethods(classType, typeof(SetupAllAttribute));
    var testMethods = GetMethods(classType, typeof(TestAttribute));
    var cleanupMethods = GetMethods(classType, typeof(CleanupAttribute));
    var cleanupAllMethods
      = GetMethods(classType, typeof(CleanupAllAttribute));
    var failureMethods = GetMethods(classType, typeof(FailureAttribute));
    return new TestSuite(
      info: classType.GetTypeInfo(),
      testClassType: classType,
      setupMethods: setupMethods,
      setupAllMethods: setupAllMethods,
      testMethods: testMethods,
      cleanupMethods: cleanupMethods,
      cleanupAllMethods: cleanupAllMethods,
      failureMethods: failureMethods
    );
  }

  /// <summary>
  /// Gets a list of test methods annotated with <see cref="TestAttribute"/>.
  /// </summary>
  /// <param name="classType">The type of a class extending
  /// <see cref="TestClass"/>.</param>
  /// <param name="attributeType">Method attribute type to look for.</param>
  /// <returns>The list of annotated test methods.</returns>
  public static List<ITestMethod> GetMethods(
    Type classType, Type attributeType
  ) => classType.GetMethods().Where(methodInfo =>
    methodInfo.GetCustomAttributes(attributeType, false).Length > 0
  ).OrderBy(
    methodInfo => (
      (TestRunnerMethodAttribute)methodInfo.GetCustomAttributes(
        attributeType, false
      ).Single()
    ).Line
  ).Select(
    methodInfo => new TestMethod(
      methodInfo, attributeType.GetTestMethodType()
    )
  ).Cast<ITestMethod>().ToList();

  /// <summary>
  /// Returns true if the specified method is asynchronous.
  /// </summary>
  /// <param name="method">The method in question.</param>
  /// <returns>True or false.</returns>
  public static bool IsAsynchronous(MethodInfo method)
    => method.GetCustomAttribute<AsyncStateMachineAttribute>(false) != null;

  /// <summary>
  /// Checks a string to see if it matches a glob pattern.
  /// Credit: https://stackoverflow.com/a/4146349
  /// </summary>
  /// <param name="str">The string in question.</param>
  /// <param name="pattern">The glob pattern.</param>
  /// <returns>True if the string satisfies the glob pattern.</returns>
  public static bool MatchesGlob(string str, string pattern) => new Regex(
    "^" +
    Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") +
    "$",
    RegexOptions.IgnoreCase | RegexOptions.Singleline
  ).IsMatch(str);
}
