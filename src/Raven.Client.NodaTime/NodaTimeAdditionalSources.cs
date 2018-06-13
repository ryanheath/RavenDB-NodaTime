namespace Raven.Client.NodaTime
{
    public static class NodaTimeCompilationExtension
    {
        public static readonly string AdditionalSources = @"
namespace Raven.Bundles.NodaTime
{
}
";

        public static readonly string AdditionalSources2 = @"
namespace NodaTime 
{
}
";
    }
}
