namespace Plus.Database.Adapter
{
    using System;
    using Interfaces;

    public class NormalQueryReactor : QueryAdapter, IQueryAdapter, IRegularQueryAdapter, IDisposable
    {
        public NormalQueryReactor(IDatabaseClient client) : base(client) => Command = client.CreateNewCommand();

        public void Dispose()
        {
            Command.Dispose();
            Client.ReportDone();
            GC.SuppressFinalize(this);
        }
    }
}