namespace CommerceBack.Common.OperationResults;


public class TestObjectMain
{
    public void MyMethod()
    {
        var x = new TestObject<string>().Set(1);
    }
}

public class TestObject<T> : TestObjectBase<T>
{
    
}

public class TestObjectBase<T> : ITestObject<T>
{
    public string Code { get; set; }
    
    public T? Entity { get; set; }
    
    public ITestObject<T> Set(string code = "Not code")
    {
        return new TestObjectBase<T>(){ Code = code };
    }

    public ITestObject<T> Set()
    {
        throw new NotImplementedException();
    }

    public ITestObject<T> Set(int intCode = 1)
    {
        throw new NotImplementedException();
    }

    ITestObject ITestObject.Set(string? code)
    {
        return Set(code);
    }
    
}

public interface ITestObject<T> : ITestObject
{
    T? Entity { get; set; }
    new ITestObject<T> Set(string code = "Not code");
    new ITestObject<T> Set();
    new ITestObject<T> Set(int intCode = 1);
}

public interface ITestObject
{
    string Code { get; set; }
    ITestObject Set(string? code = "Not code");
}