using System;
using System.Linq;
using System.Threading.Tasks;
using WebSharper;

namespace GlutenFree.OddJob.Manager.Presentation.WS
{
    public static class Remoting
    {
        [Remote]
        public static Task<string> DoSomething(string input)
        {
            return Task.FromResult(new String(input.ToCharArray().Reverse().ToArray()));
        }
    }
}