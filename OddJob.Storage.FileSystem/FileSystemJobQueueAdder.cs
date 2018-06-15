using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OddJob.Storage.FileSystem
{
    public class FileSystemJobQueueAdder : IJobQueueAdder
    {
        public FileSystemJobQueueAdder(string fileNameWithPath)
        {
            FileName = fileNameWithPath;
        }

        public string FileName { get; protected set; }
        public void WriteJobToQueue(FileSystemJobMetaData data)
        {
            bool written = false;
            while (!written)
            {
                try
                {
                    using (FileStream fs =
                 File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        byte[] toRead = null;
                        using (var memStream = new MemoryStream())
                        {
                            fs.CopyTo(memStream);
                            toRead = memStream.ToArray();
                        }
                        var serializer =
                            Newtonsoft.Json.JsonConvert.DeserializeObject<List<FileSystemJobMetaData>>(Encoding.UTF8.GetString(toRead));
                        serializer.Add(data);
                        var newData = Newtonsoft.Json.JsonConvert.SerializeObject(serializer);
                        var toWrite = Encoding.UTF8.GetBytes(newData);
                        fs.Write(toWrite, 0, toWrite.Length);
                        written = true;
                    }
                }
                catch (IOException ex)
                {
                    //Magic Number Source:
                    //https://stackoverflow.com/questions/2568875/how-to-tell-if-a-caught-ioexception-is-caused-by-the-file-being-used-by-another
                    if (ex.HResult == unchecked((int)0x80070020))
                    {
                        //Retry.
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
        public Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters = null, DateTimeOffset? executionTime = null, string queueName = "default")
        {
            var jobData = JobCreator.Create(jobExpression);
            var toSer = new FileSystemJobMetaData()
            {
                JobId = Guid.NewGuid(),
                JobArgs = jobData.JobArgs,
                MethodName = jobData.MethodName,
                RetryParameters = retryParameters,
                TypeExecutedOn = jobData.TypeExecutedOn,
                QueueName = queueName,
                CreatedOn = DateTime.Now
            };
            WriteJobToQueue(toSer);
            return toSer.JobId;
        }
    }
}
