using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using QaaS.Configuration;

namespace QaaS.Configuration.Tests;

[TestFixture]
[NonParallelizable]
public sealed class ConfigurationBootstrapTests
{
    [SetUp]
    public void SetUp() => ResetBootstrapState();

    [TearDown]
    public void TearDown() => ResetBootstrapState();

    [Test]
    public void Register_WhenReportPortalTargetAppearsAfterElasticRegistration_RetriesMissingTarget()
    {
        var elasticTarget = DefineRegistrationTarget(
            "QaaS.Framework.Executions",
            "QaaS.Framework.Executions.ExecutionLogging",
            [typeof(bool), typeof(string), typeof(string), typeof(string)]
        );

        ConfigurationBootstrap.Register();

        Assert.That(GetCallCount(elasticTarget), Is.EqualTo(1));

        var reportPortalTarget = DefineRegistrationTarget(
            "QaaS.Runner.Assertions",
            "QaaS.Runner.Assertions.ConfigurationObjects.ReporterConfigs.ReportPortalConfig",
            [typeof(bool), typeof(string), typeof(string)]
        );

        ConfigurationBootstrap.Register();

        Assert.Multiple(() =>
        {
            Assert.That(GetCallCount(elasticTarget), Is.EqualTo(1));
            Assert.That(GetCallCount(reportPortalTarget), Is.EqualTo(1));
        });
    }

    private static RegistrationTarget DefineRegistrationTarget(
        string assemblyName,
        string typeName,
        Type[] parameterTypes
    )
    {
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(assemblyName),
            AssemblyBuilderAccess.Run
        );
        var moduleBuilder = assemblyBuilder.DefineDynamicModule($"{assemblyName}.DynamicModule");
        var typeBuilder = moduleBuilder.DefineType(
            typeName,
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed
        );
        var callCountField = typeBuilder.DefineField(
            "CallCount",
            typeof(int),
            FieldAttributes.Public | FieldAttributes.Static
        );
        var registerDefaultsMethod = typeBuilder.DefineMethod(
            "RegisterDefaults",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(void),
            parameterTypes
        );

        var methodBody = registerDefaultsMethod.GetILGenerator();
        methodBody.Emit(OpCodes.Ldsfld, callCountField);
        methodBody.Emit(OpCodes.Ldc_I4_1);
        methodBody.Emit(OpCodes.Add);
        methodBody.Emit(OpCodes.Stsfld, callCountField);
        methodBody.Emit(OpCodes.Ret);

        var targetType = typeBuilder.CreateType();
        return new RegistrationTarget(targetType.GetField("CallCount")!);
    }

    private static int GetCallCount(RegistrationTarget target) =>
        (int)target.CallCountField.GetValue(null)!;

    private static void ResetBootstrapState()
    {
        SetBootstrapField("_elasticRegistered", false);
        SetBootstrapField("_reportPortalRegistered", false);
    }

    private static void SetBootstrapField(string fieldName, bool value)
    {
        typeof(ConfigurationBootstrap)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static)!
            .SetValue(null, value);
    }

    private sealed record RegistrationTarget(FieldInfo CallCountField);
}
