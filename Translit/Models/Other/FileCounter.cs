using System;

namespace Translit.Models.Other
{
    public class FileCounter : ICloneable
    {
        public long Word { get; set; }
        public long Excel { get; set; }
        public long PowerPoint { get; set; }
        public long Pdf { get; set; }
        public long Rtf { get; set; }
        public long Txt { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void IncreaseWord()
        {
            Word++;
        }

        public void IncreaseExcel()
        {
            Excel++;
        }

        public void IncreasePowerPoint()
        {
            PowerPoint++;
        }

        public void IncreasePdf()
        {
            Pdf++;
        }

        public void IncreaseRtf()
        {
            Rtf++;
        }

        public void IncreaseTxt()
        {
            Txt++;
        }

        public long GetSum()
        {
            return Word + Excel + PowerPoint + Pdf + Rtf + Txt;
        }

        public FileCounter Add(FileCounter counter)
        {
            if (counter == null) return (FileCounter) Clone();

            Word += counter.Word;
            Excel += counter.Excel;
            PowerPoint += counter.PowerPoint;
            Pdf += counter.Pdf;
            Rtf += counter.Rtf;
            Txt += counter.Txt;

            return (FileCounter) Clone();
        }

        public FileCounter Subtract(FileCounter counter)
        {
            if (counter == null) return (FileCounter)Clone();

            Word -= counter.Word;
            Excel -= counter.Excel;
            PowerPoint -= counter.PowerPoint;
            Pdf -= counter.Pdf;
            Rtf -= counter.Rtf;
            Txt -= counter.Txt;

            return (FileCounter) Clone();
        }

        public void Reset()
        {
            Word = Excel = PowerPoint = Pdf = Rtf = Txt = 0;
        }
    }
}