using Amazon.Lambda.Core;

namespace FCG.NotificationCenterLambda.Tests;

public class TestLambdaContext : ILambdaContext
{
    public string AwsRequestId => Guid.NewGuid().ToString();
    public IClientContext? ClientContext => null;
    public string FunctionName => "NotificationCenterLambdaTests";
    public string FunctionVersion => "1";
    public ICognitoIdentity? Identity => null;
    public string InvokedFunctionArn => "local:test";
    public ILambdaLogger Logger => new TestLambdaLogger();
    public string LogGroupName => "local";
    public string LogStreamName => "local";
    public int MemoryLimitInMB => 256;
    public TimeSpan RemainingTime => TimeSpan.FromMinutes(5);
}

public class TestLambdaLogger : ILambdaLogger
{
    public void Log(string message) => Console.Write(message);
    public void LogLine(string message) => Console.WriteLine(message);
}