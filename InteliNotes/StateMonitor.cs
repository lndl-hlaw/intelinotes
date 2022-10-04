using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteliNotes
{
    public interface IStateAction
    {
        //(Point, InkCanvas) GetPreviousLocation();
        void Undo();
        void Redo();
        bool CanBeDone(Func<object, bool> f);
    }
    public class AddStrokeAction: IStateAction
    {
        InkCanvas canvas;
        // Point location;
        Stroke originalStroke;
        Stroke copiedStroke;

        public AddStrokeAction(InkCanvas canvas, Stroke stroke)
        {
            this.canvas = canvas;
            this.originalStroke = stroke;
            this.copiedStroke = stroke.Clone();
            //this.location = where;
        }

        public void Undo()
        {
            canvas.Strokes.Remove(originalStroke);
        }

        public void Redo()
        {
            canvas.Strokes.Add(copiedStroke);
            originalStroke = copiedStroke;
            copiedStroke = copiedStroke.Clone();
        }

        public bool CanBeDone(Func<object, bool> f)
        {
            return f(canvas);
        }
    }
    public class AddControlAction: IStateAction
    {
        InkCanvas canvas;
        UIElement element;
        Point location;
        public AddControlAction(InkCanvas canvas, Point where, UIElement element)
        {
            this.canvas = canvas;
            this.element = element;
            location = where;
        }

        public void Undo()
        {
            canvas.Children.Remove(element);
        }

        public void Redo()
        {
            canvas.AddUIElementAtPosition(element, location);
        }
        public bool CanBeDone(Func<object, bool> f)
        {
            return f(canvas);
        }
    }
    public class AddCombinedAction: IStateAction
    {
        InkCanvas canvas;
        RemoveCombinedAction remove;
        public AddCombinedAction(InkCanvas canvas, StrokeCollection strokes, List<(UIElement, Point)> elements)
        {
            remove = new RemoveCombinedAction(canvas, strokes, elements);
            this.canvas = canvas;
        }

        public void Undo()
        {
            remove.Redo();
        }

        public void Redo()
        {
            remove.Undo();
        }
        public bool CanBeDone(Func<object, bool> f)
        {
            return f(canvas);
        }
    }
    public class RemoveCombinedAction: IStateAction
    {
        InkCanvas canvas;
        StrokeCollection originalStrokes;
        StrokeCollection copiedStrokes;
        List<(UIElement, Point)> elements;
        public RemoveCombinedAction(InkCanvas canvas, StrokeCollection strokes, List<(UIElement, Point)> elements)
        {
            this.canvas = canvas;
            this.elements = elements;
            originalStrokes = strokes;
            if(strokes != null)
            {
                copiedStrokes = strokes.Clone();
            }
        }

        public void Undo()
        {
            List<UIElement> els = null;
            if (copiedStrokes != null)
            {
                canvas.Strokes.Add(copiedStrokes);
                originalStrokes = copiedStrokes;
                copiedStrokes = copiedStrokes.Clone();
            }
            if (elements.Count > 0)
            {
                els = new List<UIElement>();
                foreach (var elem in elements)
                {
                    canvas.Children.Add(elem.Item1);
                    els.Add(elem.Item1);
                    InkCanvas.SetLeft(elem.Item1, elem.Item2.X);
                    InkCanvas.SetTop(elem.Item1, elem.Item2.Y);
                }
            }
            canvas.Select(copiedStrokes, els);
        }

        public void Redo()
        {
            if (originalStrokes != null)
            {
                canvas.Strokes.Remove(originalStrokes);
            }
            if (elements.Count > 0)
            {
                for (int i = 0; i < elements.Count; ++i)
                {
                    UIElement element = elements[i].Item1;
                    canvas.Children.Remove(element);
                }
            }
        }
        public bool CanBeDone(Func<object, bool> f)
        {
            return f(canvas);
        }
    }
    public class RemoveStrokeAction
    {
        InkCanvas canvas;
        private AddStrokeAction reverted;
        public RemoveStrokeAction(InkCanvas canvas, Stroke stroke)
        {
            reverted = new AddStrokeAction(canvas, stroke);
            this.canvas = canvas;
        }

        public void Undo()
        {
            reverted.Redo();
        }

        public void Redo()
        {
            reverted.Undo();
        }
        public bool CanBeDone(Func<object, bool> f)
        {
            return f(canvas);
        }
    }
    public class RemoveControlAction: IStateAction
    {
        InkCanvas canvas;
        AddControlAction action;
        public RemoveControlAction(InkCanvas canvas, Point where, UIElement element)
        {
            action = new AddControlAction(canvas, where, element);
            this.canvas = canvas;
        }

        public void Undo()
        {
            action.Redo();
        }

        public void Redo()
        {
            action.Undo();
        }
        public bool CanBeDone(Func<object, bool> f)
        {
            return f(canvas);
        }
    }
    public class MoveStrokesAndControlsAction: IStateAction
    {
        InkCanvas cFrom;
        InkCanvas cTo;
        Point pFrom;
        Point pTo;
        StrokeCollection strokesOriginal;
        StrokeCollection strokesCp;
        List<UIElement> elements;

        public MoveStrokesAndControlsAction(InkCanvas from, InkCanvas to, Point pfr, Point pto, StrokeCollection strokes, List<UIElement> elems)
        {
            cFrom = from;
            cTo = to;
            pFrom = pfr;
            pTo = pto;
            strokesOriginal = strokes;
            strokesCp = strokes.Clone();
            elements = elems;
        }

        public void Undo()
        {
            if(cFrom != cTo)
            {
                cTo.Strokes.Remove(strokesOriginal);
                cFrom.Strokes.Add(strokesCp);
                strokesOriginal = strokesCp;
                strokesCp = strokesCp.Clone();
                for (int i = 0; i < elements.Count; ++i)
                {
                    UIElement elem = elements[i];
                    cFrom.Children.Remove(elem);
                    cTo.Children.Add(elem);
                }
            }

            foreach (var elem in elements)
            {
                InkCanvas.SetLeft(elem, pFrom.X);
                InkCanvas.SetTop(elem, pFrom.Y);
            }
            strokesOriginal.Move(pFrom.X - pTo.X, pFrom.Y - pTo.Y);
        }

        public void Redo()
        {
            if(cFrom != cTo)
            {
                cFrom.Strokes.Remove(strokesOriginal);
                cTo.Strokes.Add(strokesCp);
                strokesOriginal = strokesCp;
                strokesCp = strokesCp.Clone();
                for (int i = 0; i < elements.Count; ++i)
                {
                    UIElement elem = elements[i];
                    cTo.Children.Remove(elem);
                    cFrom.Children.Add(elem);
                }
            }

            foreach (var elem in elements)
            {
                InkCanvas.SetLeft(elem, pTo.X);
                InkCanvas.SetTop(elem, pTo.Y);
            }
            strokesOriginal.Move(pTo.X - pFrom.X, pTo.Y - pFrom.Y);
        }
        public bool CanBeDone(Func<object, bool> f)
        {
            if(cFrom != cTo)
            {
                return f((cFrom, cTo));
            }
            else
            {
                return f(cFrom);
            }
        }
    }
    public class StateMonitor
    {
        DropOutStack<IStateAction> prevActions;
        DropOutStack<IStateAction> nextActions;
        int stackCapacity;

        public StateMonitor(int capacity)
        {
            stackCapacity = capacity;
            prevActions = new DropOutStack<IStateAction>(capacity);
            nextActions = new DropOutStack<IStateAction>(capacity);
        }

        public void AddLastAction(IStateAction state)
        {
            prevActions.Push(state);
            nextActions = new DropOutStack<IStateAction>(stackCapacity);
        }

        public void Undo()
        {
            if (prevActions.Count > 0)
            {
                IStateAction state = prevActions.Pop();
                state.Undo();
                nextActions.Push(state);
            }
        }

        public void Redo()
        {
            if(nextActions.Count > 0)
            {
                IStateAction state = nextActions.Pop();
                state.Redo();
                prevActions.Push(state);
            }
        }

        public void RemoveMatching(Func<IStateAction, bool> f)
        {
            prevActions.RemoveMatching(f);
            nextActions.RemoveMatching(f);
        }
    }
}
