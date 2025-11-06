namespace Chickensoft.GoDotTest.Tests;

using System;
using Chickensoft.Log;
using Godot;
using GoDotTest;
using LightMock.Generator;
using LightMoq;

public class TestReporterException : Exception
{
  public TestReporterException() : base("TestReporterException") { }

  public override string ToString() => "TestReporterException";
}

public class TestReporterTest : TestClass
{
  public TestReporterTest(Node testScene) : base(testScene) { }

  [Test]
  public void MethodUpdatePassedLogs()
  {
    var log = new Mock<ILog>();
    var reporter = new TestReporter(log.Object);

    var method = CreateMethod(TestMethodType.Test);
    var suite = CreateSuite();

    log.Setup(
      static log => log.Print("> OK >> TestSuite::Method [Test] > Test passed! :)")
    );

    reporter.MethodUpdate(
      suite.Object, method.Object, TestMethodEvent.Passed()
    );
    log.VerifyAll();
  }

  [Test]
  public void MethodUpdateSkipsStartedEventWhenNotATest()
  {
    var log = new Mock<ILog>();
    var reporter = new TestReporter(log.Object);

    var method = CreateMethod(TestMethodType.Cleanup);
    var suite = CreateSuite();

    reporter.MethodUpdate(
      suite.Object, method.Object, TestMethodEvent.Started()
    );
  }

  [Test]
  public void MethodUpdateSkipsPassedEventWhenNotATest()
  {
    var log = new Mock<ILog>();
    var reporter = new TestReporter(log.Object);

    var method = CreateMethod(TestMethodType.Cleanup);
    var suite = CreateSuite();

    reporter.MethodUpdate(
      suite.Object, method.Object, TestMethodEvent.Passed()
    );

    log.AssertNoOtherCalls();
  }

  [Test]
  public void MethodUpdateLogsStartedEventForTest()
  {
    var log = new Mock<ILog>();
    var reporter = new TestReporter(log.Object);

    var method = CreateMethod(TestMethodType.Test);
    var suite = CreateSuite();

    log.Setup(
      static log => log.Print("> ^^ >> TestSuite::Method [Test] > Test started! :3")
    );

    reporter.MethodUpdate(
      suite.Object, method.Object, TestMethodEvent.Started()
    );

    log.VerifyAll();
  }

  [Test]
  public void MethodUpdateLogsFailedEvent()
  {
    var log = new Mock<ILog>();
    var reporter = new TestReporter(log.Object);

    var method = CreateMethod(TestMethodType.Cleanup);
    var suite = CreateSuite();

    log.Setup(
      static log => log.Print("> !! >> TestSuite::Method [Cleanup] > Test failed! :(")
    );

    reporter.MethodUpdate(
      suite.Object, method.Object, TestMethodEvent.Failed(
        new InvalidOperationException("Ahem")
      )
    );

    log.VerifyAll();
  }

  [Test]
  public void MethodUpdateLogsSkippedEvent()
  {
    var log = new Mock<ILog>();
    var reporter = new TestReporter(log.Object);

    var method = CreateMethod(TestMethodType.Test);
    var suite = CreateSuite();

    log.Setup(
      static log => log.Print("> ^^ >> TestSuite::Method [Test] > Test skipped! :|")
    );

    reporter.MethodUpdate(
      suite.Object, method.Object, TestMethodEvent.Skipped()
    );

    log.VerifyAll();
  }

  [Test]
  public void SuiteUpdateLogsStartedEvent()
  {
    var log = new Mock<ILog>();
    var reporter = new TestReporter(log.Object);

    var suite = CreateSuite();

    log.Setup(
      static log => log.Print("> ^^ >> TestSuite > Test suite started! :3")
    );

    reporter.SuiteUpdate(suite.Object, TestSuiteEvent.Started);

    log.VerifyAll();
  }

