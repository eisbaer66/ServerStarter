using System.IO;
using Elastic.CommonSchema.Serilog;
using Serilog.Events;

namespace ServerStarter.Server
{
    public class EcsTextFormatterMetadata : EcsTextFormatter
    {
        public override void Format(LogEvent logEvent, TextWriter output)
        {
            var textWriter = new StringWriter(output.FormatProvider);
            base.Format(logEvent, textWriter);

            textWriter.Flush();
            string json         = textWriter.GetStringBuilder().ToString();
            string replacedJson = json.Replace("\"_metadata\":", "\"metadata\":");

            output.WriteLine(replacedJson);
        }
    }
}