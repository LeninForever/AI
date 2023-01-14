using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AI.Models
{
    internal class Cell
    {
        public int Number { get; private set; }

        public UIElement View { get; private set; }

        public Cell (int number, UIElement? view)
        {
            Number = number;
            View = view;
        }
    }
}
