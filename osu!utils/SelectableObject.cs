using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Utils
{
    public delegate void BatchSelectedHandler(object sender, EventArgs e);

    public class SelectableObject<T>
    {
        private bool selected;
        public event BatchSelectedHandler BatchSelected;
        public bool IsSelected
        {
            get
            {
                return selected;
            }
            set
            {
                if (value != selected)
                {
                    selected = value;
                    BatchSelected(this, EventArgs.Empty);
                }
            }
        }

        public T ObjectData { get; set; }

        public SelectableObject(T objectData)
        {
            ObjectData = objectData;
        }

        public SelectableObject(T objectData, bool isSelected)
        {
            selected = isSelected;
            ObjectData = objectData;
        }
    }
}
