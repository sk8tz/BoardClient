using System;
using System.Windows;
using System.Windows.Controls;

namespace Board.Controls
{
    public class AdaptivePanel : WrapPanel
    {
        protected override Size MeasureOverride(Size availableSize)
        {

            Size currentColumnSize = new Size();
            Size panelSize = new Size();

            foreach(UIElement element in base.InternalChildren)
            {
                element.Measure(availableSize);
                Size desiredSize = element.DesiredSize;

                if(currentColumnSize.Height + desiredSize.Height > availableSize.Height)
                {
                    // Switch to a new column (either because the 
                    //element has requested it or space has run out).
                    panelSize.Height = Math.Max(currentColumnSize.Height, panelSize.Height);
                    panelSize.Width += currentColumnSize.Width;
                    currentColumnSize = desiredSize;

                    // If the element is too high to fit using the 
                    // maximum height of the line,
                    // just give it a separate column.

                    //if(desiredSize.Height > availableSize.Height)
                    //{
                    //    panelSize.Height += desiredSize.Height;
                    //    panelSize.Width += desiredSize.Width;
                    //    currentColumnSize = new Size();
                    //}
                }
                else
                {
                    // Keep adding to the current column.
                    currentColumnSize.Height += desiredSize.Height;

                    // Make sure the line is as wide as its widest element.
                    currentColumnSize.Width = Math.Max(desiredSize.Width, currentColumnSize.Width);
                }
            }

            // Return the size required to fit all elements.
            // Ordinarily, this is the width of the availableSize, 
            // and the height is based on the size of the elements.
            // However, if an element is higher than the height given
            // to the panel,
            // the desired width will be the height of that column.
            panelSize.Height = Math.Max(currentColumnSize.Height, panelSize.Height);
            panelSize.Width += currentColumnSize.Width;
            return panelSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int firstInLine = 0;

            Size currentColumnSize = new Size();

            double accumulatedWidth = 0;

            UIElementCollection elements = base.InternalChildren;
            for(int i = 0; i < elements.Count; i++)
            {

                Size desiredSize = elements[i].DesiredSize;

                //need to switch to another column
                if(currentColumnSize.Height + desiredSize.Height > finalSize.Height)
                {
                    arrangeColumn(accumulatedWidth, currentColumnSize.Width, firstInLine, i, finalSize);

                    accumulatedWidth += currentColumnSize.Width;
                    currentColumnSize = desiredSize;

                    //the element is higher then the constraint - 
                    //give it a separate column 
                    if(desiredSize.Height > finalSize.Height)
                    {
                        arrangeColumn(accumulatedWidth, desiredSize.Width, i, ++i, finalSize);
                        accumulatedWidth += desiredSize.Width;
                        currentColumnSize = new Size();
                    }
                    firstInLine = i;
                }
                else //continue to accumulate a column
                {
                    currentColumnSize.Height += desiredSize.Height;
                    currentColumnSize.Width = currentColumnSize.Width - desiredSize.Width;
                }
            }

            if(firstInLine < elements.Count)
            {
                arrangeColumn(accumulatedWidth, currentColumnSize.Width, firstInLine, elements.Count, finalSize);
            }

            return finalSize;
        }


        private void arrangeColumn(double x, double columnWidth, int start, int end, Size arrangeBounds)
        {
            double y = 0;
            double totalChildHeight = 0;
            double widestChildWidth = 0;
            double xOffset = 0;

            UIElementCollection children = InternalChildren;
            UIElement child;

            for(int i = start; i < end; i++)
            {
                child = children[i];
                totalChildHeight += child.DesiredSize.Height;
                if(child.DesiredSize.Width > widestChildWidth)
                {
                    widestChildWidth = child.DesiredSize.Width;
                }
            }

            //work out y start offset within a given column
            y = ((arrangeBounds.Height - totalChildHeight) / 2);


            for(int i = start; i < end; i++)
            {
                child = children[i];
                if(child.DesiredSize.Width < widestChildWidth)
                {
                    xOffset = ((widestChildWidth - child.DesiredSize.Width) / 2);
                }

                child.Arrange(new Rect(x + xOffset, y, child.DesiredSize.Width, columnWidth));
                y += child.DesiredSize.Height;
                xOffset = 0;
            }
        }
    }
}
