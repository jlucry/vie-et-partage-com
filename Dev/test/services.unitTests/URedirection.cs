using Services;
using Services.UnitTests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace services.unitTests
{
    public class URedirection : UBaseProvider
    {
        /// <summary>
        /// WI 372: Lister les posts.
        /// WI 370: Ajouter un post
        /// </summary>
        public URedirection(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async Task URedirection_TestUrl()
        {
            LogMaxLevel = 1;

            FileStream fileStream = new FileStream(@"D:\dev\dfide\Wcms\test\www-vieetpartage-com_20170825T122433Z_CrawlErrors.csv", FileMode.Open);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = null;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    string[] lineInfo = line?.Split(new char[] { ',' });
                    if ((lineInfo?.Length ?? 0) != 0 && string.IsNullOrEmpty(lineInfo[0]) == false && lineInfo[0].StartsWith("http") == true)
                    {
                        string[] route = lineInfo[0].Split(new char[] { '/' });
                        if ((route?.Length ?? 0) != 0)
                        {
                            StringBuilder stb = new StringBuilder();
                            string redirection = VepUrlRedirection.Migrate(lineInfo[0], stb);
                            _Log(1, null, $"{lineInfo[0]},{redirection}," + stb.ToString());
                        }
                    }
                }
            }
            Assert.False(false);
        }
    }
}
