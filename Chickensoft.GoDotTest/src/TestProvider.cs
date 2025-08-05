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
  List<TestOp> GetTestOps(Assembly assembly);

  /// <summary>
  /// Searches through each <see cref="TestSuite"/> in the assembly for
  /// the first test suite type that matches the specified name exactly. You
  /// can specify a test suite name such as `MyTestSuite` or even an individual
  /// tests via `MyTestSuite.MyTestMethod`.
  /// </summary>
  /// <param name="assembly"></param>
  /// <param name="name">Test Suite type name.</param>
  /// <returns>The first test suite with an exact name match, if any.
  /// </returns>
  TestOp? GetTestOpByName(Assembly assembly, string name);

  /// <summary>
  /// Searches through each <see cref="TestSuite"/> in the assembly for
  /// each test suite type that matches the specified pattern. If you supply a
  /// glob pattern, test operations for all of the matching suites will be
  /// returned (case-insensitive). You may also specify a test suite name
  /// such as `MyTestSuite` or even an individual test via
  /// `MyTestSuite.MyTestMethod`.
  /// </summary>
  /// <param name="assembly"></param>
  /// <param name="pattern">Name or glob pattern to match.</param>
  /// <returns>A list of matching test suites.</returns>
  List<TestOp> GetTestOpsByPattern(
    Assembly assembly, string pattern
  );

  /// <summary>
  /// Gets an individual test method from the specified suite by name. Throws
  /// an exception if the suite or method in the suite is not found.
  /// </summary>
  /// <param name="assembly">Assembly to search in.</param>
  /// <param name="suiteName">Name of the test suite.</param>
  /// <param name="methodName">Name of the test method.</param>
  /// <returns>An individual test method operation.</returns>
  TestOp? GetIndividualTestOp(
    Assembly assembly,
    string suiteName,
    string methodName
  );
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
  public List<TestOp> GetTestOps(Assembly assembly) =>
    [.. GetTestSuites(assembly).Select(
      suite => new TestSuiteOp(suite)
    )];

  /// <inheritdoc/>
  public TestOp? GetTestOpByName(
    Assembly assembly, string name
  ) {
    var suiteName = name.Trim();
    var methodName = string.Empty;

    if (name.Contains('.')) {
      // `TestSuiteName.MethodName` will run an individual test in a suite.
      var split = name.Split('.');

      if (string.IsNullOrEmpty(split[1])) {
        return null;
      }

      suiteName = split[0];
      methodName = split[1];

      return GetIndividualTestOp(assembly, suiteName, methodName);
    }

    if (GetTestSuiteByName(assembly, name) is { } suite) {
      return new TestSuiteOp(suite);
    }
    return null;
  }

  /// <inheritdoc/>
  public List<TestOp> GetTestOpsByPattern(
    Assembly assembly, string pattern
  ) {
    if (pattern.Contains('.')) {
      return [GetTestOpByName(assembly, pattern)];
    }

    return [.. GetTestSuitesByPattern(assembly, pattern).Select(
      suite => new TestSuiteOp(suite)
    )];
  }

  /// <inheritdoc/>
  public TestOp? GetIndividualTestOp(
    Assembly assembly, string suiteName, string methodName
  ) {
    var suite = GetTestSuiteByName(assembly, suiteName);

    if (suite is null) { return null; }

    var method = suite.TestMethods.Find(
      m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)
    );

    if (method is null) { return null; }

    return new IndividualTestOp(suite, method);
  }

  /// <summary>
  /// Gets a list of test suites in the given assembly.
  /// </summary>
  /// <param name="assembly">Assembly to search for test suites in.</param>
  /// <returns>List of test suites.</returns>
  public List<ITestSuite> GetTestSuites(Assembly assembly) =>
    [.. assembly.GetTypes().Where(
      type =>
        type.IsSubclassOf(typeof(TestClass)) &&
        !type.IsAbstract
        && type.IsClass
    ).Select(GetTestSuite)];

  /// <summary>
  /// Searches through each <see cref="TestSuite"/> in the assembly for
  /// the first test suite type that matches the specified name exactly.
  /// </summary>
  /// <param name="assembly">Assembly to search in.</param>
  /// <param name="name">Test Suite type name.</param>
  /// <returns>The first test suite with an exact name match, if any.
  /// </returns>
  public ITestSuite? GetTestSuiteByName(Assembly assembly, string name) =>
    GetTestSuites(assembly).Find(suite => suite.Name == name);

  /// <summary>
  /// Searches through each <see cref="TestSuite"/> in the assembly for
  /// each test suite type that matches the specified name glob (not case
  /// sensitive).
  /// </summary>
  /// <param name="assembly">Assembly to search in.</param>
  /// <param name="nameGlob">Name glob pattern to match.</param>
  /// <returns>A list of matching test suites.</returns>
  public List<ITestSuite> GetTestSuitesByPattern(
    Assembly assembly, string nameGlob
  ) => [.. GetTestSuites(assembly).Where(
    suite => MatchesGlob(suite.Name, nameGlob)
  )];

  /// <summary>
  /// Gets a test suite operation for the given test suite class type.
  /// </summary>
  /// <param name="classType">Subclass type of a test suite.</param>
  /// <returns>The test suite operation.</returns>
  public static TestSuiteOp GetTestSuiteOp(Type classType) {
    var suite = GetTestSuite(classType);
    return new TestSuiteOp(suite);
  }

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
  ) => [.. classType.GetMethods().Where(methodInfo =>
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
  ).Cast<ITestMethod>()];

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
