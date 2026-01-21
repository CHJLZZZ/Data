using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class MatrixInfo : ObjectBase
    {
        private int row;
        public int Row
        {
            get
            {
                return this.row;
            }

            set
            {
                this.row = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Row"));
            }
        }

        private int column;
        public int Column
        {
            get
            {
                return this.column;
            }

            set
            {
                this.column = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Column"));
            }
        }

        public MatrixInfo()
        {
            this.row = 1;
            this.column = 1;
        }

        public MatrixInfo(MatrixInfo obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(MatrixInfo obj)
        {
            this.row = obj.Row;
            this.column = obj.Column;
        }
    }
}
