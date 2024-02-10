namespace ProgressionSystem
{
    public interface IProgressionEmitter
    {
        public delegate void ProgressionValueChangedHandler(float value);

        public delegate void ProgressionStateChangedHandler(bool success);

        public event ProgressionValueChangedHandler ProgressionValueChanged;
        public event ProgressionStateChangedHandler ProgressionStateChanged;

    }
}
