using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace homework_45_aruuke_maratova
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server("../../../site", 8888);
        }
    }
}