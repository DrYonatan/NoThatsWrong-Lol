public abstract class TrialSegment
{
    public abstract void Play();

    public virtual void Finish()
    {
        TrialManager.instance.OnSegmentFinished();
    }

    public abstract void HandleGameOver();

}