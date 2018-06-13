using System;
using System.IO;
using Raven.Client.Documents;
using Raven.TestDriver;

namespace Raven.Client.NodaTime.Tests
{
    public class MyRavenDBLocator : RavenServerLocator
    {
        private string _serverPath;
        private string _command = "dotnet";
        private readonly string RavenServerName = "Raven.Server";
        private string _arguments;

        public override string ServerPath
        {
            get
            {
                if (string.IsNullOrEmpty(_serverPath) == false)
                {
                    return _serverPath;
                }

                var path = Environment.GetEnvironmentVariable("Raven_Server_Test_Path");
                if (string.IsNullOrEmpty(path) == false)
                {
                    if (InitializeFromPath(path))
                        return _serverPath;
                }
                //If we got here we didn't have ENV:RavenServerTestPath setup for us maybe this is a CI enviroement
                path = Environment.GetEnvironmentVariable("Raven_Server_CI_Path");
                if (string.IsNullOrEmpty(path) == false)
                {
                    if (InitializeFromPath(path))
                        return _serverPath;
                }
                throw new FileNotFoundException($"Could not find {RavenServerName} anywhere on the device.");
            }
        }

        private bool InitializeFromPath(string path)
        {
            if (Path.GetFileNameWithoutExtension(path) != RavenServerName)
                return false;
            var ext = Path.GetExtension(path);
            if (ext == ".dll")
            {
                _serverPath = path;
                _arguments = _serverPath;
                return true;
            }
            if (ext == ".exe")
            {
                _serverPath = path;
                _command = _serverPath;
                _arguments = string.Empty;
                return true;
            }
            return false;
        }

        public override string Command => _command;
        public override string CommandArguments => _arguments;
    }

    public class MyRavenTestDriver : RavenTestDriver<MyRavenDBLocator>
    {
        protected override void PreInitialize(IDocumentStore documentStore)
        {
            documentStore.ConfigureForNodaTime();

            base.PreInitialize(documentStore);
        }

        protected IDocumentStore NewDocumentStore() => GetDocumentStore();
    }
}
