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

        public long GetSum()
        {
            return Word + Excel + PowerPoint + Pdf + Rtf + Txt;
        }

        public void Add(FileCounter counter)
        {
            Word += counter.Word;
            Excel += counter.Excel;
            PowerPoint += counter.PowerPoint;
            Pdf += counter.Pdf;
            Rtf += counter.Rtf;
            Txt += counter.Txt;
        }

        public void Subtract(FileCounter counter)
        {
            Word -= counter.Word;
            Excel -= counter.Excel;
            PowerPoint -= counter.PowerPoint;
            Pdf -= counter.Pdf;
            Rtf -= counter.Rtf;
            Txt -= counter.Txt;
        }

        public void Reset()
        {
            Word = Excel = PowerPoint = Pdf = Rtf = Txt = 0;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
