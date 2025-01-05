namespace EveryCine
{
    public interface ECCinemable
    {
        public void Pause();
        public void Resume();
        public void Stop();
        
        public void AddTime(double t);
    }
}