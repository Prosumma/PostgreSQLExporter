using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;

namespace Prosumma.PostgreSQL.IO
{
	public class Exporter
	{
		private IDictionary<Type, Action<object, StreamWriter>> writers = new Dictionary<Type, Action<object, StreamWriter>>();

		public ExportOptions Options { get; set; } = ExportOptions.IncludeHeaders;
		public char ColumnSeparator { get; set; } = '\t';
		public string NullRepresentation { get; set; } = "\\N";

		public Exporter()
		{
			writers[typeof(String)] = WriteString;
			writers[typeof(DateTime)] = WriteDateTime;
		}

		public void Export(DbDataReader inputReader, Stream outputStream, int progressInterval = 0, Action<int> progress = null)
		{
			if (!inputReader.HasRows) return;

			StreamWriter writer = new StreamWriter(outputStream, Encoding.UTF8);
			writer.NewLine = "\n";

			var count = 0;
			if (Options.HasFlag(ExportOptions.IncludeHeaders))
			{
				for (var field = 0; field < inputReader.FieldCount; field++)
				{
					if (field > 0) writer.Write(ColumnSeparator);
					writer.Write(inputReader.GetName(field));
				}
				writer.Write(writer.NewLine);
			}
			while (inputReader.Read())
			{
				WriteRow(inputReader, writer);
				++count;
				if (progressInterval > 0 && count % progressInterval == 0) progress?.Invoke(count);
			}
		}

		public void Export(DbDataReader inputReader, string outputPath, int progressInterval = 0, Action<int> progress = null)
		{
			Export(inputReader, File.Create(outputPath), progressInterval, progress);
		}

		private void WriteRow(DbDataReader reader, StreamWriter writer)
		{
			for (var field = 0; field < reader.FieldCount; field++)
			{
				if (field > 0) writer.Write(ColumnSeparator);
				WriteField(reader, field, writer);
			}
			writer.Write(writer.NewLine);
		}

		private void WriteObject(object o, StreamWriter writer)
		{
			writer.Write(o);
		}

		private void WriteString(object o, StreamWriter writer)
		{
			writer.Write("\"{0}\"", ((string)o).Replace("\"", "\"\"").Replace("\r\n", "\n"));
		}

		private void WriteDateTime(object o, StreamWriter writer)
		{
			writer.Write("{0:u}", o);
		}

		private void WriteField(DbDataReader reader, int field, StreamWriter writer)
		{
			if (reader.IsDBNull(field))
			{
				writer.Write(NullRepresentation);
			}
			else
			{
				var type = reader.GetFieldType(field);
				Action<object, StreamWriter> write = null;
				if (!writers.TryGetValue(type, out write))
				{
					write = WriteObject;
				}
				write(reader.GetValue(field), writer);
			}
		}

    }
}
