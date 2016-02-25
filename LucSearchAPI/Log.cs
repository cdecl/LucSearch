using System;
using System.IO;
using System.Diagnostics;

namespace GLASS
{
	public interface ILog
	{
		void WriteLine(string strLog);
	}

	public class FileLog : ILog
	{
		public FileLog()
		{
		}

		public FileLog(string strFileName)
		{
			strLogFile_ = strFileName;
		}

		public void WriteLine(string strLog)
		{
			System.IO.StreamWriter sw = null;

			try 
			{
				string strLogFile = MakeLogFileName(strLogFile_);
				sw = new System.IO.StreamWriter(strLogFile, true);

				sw.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString("u"), strLog));
				sw.Flush();
			}
			catch (Exception) {}
			finally 
			{
				if (sw != null) sw.Close();
			}
		}

		static string MakeLogFileName(string strFileName)
		{
			string strName = string.Format(
				"{0}\\{1}_{2}{3}",
				Path.GetDirectoryName(strFileName),
				Path.GetFileNameWithoutExtension(strFileName),
				DateTime.Now.ToShortDateString().Replace("-", ""),
				Path.GetExtension(strFileName)
				);
													
			return strName;
		}

		private string strLogFile_ = "log.txt";
	};


	public class TraceLog : ILog
	{
		public TraceLog()
		{
		}

		public TraceLog(string strFileName)
		{
		}

		public void WriteLine(string strLog)
		{
			Trace.WriteLine(strLog);
		}
	};
}



