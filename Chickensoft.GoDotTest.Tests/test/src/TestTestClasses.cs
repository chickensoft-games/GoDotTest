namespace Chickensoft.GoDotTest.Tests;

using System;
using System.Threading.Tasks;
using Godot;
using GoDotTest;

public class TestTestIgnored : TestClass
{
  public TestTestIgnored(Node testScene) : base(testScene) { }

  [SetupAll]
  public void SetupAll()
  {
    // This test will also be run stand-alone by the framework since it extends
    // TestClass, so we need to prevent doubling up the record of invocations.
    TestExecutorTest.Called.Clear();
    TestExecutorTest.Called.Add("SetupAll");
  }

  [Setup]
  public void Setup() => TestExecutorTest.Called.Add("Setup");

  [Test]
  public void Test() => TestExecutorTest.Called.Add("Test");

  [Test]
  public void FailingTest()
  {
    TestExecutorTest.Called.Add("FailingTest");
    throw new InvalidOperationException("FailingTest");
  }

  [Cleanup]
  public void Cleanup() => TestExecutorTest.Called.Add("Cleanup");

  [CleanupAll]
  public void CleanupAll() => TestExecutorTest.Called.Add("CleanupAll");

  [Failure]
  public void Failure() => TestExecutorTest.Called.Add("Failure");

  [Failure]
  public void FailingFailure()
  {
    TestExecutorTest.Called.Add("FailingFailure");
    throw new InvalidOperationException("FailingFailure");
  }

  [Failure]
  public async void FailingFailureAsyncVoid() => await Task.CompletedTask;
}

[Sequential]
public class TestTestIgnored2 : TestClass
{
  public TestTestIgnored2(Node testScene) : base(testScene) { }

  [Test]
  public void Test1() { }

  [Test]
  public void Test2() { }

  [CleanupAll]
  public void CleanupAll() { }
}

public class TestTestIgnored3 : TestClass
{
  public TestTestIgnored3(Node testScene) : base(testScene) { }

  [Test]
  public void Test1() { }

  [Test]
  public void Test2() { }

  [CleanupAll]
  public void CleanupAll() { }
}
