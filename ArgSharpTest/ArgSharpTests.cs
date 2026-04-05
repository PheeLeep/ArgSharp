using ArgSharp;
using ArgSharp.Args;
using PheeLeep.ArgSharp.Args;

namespace ArgSharpTest;

/// <summary>
/// Serial tests with isolated state - each test has fresh ArgSharpClass state.
/// </summary>
public class Tests : SerialArgSharpTest
{
    public static bool PrintActionCalled = false;

    [Test]
    [Order(0)]
    public void SimpleTest()
    {
        ArgSharpClass.Init("ArgSharpTest");
        Assert.That(ConsoleOutput,
                    Does.Not.Contain("Argument parser already initialized."),
                    "Parser should be initialized.");
    }

    [Test]
    [Order(1)]
    public void DataTypeInsertTest()
    {
        Assert.DoesNotThrow(() => ArgSharpClass.AddArgument(["-p1"], defaultValue: ""), "Default type (string) should be accepted");
        Assert.DoesNotThrow(() => ArgSharpClass.AddArgument<int>(["-p2"]), "Integer should be accepted");
        Assert.DoesNotThrow(() => ArgSharpClass.AddArgument<double>(["-p3"]), "Double value should be accepted");
        Assert.DoesNotThrow(() => ArgSharpClass.AddArgument<decimal>(["-p4"]), "Decimal value should be accepted");
        Assert.DoesNotThrow(() => ArgSharpClass.AddArgument<bool>(["-pb"]), "Boolean value should be accepted");
        Assert.Throws<ArgumentParseException>(() => ArgSharpClass.AddArgument<object>(["-p5"]), "Illegal primitive is not allowed.");
        Assert.Throws<ArgumentParseException>(() => ArgSharpClass.AddArgument<ArgStoreBase>(["-p6"]), "Class is not allowed.");
    }


    [Test]
    [Order(2)]
    public void CommandChainTest()
    {
        PrintActionCalled = false;
        ArgInvoke? act1 = null;
        act1 = ArgSharpClass.AddArgumentAction(["print"], () =>
        {
            PrintActionCalled = true;
            var value = act1?.GetValue<string>("--value");
            Console.WriteLine($"Got value: {value}");
        });
        act1.AddArgument(["--value"], defaultValue: "");
        if (act1 is null)
        {
            Assert.Fail("Argument action has not been set");
        }
    }

    [Test]
    [Order(3)]
    public void ParseTest()
    {
        Assert.DoesNotThrow(() => ArgSharpClass.Parse(["-p1", "ArgSharpTest", "-p2", "20", "-pb", "print", "--value", "TEST"]),
                                                      "Parse error");
        Assert.That(PrintActionCalled, Is.True, "Print action should have been invoked");
        Assert.That(ConsoleOutput.Contains("TEST"), Is.True, "Output should contain TEST from the print action");
    }
    [Test]
    [Order(4)]
    public void GetVariableTest()
    {

        var values = ArgSharpClass.GetArgStoreValues();
        if (values.Length == 0)
        {
            Assert.Fail("Arguments not parsed.");
        }

        if (values.Single(a => a.Parameters.Contains("-pb")) is ArgStore<bool> pB)
        {
            Assert.That(pB.TypedValue, Is.True, "Value should be true.");
        }
        else
        {
            Assert.Fail("pB should be stored as boolean.");
        }

        if (values.Single(a => a.Parameters.Contains("-p2")) is ArgStore<int> p2)
        {
            Assert.That(p2.TypedValue, Is.EqualTo(20), "Value should be the same as the supplied argument.");
        }
        else
        {
            Assert.Fail("p2 should be stored as integer.");
        }

        if (values.Single(a => a.Parameters.Contains("-p1")) is ArgStore<string> p1)
        {
            Assert.That(p1.TypedValue, Is.EqualTo("ArgSharpTest"), "Value should be the same as the supplied argument.");
        }
        else
        {
            Assert.Fail("p1 should be stored as string.");
        }
    }

    [Test]
    [Order(5)]
    public void CommandChainResultTest()
    {
        Assert.That(PrintActionCalled, Is.True, "Print action should have been invoked during ParseTest");
    }
}
