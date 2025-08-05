namespace Chickensoft.GoDotTest;

/// <summary>
/// Represents a test operation that can be executed.
/// </summary>
/// <param name="Suite">Test suite.</param>
public abstract record TestOp(ITestSuite Suite);

/// <summary>
/// Represents a single test to be executed in a test suite.
/// </summary>
/// <param name="Suite">The test suite that contains this test.</param>
/// <param name="Method">The test method to be executed.</param>
public record IndividualTestOp(ITestSuite Suite, ITestMethod Method) : TestOp(Suite);

/// <summary>
/// Represents a test suite operation to be executed.
/// </summary>
/// <param name="Suite">The test suite to be executed.</param>
public record TestSuiteOp(ITestSuite Suite) : TestOp(Suite);
