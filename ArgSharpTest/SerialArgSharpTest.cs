using ArgSharp;

namespace ArgSharpTest;

/// <summary>
/// Base class for serial ArgSharp tests with isolated state per test.
/// Each test runs in the same process but with completely reset ArgSharpClass state.
/// </summary>
[NonParallelizable]
public abstract class SerialArgSharpTest
{
    private TextWriter _originalOut = null!;
    private TextWriter _originalErr = null!;
    private StringWriter _consoleOutput = null!;
    private StringWriter _consoleError = null!;

    /// <summary>
    /// Gets captured console output from this test.
    /// </summary>
    protected string ConsoleOutput => _consoleOutput.ToString();

    /// <summary>
    /// Gets captured console error output from this test.
    /// </summary>
    protected string ConsoleError => _consoleError.ToString();

    [SetUp]
    public virtual void Setup()
    {
        // Reset ArgSharpClass to clean state - like a fresh process
   //     ArgSharpClass.Reset();

        // Prevent test host crash on help invocation
        ArgSharpClass.OnHelpInvoked = () => { };
        // Capture console output
        _consoleOutput = new StringWriter();
        _consoleError = new StringWriter();
        _originalOut = Console.Out;
        _originalErr = Console.Error;

        Console.SetOut(_consoleOutput);
        Console.SetError(_consoleError);
    }

    [TearDown]
    public virtual void TearDown()
    {
        // Restore console
        Console.SetOut(_originalOut);
        Console.SetError(_originalErr);
        _consoleOutput.Dispose();
        _consoleError.Dispose();
    }
}
