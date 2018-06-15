namespace Raven.Client.NodaTime
{
    public static class NodaTimeCompilationExtension
    {
        public static readonly string AdditionalSourcesRavenBundlesNodaTime = @"
namespace Raven.Bundles.NodaTime
{
}
";

        public static readonly string AdditionalSourcesNodaTime = @"
namespace NodaTime 
{
}
";
    }
}