  [Test]
  public void SuiteUpdateLogsFinishedEvent()
  {
    var log = new Mock<ILog>();
    var reporter = new TestReporter(log.Object);

    var suite = CreateSuite();

    log.Setup(
      static log => log.Print("> OK >> TestSuite > Test suite finished! :D")
    );

    reporter.SuiteUpdate(suite.Object, TestSuiteEvent.Finished);

    log.VerifyAll();
  }

  [Test]
  public void SuiteUpdateLogsErrorEncounteredEvent()
  {
    var log = new Mock<ILog>();
    var reporter = new TestReporter(log.Object);

    var suite = CreateSuite();

    log.Setup(
      static log => log.Print("> !! >> TestSuite > Test suite error. Aborting! :(")
    );

    reporter.SuiteUpdate(suite.Object, TestSuiteEvent.ErrorEncountered);

    log.VerifyAll();
  }

  [Test]
  public void UpdateLogsStartedEvent()
  {
    var log = new Mock<ILog>();
    var reporter = new TestReporter(log.Object);

    log.Setup(static log => log.Print("> ^^ >> Started testing! :3"));
    reporter.Update(TestEvent.Started);
    log.VerifyAll();
  }

  [Test]
  public void UpdateLogsFinishedEventWithSuccess()
  {
    var log = new Mock<ILog>();
    var reporter = new TestReporter(log.Object);

    log.Setup(static log => log.Print("> OK >> Finished testing! :D"));
    reporter.Update(TestEvent.Finished);
    log.VerifyAll();
  }

  [Test]
  public void UpdateLogsFinishedEventWithFailureAndOutputsErrors()
  {
    var log = new Mock<ILog>();
    var reporter = new TestReporter(log.Object);
    log.Setup(
      log => log.Print("> !! >> TestSuite::Method [Test] > Test failed! :(")
    );
    log.Setup(log => log.Print("> !! >> Finished testing! :("));
    log.Setup(
      log => log.Print(
        "> !! >> TestSuite::Method [Test] > Error occurred: " +
        "TestReporterException"
      )
    );
    log.Setup(log => log.Print(
      "> !! >> Test results: Passed: 0 | Failed: 1 | Skipped: 0"
    ));
    var exception = new TestReporterException();
    log.Setup(log => log.Print(exception));
    var suite = CreateSuite();
    var method = CreateMethod(TestMethodType.Test);
    reporter.MethodUpdate(
      suite.Object,
      method.Object,
      TestMethodEvent.Failed(exception)
    );
    reporter.Update(TestEvent.Finished);
    reporter.OutputFinalReport();
    log.VerifyAll();
  }

  [Test]
  public void OutputsFinalSuccessfulReport()
  {
    var log = new Mock<ILog>();
    var reporter = new TestReporter(log.Object);
    log.Setup(
      static log => log.Print("> OK >> TestSuite::Method [Test] > Test passed! :)")
    );
    log.Setup(static log => log.Print("> OK >> Finished testing! :D"));
    log.Setup(static log => log.Print(
      "> OK >> Test results: Passed: 1 | Failed: 0 | Skipped: 0"
    ));
    var suite = CreateSuite();
    var method = CreateMethod(TestMethodType.Test);
    reporter.MethodUpdate(
      suite.Object,
      method.Object,
      TestMethodEvent.Passed()
    );
    reporter.Update(TestEvent.Finished);
    reporter.OutputFinalReport();
    log.VerifyAll();
  }

  private static Mock<ITestSuite> CreateSuite(string name = "TestSuite")
  {
    var suite = new Mock<ITestSuite>();
    suite.Setup(static suite => suite.Name).Returns(name);
    return suite;
  }

  private static Mock<ITestMethod> CreateMethod(
    TestMethodType type, string name = "Method"
  )
  {
    var method = new Mock<ITestMethod>();
    method.Setup(static method => method.Type).Returns(type);
    method.Setup(static method => method.Name).Returns(name);
    return method;
  }
}
