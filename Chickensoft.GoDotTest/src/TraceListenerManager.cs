namespace Chickensoft.GoDotTest;

using System;
using System.Diagnostics;

public class TraceListenerManager : IDisposable
{
  private bool _disposedValue;
  private TraceListener? _listener;

  public TraceListenerManager(ITestEnvironment environment)
  {
    if (environment.ListenTrace)
    {
      _listener = new DefaultTraceListener();
      Trace.Listeners.Add(_listener);
    }
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposedValue)
    {
      if (disposing)
      {
        if (_listener is not null)
        {
          Trace.Listeners.Remove(_listener);
          _listener = null;
        }
      }
      _disposedValue = true;
    }
  }

  public void Dispose()
  {
    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)'
    // method
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}
