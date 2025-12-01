namespace Chickensoft.GoDotTest;

using System.Collections.Generic;

/// <summary>
/// Represents a test operation that can be executed.
/// </summary>
/// <param name="Suite">Test suite.</param>
public abstract record TestOp(ITestSuite Suite)
{
  public abstract List<ITestMethod> TestMethods { get; }
}

/// <summary>
/// Represents a single test to be executed in a test suite.
/// </summary>
/// <param name="Suite">The test suite that contains this test.</param>
/// <param name="Method">The test method to be executed.</param>
public record IndividualTestOp(ITestSuite Suite, ITestMethod Method)
  : TestOp(Suite)
{
  public override List<ITestMethod> TestMethods => [Method];
}

/// <summary>
/// Represents a test suite operation to be executed.
/// </summary>
/// <param name="Suite">The test suite to be executed.</param>
public record TestSuiteOp(ITestSuite Suite) : TestOp(Suite)
{
  public override List<ITestMethod> TestMethods => Suite.TestMethods;
}
